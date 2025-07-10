using System;
using System.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms;
using Enterprise.ZArchitecture.Core;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Moq;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class OnlinePreSchemaUpgraderEngineTest : BaseUpgraderTestCase
	{
		protected override void TearDown()
		{
			TestOnlinePreSchemaUpgrader.CleanTestResources();

			base.TearDown();
		}

		readonly PreUpgraderForTest TestOnlinePreSchemaUpgrader = new PreUpgraderForTest();

		#region Tables and Columns

		class MyTransformation : DataTransformation
		{
			public override string UserDescription => "MyTransformation";
			protected override void OnlinePreUpgradeTransform()
			{
				IsCalled = true;
			}

			public static bool IsCalled;
		}

		[RequiresLargeLogFile]
		public void TestTablesAndColumnsSynchronised()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				TablePreSynchroniser.CreatePreAddDb_ForTest();

				var mockProvider = new Mock<ITransformationMappingProvider>();

				mockProvider.Setup(x => x.GetAllMappings()).Returns(new Mapping[] { Mapping.New<MyTransformation>(new VersionLabel(9999, 0)) });

				var serviceProvider = GlobalServiceProvider.Instance;

				var mockServiceProvider = new Mock<IServiceProvider>();
				mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(x => serviceProvider.GetService(x));
				mockServiceProvider.Setup(x => x.GetService(typeof(ITransformationMappingProvider))).Returns(mockProvider.Object);
				using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
				{
					try
					{
						TestOnlinePreSchemaUpgrader.DoTestUpgrade();

						var isCalled = MyTransformation.IsCalled;
						Assert("OnlinePreUpgrade method should be called but was not", MyTransformation.IsCalled);
					}
					finally
					{
						MyTransformation.IsCalled = false;
					}
				}

				AssertColumnNotSynchronised();
				AssertColumnNotDropped();
				AssertOldTableNotRemoved();
			}
		}

		void AssertOldTableNotRemoved()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Old'",
				PreUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the Col3 (Match_ColDiff table) has been changed to VARCHAR(40)
		/// </summary>
		void AssertColumnNotSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_ColDiff' AND COLUMN_NAME = 'Col3'
				AND UPPER(DATA_TYPE) = 'VARCHAR' AND CHARACTER_MAXIMUM_LENGTH = 40
				AND UPPER(IS_NULLABLE) = 'NO'",
				PreUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(0, qtyRows);
		}

		/// <summary>
		/// Test if Col7 (Match_ColDiff table) has been removed
		/// </summary>
		void AssertColumnNotDropped()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_ColDiff' AND COLUMN_NAME = 'Col7'",
				PreUpgraderForTest.TestMainDb);
			int qtyRows = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals(1, qtyRows);
		}

		#endregion

		[UseSnapshotProtection]
		public void TestAuditTriggerIsBypassed()
		{
			// Arrange
			TestOnlinePreSchemaUpgrader.UpgradeDbCreator.AddCreateDbObjectScript(@"
			CREATE TRIGGER TG_Match_ColDiff_SystemLastEditTimeUtc_Update
					ON Match_ColDiff
					AFTER UPDATE
				AS
				BEGIN
					IF (@@ROWCOUNT = 0) OR (SELECT SESSION_CONTEXT(N'Suspend_System_Audit_Columns_Guard')) = '1' RETURN
					SET NOCOUNT ON;

					IF (NOT UPDATE([SystemLastEditTimeUtc]))
					BEGIN
						RAISERROR('Attempt to update without [SystemLastEditTimeUtc]', 16, 1);
						RETURN
					END
				END
				;
				");

			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				// Act and Assert
				AssertNoExceptionThrown(() => TestOnlinePreSchemaUpgrader.DoTestUpgrade());
			}
		}

		[UseSnapshotProtection]
		public void TestDbStatistics()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				StatisticsSwitch.Instance.SetDbStatisticsSettings(Db.AdminConnection, Db.DatabaseName, true, true, false);

				var stats = StatisticsSwitch.Instance.GetStatisticsSettings(Db.AdminConnection, Db.DatabaseName);
				AssertEquals("Precondition", "ON-ON-OFF", stats.Current.ToString());

				var upgrader = new DummyPreUpgraderForTest();
				AssertExceptionThrown<Exception>(() => upgrader.RunUpgrade());

				stats = StatisticsSwitch.Instance.GetStatisticsSettings(Db.AdminConnection, Db.DatabaseName);
				AssertEquals("Statistics has been switched OFF during upgrade", "OFF-OFF-OFF", upgrader.CurrentStatisticsSettings);
				AssertEquals("Statistics has been switched back and put ASYNC ON", "ON-ON-ON", stats.Current.ToString());
			}
		}

		public void TestTurnOffStatistics_Timeout()
		{
			var innerException = new Win32Exception(258, "The wait operation timed out");
			var sqlError = "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.";
			var sqlException = SqlExceptionBuilder.CreateSqlException(-2, byte.MaxValue, byte.MinValue, Db.ServerName, sqlError, "", 1, innerException);

			AssertTurnOffStatisticsHandlesException(sqlException);
		}

		public void TestTurnOffStatistics_WhenCouldNotObtainLock()
		{
			var sqlError = "ALTER DATABASE failed because a lock could not be placed on database. Try again later.";
			var sqlException = SqlExceptionBuilder.CreateSqlException(5061, byte.MaxValue, byte.MinValue, Db.ServerName, sqlError, "", 1);
			AssertTurnOffStatisticsHandlesException(sqlException);
		}

		public void TestTurnOffStatistics_WhenBackupInProgress()
		{
			const string sqlError = "Backup, file manipulation operations (such as ALTER DATABASE ADD FILE) and encryption changes on a database must be serialized. Reissue the statement after the current backup or file manipulation operation is completed.";
			var sqlException = SqlExceptionBuilder.CreateSqlException(3023, byte.MaxValue, byte.MinValue, Db.ServerName, sqlError, "", 1);
			AssertTurnOffStatisticsHandlesException(sqlException);
		}

		void AssertTurnOffStatisticsHandlesException(System.Data.Common.DbException sqlException)
		{
			var mockStatisticsSwitch = new Mock<IStatisticsSwitch>();
			mockStatisticsSwitch.Setup(x => x.CheckAndTurnStatistics(Db.DatabaseName, false))
				.Throws(sqlException);

			Exception actualException = null;
			try
			{
				OnlinePreSchemaUpgrader.TurnOffStatistics(mockStatisticsSwitch.Object, RetryPolicy.NoRetry);
			}
			catch (Exception ex)
			{
				actualException = ex;
			}
			AssertType<OnlinePreSchemaUpgraderEnvironmentException>(actualException);
			AssertEquals(OnlinePreSchemaUpgraderEnvironmentException.DisableStatisticsExceptionMessage, actualException.Message);
		}

		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				TablePreSynchroniser.CreatePreAddDb_ForTest();
			}

			return new PreUpgraderForTest(dummyUpgradeManager);
		}

		#region PreUpgraderForTest

		class PreUpgraderForTest : OnlinePreSchemaUpgrader
		{
			internal PreUpgraderForTest()
				: base(new DummyUpgradeManager(), Db.Connection, Db.Connection, Db.Connection, new VersionLabel(0, 0))
			{
			}

			internal PreUpgraderForTest(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
				: base(dummyUpgradeManager, Db.Connection, Db.Connection, Db.Connection, new VersionLabel(0, 0))
			{
			}

			public const string TestMainDb = UpgUtils.UpgraderPrefix + "TestMainDB";
			public const string TestDocManagerDb = TestMainDb + "_SD001";

			public void DoTestUpgrade()
			{
				CreateTestDbs();

				using (((ICurrentDbControl)Db.Connection).UseDatabase(TestMainDb))
				{
					UpgradeMainDbSchema();
				}
			}

			protected override void DoUpgrade()
			{
				using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
				{
					CreateTestDbs();

					using (((ICurrentDbControl)Db.Connection).UseDatabase(TestMainDb))
					{
						UpgradeMainDbSchema();
					}
				}
			}

			public void CleanTestResources()
			{
				DropTestDbs();
			}

			protected void CreateTestDbs()
			{
				TestDbCreator.CreateDropExisting();
				TestDocManagerDbCreator.CreateDropExisting();
			}

			protected void DropTestDbs()
			{
				TestDocManagerDbCreator.Drop();
				TestDbCreator.Drop();
			}

			protected override void UpgradeMainDbSchema()
			{
				var syncSchema = new UnitTestOnlineMainDatabaseSchemaSynchronisationWrapper(Manager, TestMainDb);
				syncSchema.Run();
			}

			public MainUpgradeDbCreatorForTesting UpgradeDbCreator => (MainUpgradeDbCreatorForTesting)TestDbCreator;

			readonly IAuxiliaryDbCreator TestDbCreator = new MainUpgradeDbCreatorForTesting(TestMainDb);
			readonly IAuxiliaryDbCreator TestDocManagerDbCreator = new AuxiliaryDbCreatorForTesting(TestDocManagerDb);
		}

		class DummyPreUpgraderForTest : OnlinePreSchemaUpgrader
		{
			public DummyPreUpgraderForTest()
				: base(new DummyUpgradeManager(), Db.Connection, Db.Connection, Db.Connection, new VersionLabel(0, 0))
			{
			}

			public string CurrentStatisticsSettings { get; set; }

			protected override void UpgradeMainDbSchema()
			{
				var stats = StatisticsSwitch.Instance.GetStatisticsSettings(Db.AdminConnection, Db.DatabaseName);

				CurrentStatisticsSettings = stats.Current.ToString();

				throw new OperationCanceledException("Upgrade aborted by user");
			}
		}

		#endregion
	}
}
