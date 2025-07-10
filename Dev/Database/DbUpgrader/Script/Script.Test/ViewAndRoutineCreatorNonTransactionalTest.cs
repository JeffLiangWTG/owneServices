using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Data;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions;
using Enterprise.DbUpgrader.Script.Test.TestSetup;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class ViewAndRoutineCreatorNonTransactionalTest : TestCase
	{
		public void TestCreateTemporaryScriptIndexes()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(new ViewStmData());

					testCreator.CreateTemporaryScriptIndexes();

					AssertEquals(@"	Base scripts
	Client Specific scripts
Comparing indexed views
Creating temporary indexes and columns for offline indexed view synchronisation
    (+) Creating temporary columns for view: [dbo].[ViewStmData]
        (~) Creating index for view: [dbo].[ViewStmData]
", testCreator.Manager.InfoMessages.ToString());

					AssertEquals(true, DbObjectCreator.ColumnExists(connection, "StmData", "CW!!ViewStmData_AddInfoString35"));
					AssertEquals(true, DbObjectCreator.IndexExists(connection, "StmData", "_WTG_ViewStmData"));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestCreateTemporaryScriptIndexes_RetainExistingIndexIfDefinitionsMatch()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(new ViewStmData());

					testCreator.CreateTemporaryScriptIndexes();
					AssertEquals(@"	Base scripts
	Client Specific scripts
Comparing indexed views
Creating temporary indexes and columns for offline indexed view synchronisation
    (+) Creating temporary columns for view: [dbo].[ViewStmData]
        (~) Creating index for view: [dbo].[ViewStmData]
", testCreator.Manager.InfoMessages.ToString());

					testCreator.Manager.InfoMessages.Clear();
					testCreator.CreateTemporaryScriptIndexes();

					AssertEquals(@"Comparing indexed views
Creating temporary indexes and columns for offline indexed view synchronisation
", testCreator.Manager.InfoMessages.ToString());

					AssertEquals(true, DbObjectCreator.ColumnExists(connection, "StmData", "CW!!ViewStmData_AddInfoString35"));
					AssertEquals(true, DbObjectCreator.IndexExists(connection, "StmData", "_WTG_ViewStmData"));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestCreateTemporaryScriptIndexes_NothingCreatedIfScriptsAreTheSame()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var script = new ViewStmData();
					connection.ExecuteNonQuery(script.Text);

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(script);

					testCreator.CreateTemporaryScriptIndexes();

					AssertEquals(false, DbObjectCreator.ColumnExists(connection, "StmData", "CW!!ViewStmData_AddInfoString35"));
					AssertEquals(false, DbObjectCreator.IndexExists(connection, "StmData", "_WTG_ViewStmData"));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestDevelopmentOnlyScriptsAreExcluded()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				testCreator.IncludeDevelopmentOnlyScriptsOverride = true;
				Assert(testCreator.GetViewAndRoutineScriptCollection_Exposed().Select(script => script.Name).Contains("ZZDummyBizo"));

				testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				testCreator.IncludeDevelopmentOnlyScriptsOverride = false;
				Assert(!testCreator.GetViewAndRoutineScriptCollection_Exposed().Select(script => script.Name).Contains("ZZDummyBizo"));
			}
		}

		[ExpectNoExceptions]
		public void TestCreateTemporaryScriptIndexes_TableDoesNotExist()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();
					connection.ExecuteNonQuery(@"
DROP VIEW ZZDummyBizo
DROP VIEW ZZDummyBizo_Idx
DROP TABLE DummyDependentBizo;
DROP TABLE DummyBizo;");

					var mockScript = new Mock<DbCreateIndexedViewScript>();
					mockScript.CallBase = true;
					mockScript.Setup(m => m.Text).Returns(@"
CREATE VIEW ZZDummyBizo
WITH SCHEMABINDING
AS
SELECT
	Z0_PK,
	CAST(CASE WHEN CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo)+1) - (CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo)+17)), '¤', '*') ELSE '' END AS VARCHAR(35)) AS Z0_AddInfoString35
