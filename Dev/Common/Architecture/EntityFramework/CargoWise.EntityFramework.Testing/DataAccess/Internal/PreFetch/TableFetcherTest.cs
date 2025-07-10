using System.Data;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed partial class TableFetcherTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestGetTableNameQueries_Performance()
		{
			var stopWatch = new Stopwatch();
			for (ZShort i = 0; i < 32000; i++)
			{
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.PK, ZGuid.NewZGuid()));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Number, i));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid()));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Short, i));
			}

			using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				stopWatch.Start();
				tableFetcher.GetTableNameQueries(fetchHintQueryStorer);
				AssertLessThan(stopWatch.ElapsedMilliseconds, 9000);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		public void TestSystemSurvivesHugeFetchHint_Parameterized()
		{
			const int TestRunCount = ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION * 3;

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				BusinessObjectFactory factoryForInsert = new BusinessObjectFactory();
				DummyBusinessObject bizO = null;
				for (int i = 0; i < TestRunCount; i++)
				{
					bizO = DummyBusinessObject.New(factoryForInsert);
					bizO.Z0_Code = i.ToString();
					Factory.AddFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, bizO.Z0_Code);
				}

				factoryForInsert.Save();

				AssertEquals("Precondition : Hints should actually exist", TestRunCount, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

				Factory.Load(typeof(DummyBusinessObject), bizO.PK); // Load existing PK
				AssertEquals(0, Factory.ActiveTableFetchHints);
				// 5 is because max param count is now 64 by default due to double index bug
				AssertEquals("DatabaseLoadCount", 5, Factory.DatabaseLoadCount);
				ErrorReporter.Clear();
			}
		}

		public void TestFetchLoadsRowsFromDatabase()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ConcatenateMultipleFetchHintTypes = true;
				using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
				{
					IFetchHint hint = new ZQueryFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "Z"));
					tableFetcher.AddFetchHint(hint);
					rowFactory.FetchRowsIntoDataSet(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));
				}
				AssertEquals("DatabaseLoadCount", 1, rowFactory.DatabaseLoadCount);
			}
		}

		public void TestFetchLoadsRowsOptimally()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ConcatenateMultipleFetchHintTypes = true;

				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.PK, PK1));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.PK, PK2));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Guid, ForeignKey1));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Guid, ForeignKey2));
				using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
				{
					rowFactory.FetchRowsIntoDataSet(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));
				}

				DataTable dataTable = rowFactory.GetTable(DummyBizoSchema.Constants.TableName);
				AssertEquals("Rows.Count", 4, dataTable.Rows.Count);
				AssertEquals("DatabaseLoadCount", 1, rowFactory.DatabaseLoadCount);
			}
		}

		// Same test as above, but will generate multiple select statements
		public void TestDeTunedFetching()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ConcatenateMultipleFetchHintTypes = false;

				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.PK, PK1));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.PK, PK2));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Guid, ForeignKey1));
				tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Guid, ForeignKey2));
				using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
				{
					rowFactory.FetchRowsIntoDataSet(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));
				}

				DataTable dataTable = rowFactory.GetTable(DummyBizoSchema.Constants.TableName);
				AssertEquals("Rows.Count", 4, dataTable.Rows.Count);
				AssertEquals("DatabaseLoadCount", 2, rowFactory.DatabaseLoadCount);
			}
		}

		public void TestFetchHintEmptyQuery()
		{
			tableFetcher.AddFetchHint(new ZQueryFetchHint(DummyBizoSchema.Instance, new ZQuery()));
			AssertEquals("Precondition : RowFactory.DatabaseLoadCount", 0, rowFactory.DatabaseLoadCount);
			using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				rowFactory.FetchRowsIntoDataSet(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));
			}
			AssertEquals("Empty query should result in no fetch hints being fired", 0, rowFactory.DatabaseLoadCount);
		}

		public void TestColumnFetchHintThenColumnLoadDoesNotHitDB()
		{
			ZString code = new ZString("123");
			tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Code, code));
			AssertEquals("Precondition : RowFactory.DatabaseLoadCount", 0, rowFactory.DatabaseLoadCount);
			using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				rowFactory.FetchRowsIntoDataSet(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));
			}
			AssertEquals("Precondition : RowFactory.DatabaseLoadCount", 1, rowFactory.DatabaseLoadCount);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, code);
			rowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("RowFactory.DatabaseLoadCount after Load", 1, rowFactory.DatabaseLoadCount);
		}

		public void TestFetchSetsDataLoaded()
		{
			FetchHint hint = new FetchHint(DummyBizoSchema.Z0_Code, ZString.Empty);
			tableFetcher.AddFetchHint(hint);
			AssertEquals("Precondition", false, hint.IsDataHintLoaded);
			using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				rowFactory.FetchRowsIntoDataSet(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));
			}
			AssertEquals(true, hint.IsDataHintLoaded);
		}

		public void TestFetchLoadsRows()
		{
			tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.PK, PK1));
			using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				rowFactory.FetchRowsIntoDataSet(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));
			}
			DataTable dataTable = rowFactory.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals("Rows.Count", 1, dataTable.Rows.Count);
		}

		public void TestFetchHintOptimisation()
		{
			ZString code = new ZString("123");
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, code);
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 56);
			tableFetcher.AddFetchHint(new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, secondQuery));
			tableFetcher.AddFetchHint(new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, secondQuery));
			tableFetcher.AddFetchHint(new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), mainQuery));
			tableFetcher.AddFetchHint(new ZQueryFetchHint(DummyBizoSchema.Instance, mainQuery));
			tableFetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, code));
			tableFetcher.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Code, code));
			AssertEquals("Precondition : RowFactory.DatabaseLoadCount", 0, rowFactory.DatabaseLoadCount);
			using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				TableNameQuery[] tableNameQueries = tableFetcher.GetTableNameQueries(fetchHintQueryStorer);
				AssertEquals(1, tableNameQueries.Length);
				AssertEquals(DummyBizoSchema.Constants.TableName, tableNameQueries[0].TableName);
				AssertEquals("Z0_Code = '123'", tableNameQueries[0].Query.LiteralTextADO);
				rowFactory.FetchRowsIntoDataSet(tableNameQueries);
			}
			AssertEquals("Precondition : RowFactory.DatabaseLoadCount", 1, rowFactory.DatabaseLoadCount);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, code);
			filter.AddToFilter(DummyBizoSchema.Z0_Number, 56);
			rowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("RowFactory.DatabaseLoadCount after Load", 1, rowFactory.DatabaseLoadCount);

			filter = new ZQuery(DummyBizoSchema.Z0_Code, code);
			rowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("RowFactory.DatabaseLoadCount after Load", 1, rowFactory.DatabaseLoadCount);
		}

		public void TestGeneratedQueryRespectsCannotBeJoinedForFetchHintFlag()
		{
			ZQuery query1 = new ZQuery(DummyBizoSchema.Z0_Code, "XYZ");
			tableFetcher.AddFetchHint(new ZQueryFetchHint(DummyBizoSchema.Instance, query1));

			ZQuery query2 = new ZQuery(DummyBizoSchema.Z0_Code, "ABC");
			((ISeparateFetchQuery)query2).CannotBeJoinedInFetchHint = true;
			tableFetcher.AddFetchHint(new ZQueryFetchHint(DummyBizoSchema.Instance, query2));

			using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(rowFactory.QueryCache))
			{
				TableNameQuery[] tableNameQueries = tableFetcher.GetTableNameQueries(fetchHintQueryStorer);
				AssertEquals(2, tableNameQueries.Length);
			}
		}

		public void TestFetchingViaPKOfStmALogErrors()
		{
			var fetcher = new TableFetcher("StmALog");
			var mock = new Mock<IFetchHint>();
			mock.Setup(m => m.GetHashKeyObject()).Returns(new FetchHint.EnumerableHashObject { "StmALogSL_PK" });
			mock.Setup(m => m.TableName).Returns("StmALog");

			mock.Object.GetHashKeyObject();

			mock.Verify(m => m.GetHashKeyObject(), Times.Once());

			fetcher.AddFetchHint(mock.Object);
			AssertEquals("FetchHintOnStmALogPK", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}
		#region Implementation

		protected override void SetUp()
		{
			// Pre-populate the uber cache to prevent polluting hit counts
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
			PK1 = DummyBusinessObject.New(Factory).PK;
			PK2 = DummyBusinessObject.New(Factory).PK;
			ForeignKey1 = ZGuid.NewZGuid();
			ForeignKey2 = ZGuid.NewZGuid();
			DummyBusinessObject bizO3 = DummyBusinessObject.New(Factory);
			bizO3.Z0_Guid = ForeignKey1;
			DummyBusinessObject bizO4 = DummyBusinessObject.New(Factory);
			bizO4.Z0_Guid = ForeignKey2;
			Factory.Save();
			rowFactory = new RowFactory();
			tableFetcher = new TableFetcher(DummyBizoSchema.Constants.TableName);
		}

		ZGuid PK1;
		ZGuid PK2;
		ZGuid ForeignKey1;
		ZGuid ForeignKey2;
		RowFactory rowFactory;
		TableFetcher tableFetcher;

		#endregion
	}
}
