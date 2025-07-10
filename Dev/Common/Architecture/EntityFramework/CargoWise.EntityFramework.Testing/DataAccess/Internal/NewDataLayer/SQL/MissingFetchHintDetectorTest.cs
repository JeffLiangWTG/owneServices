using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.DataAccess.Internal.NewDataLayer.Sql;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess.Internal.NewDataLayer.SQL
{
	[SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
	sealed class MissingFetchHintDetectorTest : TransactionedTestCase
	{
		public void TestTooManyDbHitsAtSameTimeFails()
		{
			var query = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			for (int i = 0; i < 25; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			AssertContains("Excessive db hits. Consider fetch hints to support this query", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTooManyDbHitsDetectionNotEnabledPasses()
		{
			DbRegistry.MissingFetchHintDetection.SaveValue(false, connectionInfo.DbConnection);
			MissingFetchHintDetector.Instance.InitialiseFetchHintDetectionValues(connectionInfo.DbConnection);

			var query = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			for (int i = 0; i < 25; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			AssertEquals("", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTooManyDbHitsInHalfSecondFails()
		{
			var query = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			for (int i = 0; i < 10; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			Thread.Sleep(500);

			for (int i = 0; i < 15; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			AssertContains("Excessive db hits. Consider fetch hints to support this query", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLessThanMaximumDbHitsPasses()
		{
			var query = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			for (int i = 0; i < 24; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			AssertEquals("", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDbHitsSpreadOverTwoSecondsPasses()
		{
			var query = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			for (int i = 0; i < 20; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			Thread.Sleep(2000);

			for (int i = 0; i < 20; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			AssertEquals("", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSameCallStackDifferentQueriesPasses()
		{
			var firstQuery = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			var tempQuery = new ZQuery();
			tempQuery.AddFilterAndZSQLParameterCollection("1 = 1" + new string(' ', ZSqlLoader.MaximumCommandTextLengthInCharacters), null);
			var secondQuery = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, tempQuery);

			for (int i = 0; i < 20; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(firstQuery);
			}

			for (int i = 0; i < 20; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(secondQuery);
			}

			AssertEquals("", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDifferentCallStackSameQueriesPasses()
		{
			var query = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			for (int i = 0; i < 20; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}

			var completedWithNoErrors = ChangingCallStackFunction(query);

			AssertEquals(true, completedWithNoErrors);
			AssertEquals("", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public bool ChangingCallStackFunction(ZDataQuery query)
		{
			for (int i = 0; i < 20; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(query);
			}
			if (string.IsNullOrEmpty(ErrorReporter.LastMessageReported))
			{
				return true;
			}
			return false;
		}

		public void TestPreviousUniqueQueriesRemoved()
		{
			// Changing UniqueQueryRemovalTime to 1 second to ensure tests complete faster while retaining functionality (Testing RemoveUniqueQueries method)
			MissingFetchHintDetector.Instance.ChangeQueryRemovalTimesForTesting(1, 1);

			var firstQuery = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			var tempQuery = new ZQuery();
			tempQuery.AddFilterAndZSQLParameterCollection("1 = 1" + new string(' ', ZSqlLoader.MaximumCommandTextLengthInCharacters), null);
			var secondQuery = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, tempQuery);

			for (int i = 0; i < 5; i++)
			{
				loader.LoadPersistentRowsIntoDataSet(firstQuery);
			}

			AssertEquals(1, MissingFetchHintDetector.Instance.queries.Count);
			AssertEquals(5, MissingFetchHintDetector.Instance.queries.ElementAt(0).Value.Count);

			Thread.Sleep(1500);

			loader.LoadPersistentRowsIntoDataSet(secondQuery);
			// All previous queries should be completely removed
			AssertEquals(1, MissingFetchHintDetector.Instance.queries.Count);
			AssertEquals(1, MissingFetchHintDetector.Instance.queries.ElementAt(0).Value.Count);
		}

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();
		ZSqlConnectionInfo connectionInfo;
		ZSqlLoader loader;

		protected override void SetUp()
		{
			base.SetUp();
			connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			loader = new ZSqlLoader(new DataSet(), connectionInfo, GetNewSchemaResolver());
			DbRegistry.MissingFetchHintDetection.SaveValue(true, connectionInfo.DbConnection);
			DbRegistry.MissingFetchHintThreshold.SaveValue(25, connectionInfo.DbConnection);
			MissingFetchHintDetector.Instance.InitialiseFetchHintDetectionValues(connectionInfo.DbConnection);
			// Changing QueryRemovalTime to 1 second to ensure tests complete faster while retaining functionality
			MissingFetchHintDetector.Instance.ChangeQueryRemovalTimesForTesting(1);
			MissingFetchHintDetector.Instance.queries.Clear();
		}

		protected override void TearDown()
		{
			DbRegistry.MissingFetchHintDetection.SaveValue(false, connectionInfo.DbConnection);
			DbRegistry.MissingFetchHintThreshold.SaveValue(25, connectionInfo.DbConnection);
			MissingFetchHintDetector.Instance.InitialiseFetchHintDetectionValues(connectionInfo.DbConnection);
			MissingFetchHintDetector.Instance.ChangeQueryRemovalTimesForTesting();
			MissingFetchHintDetector.Instance.queries.Clear();
			base.TearDown();
		}
	}
}
