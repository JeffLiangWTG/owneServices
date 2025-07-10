using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Moq;
using NUnit.Framework;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	class SqlSecurityManagerSingleRefDbTest
	{
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestCorrestPermissionsAreGranted: Hosted in Wise cloud shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestCorrestPermissionsAreGranted: Hosted in Wise dedicated server")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestCorrestPermissionsAreGranted: Self hosted locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestCorrestPermissionsAreGranted: Self hosted open")]
		public void TestCorrestPermissionsAreGranted(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (((ICurrentDbControl)adminConnection).UseDatabase(sharedSingleRefDbName))
			{
				Helper.DropDatabasePrincipals(adminConnection);

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				var findGuestPermissions = @"
FROM
	sys.database_permissions     AS dp
	JOIN sys.database_principals AS grantee ON dp.grantee_principal_id = grantee.principal_id
		AND grantee.name = N'guest'
";

				Assume.That(adminConnection.Exists(findGuestPermissions), Is.False, "There should be no permissions granted to guest principal before building security.");

				using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
				using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
				{
					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, sharedSingleRefDbName, trialRun: true);
					Assert.That(adminConnection.Exists(findGuestPermissions), Is.False, "There should be no permissions granted to guest principal after building security in trial mode.");

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, sharedSingleRefDbName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, "guest", "SELECT", "EXECUTE", "CONNECT");
				}
			}
		}

		#region Implementation

		static readonly string sharedSingleRefDbName = Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef);

		AdminConnection adminConnection;

		[SetUp]
		public void Setup()
		{
			adminConnection = Db.NewAdminConnection();
			adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS {sharedSingleRefDbName.QuoteName()};
CREATE DATABASE {sharedSingleRefDbName.QuoteName()};
");
		}

		[TearDown]
		public void TearDown()
		{
			adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS {sharedSingleRefDbName.QuoteName()};
");
			adminConnection?.Dispose();
		}

		#endregion Implementation
	}
}