FROM dbo.DummyBizo T
WHERE Z0_Code = 'ZZ'");
					var mockScriptT = mockScript.As<IIndexedViewWithTemporaryIndexesOrColumns>();
					mockScriptT.Setup(m => m.TemporaryIndexes).Returns(new List<IndexInfo>()
					{
						IndexInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, "DummyBizo",
								IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX + "ZZDummyBizo")
							.Unique(true)
							.Clustered(false)
							.Key("Z0_PK")
							.Include(
								"Z0_AddInfo",
								"CW!!ZZDummyBizo_AddInfoString35"
							)
							.Where("Z0_Code = 'ZZ'")
							.Option(IndexOptions.ONLINE, value: true)
							.GetInfo(),
					});
					mockScriptT.Setup(m => m.TemporaryComputedColumns).Returns(new List<ComputedColumnInfo>
					{
						ComputedColumnInfo.Builder.New(WellKnownSqlNames.DbOwnerSchema, "DummyBizo")
							.Name("CW!!ZZDummyBizo_AddInfoString35")
							.ComputedExpression("CAST(CASE WHEN CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo)+1) - (CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo)+17)), '¤', '*') ELSE '' END AS VARCHAR(35))")
							.GetInfo()
					});

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(mockScript.Object);

					testCreator.CreateTemporaryScriptIndexes();
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestCreateTemporaryScriptIndexes_DropExistingIndexes()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					connection.ExecuteNonQuery("CREATE INDEX [_WTG_ViewStmData] ON StmData ([SD_PK])");

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(new ViewStmData());

					testCreator.CreateTemporaryScriptIndexes();
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestCreateTemporaryScriptIndexes_DropExistingColumns()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					connection.ExecuteNonQuery("ALTER TABLE dbo.StmData ADD [CW!!ViewStmData_AddInfoString35] AS 'A';");
					connection.ExecuteNonQuery("CREATE INDEX [_WTG_ViewStmData] ON StmData ([CW!!ViewStmData_AddInfoString35])");

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(new ViewStmData());

					testCreator.CreateTemporaryScriptIndexes();
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestTemporaryScriptIndexesAreDropped()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					connection.ExecuteNonQuery("ALTER TABLE dbo.StmData ADD [CW!!ViewStmData_AddInfoString35] AS CURRENT_TIMESTAMP;");
					connection.ExecuteNonQuery("CREATE INDEX [_WTG_ViewStmData] ON StmData ([SD_PK])");

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(new ViewStmData());

					testCreator.Run();

					AssertEquals(false, DbObjectCreator.ColumnExists(connection, "StmData", "CW!!ViewStmData_AddInfoString35"));
					AssertEquals(false, DbObjectCreator.IndexExists(connection, "StmData", "_WTG_ViewStmData"));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestCreateTemporaryScriptIndexes_LogFullnessMonitor()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(new ViewStmData());

					var logFullnessProvider = testCreator.LogFullnessProvider;
					logFullnessProvider.BacklogSizeForTesting = 81L;
					logFullnessProvider.AfterGetCurrentBacklog = () => logFullnessProvider.BacklogSizeForTesting = 79L;
					testCreator.CreateTemporaryScriptIndexes();

					AssertEquals(@"	Base scripts
	Client Specific scripts
Comparing indexed views
Creating temporary indexes and columns for offline indexed view synchronisation
    (+) Creating temporary columns for view: [dbo].[ViewStmData]
        (~) Creating index for view: [dbo].[ViewStmData]
            waiting for backlog to clear, transaction log fullness (current: 81%, acceptable: 80%). Please run the LBK service task to reduce log fullness.
", testCreator.Manager.InfoMessages.ToString());

					AssertEquals(true, DbObjectCreator.ColumnExists(connection, "StmData", "CW!!ViewStmData_AddInfoString35"));
					AssertEquals(true, DbObjectCreator.IndexExists(connection, "StmData", "_WTG_ViewStmData"));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		/// <summary>
		/// Handles Different Character Casing Names In Transaction
		/// </summary>
		public void TestCreateOrAlterViewsAndRoutinesHandlesDifferentNameCasing()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					connection.ExecuteNonQuery("CREATE PROCEDURE TestDifferentCaseProc AS SELECT 'SOMETHING'");
					string procResult = connection.ExecuteScalar("EXEC TestDifferentCaseProc").ToString();
					AssertEquals("[PRE-CONDITION] Stored Procedure result initially", "SOMETHING", procResult);

					IDbScript testScript = new DbScript(
						"TESTDifferentCaseProc",
						"CREATE PROCEDURE TestDifferentCaseProc AS SELECT 'SOMETHING ELSE'",
						DbRoutineType.SqlProcedureTypeDesc);

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(testScript);

					testCreator.Run();

					procResult = connection.ExecuteScalar("EXEC TestDifferentCaseProc").ToString();
					AssertEquals("[PRE-CONDITION] Stored Procedure result initially", "SOMETHING ELSE", procResult);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		#region Indexed View Only Recreated If Modified

		public void TestIndexedViewOnlyRecreatedIfModified()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

					// Indexed view with no changes => should not be altered
					var idxViewUnchanged = new MockIndexedViewDbScript("_test_view_1", "CREATE VIEW _test_view_1 WITH SCHEMABINDING AS SELECT Z0_PK FROM dbo.DummyBizo", "Z0_PK");
					connection.ExecuteNonQuery(idxViewUnchanged.Text);
					connection.ExecuteNonQuery("CREATE UNIQUE CLUSTERED INDEX NR_UC___test_view_1 ON _test_view_1 (Z0_PK)");
					var idxViewUnchangedDate = idxViewUnchanged.GetViewModifiedDate(connection);
					AssertEquals("Indexed-view-unchanged index", "NR_UC__" + idxViewUnchanged.Name, idxViewUnchanged.GetViewIndexName(connection));

					// Indexed view with changes => should be altered
					var idxViewChanged = new MockIndexedViewDbScript("_test_view_2", "CREATE VIEW _test_view_2 WITH SCHEMABINDING AS SELECT Z0_PK FROM dbo.DummyBizo", "Z0_PK");
					connection.ExecuteNonQuery(idxViewChanged.Text);
					connection.ExecuteNonQuery("CREATE UNIQUE CLUSTERED INDEX NR_UC___test_view_2 ON _test_view_2 (Z0_PK)");
					var idxViewChangedDate = idxViewChanged.GetViewModifiedDate(connection);
					AssertEquals("Indexed-view-changed index", "NR_UC__" + idxViewChanged.Name, idxViewChanged.GetViewIndexName(connection));
					// Force view change
					connection.ExecuteNonQuery("ALTER VIEW _test_view_2 AS SELECT 1 AS Col1");
					var newIndexedViewChangedDate = idxViewChanged.GetViewModifiedDate(connection);
					AssertEquals("Indexed-view-changed index was dropped?", true, String.IsNullOrEmpty(idxViewChanged.GetViewIndexName(connection)));
					AssertEquals("Indexed-view-changed modified?", true, newIndexedViewChangedDate > idxViewChangedDate);
					idxViewChangedDate = newIndexedViewChangedDate;

					// Prepare test script collection with unmodified contents
					var scriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();
					scriptCollection.Add(idxViewUnchanged);
					scriptCollection.Add(idxViewChanged);

					// Run alter view method
					testCreator.Run();

					// Indexed view with no changes => should not be altered
					AssertEquals("Indexed-view-unchanged modified date", idxViewUnchangedDate, idxViewUnchanged.GetViewModifiedDate(connection));
					AssertEquals("Indexed-view-unchanged index", "NR_UC__" + idxViewUnchanged.Name, idxViewUnchanged.GetViewIndexName(connection));

					// Indexed view with changes => should be altered
					AssertEquals("Indexed-view-changed modified?", true, idxViewChanged.GetViewModifiedDate(connection) > idxViewChangedDate);
					AssertEquals("Indexed-view-changed index", "MOCK_VIEW_INDEX__" + idxViewChanged.Name, idxViewChanged.GetViewIndexName(connection));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestIndexedViewIndexes_Add()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var view = new DbCreateIndexedViewScriptForTest("test view",
					"CREATE VIEW [test view] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(view.Text);

				var index1 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 1")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				var index2 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 2")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);

				view.Indexes.Add(index1.GetInfo());
				view.Indexes.Add(index2.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(view);

				var indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				var expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", expected, indexes.Select(ind => ind.ComparableDefinition));

				// Act
				testCreator.Run();

				// Assert
				indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("Index created", expected, indexes.Select(ind => ind.ComparableDefinition));
			}
		}

		public void TestIndexedViewIndexes_Drop()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var view = new DbCreateIndexedViewScriptForTest("test view",
					"CREATE VIEW [test view] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(view.Text);

				var index1 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 1")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				var index2 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 2")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);
				index2.GetInfo().Create(connection);

				view.Indexes.Add(index1.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(view);

				var indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				var expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", expected, indexes.Select(ind => ind.ComparableDefinition));

				// Act
				testCreator.Run();

				// Assert
				indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("Index dropped", expected, indexes.Select(ind => ind.ComparableDefinition));
			}
		}

		public void TestIndexedViewIndexes_Alter_Keys()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var view = new DbCreateIndexedViewScriptForTest("test view",
					"CREATE VIEW [test view] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(view.Text);

				var index1 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 1")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				var index2 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 2")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);
				index2.GetInfo().Create(connection);

				view.Indexes.Add(index1.GetInfo());
				view.Indexes.Add(index2
					.Include("Col")
					.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(view);

				var indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				var expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", expected, indexes.Select(ind => ind.ComparableDefinition));

				// Act
				testCreator.Run();

				// Assert
				indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) INCLUDE ([Col]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("Index re-created", expected, indexes.Select(ind => ind.ComparableDefinition));
			}
		}

		public void TestIndexedViewIndexes_NonAlterable_Options()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var view = new DbCreateIndexedViewScriptForTest("test view",
					"CREATE VIEW [test view] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(view.Text);

				var index1 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 1")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				var index2 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 2")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);
				index2.GetInfo().Create(connection);

				view.Indexes.Add(index1.GetInfo());
				view.Indexes.Add(index2
					.Option(IndexOptions.DATA_COMPRESSION, DataCompression.PAGE)
					.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(view);

				var indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				var expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", expected, indexes.Select(ind => ind.ComparableDefinition));

				// Act
				testCreator.Run();

				// Assert
				indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF, DATA_COMPRESSION = PAGE)",
				};
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Index re-created", expected, indexes.Select(ind => ind.ComparableDefinition));
					var logs = testCreator.Manager.InfoMessages.ToString();
					AssertContains("Synchronising indexes for indexed view: [dbo].[test view]", logs);
					AssertContains("(-) NONCLUSTERED INDEX [index 2]", logs);
					AssertContains("(+) NONCLUSTERED INDEX [index 2]", logs);
				});
			}
		}

		public void TestIndexedViewIndexes_Alterable_Options()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var view = new DbCreateIndexedViewScriptForTest("test view",
					"CREATE VIEW [test view] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(view.Text);

				var index1 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 1")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				var index2 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 2")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);
				index2.GetInfo().Create(connection);

				view.Indexes.Add(index1.GetInfo());
				view.Indexes.Add(index2
					.Option(IndexOptions.ALLOW_ROW_LOCKS, false)
					.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(view);

				// Act
				testCreator.Run();

				// Assert
				var indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				var expected = new[]
				{
					"UNIQUE CLUSTERED INDEX [index 1] ON [dbo].[test view] ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
					"NONCLUSTERED INDEX [index 2] ON [dbo].[test view] ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = OFF, ONLINE = ON)",
				};

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Index altered", expected, indexes.Select(ind => ind.Definition));
					var logs = testCreator.Manager.InfoMessages.ToString();
					AssertContains("Synchronising indexes for indexed view: [dbo].[test view]", logs);
					AssertNotContains("(-) NONCLUSTERED INDEX [index 2]", logs);
					AssertNotContains("(+) NONCLUSTERED INDEX [index 2]", logs);
					AssertContains("(~) ALTER INDEX [index 2] ON [dbo].[test view] SET (ALLOW_PAGE_LOCKS = OFF, ALLOW_ROW_LOCKS = OFF)", logs);
				});
			}
		}

		public void TestIndexedViewIndexes_Alter_DefaultOptions()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var view = new DbCreateIndexedViewScriptForTest("test view",
					"CREATE VIEW [test view] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(view.Text);

				var index1 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 1")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				var index2 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 2")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);
				index2.GetInfo().Create(connection);

				view.Indexes.Add(index1.GetInfo());
				view.Indexes.Add(index2
					.Option(IndexOptions.PAD_INDEX, false) // default
					.Option(IndexOptions.FILLFACTOR, 100) // default
					.Option(IndexOptions.SORT_IN_TEMPDB, false) // run-time
					.Option(IndexOptions.IGNORE_DUP_KEY, false) // default
					.Option(IndexOptions.STATISTICS_NORECOMPUTE, false) // default
					.Option(IndexOptions.STATISTICS_INCREMENTAL, false) // default
					.Option(IndexOptions.DROP_EXISTING, false) // run-time
					.Option(IndexOptions.ONLINE, true) // run-time
					.Option(IndexOptions.ALLOW_ROW_LOCKS, true) // default
					.Option(IndexOptions.ALLOW_PAGE_LOCKS, true) // default
					.Option(IndexOptions.MAXDOP, 2) // run-time
					.Option(IndexOptions.DATA_COMPRESSION, DataCompression.NONE) // default
					.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(view);

				var indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				var expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", expected, indexes.Select(ind => ind.ComparableDefinition));

				// Act
				testCreator.Run();

				// Assert
				indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				expected = new[]
				{
					"[dbo].[test view].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code])",
				};
				AssertContainsExactElementsInAnyOrder("No changes", expected, indexes.Select(ind => ind.ComparableDefinition));
			}
		}

		public void TestIndexedViewIndexes_Alter_Clustered()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var view = new DbCreateIndexedViewScriptForTest("test view",
					"CREATE VIEW [test view] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(view.Text);

				var index2 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 2")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index2.GetInfo().Create(connection);

				var index1 = IndexInfo.Builder.New(view.SchemaName, view.Name, "index 1")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				view.Indexes.Add(index2
					.Key("Z0_FK_Code")
					.GetInfo());
				view.Indexes.Add(index1.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(view);

				var indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				var expected = new[]
				{
					"[dbo].[test view].[index 2] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 1] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", expected, indexes.Select(ind => ind.ComparableDefinition));

				// Act
				testCreator.Run();

				// Assert
				indexes = IndexLoader.Load(connection, view.SchemaName, view.Name, indexName: null);
				expected = new[]
				{
					"[dbo].[test view].[index 2] UNIQUE CLUSTERED INDEX ([Z0_PK], [Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view].[index 1] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("Index re-created", expected, indexes.Select(ind => ind.ComparableDefinition));
			}
		}

		public void TestIndexedViewIndexes_Dependency()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				var viewParent = new DbCreateIndexedViewScriptForTest("test view parent",
					"CREATE VIEW [test view parent] WITH SCHEMABINDING AS SELECT Z0_PK, Z0_FK_Code, Col = 1 FROM dbo.DummyBizo");
				connection.ExecuteNonQuery(viewParent.Text);

				var index1 = IndexInfo.Builder.New(viewParent.SchemaName, viewParent.Name, "index 1")
					.Unique()
					.Clustered()
					.Key("Z0_PK")
					.Option(IndexOptions.ONLINE, false);
				index1.GetInfo().Create(connection);

				var index2 = IndexInfo.Builder.New(viewParent.SchemaName, viewParent.Name, "index 2")
					.Key("Z0_FK_Code")
					.Option(IndexOptions.ONLINE, false);

				var procChild = new DbCreateStoredProcedureScriptForTest("test proc child",
					"CREATE PROCEDURE [test proc child] AS BEGIN SELECT Z0_PK FROM dbo.[test view parent] WITH (NOEXPAND) END");
				connection.ExecuteNonQuery(procChild.Text);

				var indexes = IndexLoader.Load(connection, viewParent.SchemaName, viewParent.Name, indexName: null);
				var expected = new[]
				{
					"[dbo].[test view parent].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", expected, indexes.Select(ind => ind.ComparableDefinition));

				index1.GetInfo().Drop(connection);
				indexes = IndexLoader.Load(connection, viewParent.SchemaName, viewParent.Name, indexName: null);
				AssertContainsExactElementsInAnyOrder("PRE-CONDITION", Enumerable.Empty<string>(), indexes.Select(ind => ind.ComparableDefinition));

				viewParent.Indexes.Add(index1.GetInfo());
				viewParent.Indexes.Add(index2.GetInfo());
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(viewParent);
				testCreator.GetViewAndRoutineScriptCollection_Exposed().Add(procChild);

				// Act
				testCreator.Run();

				// Assert
				indexes = IndexLoader.Load(connection, viewParent.SchemaName, viewParent.Name, indexName: null);
				expected = new[]
				{
					"[dbo].[test view parent].[index 1] UNIQUE CLUSTERED INDEX ([Z0_PK]) WITH (ALLOW_PAGE_LOCKS = OFF)",
					"[dbo].[test view parent].[index 2] NONCLUSTERED INDEX ([Z0_FK_Code]) WITH (ALLOW_PAGE_LOCKS = OFF)",
				};
				AssertContainsExactElementsInAnyOrder("Index re-created", expected, indexes.Select(ind => ind.ComparableDefinition));
			}
		}

		public void TestIndexedViewIndexes_DependencyAll()
		{
			using (var connection = Db.NewAdminConnection())
			using (var manager = connection.BeginTransactionWithManager())
			{
				// Arrange
				foreach (var script in CoreScriptIndex.GetScripts())
				{
					if (script is DbCreateIndexedViewScript indexedView)
					{
						var clusteredIndex = indexedView.Indexes.FirstOrDefault(ind => ind.IsClustered);
						if (clusteredIndex != null)
						{
							clusteredIndex.Drop(connection);
						}
					}
				}

				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

				// Act
				// Assert
				AssertNoExceptionThrown(testCreator.Run);
			}
		}

		class MockViewDbScript : DbScript
		{
			public MockViewDbScript(string scriptName, string scriptText)
				: base(scriptName, scriptText, DbRoutineType.SqlViewTypeDesc)
			{
			}

			public string GetViewIndexName(DbConnection conn)
			{
				string sqlText = String.Format(@"
					SELECT ind.name
					FROM sys.views vw
					INNER JOIN sys.indexes ind ON ind.object_id = vw.object_id
					WHERE vw.name = '{0}' AND ind.type = 1;",
					this.Name);
				var objValue = conn.ExecuteScalar(sqlText);
				return (objValue == null) ? null : objValue.ToString();
			}

			public DateTime GetViewModifiedDate(DbConnection conn)
			{
				string sqlText = String.Format(@"
					SELECT vw.modify_date
					FROM sys.views vw
					WHERE vw.name = '{0}';",
					this.Name);
				return Convert.ToDateTime(conn.ExecuteScalar(sqlText));
			}
		}

		class MockIndexedViewDbScript : MockViewDbScript, IIndexedViewDbScript
		{
			public MockIndexedViewDbScript(string scriptName, string scriptText, string indexColumnList)
				: base(scriptName, scriptText)
			{
				this.indexColumnList = indexColumnList;
			}

			readonly string indexColumnList;

			public string IndexCreateScript
			{
				get
				{
					return string.Format("CREATE UNIQUE CLUSTERED INDEX MOCK_VIEW_INDEX__{0} ON {0} ({1})", this.Name, this.indexColumnList);
				}
			}
		}

		class DbCreateIndexedViewScriptForTest : DbCreateIndexedViewScript
		{
			public DbCreateIndexedViewScriptForTest(string name, string text, params IndexInfo[] indexes)
			{
				ViewName = name;
				ViewText = text;
				Indexes = indexes?.ToList() ?? new List<IndexInfo>();
			}

			string ViewName { get; }
			string ViewText { get; }

			public override string Name => ViewName;
			public override string Text => ViewText;
			public override List<IndexInfo> Indexes { get; }
		}

		class DbCreateStoredProcedureScriptForTest : DbCreateStoredProcedureScript
		{
			public DbCreateStoredProcedureScriptForTest(string name, string text)
			{
				ProcName = name;
				ProcText = text;
			}

			string ProcName { get; }
			string ProcText { get; }

			public override string Name => ProcName;
			public override string Text => ProcText;
		}

		#endregion

		public void TestObjectsDroppedInReverseBindingOrder()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var newScriptCollection = testCreator.GetViewAndRoutineScriptCollection_Exposed();

					// Create objects with mixed dependency levels and check they're dropped in dependency order
					connection.ExecuteNonQuery(@"CREATE VIEW dbo._TestObjectsDeletedInReverseBindingOrder_01 WITH SCHEMABINDING AS
				SELECT GS_Code, NumberOfRows = count(*) FROM dbo.GlbStaff GROUP BY GS_Code");
					connection.ExecuteNonQuery(@"CREATE VIEW dbo._TestObjectsDeletedInReverseBindingOrder_02 WITH SCHEMABINDING AS
				SELECT GS_Code, RowNumber = ROW_NUMBER() OVER(ORDER BY GS_Code) FROM dbo._TestObjectsDeletedInReverseBindingOrder_01");
					connection.ExecuteNonQuery(@"CREATE VIEW dbo._TestObjectsDeletedInReverseBindingOrder_03 WITH SCHEMABINDING AS
					SELECT GS_Code FROM dbo._TestObjectsDeletedInReverseBindingOrder_02");
					connection.ExecuteNonQuery(@"CREATE FUNCTION dbo._TestObjectsDeletedInReverseBindingOrder_04() RETURNS TABLE WITH SCHEMABINDING AS
					RETURN SELECT GS_Code FROM dbo._TestObjectsDeletedInReverseBindingOrder_01");

					AssertObjectCount("[_]TestObjectsDeletedInReverseBindingOrder%", 4, connection);
					testCreator.Run();
					AssertObjectCount("[_]TestObjectsDeletedInReverseBindingOrder%", 0, connection);

					// Play with create/modify date and check dependency order is still respected
					connection.ExecuteNonQuery(@"CREATE VIEW dbo._TestObjectsDeletedInReverseBindingOrder_11 AS
				SELECT GS_Code, NumberOfRows = count(*) FROM dbo.GlbStaff GROUP BY GS_Code");
					connection.ExecuteNonQuery(@"CREATE VIEW dbo._TestObjectsDeletedInReverseBindingOrder_12 AS
				SELECT GS_Code FROM dbo._TestObjectsDeletedInReverseBindingOrder_11");
					connection.ExecuteNonQuery(@"CREATE VIEW dbo._TestObjectsDeletedInReverseBindingOrder_13 WITH SCHEMABINDING AS
				SELECT GS_Code, NumberOfRows = count(*) FROM dbo.GlbStaff GROUP BY GS_Code");
					connection.ExecuteNonQuery(@"ALTER VIEW dbo._TestObjectsDeletedInReverseBindingOrder_12 WITH SCHEMABINDING AS
				SELECT GS_Code FROM dbo._TestObjectsDeletedInReverseBindingOrder_13");

					AssertObjectCount("[_]TestObjectsDeletedInReverseBindingOrder%", 3, connection);
					testCreator.Run();
					AssertObjectCount("[_]TestObjectsDeletedInReverseBindingOrder%", 0, connection);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestRoutinesAreInOrderOfDependency()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var sql = @"
SELECT COUNT(*)
FROM sys.objects AS o
WHERE
	o.is_ms_shipped = 0
	AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF', 'SQ')
	AND schema_id = SCHEMA_ID(N'dbo')
	AND [name] NOT LIKE N'RptDt%'
	AND [name] NOT LIKE N'TG[_]%[_]UpdateAutoVersion'
	AND [name] NOT LIKE N'TG[_]%[_]AuditDetailsAreNotMissing[_]Insert'
	AND [name] NOT LIKE N'TG[_]%[_]AuditDetailsAreNotMissing[_]Update'
	AND [name] NOT LIKE N'TG[_]%[_]SystemLastEditAuditInfoMustBeUpdated[_]Update';";
					int objCount = (int)connection.ExecuteScalar(sql);
					AssertEquals(true, objCount > 0);

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

					// Drop all objects
					testCreator.ExpectedList = new DbRoutineScriptCollection();
					testCreator.Run();
					objCount = (int)connection.ExecuteScalar(sql);
					AssertEquals(false, objCount > 0);

					// Create all objects
					testCreator.ExpectedList = null;
					testCreator.Run();
					objCount = (int)connection.ExecuteScalar(sql);
					AssertEquals(true, objCount > 0);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		#region Schemabinding

		public void TestSchemabound_Alter_Client()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();
					var dummyTableName = "DummyBizo";

					var view_1_name = "_test_vw_1";
					var view_2_name = "_test_vw_2";
					var view_3_name = "Client_test_vw_3";

					var vw_1_original = DummyView(view_1_name, dummyTableName);
					var vw_2_original = DummyView(view_2_name, view_1_name);
					var vw_3_client = DummyView(view_3_name, view_2_name);

					var vw_1_altered = DummyView(view_1_name, dummyTableName, toLower: true);

					// Actual
					connection.ExecuteNonQuery(vw_1_original.Text);
					connection.ExecuteNonQuery(vw_2_original.Text);
					connection.ExecuteNonQuery(vw_3_client.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1_altered);
					collection.Add(vw_2_original);

					AssertEquals("Precondition: Does view _test_vw_1 exist?", true, creator.ObjectExists(view_1_name));
					AssertEquals("Precondition: Does view _test_vw_2 exist?", true, creator.ObjectExists(view_2_name));
					AssertEquals("Precondition: Does view Client_test_vw_3 exist?", true, creator.ObjectExists(view_3_name));

					// drop all client objects because of dependency
					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_vw_1 definition", vw_1_altered.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("_test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("Client_test_vw_3 definition", null, GetScriptFromDb(connection, view_3_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Alter_Schemabound()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var view_1_name = "_test_vw_1";
					var view_2_name = "_test_vw_2";
					var view_3_name = "_test_vw_3";

					var vw_1_original = DummyView(view_1_name, dummyTableName, withSchemabinding: false);
					var vw_2_original = DummyView(view_2_name, view_1_name, withSchemabinding: false);
					var vw_3_original = DummyView(view_3_name, view_2_name, withSchemabinding: false);

					var vw_1_altered = DummyView(view_1_name, dummyTableName, withSchemabinding: true);
					var vw_2_altered = DummyView(view_2_name, view_1_name, withSchemabinding: true);
					var vw_3_altered = DummyView(view_3_name, view_2_name, withSchemabinding: true);

					// Actual
					connection.ExecuteNonQuery(vw_1_original.Text);
					connection.ExecuteNonQuery(vw_2_original.Text);
					connection.ExecuteNonQuery(vw_3_original.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1_altered);
					collection.Add(vw_2_altered);
					collection.Add(vw_3_altered);

					AssertEquals("Precondition: _test_vw_1 definition", vw_1_original.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("Precondition: _test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("Precondition: _test_vw_3 definition", vw_3_original.Text, GetScriptFromDb(connection, view_3_name));

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_vw_1 definition", vw_1_altered.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("_test_vw_2 definition", vw_2_altered.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("_test_vw_3 definition", vw_3_altered.Text, GetScriptFromDb(connection, view_3_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Alter_ToLower()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var view_1_name = "_test_vw_1";
					var view_2_name = "_test_vw_2";
					var view_3_name = "_test_vw_3";

					var vw_1_original = DummyView(view_1_name, dummyTableName);
					var vw_2_original = DummyView(view_2_name, view_1_name);
					var vw_3_original = DummyView(view_3_name, view_2_name);

					var vw_1_altered = DummyView(view_1_name, dummyTableName, toLower: true);

					// Actual
					connection.ExecuteNonQuery(vw_1_original.Text);
					connection.ExecuteNonQuery(vw_2_original.Text);
					connection.ExecuteNonQuery(vw_3_original.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1_altered);
					collection.Add(vw_2_original);
					collection.Add(vw_3_original);

					AssertEquals("Precondition: _test_vw_1 definition", vw_1_original.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("Precondition: _test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("Precondition: _test_vw_3 definition", vw_3_original.Text, GetScriptFromDb(connection, view_3_name));

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_vw_1 definition", vw_1_altered.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("_test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("_test_vw_3 definition", vw_3_original.Text, GetScriptFromDb(connection, view_3_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Client_ClientSpecific()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var view_1_name = "Client_test_vw_1";
					var view_2_name = "Client_test_vw_2";
					var view_3_name = "Client_test_vw_3";

					var vw_1_original = DummyView(view_1_name, dummyTableName);
					var vw_2_original = DummyView(view_2_name, view_1_name);
					var vw_3_client = DummyView(view_3_name, dummyTableName);

					var vw_1_altered = DummyView(view_1_name, dummyTableName, toLower: true);

					// Actual
					connection.ExecuteNonQuery(vw_1_original.Text);
					connection.ExecuteNonQuery(vw_2_original.Text);
					connection.ExecuteNonQuery(vw_3_client.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1_altered);
					collection.Add(vw_2_original);

					AssertEquals("Precondition: Does view Client_test_vw_1 exist?", true, creator.ObjectExists(view_1_name));
					AssertEquals("Precondition: Does view Client_test_vw_2 exist?", true, creator.ObjectExists(view_2_name));
					AssertEquals("Precondition: Does view Client_test_vw_3 exist?", true, creator.ObjectExists(view_3_name));

					// don't drop client objects if it's not required by dependency check
					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("Client_test_vw_1 definition", vw_1_altered.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("Client_test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("Client_test_vw_3 definition", vw_3_client.Text, GetScriptFromDb(connection, view_3_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Create()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var vw_1_name = "_test_vw_1";
					var vw_2_name = "_test_vw_2";
					var vw_3_name = "_test_vw_3";

					var vw_1 = DummyView(vw_1_name, dummyTableName);
					var vw_2 = DummyView(vw_2_name, vw_1_name);
					var vw_3 = DummyView(vw_3_name, vw_2_name);

					// Actual - nothing

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1);
					collection.Add(vw_2);
					collection.Add(vw_3);

					AssertEquals("Precondition: Does view _test_vw_1 exist?", false, creator.ObjectExists(vw_1_name));
					AssertEquals("Precondition: Does view _test_vw_2 exist?", false, creator.ObjectExists(vw_2_name));
					AssertEquals("Precondition: Does view _test_vw_3 exist?", false, creator.ObjectExists(vw_3_name));

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("Does view _test_vw_1 exist?", true, creator.ObjectExists(vw_1_name));
					AssertEquals("Does view _test_vw_2 exist?", true, creator.ObjectExists(vw_2_name));
					AssertEquals("Does view _test_vw_3 exist?", true, creator.ObjectExists(vw_3_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Create_Alter()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var view_1_name = "_test_vw_1";
					var view_2_name = "_test_vw_2";
					var view_3_name = "_test_vw_3";
					var view_4_name = "_test_vw_4";

					var vw_1_original = DummyView(view_1_name, dummyTableName);
					var vw_2_original = DummyView(view_2_name, dummyTableName);
					var vw_3_original = DummyView(view_3_name, view_2_name);
					var vw_4_original = DummyView(view_4_name, view_3_name, withSchemabinding: false);

					var vw_2_altered = DummyView(view_2_name, view_1_name);

					// Actual
					connection.ExecuteNonQuery(vw_2_original.Text);
					connection.ExecuteNonQuery(vw_3_original.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1_original);
					collection.Add(vw_2_altered);
					collection.Add(vw_3_original);
					collection.Add(vw_4_original);

					AssertEquals("Precondition: _test_vw_1 definition", null, GetScriptFromDb(connection, view_1_name));
					AssertEquals("Precondition: _test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("Precondition: _test_vw_3 definition", vw_3_original.Text, GetScriptFromDb(connection, view_3_name));
					AssertEquals("Precondition: _test_vw_4 definition", null, GetScriptFromDb(connection, view_4_name));

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_vw_1 definition", vw_1_original.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("_test_vw_2 definition", vw_2_altered.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("_test_vw_3 definition", vw_3_original.Text, GetScriptFromDb(connection, view_3_name));
					AssertEquals("_test_vw_4 definition", vw_4_original.Text, GetScriptFromDb(connection, view_4_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Drop()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var vw_1_name = "_test_vw_1";
					var vw_2_name = "_test_vw_2";
					var vw_3_name = "_test_vw_3";

					var vw_1 = DummyView(vw_1_name, dummyTableName);
					var vw_2 = DummyView(vw_2_name, vw_1_name);
					var vw_3 = DummyView(vw_3_name, vw_2_name);

					// Actual
					connection.ExecuteNonQuery(vw_1.Text);
					connection.ExecuteNonQuery(vw_2.Text);
					connection.ExecuteNonQuery(vw_3.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();

					AssertEquals("Precondition: Does view _test_vw_1 exist?", true, creator.ObjectExists(vw_1_name));
					AssertEquals("Precondition: Does view _test_vw_2 exist?", true, creator.ObjectExists(vw_2_name));
					AssertEquals("Precondition: Does view _test_vw_3 exist?", true, creator.ObjectExists(vw_3_name));

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("Does view _test_vw_1 exist?", false, creator.ObjectExists(vw_1_name));
					AssertEquals("Does view _test_vw_2 exist?", false, creator.ObjectExists(vw_2_name));
					AssertEquals("Does view _test_vw_3 exist?", false, creator.ObjectExists(vw_3_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Drop_Alter_Swap()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var view_11_name = "_test_vw_11";
					var view_12_name = "_test_vw_12";
					var view_13_name = "_test_vw_13";

					var view_21_name = "_test_vw_21";
					var view_22_name = "_test_vw_22";
					var view_23_name = "_test_vw_23";

					var vw_11_original = DummyView(view_11_name, dummyTableName);
					var vw_12_original = DummyView(view_12_name, view_11_name);
					var vw_13_original = DummyView(view_13_name, view_12_name);

					var vw_21_original = DummyView(view_21_name, dummyTableName);
					var vw_22_original = DummyView(view_22_name, view_21_name);
					var vw_23_original = DummyView(view_23_name, view_22_name);

					var vw_12_altered = DummyView(view_12_name, view_22_name);
					var vw_22_altered = DummyView(view_22_name, view_11_name);

					// Actual
					connection.ExecuteNonQuery(vw_11_original.Text);
					connection.ExecuteNonQuery(vw_12_original.Text);
					connection.ExecuteNonQuery(vw_13_original.Text);

					connection.ExecuteNonQuery(vw_21_original.Text);
					connection.ExecuteNonQuery(vw_22_original.Text);
					connection.ExecuteNonQuery(vw_23_original.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_11_original);
					collection.Add(vw_22_altered);
					collection.Add(vw_23_original);
					collection.Add(vw_12_altered);
					collection.Add(vw_13_original);

					AssertEquals("Precondition: _test_vw_11 definition", vw_11_original.Text, GetScriptFromDb(connection, view_11_name));
					AssertEquals("Precondition: _test_vw_12 definition", vw_12_original.Text, GetScriptFromDb(connection, view_12_name));
					AssertEquals("Precondition: _test_vw_13 definition", vw_13_original.Text, GetScriptFromDb(connection, view_13_name));
					AssertEquals("Precondition: _test_vw_21 definition", vw_21_original.Text, GetScriptFromDb(connection, view_21_name));
					AssertEquals("Precondition: _test_vw_22 definition", vw_22_original.Text, GetScriptFromDb(connection, view_22_name));
					AssertEquals("Precondition: _test_vw_23 definition", vw_23_original.Text, GetScriptFromDb(connection, view_23_name));

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_vw_11 definition", vw_11_original.Text, GetScriptFromDb(connection, view_11_name));
					AssertEquals("_test_vw_12 definition", vw_12_altered.Text, GetScriptFromDb(connection, view_12_name));
					AssertEquals("_test_vw_13 definition", vw_13_original.Text, GetScriptFromDb(connection, view_13_name));
					AssertEquals("_test_vw_21 definition", null, GetScriptFromDb(connection, view_21_name));
					AssertEquals("_test_vw_22 definition", vw_22_altered.Text, GetScriptFromDb(connection, view_22_name));
					AssertEquals("_test_vw_23 definition", vw_23_original.Text, GetScriptFromDb(connection, view_23_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSchemabound_Refresh()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var view_1_name = "_test_vw_1";
					var view_2_name = "_test_vw_2";
					var view_3_name = "_test_vw_3";
					var view_4_name = "_test_vw_4";

					var vw_1_original = DummyView(view_1_name, dummyTableName);
					var vw_2_original = DummyView(view_2_name, view_1_name);
					var vw_3_original = DummyView(view_3_name, view_2_name);

					var view_4_text = String.Format("CREATE VIEW [{0}].[{1}] AS SELECT o.* FROM [{0}].[{2}] AS o JOIN [{0}].[{3}] AS v ON v.Z0_PK = o.Z0_PK;",
						Db.SqlDbOwnerSchema, // 0
						view_4_name,              // 1
						dummyTableName,          // 2
						view_3_name             // 3
						);
					var vw_4_original = new DbScript(view_4_name, view_4_text, DbRoutineType.SqlViewTypeDesc);

					var vw_1_altered = DummyView(view_1_name, dummyTableName, toLower: true);

					// Actual
					connection.ExecuteNonQuery(vw_1_original.Text);
					connection.ExecuteNonQuery(vw_2_original.Text);
					connection.ExecuteNonQuery(vw_3_original.Text);
					connection.ExecuteNonQuery(vw_4_original.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1_altered);
					collection.Add(vw_2_original);
					collection.Add(vw_3_original);
					collection.Add(vw_4_original);

					AssertEquals("Precondition: _test_vw_1 definition", vw_1_original.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("Precondition: _test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("Precondition: _test_vw_3 definition", vw_3_original.Text, GetScriptFromDb(connection, view_3_name));
					AssertEquals("Precondition: _test_vw_4 definition", vw_4_original.Text, GetScriptFromDb(connection, view_4_name));
					var expectedFieldsCount = (int)connection.ExecuteScalar("SELECT cnt = COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[_test_vw_4]');");

					connection.ExecuteNonQuery("ALTER TABLE [dbo].[DummyBizo] ADD _test_column int NULL;");
					expectedFieldsCount++;

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_vw_1 definition", vw_1_altered.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("_test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
					AssertEquals("_test_vw_3 definition", vw_3_original.Text, GetScriptFromDb(connection, view_3_name));
					AssertEquals("_test_vw_4 definition", vw_4_original.Text, GetScriptFromDb(connection, view_4_name));
					var actualFieldsCount = (int)connection.ExecuteScalar("SELECT cnt = COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[_test_vw_4]');");
					AssertEquals("Upgrade should refresh metadata for non-schemabound objects", expectedFieldsCount, actualFieldsCount);

					var messages = creator.Manager.InfoMessages.ToString().SplitByLine();

					AssertCollectionContains(messages,
						message => message.Equals($"    (~) Refreshing module [dbo].[{view_4_name}] ({DbRoutineType.SqlViewTypeDesc})")
					);
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestRefreshHasLogs()
		{
			using var connection = Db.NewAdminConnection();

			try
			{
				connection.BeginTransaction();

				var scriptName = "_test_refresh_stored_proc_script_";

				var script = new DbScript(
					scriptName,
					$"CREATE PROCEDURE {scriptName} AS SELECT 1;",
					DbRoutineType.SqlProcedureTypeDesc
				);

				// Add actual script to DB
				connection.ExecuteNonQuery(script.Text);

				// Add expected script to creator
				var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
				creator.GetViewAndRoutineScriptCollection_Exposed().Add(script);

				creator.Run();
				
				var messages = creator.Manager.InfoMessages.ToString().SplitByLine();

				AssertCollectionContains(messages,
					message => message.Equals($"    (~) Refreshing module [dbo].[{scriptName}] ({DbRoutineType.SqlProcedureTypeDesc})")
				);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestSchemabound_Refresh_NonSchemabound()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var table_name = "_test_table";
					var view_1_name = "_test_vw_1";
					var view_2_name = "_test_vw_2";

					var sqlDropTable = String.Format("if (OBJECT_ID('dbo.{0}', 'U') is NOT NULL) DROP TABLE dbo.{0};", table_name);
					var sqlCreateTable_original = String.Format("CREATE TABLE dbo.{0} (Col1 int, Col2 int);", table_name);
					var sqlCreateTable_altered = String.Format("CREATE TABLE dbo.{0} (Col1 int, Col3 uniqueidentifier, Col2 int);", table_name);

					var view_1_text = String.Format("CREATE VIEW {0} AS SELECT * FROM dbo.{1};", view_1_name, table_name);
					var view_2_text = String.Format("CREATE VIEW {0} AS SELECT * FROM dbo.{1} WHERE Col2 = 5;", view_2_name, view_1_name);

					var vw_1_original = new DbScript(view_1_name, view_1_text, DbRoutineType.SqlViewTypeDesc);
					var vw_2_original = new DbScript(view_2_name, view_2_text, DbRoutineType.SqlViewTypeDesc);

					// Actual
					connection.ExecuteNonQuery(sqlDropTable);
					connection.ExecuteNonQuery(sqlCreateTable_original);

					connection.ExecuteNonQuery(vw_1_original.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_1_original);
					collection.Add(vw_2_original);

					AssertEquals("Precondition: _test_vw_1 definition", vw_1_original.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("Precondition: _test_vw_2 definition", null, GetScriptFromDb(connection, view_2_name));

					connection.ExecuteNonQuery(sqlDropTable);
					connection.ExecuteNonQuery(sqlCreateTable_altered);

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_vw_1 definition", vw_1_original.Text, GetScriptFromDb(connection, view_1_name));
					AssertEquals("_test_vw_2 definition", vw_2_original.Text, GetScriptFromDb(connection, view_2_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		#endregion // Schemabinding

		#region Database Triggers

		public void TestDatabaseTrigger_Create()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var trigger_name = "_database_trigger_test_1";
					var trigger_text = FormattableString.Invariant($"CREATE TRIGGER {trigger_name} ON DATABASE FOR ADD_ROLE_MEMBER AS ROLLBACK;");

					var trg_original = new DbScript(trigger_name, trigger_text, DbRoutineType.SqlTriggerTypeDesc);

					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(trg_original);

					AssertEquals("Precondition: trg_original definition is null", null, GetScriptFromDb(connection, trigger_name));

					// Act
					AssertNoExceptionThrown(() => creator.Run());

					// Assert
					AssertEquals("trg_original definition", trg_original.Text, GetScriptFromDb(connection, trigger_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestDatabaseTrigger_Create_Alter()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var trigger_name = "_database_trigger_test_1";
					var trigger_text_original = FormattableString.Invariant($"CREATE TRIGGER {trigger_name} ON DATABASE FOR ADD_ROLE_MEMBER AS ROLLBACK;");
					var trg_original = new DbScript(trigger_name, trigger_text_original, DbRoutineType.SqlTriggerTypeDesc);

					connection.ExecuteNonQuery(trg_original.Text);

					var trigger_text_altered = FormattableString.Invariant($"CREATE TRIGGER {trigger_name} ON DATABASE FOR DROP_ROLE_MEMBER AS SELECT NULL;");
					var trg_altered = new DbScript(trigger_name, trigger_text_altered, DbRoutineType.SqlTriggerTypeDesc);

					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(trg_altered);

					AssertEquals("Precondition: trg_original definition", trigger_text_original, GetScriptFromDb(connection, trigger_name));

					// Act
					AssertNoExceptionThrown(() => creator.Run());

					// Assert
					AssertEquals("trg_altered definition not instated", trigger_text_altered, GetScriptFromDb(connection, trigger_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestDatabaseTrigger_Drop()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var trigger_name = "_database_trigger_test_1";
					var trigger_text = FormattableString.Invariant($"CREATE TRIGGER {trigger_name} ON DATABASE FOR ADD_ROLE_MEMBER AS ROLLBACK;");

					var trg_original = new DbScript(trigger_name, trigger_text, DbRoutineType.SqlTriggerTypeDesc);
					connection.ExecuteNonQuery(trg_original.Text);

					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					AssertEquals("Precondition: Does trigger _database_trigger_test_1 exist?", true, creator.ObjectExists(trigger_name));

					// Act
					AssertNoExceptionThrown(() => creator.Run());

					// Assert
					AssertEquals("Does trigger _database_trigger_test_1 exist?", false, creator.ObjectExists(trigger_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		#endregion // Database Triggers

		public void TestTriggerRecreatedAfterViewModified()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var dummyTableName = "DummyBizo";

					var vw_name = "_test_view";
					var tr_1_name = "_test_trigger_1";
					var tr_2_name = "_test_trigger_2";
					var tr_3_name = "_test_trigger_3";

					var vw_original = DummyView(vw_name, dummyTableName, withSchemabinding: true, toLower: false);
					var tr_1_original = DummyTrigger(tr_1_name, "INSERT", vw_name, toLower: false);
					var tr_2_original = DummyTrigger(tr_2_name, "UPDATE", vw_name, toLower: false);
					var tr_3_original = DummyTrigger(tr_3_name, "DELETE", vw_name, toLower: false);

					var vw_altered = DummyView(vw_name, dummyTableName, withSchemabinding: true, toLower: true);

					// Actual
					connection.ExecuteNonQuery(vw_original.Text);
					connection.ExecuteNonQuery(tr_1_original.Text);
					connection.ExecuteNonQuery(tr_2_original.Text);
					connection.ExecuteNonQuery(tr_3_original.Text);

					// Expected
					var creator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);
					var collection = creator.GetViewAndRoutineScriptCollection_Exposed();
					collection.Add(vw_altered);
					collection.Add(tr_1_original);
					collection.Add(tr_2_original);
					collection.Add(tr_3_original);

					AssertEquals("Precondition: _test_view definition", vw_original.Text, GetScriptFromDb(connection, vw_name));
					AssertEquals("Precondition: _test_trigger_1 definition", tr_1_original.Text, GetScriptFromDb(connection, tr_1_name));
					AssertEquals("Precondition: _test_trigger_2 definition", tr_2_original.Text, GetScriptFromDb(connection, tr_2_name));
					AssertEquals("Precondition: _test_trigger_3 definition", tr_3_original.Text, GetScriptFromDb(connection, tr_3_name));

					AssertNoExceptionThrown(() => creator.Run());

					AssertEquals("_test_view definition", vw_altered.Text, GetScriptFromDb(connection, vw_name));
					AssertEquals("_test_trigger_1 definition", tr_1_original.Text, GetScriptFromDb(connection, tr_1_name));
					AssertEquals("_test_trigger_2 definition", tr_2_original.Text, GetScriptFromDb(connection, tr_2_name));
					AssertEquals("_test_trigger_3 definition", tr_3_original.Text, GetScriptFromDb(connection, tr_3_name));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestObjectsInReservedCdcSchemaAreNotDropped()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					string sqlText = "EXEC sys.sp_cdc_enable_db";
					connection.ExecuteNonQuery(sqlText);

					string dummyFunctionName = "fn_dummy_668DA7960D3E41A5BB96B1C10044D66A";

					sqlText = String.Format("CREATE FUNCTION cdc.{0}() RETURNS TABLE RETURN SELECT 0 as 'c1'", dummyFunctionName);
					connection.ExecuteNonQuery(sqlText);

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

					AssertEquals("[PRE-CONDITION] Dummy CDC function exists?", true, testCreator.ObjectExists(dummyFunctionName, "cdc"));
					testCreator.Run();
					AssertEquals("Dummy CDC function still exists?", true, testCreator.ObjectExists(dummyFunctionName, "cdc"));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSkipObjectsAreReallySkiped()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();
					var sql = FormattableString.Invariant($"CREATE TRIGGER [{DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager}] ON StorageDocs FOR INSERT AS RETURN");
					connection.ExecuteNonQuery(sql);

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

					Assert(DataUtils.ObjectExists(connection, DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager));
					testCreator.Run();
					Assert(DataUtils.ObjectExists(connection, DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSkipUpdateAutoVersionTriggers()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();
					var triggerName = "[dbo].[TG_IncidentRequest_UpdateAutoVersion]";
					if (!DataUtils.ObjectExists(connection, triggerName))
					{
						var sql = FormattableString.Invariant($"CREATE TRIGGER {triggerName} ON [dbo].[IncidentRequest] AFTER UPDATE AS RETURN");
						connection.ExecuteNonQuery(sql);
					}

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

					Assert(DataUtils.ObjectExists(connection, triggerName));
					testCreator.Run();
					Assert(DataUtils.ObjectExists(connection, triggerName));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSkipUpdateAuditTriggers()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					connection.BeginTransaction();

					var insertTriggerName = "[dbo].[TG_WhsDocket_AuditDetailsAreNotMissing_Insert]";
					var updateTriggerName = "[dbo].[TG_WhsDocket_AuditDetailsAreNotMissing_Update]";
					if (!DataUtils.ObjectExists(connection, insertTriggerName))
					{
						var sql = FormattableString.Invariant($"CREATE TRIGGER {insertTriggerName} ON [dbo].[WhsDocket] AFTER INSERT AS RETURN");
						connection.Command(sql).ExecuteNonQuery();
					}

					if (!DataUtils.ObjectExists(connection, updateTriggerName))
					{
						var sql = FormattableString.Invariant($"CREATE TRIGGER {updateTriggerName} ON [dbo].[WhsDocket] AFTER UPDATE AS RETURN");
						connection.Command(sql).ExecuteNonQuery();
					}

					var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

					Assert(DataUtils.ObjectExists(connection, insertTriggerName));
					Assert(DataUtils.ObjectExists(connection, updateTriggerName));
					testCreator.Run();
					Assert(DataUtils.ObjectExists(connection, insertTriggerName));
					Assert(DataUtils.ObjectExists(connection, updateTriggerName));
				}
				finally
				{
					connection.RollbackTransaction();
				}
			}
		}

		public void TestSkipUpdateLastEditAuditTriggers()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection())
			using (new DisposableAction(
				() => connection.BeginTransaction(),
				() => connection.RollbackTransaction()))
			{
				var tableName = nameof(TestSkipUpdateLastEditAuditTriggers);
				var triggerName = $"[dbo].[TG_{tableName}_SystemLastEditAuditInfoMustBeUpdated_Update]";
				connection.ExecuteNonQuery($@"
CREATE TABLE {tableName.QuoteName()} (
   [TTT_PK] UNIQUEIDENTIFIER NOT NULL,
   [TTT_SystemCreateTimeUtc] DATETIME NULL,
   [TTT_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [TTT_SystemLastEditTimeUtc] DATETIME NULL,
   [TTT_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
");
				connection.ExecuteNonQuery($"CREATE TRIGGER {triggerName} ON [dbo].{tableName.QuoteName()} AFTER UPDATE AS RETURN");

				var testCreator = new ViewAndRoutineCreatorForTesting(connection.CurrentDatabase, connection);

				// Act
				testCreator.Run();

				// Assert
				AssertEquals(true, DataUtils.ObjectExists(connection, triggerName));
			}
		}

		#region Implementation

		void AssertObjectCount(string objectNamePatern, int expectedCount, DbConnection connection)
		{
			string sqlText = String.Format("SELECT count(*) FROM sys.objects WHERE name like '{0}'", objectNamePatern);
			AssertEquals("Count of objects like " + objectNamePatern, expectedCount, Convert.ToInt32(connection.ExecuteScalar(sqlText)));
		}

		static DbScript DummyView(string name, string referencedName, bool withSchemabinding = true, bool toLower = false)
		{
			return DummyView(Db.SqlDbOwnerSchema, name, referencedName, withSchemabinding, toLower);
		}

		static DbScript DummyView(string schemaName, string name, string referencedName, bool withSchemabinding = true, bool toLower = false)
		{
			var viewText = String.Format("CREATE VIEW [{0}].[{1}] {3}AS SELECT Z0_PK FROM [dbo].[{2}];", schemaName, name, referencedName, (withSchemabinding) ? "WITH SCHEMABINDING " : "");
			if (toLower)
			{
				viewText = viewText.ToLower();
			}

			return new DbScript(schemaName, name, viewText, DbRoutineType.SqlViewTypeDesc);
		}

		static DbScript DummyTrigger(string name, string triggerType, string referencedName, bool toLower = false)
		{
			return DummyTrigger(Db.SqlDbOwnerSchema, name, triggerType, referencedName, toLower);
		}

		static DbScript DummyTrigger(string schemaName, string name, string triggerType, string referencedName, bool toLower = false)
		{
			var objectText = String.Format("CREATE TRIGGER [{0}].[{1}] ON [{0}].[{2}] INSTEAD OF {3} AS BEGIN SET NOCOUNT ON; END", schemaName, name, referencedName, triggerType);
			if (toLower)
			{
				objectText = objectText.ToLower();
			}

			return new DbScript(schemaName, name, objectText, DbRoutineType.SqlTriggerTypeDesc);
		}

		static string GetScriptFromDb(DbConnection connection, string objectName)
		{
			var sqlText = string.Format(@"SELECT TOP 1 definition FROM sys.sql_modules WHERE object_id IN
(
	SELECT OBJECT_ID('[dbo].[{0}]')
	UNION
	SELECT object_id FROM sys.triggers WHERE name = '{0}'
);", objectName);
			var definition = connection.ExecuteScalar(sqlText);
			return (definition == null) ? null : definition.ToString();
		}

		#endregion // Implementation
	}
}
