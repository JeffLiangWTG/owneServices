using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static CargoWise.Data.AdoTestUtils;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DbRecoveryModelManagerTest : TransactionedTestCase
	{
		public void TestGetActual()
		{
			void Test(DbRecoveryModel recoveryModel)
			{
				// Arrange
				using (var connection = Db.NewAdminConnection())
				using (SetupEmptyDatabase(connection, TestMainDbName, recoveryModel))
				{
					// Act
					var readFromDatabase = new DbRecoveryModelManager().GetActual(connection, TestMainDbName);

					// Assert
					AssertEquals(recoveryModel, readFromDatabase);
				}
			}

			CombineAssertions(() =>
			{
				DbRecoveryModel.All.ForEach(Test);
			});
		}

		public void TestGetActualThrowsKeyNotFoundException()
		{
			var ex = AssertExceptionThrown<KeyNotFoundException>(() => new DbRecoveryModelManager().GetActual(Db.Connection, "WrongDbName"));
			AssertContains(nameof(ex.Message), "Database does not exist", ex.Message);
		}

		public void TestGetDesired()
		{
			using (var connection = Db.NewAdminConnection())
			using (SetupTestMainDatabase(connection))
			{
				CombineAssertions(() =>
				{
					foreach (var testCase in GetTestCases())
					{
						// Arrange
						using (SetupEnvironment(testCase))
						{
							// Act
							var desired = new DbRecoveryModelManager().GetDesired(connection, TestMainDbName, testCase.DbName);

							// Assert
							AssertEquals(testCase.GetDescription(), testCase.ExpectedRecoveryModel, desired);
						}
					}
				});
			}
		}

		// This test is modifying the audit and EDW databases when it shouldn't be.
		[UseSnapshotProtection(new[] { DatabaseType.BI })]
		public void TestAdjustDatabase()
		{
			adminConnectionsOfPossibleTypes.ForEach(adminConnection =>
			{
				CombineAssertions(() =>
				{
					foreach (var testCase in GetAdjustDatabaseTestCases())
					{
						// Arrange
						using (var connection = Db.NewAdminConnection())
						using (SetupTestMainDatabase(connection))
						using (testCase.DbName != TestMainDbName ? SetupEmptyDatabase(connection, testCase.DbName) : null)
						using (SetupEnvironment(testCase))
						{
							// Act
							new DbRecoveryModelManager().AdjustDatabase(adminConnection, TestMainDbName, testCase.DbName);

							// Assert
							AssertEquals(testCase.GetDescription(), testCase.ExpectedRecoveryModel ?? DbRecoveryModel.BulkLogged, GetActualForTest(connection, testCase.DbName));
						}
					}
				});
			});
		}

		public void TestAdjustDatabaseWhileCreating()
		{
			using var connection = Db.NewAdminConnection();
			using (SetupTestMainDatabase(connection))
			using (SetupEnvironment(EnvironmentType.WTGCloud, alwaysOn: true, production: true))
			{
				DoTestDatabaseWhileCreating(connection, $"{TestMainDbName}_UserRepository");
			}
		}

		public void TestAdjustDatabaseWhileCreatingMainDatabase()
		{
			using var connection = Db.NewAdminConnection();

			CombineAssertions(() =>
			{
				using (SetupEnvironment(EnvironmentType.SelfHosted, alwaysOn: false, production: true))
				{
					DoTestDatabaseWhileCreating(connection, TestMainDbName);
				}

				using (SetupEnvironment(EnvironmentType.DAT, alwaysOn: false, production: true))
				{
					DoTestDatabaseWhileCreating(connection, TestMainDbName);
				}
			});
		}

		void DoTestDatabaseWhileCreating(AdminConnection connection, string databaseName)
		{
			// When AdminConnection is creating a database, it first creates the db with a different name, finally when every thing is configured
			// and db is ready, it will rename it to the desired name. So, setting Recovery model should be able to be applied on a database
			// that still has a different name.

			// Arrange
			const string tempDbNameWhileCreating = "temp_random_name";
			using (CreateDbDropExistingDisposable(connection, tempDbNameWhileCreating, DbRecoveryModel.BulkLogged, Db.DatabaseName))
			{
				// Act
				new DbRecoveryModelManager().AdjustDatabase(connection, TestMainDbName, databaseName, tempDbNameWhileCreating);

				// Assert
				AssertEquals(DbRecoveryModel.Full, GetActualForTest(connection, tempDbNameWhileCreating));
			}
		}

		public void TestAdjustDatabaseOnDeveloperMachines()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection())
			using (SetupTestMainDatabase(connection))
			using (new DisposableAction(() => { Globals.IsTest_ForTest.ResetValue(); }))
			{
				Globals.IsTest_ForTest.Value = false;

				// Act
				new DbRecoveryModelManager().AdjustDatabase(connection, TestMainDbName, TestMainDbName);

				// Assert
				AssertEquals("In Debug Mode recovery model should not be adjusted", DbRecoveryModel.BulkLogged, GetActualForTest(connection, TestMainDbName));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })] // Because of Db.Database manipulation, Test infrastructure gets confused. Otherwise, no change is applied on the MainDB
		public void TestLoadAndCacheRequiredDbValues()
		{
			using (var connection = Db.NewAdminConnection())
			using (SetupTestMainDatabase(connection))
			using (((ICurrentDbControl)connection).UseDatabase(TestMainDbName))
			{
				DbRegistry.DatabaseRecoveryModel.SaveValue(DbRecoveryModel.Full.Name, connection);
				DbRegistry.SingleRefDatabaseName.SaveValue("SingleRefForTest", connection);

				var dbRecoveryModelManager = new DbRecoveryModelManager();
				using (dbRecoveryModelManager.LoadAndCacheRequiredDbValues(connection))
				{
					DbRegistry.DatabaseRecoveryModel.SaveValue(DbRecoveryModel.Simple.Name, connection);
					DbRegistry.SingleRefDatabaseName.SaveValue("NewSingleRefForTest", connection);

					AssertEquals(DbRecoveryModel.Full, dbRecoveryModelManager.GetRegistryValue(connection));
					AssertEquals("SingleRefForTest", dbRecoveryModelManager.GetSingleRefDbName(connection));
				}

				AssertEquals(DbRecoveryModel.Simple, dbRecoveryModelManager.GetRegistryValue(connection));
				AssertEquals("NewSingleRefForTest", dbRecoveryModelManager.GetSingleRefDbName(connection));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestAdjustAllDatabases()
		{
			adminConnectionsOfPossibleTypes.ForEach(Test);

			void Test(DbConnection adminConnection)
			{
				// Arrange
				const string auditDbName = $"{TestMainDbName}_Audit";
				const string edwDbName = $"{TestMainDbName}_EDW";
				const string sdDbName = $"{TestMainDbName}_SD001";
				const string userRepositoryDbName = $"{TestMainDbName}_UserRepository";
				const string exclusiveRefDbName = $"{TestMainDbName}_RefDb_Aaa_BB";
				const string sharedRefDbName = "CW-RefDb-Ent-US-000023";
				const string sharedAgRefDbName = "CW-AG-RefDb-OR1WP4-CP1AS1-Ent-US-1234567";
				const string singleSharedRefDbName = "CW-RefDatabase-ForTest";

				var databases = new[]
				{
					(dbName: TestMainDbName, expected: DbRecoveryModel.Full),
					(dbName: sdDbName, expected: DbRecoveryModel.Full),
					(dbName: userRepositoryDbName, expected: DbRecoveryModel.Full),
					(dbName: auditDbName, expected: DbRecoveryModel.Full),
					(dbName: edwDbName, expected: DbRecoveryModel.Full),
					(dbName: exclusiveRefDbName, expected: DbRecoveryModel.Full),
					(dbName: sharedRefDbName, expected: DbRecoveryModel.Full),
					(dbName: sharedAgRefDbName, expected: DbRecoveryModel.Full),
					(dbName: singleSharedRefDbName, expected: DbRecoveryModel.Simple),
				};

				using var connection = Db.NewAdminConnection();
				using (SetupTestMainDatabase(connection))
				using (SetupEnvironment(EnvironmentType.SelfHosted, alwaysOn: false, production: true))
				using (SetupBiDatabase(connection, auditDbName))
				using (SetupBiDatabase(connection, edwDbName))
				using (SetupRefDatabase(connection, exclusiveRefDbName))
				using (SetupRefDatabase(connection, sharedRefDbName))
				using (SetupRefDatabase(connection, sharedAgRefDbName))
				using (SetupEmptyDatabase(connection, sdDbName))
				using (SetupEmptyDatabase(connection, userRepositoryDbName))
				using (SetupEmptyDatabase(connection, singleSharedRefDbName))
				{
					var dbRecoveryModelManager = new DbRecoveryModelManager();
					var expectedMessages = databases.Select(d => $"Recovery model of database {d.dbName} was changed from {DbRecoveryModel.BulkLogged} to {d.expected}.");

					// Act
					var appliedChanges = dbRecoveryModelManager.AdjustAllDatabases(adminConnection, TestMainDbName);

					// Assert
					CombineAssertions(() =>
					{
						foreach (var db in databases)
						{
							AssertEquals(db.dbName, db.expected, GetActualForTest(connection, db.dbName));
						}

						AssertContainsExactElementsInAnyOrder(expectedMessages, appliedChanges);
					});
				}
			}
		}

		public void TestGetDesiredForNonExistingMainDb()
		{
			// Arrange
			using var connection = Db.NewAdminConnection();
			using (SetupEnvironment(EnvironmentType.WTGCloud, alwaysOn: false, production: false))
			{
				// Act
				var desired = new DbRecoveryModelManager().GetDesired(connection, TestMainDbName, TestMainDbName);

				// Assert
				AssertEquals(DbRecoveryModel.Full, desired);
			}
		}

		public void TestAdjustDatabase_WithRetry()
		{
			var testCase = new TestCase(TestMainDbName, EnvironmentType.WTGCloud, shouldBe: DbRecoveryModel.Full);

			using (var connection = Db.NewAdminConnection())
			using (SetupTestMainDatabase(connection))
			using (SetupEnvironment(testCase))
			using (connection.PrepareRetryContextForTest(
				condition: (sqlText, executionCount) => sqlText.Contains(" SET RECOVERY "),
				action: (string sqlText, int executionCount) =>
				{
					if (executionCount < 2)
					{
						throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
					}
				}))
			{
				new DbRecoveryModelManager().AdjustDatabase(connection, TestMainDbName, testCase.DbName);

				AssertEquals(2, connection.RetryContext_ForTest.ExecutionCount);
			}
		}

		static DbRecoveryModel GetActualForTest(DbConnection connection, string databaseName)
		{
			var recoveryModel = connection.ExecuteScalar(
				"SELECT recovery_model FROM sys.databases WHERE name = @DbName"
				, cmd =>
				{
					cmd.AddParameter("@DbName", System.Data.SqlDbType.NVarChar, 128, databaseName);
				});

			return DbRecoveryModel.Get(Convert.ToInt32(recoveryModel));
		}

		static IEnumerable<TestCase> GetTestCases()
		{
			foreach (var db in
				new[]
				{
					TestMainDbName, // Main DB
					$"{TestMainDbName}_SD001",
					$"{TestMainDbName}_UserRepository",
					$"{TestMainDbName}_Audit",
					$"{TestMainDbName}_EDW",
					$"{TestMainDbName}_RefDb_Aaa_BB", // Exclusive Ref DB
					"CW-RefDb-Ent-US-000023", // Shared Ref DB
				})
			{
				foreach (var testCase in MainDbSetTestCases(db))
				{
					yield return testCase;
				}
			}

			IEnumerable<TestCase> MainDbSetTestCases(string dbName)
			{
				yield return new TestCase(dbName, EnvironmentType.WTGCloud, shouldBe: DbRecoveryModel.Full);
				yield return new TestCase(dbName, EnvironmentType.WTGCloud, alwaysOn: true, shouldBe: DbRecoveryModel.Full);
				yield return new TestCase(dbName, EnvironmentType.WTGCloud, production: false, shouldBe: DbRecoveryModel.Full);
				yield return new TestCase(dbName, EnvironmentType.WTGCloud, alwaysOn: true, production: false, shouldBe: DbRecoveryModel.Full);

				yield return new TestCase(dbName, EnvironmentType.DAT, shouldBe: registryValue);
				yield return new TestCase(dbName, EnvironmentType.DAT, alwaysOn: true, shouldBe: DbRecoveryModel.Full);
				yield return new TestCase(dbName, EnvironmentType.DAT, production: false, shouldBe: registryValue);
				yield return new TestCase(dbName, EnvironmentType.DAT, alwaysOn: true, production: false, shouldBe: DbRecoveryModel.Full);

				yield return new TestCase(dbName, EnvironmentType.SAND, shouldBe: registryValue);
				yield return new TestCase(dbName, EnvironmentType.SAND, alwaysOn: true, shouldBe: DbRecoveryModel.Full);
				yield return new TestCase(dbName, EnvironmentType.SAND, production: false, shouldBe: registryValue);
				yield return new TestCase(dbName, EnvironmentType.SAND, alwaysOn: true, production: false, shouldBe: DbRecoveryModel.Full);

				yield return new TestCase(dbName, EnvironmentType.DeveloperPC, shouldBe: null /* No Adjustment */);
				yield return new TestCase(dbName, EnvironmentType.DeveloperPC, alwaysOn: true, shouldBe: null /* No Adjustment */);
				yield return new TestCase(dbName, EnvironmentType.DeveloperPC, production: false, shouldBe: null /* No Adjustment */);
				yield return new TestCase(dbName, EnvironmentType.DeveloperPC, alwaysOn: true, production: false, shouldBe: null /* No Adjustment */);

				yield return new TestCase(dbName, EnvironmentType.SelfHosted, shouldBe: DbRecoveryModel.Full);
				yield return new TestCase(dbName, EnvironmentType.SelfHosted, alwaysOn: true, shouldBe: DbRecoveryModel.Full);
				yield return new TestCase(dbName, EnvironmentType.SelfHosted, production: false, shouldBe: registryValue);
				yield return new TestCase(dbName, EnvironmentType.SelfHosted, alwaysOn: true, production: false, shouldBe: DbRecoveryModel.Full);
			}

			// Shared Availability Group Ref DB
			yield return new TestCase("CW-AG-RefDb-OR1WP4-CP1AS1-Ent-US-1234567", EnvironmentType.WTGCloud, alwaysOn: true, shouldBe: DbRecoveryModel.Full);

			// Single Shared Ref DB
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.WTGCloud, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.DAT, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.SAND, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.DeveloperPC, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.SelfHosted, shouldBe: DbRecoveryModel.Simple);

			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.WTGCloud, production: false, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.DAT, production: false, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.SAND, production: false, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.DeveloperPC, production: false, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("CW-RefDatabase-ForTest", EnvironmentType.SelfHosted, production: false, shouldBe: DbRecoveryModel.Simple);

			// Any other databases should be simple
			yield return new TestCase($"DBUPG_NewTemplateDB_{TestMainDbName}", EnvironmentType.WTGCloud, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase($"DBUPG_NewTemplateDB_{TestMainDbName}_DbType", EnvironmentType.DAT, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("Build-CW-RefDatabase-ForTest", EnvironmentType.SAND, shouldBe: DbRecoveryModel.Simple);
			yield return new TestCase("Random-DB-Name", EnvironmentType.SelfHosted, shouldBe: DbRecoveryModel.Simple);
		}

		/// <summary>
		/// Returning selected test cases with a fine coverage to speed-up TestAdjustDatabase
		/// </summary>
		static IEnumerable<TestCase> GetAdjustDatabaseTestCases()
		{
			var remainingEnvironments = new HashSet<EnvironmentType>();
			var remainingRecoveryModels = new HashSet<DbRecoveryModel?>();
			var allDatabaseNames = new HashSet<string>();

			foreach (var testCase in GetTestCases())
			{
				var newEnv = remainingEnvironments.Add(testCase.Environment);
				var newRecovery = remainingRecoveryModels.Add(testCase.ExpectedRecoveryModel);
				var newDbName = allDatabaseNames.Add(testCase.DbName);

				if (newEnv || newRecovery || newDbName)
				{
					yield return testCase;
				}
			}
		}

		static IDisposable SetupTestMainDatabase(AdminConnection connection)
		{
			var dbDisposable = SetupEmptyDatabase(connection, TestMainDbName);
			using (((ICurrentDbControl)connection).UseDatabase(TestMainDbName))
			{
				connection.ExecuteNonQuery(DataUtils.SQL_InitialTablesForEmptyDatabase());
				DbRegistry.DatabaseRecoveryModel.SaveValue(DbRecoveryModel.BulkLogged.Name, connection);
				DbRegistry.SingleRefDatabaseName.SaveValue("CW-RefDatabase-ForTest", connection);
			}

			var originalDatabaseName = Db.DatabaseName;
			var serverName = Db.ServerName;
			Db.ClearServerDetails();
			Db.InitializeDatabaseDetails(serverName, TestMainDbName);

			var noUpgradeCheckDisposable = Db.DisableSchemaVersionCheck();

			return new DisposableAction(() =>
			{
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(serverName, originalDatabaseName);
				dbDisposable.Dispose();
				noUpgradeCheckDisposable.Dispose();
			});
		}

		static IDisposable SetupBiDatabase(AdminConnection connection, string dbName)
		{
			var dbDisposable = SetupEmptyDatabase(connection, dbName);
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				connection.ExecuteNonQuery(DataUtils.SQL_InitialTablesForEmptyDatabase());
				DbRegistry.BiAuditServer.SaveValue(connection.ServerName, connection);
			}

			return dbDisposable;
		}

		static IDisposable SetupRefDatabase(AdminConnection connection, string dbName)
		{
			var dbDisposable = SetupEmptyDatabase(connection, dbName);
			using (((ICurrentDbControl)connection).UseDatabase(TestMainDbName))
			{
				var tableName = "T" + Guid.NewGuid().ToString("N");
				connection.ExecuteNonQuery($"CREATE TABLE [{dbName}].[dbo].{tableName} (pk int primary key not null, value int)");
				connection.ExecuteNonQuery($"CREATE SYNONYM [dbo].[RefDbEntCA_{tableName}] FOR [{dbName}].[dbo].[{tableName}]");
			}

			return dbDisposable;
		}

		static IDisposable SetupEmptyDatabase(AdminConnection connection, string dbName, DbRecoveryModel? recoveryModel = null)
		{
			var dbDisposable = CreateDbDropExistingDisposable(connection, dbName, recoveryModel ?? DbRecoveryModel.BulkLogged, TestMainDbName);

			return new DisposableAction(() =>
			{
				DbCommitTracker.Reset(dbName);
				dbDisposable.Dispose();
			});
		}

		static IDisposable SetupEnvironment(TestCase testCase)
		{
			return SetupEnvironment(testCase.Environment, testCase.AlwaysOn, testCase.Production);
		}

		static IDisposable SetupEnvironment(EnvironmentType environment, bool alwaysOn, bool production)
		{
			switch (environment)
			{
				case EnvironmentType.WTGCloud:
					DbRecoveryModelManager.ServerDomain_ForTest.Value = "corporate.wtg.zone";
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;
					Globals.IsDebugMode_ForTest.Value = false;
					Globals.IsTest_ForTest.Value = false;
					break;
				case EnvironmentType.DAT:
					DbRecoveryModelManager.ServerDomain_ForTest.Value = "dat.wtg.zone";
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;
					Globals.IsDebugMode_ForTest.Value = true;
					Globals.IsTest_ForTest.Value = true;
					break;
				case EnvironmentType.SAND:
					DbRecoveryModelManager.ServerDomain_ForTest.Value = "sand.wtg.zone";
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;
					Globals.IsDebugMode_ForTest.Value = false;
					Globals.IsTest_ForTest.Value = false;
					break;
				case EnvironmentType.DeveloperPC:
					DbRecoveryModelManager.ServerDomain_ForTest.Value = "wtg.zone";
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
					Globals.IsDebugMode_ForTest.Value = true;
					Globals.IsTest_ForTest.Value = false;
					break;
				case EnvironmentType.SelfHosted:
					DbRecoveryModelManager.ServerDomain_ForTest.Value = "random-name.net";
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
					Globals.IsDebugMode_ForTest.Value = false;
					Globals.IsTest_ForTest.Value = false;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(environment));
			}

			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = alwaysOn;
			Db.Connection.IsAlwaysOnEnabledForTest = alwaysOn;
			var productionDisposable = SetProductRegistrationDbType(production ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Training);
			RefDbTableNameResolver.ResetSingleRefDatabaseName("CW-RefDatabase-ForTest");

			return new DisposableAction(() =>
			{
				productionDisposable.Dispose();

				Globals.IsTest_ForTest.ResetValue();
				Globals.IsDebugMode_ForTest.ResetValue();
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.ResetValue();
				Db.Connection.IsAlwaysOnEnabledForTest = null;
				RefDbTableNameResolver.ResetSingleRefDatabaseName();
				DbRecoveryModelManager.ServerDomain_ForTest.ResetValue();
			});
		}

		static IDisposable SetProductRegistrationDbType(string dbType)
		{
			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.SetupGet(r => r.Key).Returns(Mock.Of<IProductRegistrationKey>(k => k.DatabaseType == dbType));
			return ObjectFactory.Substitute(mockProductRegistration.Object);
		}

		static readonly DbRecoveryModel registryValue = DbRecoveryModel.BulkLogged; // Use neither Simple nor Full, for testing purpose
		const string TestMainDbName = "OdysseyRecoveryModelTest";

		DbConnection[] adminConnectionsOfPossibleTypes;
		protected override void SetUp()
		{
			base.SetUp();
			adminConnectionsOfPossibleTypes = new DbConnection[]
			{
				Db.NewAdminConnection(),
				new DbBackupAndRestoreToolConnection(Db.ServerName, Db.DatabaseName, Mock.Of<IErrorReporter>()),
			};
		}

		protected override void TearDown()
		{
			adminConnectionsOfPossibleTypes.ForEach(c => c.Dispose());
			base.TearDown();
		}

		[DebuggerDisplay("{DbName,nq},{Environment,nq}{AlwaysOn ? \",AO\": \"\",nq}{!Production ? \",Non-Prod\": \"\",nq}")]
		class TestCase
		{
			public TestCase(
				string dbName,
				EnvironmentType environment,
				DbRecoveryModel? shouldBe,
				bool alwaysOn = false,
				bool production = true)
			{
				DbName = dbName;
				Environment = environment;
				AlwaysOn = alwaysOn;
				Production = production;
				ExpectedRecoveryModel = shouldBe;
			}

			public string GetDescription()
			{
				return $"Name: {DbName}, Environment: {Environment}, AlwaysOn: {AlwaysOn}, Production: {Production}";
			}

			public string DbName { get; }
			public EnvironmentType Environment { get; }
			public DbRecoveryModel? ExpectedRecoveryModel { get; }
			public bool AlwaysOn { get; }
			public bool Production { get; }
		}

		enum EnvironmentType
		{
			WTGCloud,
			DAT,
			SAND,
			DeveloperPC,
			SelfHosted,
		}
	}
}
