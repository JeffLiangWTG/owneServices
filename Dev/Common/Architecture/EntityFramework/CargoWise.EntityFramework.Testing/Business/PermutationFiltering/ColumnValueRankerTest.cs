using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ColumnValueRankerTest : TestCaseWithFactory
	{
		public void TestRankerParameterisesOrderByArguments()
		{
			ColumnValueRanker ranker = new ColumnValueRanker();
			ranker.Add(DummyBizoSchema.Z0_Code, (ZString)"A", (ZString)"B", ZString.Empty);
			ranker.GetBestMatch<DummyBusinessObject>(Factory, new ZQuery());
			Assert(SqlEventTracker.Instance.LastSqlEvent.Contains("ORDER BY CASE WHEN Z0_Code = @CWRanker1 THEN 8 ELSE 0 END + CASE WHEN Z0_Code = @CWRanker2 THEN 4 ELSE 0 END + CASE WHEN Z0_Code = @CWRanker3 THEN 2 ELSE 0 END DESC"));
		}

		public void TestEmptyRanker()
		{
			AssertEquals(null, new ColumnValueRanker().GetBestMatch<DummyBusinessObject>(Factory, new ZQuery()));
		}

		public void TestRanker()
		{
			AssertRanker();
		}

		public void TestRankerWithInMemoryFiltering()
		{
			AssertRanker(true);
		}

		void AssertRanker(bool useInMemoryFiltering = false)
		{
			DummyBusinessObject GetBestMatch(ColumnValueRanker valueRanker, ZQuery query)
			{
				if (useInMemoryFiltering)
				{
					return valueRanker.GetBestMatches<DummyBusinessObject>(Factory, query, true, "", 0)[0];
				}
				else
				{
					return valueRanker.GetBestMatch<DummyBusinessObject>(Factory, query);
				}
			}

			BusinessObjectFactory makeFactory = new BusinessObjectFactory();

			DummyBusinessObject bizO4 = makeFactory.New<DummyBusinessObject>();
			bizO4.Z0_Code = "B";
			bizO4.Z0_Number = 1;
			DummyBusinessObject bizO5 = makeFactory.New<DummyBusinessObject>();
			bizO5.Z0_Code = "B";
			bizO5.Z0_Number = 0;
			DummyBusinessObject bizO5a = makeFactory.New<DummyBusinessObject>();
			bizO5a.Z0_Code = "B";
			bizO5a.Z0_Number = 0;
			bizO5a.Z0_Bool = ZBool.True;
			DummyBusinessObject bizO6 = makeFactory.New<DummyBusinessObject>();
			bizO6.Z0_Code = "B";
			bizO6.Z0_Number = 2;

			// To make sure that order does not affect outcome
			DummyBusinessObject bizO1 = makeFactory.New<DummyBusinessObject>();
			bizO1.Z0_Code = "A";
			bizO1.Z0_Number = 1;
			DummyBusinessObject bizO2 = makeFactory.New<DummyBusinessObject>();
			bizO2.Z0_Code = "A";
			bizO2.Z0_Number = 0;
			DummyBusinessObject bizO3 = makeFactory.New<DummyBusinessObject>();
			bizO3.Z0_Code = "A";
			bizO3.Z0_Number = 2;

			DummyBusinessObject bizO7 = makeFactory.New<DummyBusinessObject>();
			bizO7.Z0_Number = 1;
			DummyBusinessObject bizO8 = makeFactory.New<DummyBusinessObject>();
			bizO8.Z0_Number = 0;
			DummyBusinessObject bizO9 = makeFactory.New<DummyBusinessObject>();
			bizO9.Z0_Number = 2;

			DummyBusinessObject bizO10 = makeFactory.New<DummyBusinessObject>();
			bizO10.Z0_Code = "C";
			bizO10.Z0_Number = 1;
			DummyBusinessObject bizO11 = makeFactory.New<DummyBusinessObject>();
			bizO11.Z0_Code = "C";
			bizO11.Z0_Number = 0;
			DummyBusinessObject bizO12 = makeFactory.New<DummyBusinessObject>();
			bizO12.Z0_Code = "C";
			bizO12.Z0_Number = 2;

			makeFactory.Save();

			ColumnValueRanker ranker = new ColumnValueRanker();
			ranker.Add(DummyBizoSchema.Z0_Code, (ZString)"A", (ZString)"B", ZString.Empty);
			ranker.Add(DummyBizoSchema.Z0_Number, (ZInt)1, ZInt.Zero);
			DummyBusinessObject bizORank1 = GetBestMatch(ranker, new ZQuery());
			AssertEquals("Z0_Code = " + bizORank1.Z0_Code + System.Environment.NewLine + "Z0_Number = " + bizORank1.Z0_Number.ToString(), bizO1.PK, bizORank1.PK);

			ColumnValueRanker ranker2 = new ColumnValueRanker();
			ranker2.Add(DummyBizoSchema.Z0_Code, (ZString)"B", (ZString)"A", ZString.Empty);
			ranker2.Add(DummyBizoSchema.Z0_Number, ZInt.Zero, (ZInt)1);
			DummyBusinessObject bizORank2 = GetBestMatch(ranker2, new ZQuery(DummyBizoSchema.Z0_Bool, ZBool.True));
			AssertEquals(bizO5a.PK, bizORank2.PK);

			ColumnValueRanker ranker3 = new ColumnValueRanker();
			ranker3.Add(DummyBizoSchema.Z0_Code, (ZString)"B", (ZString)"A", ZString.Empty);
			ranker3.Add(DummyBizoSchema.Z0_Number, ZInt.Zero, (ZInt)1);
			DummyBusinessObject bizORank3 = GetBestMatch(ranker3, new ZQuery(DummyBizoSchema.Z0_Bool, ZBool.False));
			AssertEquals(bizO5.PK, bizORank3.PK);

			ColumnValueRanker ranker4 = new ColumnValueRanker();
			DummyBusinessObject bizORank4 = GetBestMatch(ranker4, new ZQuery(DummyBizoSchema.Z0_Bool, ZBool.False));
			AssertNotNull(bizORank4);
		}

		public void TestGetBestMatches()
		{
			AssertGetBestMatches();
		}

		public void TestGetBestMatchesWithInMemoryFiltering()
		{
			AssertGetBestMatches(true);
		}

		void AssertGetBestMatches(bool useInMemoryFiltering = false)
		{
			using (ColumnValueRanker.ResetCacheOnDisposedForTest())
			{
				#region Test Data

				BusinessObjectFactory makeFactory = new BusinessObjectFactory();

				DummyBusinessObject bizO4 = makeFactory.New<DummyBusinessObject>();
				bizO4.Z0_Code = "B";
				bizO4.Z0_Number = 1;
				DummyBusinessObject bizO5 = makeFactory.New<DummyBusinessObject>();
				bizO5.Z0_Code = "B";
				bizO5.Z0_Number = 0;
				DummyBusinessObject bizO5a = makeFactory.New<DummyBusinessObject>();
				bizO5a.Z0_Code = "B";
				bizO5a.Z0_Number = 0;
				bizO5a.Z0_Bool = ZBool.True;
				DummyBusinessObject bizO6 = makeFactory.New<DummyBusinessObject>();
				bizO6.Z0_Code = "B";
				bizO6.Z0_Number = 2;

				// To make sure that order does not affect outcome
				DummyBusinessObject bizO1 = makeFactory.New<DummyBusinessObject>();
				bizO1.Z0_Code = "A";
				bizO1.Z0_Number = 1;
				DummyBusinessObject bizO2 = makeFactory.New<DummyBusinessObject>();
				bizO2.Z0_Code = "A";
				bizO2.Z0_Number = 0;
				DummyBusinessObject bizO3 = makeFactory.New<DummyBusinessObject>();
				bizO3.Z0_Code = "A";
				bizO3.Z0_Number = 2;

				DummyBusinessObject bizO7 = makeFactory.New<DummyBusinessObject>();
				bizO7.Z0_Number = 1;
				DummyBusinessObject bizO8 = makeFactory.New<DummyBusinessObject>();
				bizO8.Z0_Number = 0;
				DummyBusinessObject bizO9 = makeFactory.New<DummyBusinessObject>();
				bizO9.Z0_Number = 2;

				DummyBusinessObject bizO10 = makeFactory.New<DummyBusinessObject>();
				bizO10.Z0_Code = "C";
				bizO10.Z0_Number = 1;
				DummyBusinessObject bizO11 = makeFactory.New<DummyBusinessObject>();
				bizO11.Z0_Code = "C";
				bizO11.Z0_Number = 0;
				DummyBusinessObject bizO12 = makeFactory.New<DummyBusinessObject>();
				bizO12.Z0_Code = "C";
				bizO12.Z0_Number = 2;

				makeFactory.Save();

				#endregion

				int originalLoadCount = Factory.DatabaseLoadCount;
				int cacheFactoryLoadCount = ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount;
				ColumnValueRanker ranker = new ColumnValueRanker();
				ranker.Add(DummyBizoSchema.Z0_Code, (ZString)"A", (ZString)"B", ZString.Empty);
				ranker.Add(DummyBizoSchema.Z0_Number, (ZInt)1, ZInt.Zero);

				DummyBusinessObject[] matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), useInMemoryFiltering, "KEY1", 10);
				AssertEquals(5, matches.Length);
				AssertEquals(bizO1.PK, matches[0].PK);
				AssertEquals(bizO2.PK, matches[1].PK);
				AssertEquals(bizO4.PK, matches[2].PK);
				AssertEquals(bizO5.PK, matches[3].PK);
				AssertEquals(bizO5a.PK, matches[4].PK);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);
				AssertEquals("Should only be doing 1 LOAD", cacheFactoryLoadCount + 1, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);

				ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), useInMemoryFiltering, "KEY1", 10);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);
				AssertEquals("Should be fetching the records from the cache", cacheFactoryLoadCount + 1, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);

				ranker = new ColumnValueRanker();
				ranker.Add(DummyBizoSchema.Z0_Code, (ZString)"B", (ZString)"A", ZString.Empty);
				ranker.Add(DummyBizoSchema.Z0_Number, (ZInt)1, ZInt.Zero);
				matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), useInMemoryFiltering, "KEY1", 10);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);

				if (useInMemoryFiltering)
				{
					AssertEquals("Should only no extra loads", cacheFactoryLoadCount + 1, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);
				}
				else
				{
					AssertEquals("Should only be doing 1 LOAD", cacheFactoryLoadCount + 2, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);
				}

				AssertEquals(5, matches.Length);
				AssertEquals(bizO4.PK, matches[0].PK);
				AssertEquals(bizO5.PK, matches[1].PK);
				AssertEquals(bizO5a.PK, matches[2].PK);
				AssertEquals(bizO1.PK, matches[3].PK);
				AssertEquals(bizO2.PK, matches[4].PK);

				matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), useInMemoryFiltering, "KEY1", 10);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);
				AssertEquals("Should be fetching the records from the cache", cacheFactoryLoadCount + (useInMemoryFiltering ? 1 : 2), ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);
			}
		}

		[TestDate(2005, 1, 1, 1, 1, 1)]
		public void TestGetBestMatches_CacheTimedOut()
		{
			using (ColumnValueRanker.ResetCacheOnDisposedForTest())
			{
				#region Test Data

				BusinessObjectFactory makeFactory = new BusinessObjectFactory();

				DummyBusinessObject bizO4 = makeFactory.New<DummyBusinessObject>();
				bizO4.Z0_Code = "B";
				bizO4.Z0_Number = 1;
				DummyBusinessObject bizO5 = makeFactory.New<DummyBusinessObject>();
				bizO5.Z0_Code = "B";
				bizO5.Z0_Number = 0;
				DummyBusinessObject bizO5a = makeFactory.New<DummyBusinessObject>();
				bizO5a.Z0_Code = "B";
				bizO5a.Z0_Number = 0;
				bizO5a.Z0_Bool = ZBool.True;
				DummyBusinessObject bizO6 = makeFactory.New<DummyBusinessObject>();
				bizO6.Z0_Code = "B";
				bizO6.Z0_Number = 2;

				// To make sure that order does not affect outcome
				DummyBusinessObject bizO1 = makeFactory.New<DummyBusinessObject>();
				bizO1.Z0_Code = "A";
				bizO1.Z0_Number = 1;
				DummyBusinessObject bizO2 = makeFactory.New<DummyBusinessObject>();
				bizO2.Z0_Code = "A";
				bizO2.Z0_Number = 0;
				DummyBusinessObject bizO3 = makeFactory.New<DummyBusinessObject>();
				bizO3.Z0_Code = "A";
				bizO3.Z0_Number = 2;

				DummyBusinessObject bizO7 = makeFactory.New<DummyBusinessObject>();
				bizO7.Z0_Number = 1;
				DummyBusinessObject bizO8 = makeFactory.New<DummyBusinessObject>();
				bizO8.Z0_Number = 0;
				DummyBusinessObject bizO9 = makeFactory.New<DummyBusinessObject>();
				bizO9.Z0_Number = 2;

				DummyBusinessObject bizO10 = makeFactory.New<DummyBusinessObject>();
				bizO10.Z0_Code = "C";
				bizO10.Z0_Number = 1;
				DummyBusinessObject bizO11 = makeFactory.New<DummyBusinessObject>();
				bizO11.Z0_Code = "C";
				bizO11.Z0_Number = 0;
				DummyBusinessObject bizO12 = makeFactory.New<DummyBusinessObject>();
				bizO12.Z0_Code = "C";
				bizO12.Z0_Number = 2;

				makeFactory.Save();

				#endregion

				int originalLoadCount = Factory.DatabaseLoadCount;
				int cacheFactoryLoadCount = ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount;
				ColumnValueRanker ranker = new ColumnValueRanker();
				ranker.Add(DummyBizoSchema.Z0_Code, (ZString)"A", (ZString)"B", ZString.Empty);
				ranker.Add(DummyBizoSchema.Z0_Number, (ZInt)1, ZInt.Zero);
				DummyBusinessObject[] matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), false, "", 10);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);
				AssertEquals("Should only be doing 1 LOAD", originalLoadCount + 1, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);
				matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), false, "", 10);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);
				AssertEquals("Cache not yet expired, should be fetching the records from the cache", cacheFactoryLoadCount + 1, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);
				matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), false, "", 10);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);
				AssertEquals("Cache not yet expired, should be fetching the records from the cache", cacheFactoryLoadCount + 1, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMilliseconds(100);
				matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), false, "", 10);
				AssertEquals("Should not be doing load from this factory", originalLoadCount, Factory.DatabaseLoadCount);
				AssertEquals("Cache expired, should be loading this again from the database", cacheFactoryLoadCount + 2, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);
			}
		}

		public void TestGetBestMatches_NoCaching()
		{
			using (ColumnValueRanker.ResetCacheOnDisposedForTest())
			{
				#region Test Data

				BusinessObjectFactory makeFactory = new BusinessObjectFactory();

				DummyBusinessObject bizO4 = makeFactory.New<DummyBusinessObject>();
				bizO4.Z0_Code = "B";
				bizO4.Z0_Number = 1;
				DummyBusinessObject bizO5 = makeFactory.New<DummyBusinessObject>();
				bizO5.Z0_Code = "B";
				bizO5.Z0_Number = 0;
				DummyBusinessObject bizO5a = makeFactory.New<DummyBusinessObject>();
				bizO5a.Z0_Code = "B";
				bizO5a.Z0_Number = 0;
				bizO5a.Z0_Bool = ZBool.True;
				DummyBusinessObject bizO6 = makeFactory.New<DummyBusinessObject>();
				bizO6.Z0_Code = "B";
				bizO6.Z0_Number = 2;

				// To make sure that order does not affect outcome
				DummyBusinessObject bizO1 = makeFactory.New<DummyBusinessObject>();
				bizO1.Z0_Code = "A";
				bizO1.Z0_Number = 1;
				DummyBusinessObject bizO2 = makeFactory.New<DummyBusinessObject>();
				bizO2.Z0_Code = "A";
				bizO2.Z0_Number = 0;
				DummyBusinessObject bizO3 = makeFactory.New<DummyBusinessObject>();
				bizO3.Z0_Code = "A";
				bizO3.Z0_Number = 2;

				DummyBusinessObject bizO7 = makeFactory.New<DummyBusinessObject>();
				bizO7.Z0_Number = 1;
				DummyBusinessObject bizO8 = makeFactory.New<DummyBusinessObject>();
				bizO8.Z0_Number = 0;
				DummyBusinessObject bizO9 = makeFactory.New<DummyBusinessObject>();
				bizO9.Z0_Number = 2;

				DummyBusinessObject bizO10 = makeFactory.New<DummyBusinessObject>();
				bizO10.Z0_Code = "C";
				bizO10.Z0_Number = 1;
				DummyBusinessObject bizO11 = makeFactory.New<DummyBusinessObject>();
				bizO11.Z0_Code = "C";
				bizO11.Z0_Number = 0;
				DummyBusinessObject bizO12 = makeFactory.New<DummyBusinessObject>();
				bizO12.Z0_Code = "C";
				bizO12.Z0_Number = 2;

				makeFactory.Save();

				#endregion

				int originalLoadCount = Factory.DatabaseLoadCount;
				int cacheFactoryLoadCount = ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount;
				ColumnValueRanker ranker = new ColumnValueRanker();
				ranker.Add(DummyBizoSchema.Z0_Code, (ZString)"A", (ZString)"B", ZString.Empty);
				ranker.Add(DummyBizoSchema.Z0_Number, (ZInt)1, ZInt.Zero);

				DummyBusinessObject[] matches = ranker.GetBestMatches<DummyBusinessObject>(Factory, new ZQuery(), false, "", 0);
				AssertEquals(5, matches.Length);
				AssertEquals(bizO1.PK, matches[0].PK);
				AssertEquals(bizO2.PK, matches[1].PK);
				AssertEquals(bizO4.PK, matches[2].PK);
				AssertEquals(bizO5.PK, matches[3].PK);
				AssertEquals(bizO5a.PK, matches[4].PK);
				AssertEquals("Should be loading the templates", originalLoadCount + 1, Factory.DatabaseLoadCount);
				AssertEquals("Should not use the CacheFactory", cacheFactoryLoadCount, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				int newFactoryOriginalLoadCount = newFactory.DatabaseLoadCount;
				ranker.GetBestMatches<DummyBusinessObject>(newFactory, new ZQuery(), false, "", 0);
				AssertEquals("Should be re-loading the templates", newFactoryOriginalLoadCount + 1, newFactory.DatabaseLoadCount);
				AssertEquals("Should not use the CacheFactory", cacheFactoryLoadCount, ColumnValueRanker.CacheFactoryForTest.DatabaseLoadCount);
			}
		}
	}
}
