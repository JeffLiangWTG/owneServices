using System;
using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class LoadStatTest : TestCaseWithDummy
	{
		public void TestConstructor()
		{
			var hitCount = new TableHitCount(DummyBizoSchema.Constants.TableName, 2);
			var loadStat = new LoadStat(3, new[] { hitCount });
			AssertEquals(nameof(loadStat.DatabaseLoadCount), 3, loadStat.DatabaseLoadCount);
			AssertContainsExactElementsInAnyOrder(nameof(loadStat.HitCounts), new[] { hitCount }, loadStat.HitCounts);
		}

		public void TestSubtract()
		{
			var hitCount1_1 = new TableHitCount(DummyBizoSchema.Constants.TableName, 3);
			var hitCount1_2 = new TableHitCount(StmALogSchema.Constants.TableName, 9);
			var loadStat1 = new LoadStat(6, new[] { hitCount1_1, hitCount1_2 });

			var hitCount2_1 = new TableHitCount(DummyBizoSchema.Constants.TableName, 1);
			var hitCount2_2 = new TableHitCount(StmALogSchema.Constants.TableName, 6);
			var loadStat2 = new LoadStat(2, new[] { hitCount2_2, hitCount2_1 });

			var loadStat3 = loadStat1 - loadStat2;
			AssertEquals(nameof(loadStat3.DatabaseLoadCount), 4, loadStat3.DatabaseLoadCount);

			var hitCount3_1 = loadStat3.HitCounts.Single(hc => hc.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(nameof(hitCount3_1.Value), 2, hitCount3_1.Value);
			AssertNull(nameof(hitCount3_1.Queries), hitCount3_1.Queries);

			var hitCount3_2 = loadStat3.HitCounts.Single(hc => hc.TableName == StmALogSchema.Constants.TableName);
			AssertEquals(nameof(hitCount3_2.Value), 3, hitCount3_2.Value);
			AssertNull(nameof(hitCount3_2.Queries), hitCount3_2.Queries);
		}

		public void TestSubtract_NullThrows()
		{
			var hitCount1 = new TableHitCount(DummyBizoSchema.Constants.TableName, 3);
			var loadStat1 = new LoadStat(1, new[] { hitCount1 });

			AssertExceptionThrown<ArgumentNullException>(() => _ = loadStat1 - null);
			AssertExceptionThrown<ArgumentNullException>(() => _ = null - loadStat1);
		}

		public void TestSubtract_EmptyHitCountInOriginal()
		{
			var hitCount1 = new TableHitCount(DummyBizoSchema.Constants.TableName, 3);
			var loadStat1 = new LoadStat(6, new[] { hitCount1 });

			var loadStat2 = new LoadStat(2, Enumerable.Empty<TableHitCount>());

			var loadStat3 = loadStat1 - loadStat2;
			AssertEquals(nameof(loadStat3.DatabaseLoadCount), 4, loadStat3.DatabaseLoadCount);

			var hitCount3 = loadStat3.HitCounts.Single();
			AssertEquals(nameof(hitCount3.TableName), DummyBizoSchema.Constants.TableName, hitCount3.TableName);
			AssertEquals(nameof(hitCount3.Value), 3, hitCount3.Value);
			AssertNull(nameof(hitCount3.Queries), hitCount3.Queries);
		}

		public void TestSubtract_NoChangeInLoadCount_ShouldExcludeHitCountData()
		{
			var hitCount1 = new TableHitCount(DummyBizoSchema.Constants.TableName, 3);
			var loadStat1 = new LoadStat(2, new[] { hitCount1 });

			var hitCount2 = new TableHitCount(DummyBizoSchema.Constants.TableName, 1);
			var loadStat2 = new LoadStat(2, new[] { hitCount2 });

			var loadStat3 = loadStat1 - loadStat2;
			AssertEquals(nameof(loadStat3.DatabaseLoadCount), 0, loadStat3.DatabaseLoadCount);
			AssertEquals(nameof(loadStat3.HitCounts), 0, loadStat3.HitCounts.Count());
		}

		public void TestSubtract_WithQueries()
		{
			var query1 = new TableHitQuery("SOME QUERY", "SOME STACKTRACE");
			var query2 = new TableHitQuery("SOME OTHER QUERY", "SOME OTHER STACKTRACE");

			var hitCount1 = new TableHitCount(DummyBizoSchema.Constants.TableName, 3, new[] { query1, query2 });
			var loadStat1 = new LoadStat(6, new[] { hitCount1 });

			var hitCount2 = new TableHitCount(DummyBizoSchema.Constants.TableName, 1, new[] { query1 });
			var loadStat2 = new LoadStat(2, new[] { hitCount2 });

			var loadStat3 = loadStat1 - loadStat2;
			AssertEquals(nameof(loadStat3.DatabaseLoadCount), 4, loadStat3.DatabaseLoadCount);

			var hitCount3 = loadStat3.HitCounts.Single();
			AssertEquals(nameof(hitCount3.TableName), DummyBizoSchema.Constants.TableName, hitCount3.TableName);
			AssertEquals(nameof(hitCount3.Value), 2, hitCount3.Value);
			AssertContainsExactElementsInAnyOrder(nameof(hitCount3.Queries), new[] { query2 }, hitCount3.Queries);
		}

		public void TestSubtract_WithQueries_NullInOriginalLoadStat()
		{
			var query = new TableHitQuery("SOME QUERY", "SOME STACKTRACE");

			var hitCount1 = new TableHitCount(DummyBizoSchema.Constants.TableName, 3, new[] { query });
			var loadStat1 = new LoadStat(6, new[] { hitCount1 });

			var hitCount2 = new TableHitCount(DummyBizoSchema.Constants.TableName, 1);
			var loadStat2 = new LoadStat(2, new[] { hitCount2 });

			var loadStat3 = loadStat1 - loadStat2;
			AssertEquals(nameof(loadStat3.DatabaseLoadCount), 4, loadStat3.DatabaseLoadCount);

			var hitCount3 = loadStat3.HitCounts.Single();
			AssertEquals(nameof(hitCount3.TableName), DummyBizoSchema.Constants.TableName, hitCount3.TableName);
			AssertEquals(nameof(hitCount3.Value), 2, hitCount3.Value);
			AssertContainsExactElementsInAnyOrder(nameof(hitCount3.Queries), new[] { query }, hitCount3.Queries);
		}
	}
}
