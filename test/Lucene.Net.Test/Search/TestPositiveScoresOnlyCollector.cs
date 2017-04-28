/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Lucene.Net.Store;
using NUnit.Framework;

using LuceneTestCase = Lucene.Net.Util.LuceneTestCase;

namespace Lucene.Net.Search
{
	
    [TestFixture]
	public class TestPositiveScoresOnlyCollector:LuceneTestCase
	{
		
		private sealed class SimpleScorer:Scorer
		{
			private int idx = - 1;
			
			public SimpleScorer():base(null)
			{
			}
			
			public override float Score(IState state)
			{
			    return idx == scores.Length ? float.NaN : scores[idx];
			}
			
			public override int DocID()
			{
				return idx;
			}
			
			public override int NextDoc(IState state)
			{
			    return ++idx != scores.Length ? idx : NO_MORE_DOCS;
			}
			
			public override int Advance(int target, IState state)
			{
				idx = target;
			    return idx < scores.Length ? idx : NO_MORE_DOCS;
			}
		}
		
		// The scores must have positive as well as negative values
		private static readonly float[] scores = new float[]{0.7767749f, - 1.7839992f, 8.9925785f, 7.9608946f, - 0.07948637f, 2.6356435f, 7.4950366f, 7.1490803f, - 8.108544f, 4.961808f, 2.2423935f, - 7.285586f, 4.6699767f};
		
        [Test]
		public virtual void  TestNegativeScores()
		{
			
			// The Top*Collectors previously filtered out documents with <= scores. This
			// behavior has changed. This test checks that if PositiveOnlyScoresFilter
			// wraps one of these collectors, documents with <= 0 scores are indeed
			// filtered.
			
			int numPositiveScores = 0;
			for (int i = 0; i < scores.Length; i++)
			{
				if (scores[i] > 0)
				{
					++numPositiveScores;
				}
			}
			
			Scorer s = new SimpleScorer();
			TopDocsCollector<ScoreDoc> tdc = TopScoreDocCollector.Create(scores.Length, true);
			Collector c = new PositiveScoresOnlyCollector(tdc);
			c.SetScorer(s);
			while (s.NextDoc(null) != DocIdSetIterator.NO_MORE_DOCS)
			{
				c.Collect(0, null);
			}
			TopDocs td = tdc.TopDocs();
			ScoreDoc[] sd = td.ScoreDocs;
			Assert.AreEqual(numPositiveScores, td.TotalHits);
			for (int i = 0; i < sd.Length; i++)
			{
				Assert.IsTrue(sd[i].Score > 0, "only positive scores should return: " + sd[i].Score);
			}
		}
	}
}