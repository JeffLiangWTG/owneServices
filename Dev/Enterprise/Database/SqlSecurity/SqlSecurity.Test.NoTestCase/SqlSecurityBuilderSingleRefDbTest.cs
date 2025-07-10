using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Moq;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	class SqlSecurityBuilderSingleRefDbTest
	{
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestPrincipalsAndMembershipsListIsEmpty: Hosted in Wise cloud shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestPrincipalsAndMembershipsListIsEmpty: Hosted in Wise dedicateed server")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestPrincipalsAndMembershipsListIsEmpty: Self hosted locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestPrincipalsAndMembershipsListIsEmpty: Self hosted open")]
		public void TestPrincipalsAndMembershipsListIsEmpty(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(sharedSingleRefDbName))
			{
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, DatabaseType.SingleSharedRef);
				var proposedPrincipalsList = Helper.DatabasePrincipalsList(adminConnection, result.ProposedPrincipalsAndMemberships);

				// Assert
				Assert.That(proposedPrincipalsList, Is.Empty, "Proposed principals and memberships list should be empty for Single reference database.");
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestCorrectPermissionsListIsGenerate: Hosted in Wise cloud shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestCorrectPermissionsListIsGenerate: Hosted in Wise dedicateed server")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestCorrectPermissionsListIsGenerate: Self hosted locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestCorrectPermissionsListIsGenerate: Self hosted open")]
		public void TestCorrectPermissionsListIsGenerate(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var isHostedInWiseCloud = (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer);
			var isDedicated = databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer;

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (((ICurrentDbControl)adminConnection).UseDatabase(sharedSingleRefDbName))
			{
				var expectedPermissions = ApplicationUsersAndRoles.Permissions.ByDatabaseType(DatabaseType.SingleSharedRef, Db.DatabaseName);
				var expectedPermissioinsList = Helper.DatabasePermissionsList(adminConnection,
									$@"
SELECT * FROM (VALUES
	('G', N'CONNECT', N'DATABASE', N'', N'{sharedSingleRefDbName}', N'', N'guest', N''),
	('G', N'SELECT', N'DATABASE', N'', N'{sharedSingleRefDbName}', N'', N'guest', N''),
	('G', N'EXECUTE', N'DATABASE', N'', N'{sharedSingleRefDbName}', N'', N'guest', N'')
 ) AS PER (State, Permission, SecurableType, SecurableSchema, Securable, SecurableColumn, Grantee, Grantor)
");
				var sqlSecurityBuilder = new SqlSecurityBuilder(Db.DatabaseName, isHostedInWiseCloud, isDedicated, Mock.Of<IStaffInfoProvider>());

				// Act
				var result = sqlSecurityBuilder.GetDatabaseLevelInfo(adminConnection, adminConnection, DatabaseType.SingleSharedRef);
				var proposedPermissionsList = Helper.DatabasePermissionsList(adminConnection, result.ProposedPermissions);

				// Assert
				Assert.That(proposedPermissionsList, Is.EquivalentTo(expectedPermissioinsList).IgnoreCase, "Expected permissions should be the same as proposed permissionis.");
			}
		}

		#region Implementation

		static readonly string sharedSingleRefDbName = Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef);

		AdminConnection adminConnection;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			adminConnection = Db.NewAdminConnection();
			adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS {sharedSingleRefDbName.QuoteName()};
CREATE DATABASE {sharedSingleRefDbName.QuoteName()};
");
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS {sharedSingleRefDbName.QuoteName()};
");
			adminConnection?.Dispose();
		}

		#endregion Implementation
	}
}
