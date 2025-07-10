using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class SchemaUpgraderTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestLostConnectionErrorMessage()
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();
				connection.CloseConnection();

				var upgrader = new SchemaUpgraderForVersionTesting(connection);
				try
				{
					upgrader.RunUpgrade();
					Fail("Expected an exception.");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Exception message", "Database Schema Upgrade failed.\r\nDatabase connection was lost. Rolling back upgrade to ensure data consistency.", ex.Message);
						AssertEquals("Inner exception message", "Invalid operation. The connection is closed.", ex.InnerException?.Message);
					});
				}
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNewlyCreatedEDocDbsAreIncludedInUpgrade()
		{
			// Arrange
			const string testMainDatabase = nameof(TestNewlyCreatedEDocDbsAreIncludedInUpgrade);

			var upgradeManager = new Mock<IUpgradeManager>();
			upgradeManager
				.Setup(x => x.SchemaVersionBeforeUpgrade)
				.Returns(new VersionLabel(10000, 0));

			using (AdoTestUtils.CreateDbDropExistingDisposable(testMainDatabase))
			using (var adminConnection = Db.NewAdminConnection(Db.ServerName, testMainDatabase))
			using (new DisposableAction(adminConnection.RollbackTransaction))
			using (((ICurrentDbControl)adminConnection).UseDatabase(testMainDatabase))
			using (ObjectFactory.Substitute(Mock.Of<ITransformationMappingProvider>(x => x.GetAllMappings() == Array.Empty<Mapping>())))
			{
				adminConnection.BeginTransaction();
				createStmDataTable(adminConnection);
				var schemaUpgraderMock = new Mock<SchemaUpgrader>(upgradeManager.Object, adminConnection, adminConnection, adminConnection)
				{
					CallBase = true
				};
				schemaUpgraderMock
					.Protected()
					.Setup<IEnumerable<string>>("DocManagerDBsToUpgrade")
					.Returns(Array.Empty<string>());

				var cacheEdocDbList = schemaUpgraderMock.Object.EstimatedNumberOfTasks;

				// Act
				schemaUpgraderMock.Object.RunUpgrade();

				// Assert
				schemaUpgraderMock
					.Protected()
					.Verify<IEnumerable<string>>("DocManagerDBsToUpgrade", Times.Exactly(2));
			}

			void createStmDataTable(DbConnection connection)
			{
				var createTestTableScript = @"
CREATE TABLE dbo.StmData
( 
	[SD_PK] UNIQUEIDENTIFIER NOT NULL,
	[SD_Name] VARCHAR(300) NOT NULL DEFAULT '',
	[SD_Owner] UNIQUEIDENTIFIER NULL,
	[SD_DepartmentGuid] UNIQUEIDENTIFIER NULL,
	[SD_Type] CHAR(3) NOT NULL DEFAULT '',
	[SD_IsLogged] BIT NOT NULL DEFAULT 0,
	[SD_BinaryValue] VARBINARY(MAX) NULL,
	[SD_GuidValue] UNIQUEIDENTIFIER NULL,
	[SD_IsCancelled] BIT NOT NULL DEFAULT 0,
	[SD_PreserveTestValue] BIT NOT NULL DEFAULT 0,
	[SD_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[SD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[SD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
	[SD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
)"
;
				connection.ExecuteNonQuery(createTestTableScript);
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestUpgradeReferenceDatabasesWithoutEdw()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var upgrader = new SchemaUpgraderForVersionTesting(connection, auditConnection: null, dataWarehouseConnection: null);
				upgrader.UpgradeReferenceDbs_Exposed();
			}
		}

		[UseSnapshotProtection([DatabaseType.EDW])]
		public void TestEdwHaveReferenceDatabaseSynonymsCreated_SameServer()
		{
			using (var connection = Db.NewAdminConnection())
			using (var edwConnection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var mainDbSynonyms = GetSynonyms(connection, Db.DatabaseName);
				Assert(mainDbSynonyms.Count != 0);

				ClearSynonyms(connection, Db.EdwDatabaseName);
				var edwSynonyms = GetSynonyms(connection, Db.EdwDatabaseName);
				AssertEquals("Arrange: EDW synonyms count should be 0", 0, edwSynonyms.Count);

				var upgrader = new SchemaUpgraderForVersionTesting(connection, null, edwConnection);

				upgrader.UpgradeDatawarehouseReferenceDbsRequired_ForTest = true;
				upgrader.DoReferenceDbUpgradeForEdw_Exposed();
				AssertContainsExactElementsInAnyOrder("EDW synonyms", mainDbSynonyms, GetSynonyms(connection, Db.EdwDatabaseName));
			}
		}

		[UseSnapshotProtection([DatabaseType.EDW])]
		public void TestEdwHaveReferenceDatabaseSynonymsCreated_DifferentServer()
		{
			using (var connection = Db.NewAdminConnection())
			using (var edwConnection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var mainDbSynonyms = GetSynonyms(connection, Db.DatabaseName);
				Assert(mainDbSynonyms.Count != 0);

				ClearSynonyms(connection, Db.EdwDatabaseName);
				var edwSynonyms = GetSynonyms(connection, Db.EdwDatabaseName);
				AssertEquals("Arrange: EDW synonyms count should be 0", 0, edwSynonyms.Count);

				var upgrader = new SchemaUpgraderForVersionTesting(connection, null, edwConnection);

				upgrader.UpgradeDatawarehouseReferenceDbsRequired_ForTest = false;
				upgrader.DoReferenceDbUpgradeForEdw_Exposed();
				AssertContainsExactElementsInAnyOrder("EDW synonyms", mainDbSynonyms, GetSynonyms(connection, Db.EdwDatabaseName));
			}
		}

		void ClearSynonyms(DbConnection connection, string database)
		{
			var synonyms = GetSynonyms(connection, database);
			using ((connection as ICurrentDbControl).UseDatabase(database))
			{
				foreach (var synonym in synonyms)
				{
					connection.ExecuteNonQuery($"DROP SYNONYM {synonym}");
				}
			}
		}

		List<string> GetSynonyms(DbConnection connection, string database)
		{
			var synonyms = new List<string>();
			connection.ExecuteReader($"select name from {database}.sys.synonyms where name like 'RefDatabase_%'", r => synonyms.Add(r[0] as string));
			return synonyms;
		}

		class SchemaUpgraderForVersionTesting : SchemaUpgrader
		{
			public SchemaUpgraderForVersionTesting(DbConnection connection)
				: base(new DummyUpgradeManager(), connection, connection, connection)
			{
			}

			public SchemaUpgraderForVersionTesting(DbConnection connection, DbConnection auditConnection, DbConnection dataWarehouseConnection)
				: base(new DummyUpgradeManager(), connection, auditConnection, dataWarehouseConnection)
			{
			}

			public void UpgradeReferenceDbs_Exposed()
			{
				base.UpgradeReferenceDbs();
			}

			public void DoReferenceDbUpgradeForEdw_Exposed()
			{
				base.DoReferenceDbUpgradeForEdw();
			}

			public override bool UpgradeDatawarehouseReferenceDbsRequired => UpgradeDatawarehouseReferenceDbsRequired_ForTest;
			public bool UpgradeDatawarehouseReferenceDbsRequired_ForTest;
		}
	}
}
