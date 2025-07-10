using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Transactions;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using WTG.Data.SqlDbSecuritySynchroniser;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	class SqlSecurityManagerTests : SqlSecurityTestFixture
	{
		#region Testing BuildApplicationLoginsSecurity

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildApplicationLoginsSecurityBuildsSecurityForApplicationLoginsUsersAndRoles: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildApplicationLoginsSecurityBuildsSecurityForApplicationLoginsUsersAndRoles: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildApplicationLoginsSecurityBuildsSecurityForApplicationLoginsUsersAndRoles: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildApplicationLoginsSecurityBuildsSecurityForApplicationLoginsUsersAndRoles: Wise cloud hosted dedicated")]
		public void TestBuildApplicationLoginsSecurityBuildsSecurityForApplicationLoginsUsersAndRoles(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var databases = Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef));

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);

			foreach (var databaseName in databases)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
				}
			}

			using (((ICurrentDbControl)adminConnection).UseDatabase(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
			{
				var findGuestPermissions = @"
FROM
	sys.database_permissions     AS dp
	JOIN sys.database_principals AS grantee ON dp.grantee_principal_id = grantee.principal_id
		AND grantee.name = N'guest'
";

				Assume.That(adminConnection.Exists(findGuestPermissions), Is.False, "There should be no permissions granted to guest principal before building security.");
			}

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbReaderLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, dbReaderLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, dbReaderLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(adminConnection, $"Login '{dbReaderLogin}' should have correct password.", dbReaderLogin, testReaderCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, dbReaderLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(adminConnection, dbReaderLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(adminConnection, dbReaderLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, dbReaderLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(adminConnection, dbReaderLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbWriterLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, dbWriterLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, dbWriterLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(adminConnection, $"Login '{dbWriterLogin}' should have correct password.", dbWriterLogin, testWriterCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, dbWriterLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(adminConnection, dbWriterLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(adminConnection, dbWriterLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, dbWriterLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(adminConnection, dbWriterLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbRestrictedReaderLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, dbRestrictedReaderLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(adminConnection, $"Login '{dbRestrictedReaderLogin}' should have correct password.", dbRestrictedReaderLogin, testRestrictedReaderCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, dbRestrictedReaderLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(adminConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(adminConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, dbRestrictedReaderLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(adminConnection, dbRestrictedReaderLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbRestrictedWriterLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, dbRestrictedWriterLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(adminConnection, $"Login '{dbRestrictedWriterLogin}' should have correct password.", dbRestrictedWriterLogin, testRestrictedWriterCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, dbRestrictedWriterLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(adminConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(adminConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, dbRestrictedWriterLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(adminConnection, dbRestrictedWriterLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin));

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbUnrestrictedWriterLogin, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, dbUnrestrictedWriterLogin);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, dbUnrestrictedWriterLogin, new Dictionary<string, string[]>() { { "G", new[] { "CONNECT SQL" } } });
				AssertionsHelper.AssertLoginHashMatchesPassword(adminConnection, $"Login '{dbUnrestrictedWriterLogin}' should have correct password.", dbUnrestrictedWriterLogin, testUnrestrictedWriterCredentials.Password);
				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, dbUnrestrictedWriterLogin, Db.SqlMasterDb);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(adminConnection, dbUnrestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(adminConnection, dbUnrestrictedWriterLogin);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, dbUnrestrictedWriterLogin, "us_english");
				AssertionsHelper.AssertSqlLoginSidEquals(adminConnection, dbUnrestrictedWriterLogin, SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin));

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbReaderLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalHasRoles(adminConnection, dbReaderLogin, DbRoleTypes.CwReaderRole, "db_datareader");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, dbReaderLogin, "CONNECT", "EXECUTE");

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalHasRoles(adminConnection, dbWriterLogin, DbRoleTypes.CwRestrictedWriterRole);
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, dbWriterLogin, "CONNECT", "EXECUTE");

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbRestrictedReaderLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalHasRoles(adminConnection, dbRestrictedReaderLogin, DbRoleTypes.CwRestrictedReaderRole);
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, dbRestrictedReaderLogin, "CONNECT", "EXECUTE");

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbRestrictedWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalHasRoles(adminConnection, dbRestrictedWriterLogin, DbRoleTypes.CwRestrictedWriterRole);
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, dbRestrictedWriterLogin, "CONNECT", "EXECUTE");

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbUnrestrictedWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalHasRoles(adminConnection, dbUnrestrictedWriterLogin, DbRoleTypes.CwUnrestrictedWriterRole);
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, dbUnrestrictedWriterLogin, "CONNECT", "EXECUTE");

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwHRMStaffRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalHasNoRoles(adminConnection, DbRoleTypes.CwHRMStaffRole);
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlHrmSchema, "SELECT");
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, "dbo");
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, "OrderTracking");
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlCdcSchema);
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlStagingSchema);

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwHRMStaffRole, 3); /* 3 for schema */

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwReaderRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalHasRoles(adminConnection, DbRoleTypes.CwReaderRole, "db_datareader");

						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, DbRoleTypes.CwReaderRole, "SHOWPLAN", "VIEW DEFINITION");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllCustomTableValueTypesAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "EXECUTE");

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnTypesThatAreNotCustomTableValueTypes(adminConnection, DbRoleTypes.CwReaderRole);

						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllObjectsOfTypeAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "AF", "EXECUTE");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllObjectsOfTypeAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "FS", "EXECUTE");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllObjectsOfTypeAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "FN", "EXECUTE");

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnMicrosoftShippedObjects(adminConnection, DbRoleTypes.CwReaderRole);
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnObjectsOfTypesOtherThanExcluded(adminConnection, DbRoleTypes.CwReaderRole, "AF", "FN", "FS");

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwReaderRole, 0, 1, 6); /* 0 for database, 1 for object or Column, 6 for type */

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalHasNoRoles(adminConnection, DbRoleTypes.CwRestrictedReaderRole);

						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "SHOWPLAN", "VIEW DEFINITION");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "dbo", "EXECUTE", "SELECT");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "OrderTracking", "EXECUTE", "SELECT");

						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnObjectOfTypeAndNoOtherPermissionsOnThisType(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "V", "sys.sql_expression_dependencies", "SELECT");
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnObjectsOtherThanSpecified(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "sys.sql_expression_dependencies");

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, DbSecurity.SqlHrmSchema);
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, DbSecurity.SqlStagingSchema);
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, DbSecurity.SqlCdcSchema);

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwRestrictedReaderRole, 0, 1, 3); /* 0 for database, 1 for object, 3 for schema */

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalHasNoRoles(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "ALTER", "CREATE SCHEMA", "REFERENCES", "SHOWPLAN", "VIEW DATABASE STATE", "VIEW DEFINITION");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, DbRoleTypes.CwRestrictedWriterRole, dbRestrictedReaderLogin, dbReaderLogin);

						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "dbo", "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "OrderTracking", "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");

						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnObjectOfTypeAndNoOtherPermissionsOnThisType(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "V", "sys.sql_expression_dependencies", "SELECT");
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnObjectsOtherThanSpecified(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "sys.sql_expression_dependencies");

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, DbSecurity.SqlHrmSchema);
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, DbSecurity.SqlStagingSchema);
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, DbSecurity.SqlCdcSchema);

						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwRestrictedWriterRole, 0, 1, 3, 4); /* 0 for database, 1 for object, 3 for schema, 4 for principals */

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalHasNoRoles(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "VIEW DATABASE STATE", "VIEW DEFINITION");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, "dbo", "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, "OrderTracking", "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, DbSecurity.SqlStagingSchema, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, DbSecurity.SqlCdcSchema, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, "hrm", "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
						DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, 0, 3); /* 0 for database, 3 for schema */
					}
				}

				using (((ICurrentDbControl)adminConnection).UseDatabase(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, "guest", "SELECT", "EXECUTE", "CONNECT");
				}
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrationDisabled: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrationDisabled: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrationDisabled: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrationDisabled: Wise cloud hosted dedicated")]
		public void TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrationDisabled(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			var databases = Helper.Databases();

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

			var allDatabases = databases.Append(Db.DatabaseName);

			foreach (var databaseName in allDatabases)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
				}
			}

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameReader);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameDeveloper);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

				foreach (var databaseName in allDatabases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrated: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrated: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrated: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrated: Wise cloud hosted dedicated")]
		public void TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersWhenADIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole });

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				var databases = Helper.Databases();

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
					}
				}

				using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
				{
					// Act
					sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

					// Assert
					AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
					AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

					foreach (var databaseName in databases)
					{
						using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
						{
							DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
							DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
						}
					}
				}
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, false, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Self-hosted open, AD integration disabled")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, false, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Self-hosted locked, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Wise clound hosted shared, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Wise cloud hosted dedicated, AD integration disabled")]

		[TestCase(DatabaseTestMode.SelfHostedOpen, true, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Self-hosted open, AD integrated")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, true, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Self-hosted locked, AD integrated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Wise clound hosted shared, AD integrated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern: Wise cloud hosted dedicated, AD integrated")]
		public void TestBuildApplicationLoginsSecurityOnlyDropUsersDatabaseRolesAndLoginsSatifyingApplicationLoginOrDatabaseRolePattern(DatabaseTestMode databaseTestMode, bool isADIntegrated)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: isADIntegrated, "ADTest_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlLoginNameForADIntegratedAccountWithUserNamePrefix = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginNameWithUserNamePrefix = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				var sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix = Helper.GetEnterpriseLoginFullName(Helper.ADTestUserLongNameB.Name, Db.DatabaseName);
				var windowsLoginNameWithNoUserNamePrefix = Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000;

				var staffLoginWithDatabaseAccess = "TestReader";
				var staffLoginWithNoDatabaseAccess = "TestDeveloper";

				var sqlLoginWithDatabaseAccess = Helper.GetEnterpriseLoginFullName(staffLoginWithDatabaseAccess, Db.DatabaseName);
				var sqlLoginWithNoDatabaseAccess = Helper.GetEnterpriseLoginFullName(staffLoginWithNoDatabaseAccess, Db.DatabaseName);

				var randomLoginName = "_Tst_SomeRandomName";
				var randomLoginNameSatisfyingStaffPattern = Helper.GetEnterpriseLoginFullName(randomLoginName, Db.DatabaseName);
				var randomLoginNameSatisfyingApplicationLoginPattern = $"{Db.DatabaseName}_{randomLoginName}";
				var randomRoleNameSatisfyingApplicationRolePattern = "cwSomeRandomRole";

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole });

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					Helper.ADTestUserLongNameB.Name,
					TestConstants.Domain,
					Guid.Parse(Helper.ADTestUserLongNameB.Guid),
					adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole });

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginWithDatabaseAccess, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwReaderRole });
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginWithNoDatabaseAccess, null, null, adminConnection, Array.Empty<string>());

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {windowsLoginNameWithUserNamePrefix.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginNameForADIntegratedAccountWithUserNamePrefix.QuoteName()} WITH PASSWORD = N'AADG1232134[][]dsdg';

CREATE LOGIN {windowsLoginNameWithNoUserNamePrefix.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix.QuoteName()} WITH PASSWORD = N'AADG1232134[][]dsdg';

CREATE LOGIN {sqlLoginWithDatabaseAccess.QuoteName()} WITH PASSWORD = N'AADG1232134[][]dsdg';
CREATE LOGIN {sqlLoginWithNoDatabaseAccess.QuoteName()} WITH PASSWORD = N'AADG1232134[][]dsdg';

CREATE LOGIN {randomLoginNameSatisfyingStaffPattern.QuoteName()} WITH PASSWORD = N'AADG1232134[][]dsdg';
CREATE LOGIN {randomLoginName.QuoteName()} WITH PASSWORD = N'AADG1232134[][]dsdg';
CREATE LOGIN {randomLoginNameSatisfyingApplicationLoginPattern.QuoteName()} WITH PASSWORD = N'AADG1232134[][]dsdg';

CREATE SERVER ROLE {randomRoleNameSatisfyingApplicationRolePattern.QuoteName()};
");
				var singleRefDbName = Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef);

				var databases = Helper.Databases().Except(singleRefDbName);

				foreach (var database in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(database))
					{
						adminConnection.ExecuteNonQuery($@"
CREATE USER {windowsLoginNameWithUserNamePrefix.QuoteName()} FROM LOGIN {windowsLoginNameWithUserNamePrefix.QuoteName()};
CREATE USER {sqlLoginNameForADIntegratedAccountWithUserNamePrefix.QuoteName()} FROM LOGIN {sqlLoginNameForADIntegratedAccountWithUserNamePrefix.QuoteName()};

CREATE USER {windowsLoginNameWithNoUserNamePrefix.QuoteName()} FROM LOGIN {windowsLoginNameWithNoUserNamePrefix.QuoteName()};
CREATE USER {sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix.QuoteName()} FROM LOGIN {sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix.QuoteName()};

CREATE USER {sqlLoginWithDatabaseAccess.QuoteName()} FROM LOGIN {sqlLoginWithDatabaseAccess.QuoteName()};
CREATE USER {sqlLoginWithNoDatabaseAccess.QuoteName()} FROM LOGIN {sqlLoginWithNoDatabaseAccess.QuoteName()};

CREATE USER {randomLoginNameSatisfyingStaffPattern.QuoteName()} FROM LOGIN {randomLoginNameSatisfyingStaffPattern.QuoteName()};
CREATE USER {randomLoginName.QuoteName()} FROM LOGIN {randomLoginName.QuoteName()};
CREATE USER {randomLoginNameSatisfyingApplicationLoginPattern.QuoteName()} FROM LOGIN {randomLoginNameSatisfyingApplicationLoginPattern.QuoteName()};

CREATE ROLE {randomRoleNameSatisfyingApplicationRolePattern.QuoteName()};
");
					}
				}

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginWithDatabaseAccess, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginWithNoDatabaseAccess, "S");

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, randomLoginNameSatisfyingStaffPattern, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, randomLoginName, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, randomLoginNameSatisfyingApplicationLoginPattern, "S");

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, randomRoleNameSatisfyingApplicationRolePattern, "R");

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");

						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");

						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginWithDatabaseAccess, "S");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginWithNoDatabaseAccess, "S");

						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, randomLoginNameSatisfyingStaffPattern, "S");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, randomLoginName, "S");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, randomLoginNameSatisfyingApplicationLoginPattern, "S");

						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, randomRoleNameSatisfyingApplicationRolePattern, "R");
					}
				}

				using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
				{
					// Act
					sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

					// Assert
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");

					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");

					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginWithDatabaseAccess, "S");
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginWithNoDatabaseAccess, "S");

					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, randomLoginNameSatisfyingStaffPattern, "S");
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, randomLoginName, "S");

					AssertionsHelper.AssertPrincipalMissing(adminConnection, randomLoginNameSatisfyingApplicationLoginPattern);

					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, randomRoleNameSatisfyingApplicationRolePattern, "R");

					foreach (var databaseName in databases)
					{
						using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
						{
							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");

							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");
							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");

							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginWithDatabaseAccess, "S");
							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginWithNoDatabaseAccess, "S");

							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, randomLoginNameSatisfyingStaffPattern, "S");
							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, randomLoginName, "S");

							DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, randomLoginNameSatisfyingApplicationLoginPattern);

							if ((Enterprise.SqlSecurity.Helper.GetDatabaseType(Db.DatabaseName, singleRefDbName, databaseName) & DatabaseType.SharedRef) == 0)
							{
								DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, randomRoleNameSatisfyingApplicationRolePattern);
							}
						}
					}
				}
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrationDisabled: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrationDisabled: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrationDisabled: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrationDisabled: Wise cloud hosted dedicated")]
		public void TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrationDisabled(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			var databases = Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef));

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameReader, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameDeveloper, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameBackupOperator, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameHrmStaff, "S");

				Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);

				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameReader, "S");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameDeveloper, "S");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameBackupOperator, "S");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameHrmStaff, "S");

						Helper.DropDatabasePrincipals(adminConnection);

						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}

				// Act
				sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameReader);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameDeveloper);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrated: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrated: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrated: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrated: Wise cloud hosted dedicated")]
		public void TestBuildApplicationLoginsSecurityDoesNotCreateUsersAndLoginsForStaffMembersAfterACallToBuildSecurityWnenADIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, "ADTest_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				var sqlLoginNameForADIntegratedAccountWithUserNamePrefix = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginNameWithUserNamePrefix = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				var sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix = Helper.GetEnterpriseLoginFullName(Helper.ADTestUserLongNameB.Name, Db.DatabaseName);
				var windowsLoginNameWithNoUserNamePrefix = Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000;

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole });

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					Helper.ADTestUserLongNameB.Name,
					TestConstants.Domain,
					Guid.Parse(Helper.ADTestUserLongNameB.Guid),
					adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole });

				var databases = Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef));

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				if (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer)
				{
					AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");
					AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");
				}

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");

				Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);

				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithUserNamePrefix);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithNoUserNamePrefix);

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						if (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer)
						{
							DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");
							DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");
						}

						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
						DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");

						Helper.DropDatabasePrincipals(adminConnection);

						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithUserNamePrefix);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithNoUserNamePrefix);
					}
				}

				// Act
				sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, windowsLoginNameWithUserNamePrefix);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, windowsLoginNameWithNoUserNamePrefix);

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, windowsLoginNameWithUserNamePrefix);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, windowsLoginNameWithNoUserNamePrefix);
					}
				}
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrationDisabled: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrationDisabled: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrationDisabled: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrationDisabled: Wise cloud hosted dedicated")]
		public void TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrationDisabled(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			var databases = Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef));

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}

				// Act
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameReader, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameDeveloper, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameBackupOperator, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameHrmStaff, "S");

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameReader, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameDeveloper, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameBackupOperator, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameHrmStaff, "S");
					}
				}
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrated: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrated: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrated: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrated: Wise cloud hosted dedicated")]
		public void TestBuildSecurityCreatesUsersAndLoginsForStaffMembersAfterACallToBuildApplicationLoginsSecurityWhenAdIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, "ADTest_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				var sqlLoginNameForADIntegratedAccountWithUserNamePrefix = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginNameWithUserNamePrefix = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				var sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix = Helper.GetEnterpriseLoginFullName(Helper.ADTestUserLongNameB.Name, Db.DatabaseName);
				var windowsLoginNameWithNoUserNamePrefix = Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000;

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole });

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					Helper.ADTestUserLongNameB.Name,
					TestConstants.Domain,
					Guid.Parse(Helper.ADTestUserLongNameB.Guid),
					adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbDataWriterRole });
				var databases = Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef));

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
				sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithUserNamePrefix);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithNoUserNamePrefix);

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithUserNamePrefix);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginNameWithNoUserNamePrefix);
					}
				}

				// Act
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				if (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer)
				{
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");
					AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");
				}

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");

				foreach (var databaseName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						if (databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer)
						{
							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithUserNamePrefix, "S");
							DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameForADIntegratedAccountWithNoUserNamePrefix, "S");
						}

						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginNameWithUserNamePrefix, "U");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginNameWithNoUserNamePrefix, "U");
					}
				}
			}
		}

		#endregion Testing BuildApplicationLoginsSecurity

		[Test]
		public void TestCanHandleStaffSqlLoginNamesWithSingleQuotes()
		{
			// Arrange
			var staffLoginWithSingleQuote = "_Tst_Name's with single quote";
			var staffLoginWithDoubleSingleQuote = "_Tst_Name''s with double single quotes";

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLoginWithSingleQuote,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.CwRestrictedReaderRole });

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLoginWithDoubleSingleQuote,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.DbBackupOperatorRole });

			var sqlLoginNameWithSingleQuote = Helper.GetEnterpriseLoginFullName(staffLoginWithSingleQuote, Db.DatabaseName);
			var sqlLoginNameWithDoubleSingleQuote = Helper.GetEnterpriseLoginFullName(staffLoginWithDoubleSingleQuote, Db.DatabaseName);

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLoginWithSingleQuote);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLoginWithSingleQuote), Is.GreaterThan(0), $"Staff member '{staffLoginWithSingleQuote}' should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithSingleQuote);
			DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithSingleQuote);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLoginWithDoubleSingleQuote);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLoginWithDoubleSingleQuote), Is.GreaterThan(0), $"Staff member '{staffLoginWithDoubleSingleQuote}' should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithDoubleSingleQuote);
			DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithDoubleSingleQuote);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameWithSingleQuote, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameWithDoubleSingleQuote, "S");

				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameWithSingleQuote, "S");
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameWithDoubleSingleQuote, "S");
			}
		}

		[Test]
		public void TestCanHandleSchemaNamesWithSingleQuotes()
		{
			// Arrange
			var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);
			foreach (var dbName in databases)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					Helper.EnsureSchema(adminConnection, schemaNameWithSingleQuote);
					DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, schemaNameWithSingleQuote);
				}
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, schemaNameWithSingleQuote, "EXECUTE", "SELECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, schemaNameWithSingleQuote, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");
				}

				foreach (var dbName in databases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
					{
						DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, schemaNameWithSingleQuote, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
					}
				}
			}
		}

		[Test]
		public void TestBuildSecurityGrantsCorrectPermissionsOnSchemas()
		{
			// Arrange
			var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

			foreach (var dbName in databases)
			{
				var randomSchemaName = $"RandomSchema_{dbName}";
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					Helper.EnsureSchema(adminConnection, randomSchemaName);
					DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, randomSchemaName);
				}
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			// Act
			sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

			// Assert

			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				var randomSchemaName = $"RandomSchema_{Db.DatabaseName}";
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, randomSchemaName, "EXECUTE", "SELECT");
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, randomSchemaName, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");
			}

			foreach (var dbName in databases)
			{
				var randomSchemaName = $"RandomSchema_{dbName}";
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, randomSchemaName, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
				}
			}
		}

		[Test]
		public void TestBuildSecurityForAllDatabasesGrantsCorrectPermissionsOnSchemas()
		{
			// Arrange
			var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

			foreach (var dbName in databases)
			{
				var randomSchemaName = $"RandomSchema_{dbName}";
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					Helper.EnsureSchema(adminConnection, randomSchemaName);
					DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, randomSchemaName);
				}
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			// Act
			sqlSecurityManager.BuildSecurityForAllDatabases(adminConnection, cancellationTokenSource.Token);

			// Assert

			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				var randomSchemaName = $"RandomSchema_{Db.DatabaseName}";
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, randomSchemaName, "EXECUTE", "SELECT");
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, randomSchemaName, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");
			}

			foreach (var dbName in databases)
			{
				var randomSchemaName = $"RandomSchema_{dbName}";
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, randomSchemaName, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
				}
			}
		}

		[Test]
		public void TestBuildApplicationLoginsSecurityForAllDatabasesGrantsCorrectPermissionsOnSchemas()
		{
			// Arrange
			var databases = adminConnection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

			foreach (var dbName in databases)
			{
				var randomSchemaName = $"RandomSchema_{dbName}";
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					Helper.EnsureSchema(adminConnection, randomSchemaName);
					DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, randomSchemaName);
				}
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			// Act
			sqlSecurityManager.BuildApplicationLoginsSecurity(adminConnection, cancellationTokenSource.Token);

			// Assert

			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				var randomSchemaName = $"RandomSchema_{Db.DatabaseName}";
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, randomSchemaName, "EXECUTE", "SELECT");
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, randomSchemaName, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");
			}

			foreach (var dbName in databases)
			{
				var randomSchemaName = $"RandomSchema_{dbName}";
				using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, randomSchemaName, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
				}
			}
		}

		[Test]
		public void TestCanHandleFunctionNamesWithSingleQuotes()
		{
			// Arrange
			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE FUNCTION dbo.[{functionNameWithSingleQuote}]()
RETURNS int
BEGIN
	RETURN 0
END");
				DatabaseAssertionsHelper.AssumeFunctionExists(adminConnection, "dbo", "FN", functionNameWithSingleQuote);
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllObjectsOfTypeAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "FN", "EXECUTE");
				}
			}
		}

		[Test]
		public void TestCanHandleFunctionNamesWithSingleQuoteBelongingToSchemaWithSingleQuote()
		{
			// Arrange
			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				Helper.EnsureSchema(adminConnection, schemaNameWithSingleQuote);
				adminConnection.ExecuteNonQuery($@"
CREATE FUNCTION [{schemaNameWithSingleQuote}].[{functionNameWithSingleQuote}]()
RETURNS int
BEGIN
	RETURN 0
END");
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, schemaNameWithSingleQuote);
				DatabaseAssertionsHelper.AssumeFunctionExists(adminConnection, schemaNameWithSingleQuote, "FN", functionNameWithSingleQuote);
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllObjectsOfTypeAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "FN", "EXECUTE");

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, schemaNameWithSingleQuote, "EXECUTE", "SELECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, schemaNameWithSingleQuote, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, schemaNameWithSingleQuote, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
				}
			}
		}

		[Test]
		public void TestCanHandleTypeNamesWithSingleQuotes()
		{
			// Arrange
			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				Helper.CreateType(adminConnection, "dbo", typeNameWithSingleQuote);
				DatabaseAssertionsHelper.AssumeTypeExists(adminConnection, "dbo", typeNameWithSingleQuote);
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllCustomTableValueTypesAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "EXECUTE");
				}
			}
		}

		[Test]
		public void TestCanHandleTypeNamesWithSingleQuotesBelongingToSchemaWithSingleQuote()
		{
			// Arrange
			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				Helper.EnsureSchema(adminConnection, schemaNameWithSingleQuote);
				Helper.CreateType(adminConnection, schemaNameWithSingleQuote, typeNameWithSingleQuote);
				DatabaseAssertionsHelper.AssumeSchemaExists(adminConnection, schemaNameWithSingleQuote);
				DatabaseAssertionsHelper.AssumeTypeExists(adminConnection, schemaNameWithSingleQuote, typeNameWithSingleQuote);
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
				{
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnAllCustomTableValueTypesAndNoOther(adminConnection, DbRoleTypes.CwReaderRole, "EXECUTE");

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, schemaNameWithSingleQuote, "EXECUTE", "SELECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, schemaNameWithSingleQuote, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, schemaNameWithSingleQuote, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
				}
			}
		}

		[Test]
		public void TestBuildServerSecurityThrowsExceptionIfEnlistedInTransaction()
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			// Act
			// Assert
			using (new TransactionScope())
			{
				using (var connection = Db.NewAdminConnection())
				{
					Assert.That(() => sqlSecurityManager.BuildServerSecurity(connection, trialRun: false), Throws.InstanceOf<InvalidOperationException>());
				}
			}
		}

		[Test]
		public void TestBuildServerSecurityDoesNotThrowsExceptionIfEnlistedInTransactionWhenAllowsTransaction()
		{
			// Arrange
			using (Globals.TemporaryOverrideForIsTest(true))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase, allowTransaction: true);

				// Act
				// Assert
				using (new TransactionScope())
				{
					using (var connection = Db.NewAdminConnection())
					{
						Assert.That(() => sqlSecurityManager.BuildServerSecurity(connection, trialRun: false), Throws.Nothing);
					}
				}
			}
		}

		[Test]
		public void TestBuildDatabaseSecurityThrowsExceptionIfEnlistedInTransaction()
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			// Act
			// Assert
			using (new TransactionScope())
			{
				using (var connection = Db.NewAdminConnection())
				{
					Assert.That(() => sqlSecurityManager.BuildDatabaseSecurity(connection, Db.DatabaseName, false), Throws.InstanceOf<InvalidOperationException>());
				}
			}
		}

		[Test]
		public void TestBuildServerSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised()
		{
			// Arrange
			var errorReporterMock = new Mock<IErrorReporter>();
			var loginName = $"{Helper.MainDatabaseNameOutsideTestCase}_LoginToDropThatCannotBeDropped";
			try
			{
				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{loginName}')
	CREATE LOGIN [{loginName}] WITH PASSWORD = N'[SOME RAONdom pawd 123]';
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = '_Tst_RoleCannotBeDropped')
	CREATE SERVER ROLE _Tst_RoleCannotBeDropped
ALTER AUTHORIZATION ON SERVER ROLE::[_Tst_RoleCannotBeDropped] TO [{loginName}];
");

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
				ErrorReporter.Clear();
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					// Act
					sqlSecurityManager.BuildServerSecurity(adminConnection, false);

					// Assert
					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.ReportDeveloperExceptionOrHandleSilently(
							$"Synchronization of server '{Db.ServerName}' with main database name '{Db.DatabaseName}' incomplete. There were more commands generated after synchronization.",
							It.IsAny<string>(),
							It.IsAny<Exception>()),
						Times.Once,
						"There should be report once reporting that differences still present.");
				}
			}
			finally
			{
				adminConnection.ExecuteNonQuery($@"
IF EXISTS (SELECT name FROM sys.server_principals WHERE name = '_Tst_RoleCannotBeDropped')
	DROP SERVER ROLE _Tst_RoleCannotBeDropped;
");
			}
		}

		[Test]
		public void TestBuildServerSecurityDoesNotIssueReportOnceForTrialRun()
		{
			// Arrange
			var errorReporterMock = new Mock<IErrorReporter>();
			var loginName = $"{Helper.MainDatabaseNameOutsideTestCase}_LoginToDropThatCannotBeDropped";
			try
			{
				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{loginName}')
	CREATE LOGIN [{loginName}] WITH PASSWORD = N'[SOME RAONdom pawd 123]';
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = '_Tst_RoleCannotBeDropped')
	CREATE SERVER ROLE _Tst_RoleCannotBeDropped
ALTER AUTHORIZATION ON SERVER ROLE::[_Tst_RoleCannotBeDropped] TO [{loginName}];
");

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
				ErrorReporter.Clear();
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					// Act
					sqlSecurityManager.BuildServerSecurity(adminConnection, true);

					// Assert
					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.ReportDeveloperExceptionOrHandleSilently(
							It.Is<string>(message => message == $"Sql security for server '{Db.ServerName}' is not synchronised for system with main database '{Db.DatabaseName}'."),
							It.IsAny<string>(),
							It.IsAny<Exception>()),
						Times.Never());

					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.Report(
							It.Is<string>(message => message == $"Sql security for server '{Db.ServerName}' is not synchronised for system with main database '{Db.DatabaseName}'."),
							It.IsAny<string>(),
							It.IsAny<Exception>()),
						Times.Never());
				}
			}
			finally
			{
				adminConnection.ExecuteNonQuery($@"
IF EXISTS (SELECT name FROM sys.server_principals WHERE name = '_Tst_RoleCannotBeDropped')
	DROP SERVER ROLE _Tst_RoleCannotBeDropped;
");
			}
		}

		[TestCase(DatabaseType.Main, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised: Main database")]
		[TestCase(DatabaseType.Audit, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised: EDW database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised: User repository database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised: SD database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised: Exclusive Ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised: Shared Ref database")]
		public void TestBuildDatabaseSecurityIssuesDeveloperReportOnceIfSomthingLeftUnsynchronised(DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var errorReporterMock = new Mock<IErrorReporter>();
			var loginName = $"{Helper.MainDatabaseNameOutsideTestCase}_LoginMappedToUserThatCannotBeDropped";

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{loginName}] WITH PASSWORD = N'[SOME RAONdom pawd 123]';
");
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				try
				{
					adminConnection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS Test;
DROP USER IF EXISTS [{loginName}];
CREATE USER [{loginName}] FROM LOGIN [{loginName}];
CREATE TABLE Test (val int)
ALTER AUTHORIZATION ON OBJECT::dbo.Test TO [{loginName}];
");
					var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
					ErrorReporter.Clear();
					using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
					{
						// Act
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

						// Assert
						errorReporterMock.Verify(
							errorReporter =>
							errorReporter.ReportDeveloperExceptionOrHandleSilently(
								It.Is<string>(key => key == $"Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised."),
								It.IsAny<string>(),
								null),
							Times.Once());
					}
				}
				finally
				{
					adminConnection.ExecuteNonQuery("DROP TABLE IF EXISTS dbo.Test");
				}
			}
		}

		[TestCase(DatabaseType.Main, TestName = "TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun: Main database")]
		[TestCase(DatabaseType.Audit, TestName = "TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun: EDW database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun: User repository database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun: SD database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun: Ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun: Shared Ref database")]
		public void TestBuildDatabaseSecurityDoesNotIssueDevelperReportOnceForTrialRun(DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var errorReporterMock = new Mock<IErrorReporter>();
			var loginName = $"{Helper.MainDatabaseNameOutsideTestCase}_LoginMappedToUserThatCannotBeDropped";

			adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{loginName}')
	CREATE LOGIN [{loginName}] WITH PASSWORD = N'[SOME RAONdom pawd 123]';
");
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				try
				{
					adminConnection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS Test;
DROP USER IF EXISTS [{loginName}];
CREATE USER [{loginName}] FROM LOGIN [{loginName}];
CREATE TABLE Test (val int)
ALTER AUTHORIZATION ON OBJECT::dbo.test TO [{loginName}];
");
					var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
					ErrorReporter.Clear();
					using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
					{
						// Act
						sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

						// Assert
						errorReporterMock.Verify(
							errorReporter =>
							errorReporter.ReportDeveloperExceptionOrHandleSilently(
								It.Is<string>(key => key == $"Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised."),
								It.IsAny<string>(),
								It.IsAny<Exception>()),
							Times.Never());
					}
				}
				finally
				{
					adminConnection.ExecuteNonQuery("DROP TABLE IF EXISTS dbo.Test");
				}
			}
		}

		[Test]
		public void TestLogsErrorMessageWhenDatabaseNameInconsistentWithMainDatabaseNameIsPassed()
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			var dbNameWithUnmatchedDbType = Db.DatabaseName + $"_{Guid.NewGuid()}";

			// Act
			sqlSecurityManager.BuildDatabaseSecurity(adminConnection, dbNameWithUnmatchedDbType, false);

			// Assert
			loggerMock.VerifyCalled(
				IntegrationLogging.LogType.Error,
				typeof(ArgumentException),
				$"There should be an error log regarding failing to build Sql Security for databse '{dbNameWithUnmatchedDbType}' with an ArgumentException attached.",
				Times.Once(),
				$"Failed to build Sql security for database '{dbNameWithUnmatchedDbType}'.");
		}

		[TestCaseSource(nameof(DatabasesTestCaseSource))]
		public void TestBuildSecurityForAllDatabasesCreatesUsersAndRolesForAllDatabases(IEnumerable<string> databases)
		{
			// Arrange
			foreach (var database in databases)
			{
				adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS {database.QuoteName()};
CREATE DATABASE {database.QuoteName()};
");
			}

			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			var loggerMock = new Mock<IntegrationLogging.ILogger>();

			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);
			var allDatabases = databases.Append(Db.DatabaseName);

			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				foreach (var databaseName in allDatabases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}

				using (((ICurrentDbControl)adminConnection).UseDatabase(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					var findGuestPermissions = @"
FROM
	sys.database_permissions     AS dp
	JOIN sys.database_principals AS grantee ON dp.grantee_principal_id = grantee.principal_id
		AND grantee.name = N'guest'
";

					Assume.That(adminConnection.Exists(findGuestPermissions), Is.False, "There should be no permissions granted to guest principal before building security.");
				}

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {dbReaderLogin.QuoteName()} WITH PASSWORD = N'{testReaderCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin)};
CREATE LOGIN {dbWriterLogin.QuoteName()} WITH PASSWORD = N'{testWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin)};
CREATE LOGIN {dbRestrictedReaderLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin)};
CREATE LOGIN {dbRestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin)};
CREATE LOGIN {dbUnrestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testUnrestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin)};

CREATE LOGIN {sqlLoginNameReader.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
CREATE LOGIN {sqlLoginNameDeveloper.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
CREATE LOGIN {sqlLoginNameBackupOperator.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
CREATE LOGIN {sqlLoginNameHrmStaff.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
");

				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, dbReaderLogin, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, dbWriterLogin, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, dbRestrictedReaderLogin, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, dbRestrictedWriterLogin, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, dbUnrestrictedWriterLogin, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameReader, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameDeveloper, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameBackupOperator, "S");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameHrmStaff, "S");

				// Act
				sqlSecurityManager.BuildSecurityForAllDatabases(adminConnection, cancellationTokenSource.Token);

				// Assert
				foreach (var databaseName in allDatabases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbReaderLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbRestrictedReaderLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbRestrictedWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbUnrestrictedWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwHRMStaffRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwReaderRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameReader, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameDeveloper, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameBackupOperator, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameHrmStaff, "S");
					}

					loggerMock.Verify(logger => logger.Log(IntegrationLogging.LogType.Information, $"Building Sql security for database '{databaseName}'."), Times.Once());
				}
			}

			using (((ICurrentDbControl)adminConnection).UseDatabase(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
			{
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, "guest", "SELECT", "EXECUTE", "CONNECT");
			}
		}

		[TestCaseSource(nameof(DatabasesTestCaseSource))]
		public void TestBuildSecurityCreatesAllLoginsAndUsersAndRolesForAllDatabases(IEnumerable<string> databases)
		{
			// Arrange
			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			foreach (var database in databases)
			{
				adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS {database.QuoteName()};
CREATE DATABASE {database.QuoteName()};
");
			}

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

			var allDatabases = databases.Append(Db.DatabaseName);

			foreach (var databaseName in allDatabases)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}
			}

			using (((ICurrentDbControl)adminConnection).UseDatabase(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
			{
				var findGuestPermissions = @"
FROM
	sys.database_permissions     AS dp
	JOIN sys.database_principals AS grantee ON dp.grantee_principal_id = grantee.principal_id
		AND grantee.name = N'guest'
";

				Assume.That(adminConnection.Exists(findGuestPermissions), Is.False, "There should be no permissions granted to guest principal before building security.");
			}

			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbReaderLogin, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbWriterLogin, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbRestrictedReaderLogin, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbRestrictedWriterLogin, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, dbUnrestrictedWriterLogin, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameReader, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameDeveloper, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameBackupOperator, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameHrmStaff, "S");

				foreach (var databaseName in allDatabases)
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbReaderLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbRestrictedReaderLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbRestrictedWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, dbUnrestrictedWriterLogin, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwHRMStaffRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwReaderRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwRestrictedWriterRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, "R");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameReader, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameDeveloper, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameBackupOperator, "S");
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameHrmStaff, "S");
					}
				}
			}

			using (((ICurrentDbControl)adminConnection).UseDatabase(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
			{
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabaseOnly(adminConnection, "guest", "SELECT", "EXECUTE", "CONNECT");
			}
		}

		[Test]
		public void TestBuildSecuritySyncrhoisesWritableDatabasesButSkipsReadonlyDatabase()
		{
			// Assert
			var sdDatabaseName = Helper.DatabaseNameFromDatabaseType(DatabaseType.SD);
			var allDatabases = Helper.Databases();

			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			foreach (var database in allDatabases)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(database))
				{
					DatabaseAssertionsHelper.AssumePrincipalsMissing(
						adminConnection,
						dbReaderLogin,
						dbWriterLogin,
						dbRestrictedReaderLogin,
						dbRestrictedWriterLogin,
						sqlLoginNameReader,
						sqlLoginNameDeveloper,
						sqlLoginNameBackupOperator,
						sqlLoginNameHrmStaff,
						DbRoleTypes.CwHRMStaffRole,
						DbRoleTypes.CwReaderRole,
						DbRoleTypes.CwRestrictedReaderRole,
						DbRoleTypes.CwRestrictedWriterRole,
						DbRoleTypes.CwUnrestrictedWriterRole);
				}
			}

			adminConnection.AlterDbWriteableState(sdDatabaseName, writeable: false);

			// Act
			ErrorReporter.Clear();
			using (ErrorReporter.SetTemporaryInstanceForTest(Mock.Of<IErrorReporter>()))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				foreach (var database in allDatabases.Except(sdDatabaseName))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
					{
						DatabaseAssertionsHelper.AssertSqlUsersExist(
						adminConnection,
						dbReaderLogin,
						dbWriterLogin,
						dbRestrictedReaderLogin,
						dbRestrictedWriterLogin,
						sqlLoginNameReader,
						sqlLoginNameDeveloper,
						sqlLoginNameBackupOperator,
						sqlLoginNameHrmStaff);

						DatabaseAssertionsHelper.AssertRolesExist(
						adminConnection,
						DbRoleTypes.CwHRMStaffRole,
						DbRoleTypes.CwReaderRole,
						DbRoleTypes.CwRestrictedReaderRole,
						DbRoleTypes.CwRestrictedWriterRole,
						DbRoleTypes.CwUnrestrictedWriterRole);
					}
				}

				using (((ICurrentDbControl)adminConnection).UseDatabase(sdDatabaseName))
				{
					DatabaseAssertionsHelper.AssertPrincipalsMissing(
						adminConnection,
						dbReaderLogin,
						dbWriterLogin,
						dbRestrictedReaderLogin,
						dbRestrictedWriterLogin,
						sqlLoginNameReader,
						sqlLoginNameDeveloper,
						sqlLoginNameBackupOperator,
						sqlLoginNameHrmStaff,
						DbRoleTypes.CwHRMStaffRole,
						DbRoleTypes.CwReaderRole,
						DbRoleTypes.CwRestrictedReaderRole,
						DbRoleTypes.CwRestrictedWriterRole,
						DbRoleTypes.CwUnrestrictedWriterRole);
				}
			}
		}

		[Test]
		public void TestBuildSecurityForAllDatabasesSyncrhoisesWritableDatabasesButSkipsReadonlyDatabase()
		{
			// Arrange
			var sdDatabaseName = Helper.DatabaseNameFromDatabaseType(DatabaseType.SD);
			var allDatabases = Helper.Databases();

			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			foreach (var database in allDatabases)
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(database))
				{
					DatabaseAssertionsHelper.AssumePrincipalsMissing(
						adminConnection,
						dbReaderLogin,
						dbWriterLogin,
						dbRestrictedReaderLogin,
						dbRestrictedWriterLogin,
						sqlLoginNameReader,
						sqlLoginNameDeveloper,
						sqlLoginNameBackupOperator,
						sqlLoginNameHrmStaff,
						DbRoleTypes.CwHRMStaffRole,
						DbRoleTypes.CwReaderRole,
						DbRoleTypes.CwRestrictedReaderRole,
						DbRoleTypes.CwRestrictedWriterRole,
						DbRoleTypes.CwUnrestrictedWriterRole);
				}
			}

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {dbReaderLogin.QuoteName()} WITH PASSWORD = N'{testReaderCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin)};
CREATE LOGIN {dbWriterLogin.QuoteName()} WITH PASSWORD = N'{testWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin)};
CREATE LOGIN {dbRestrictedReaderLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin)};
CREATE LOGIN {dbRestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin)};
CREATE LOGIN {dbUnrestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testUnrestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin)};

CREATE LOGIN {sqlLoginNameReader.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
CREATE LOGIN {sqlLoginNameDeveloper.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
CREATE LOGIN {sqlLoginNameBackupOperator.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
CREATE LOGIN {sqlLoginNameHrmStaff.QuoteName()} WITH PASSWORD = N'[]12SOMErandom';
");

			adminConnection.AlterDbWriteableState(sdDatabaseName, writeable: false);

			// Act
			ErrorReporter.Clear();
			using (ErrorReporter.SetTemporaryInstanceForTest(Mock.Of<IErrorReporter>()))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				sqlSecurityManager.BuildSecurityForAllDatabases(adminConnection, cancellationTokenSource.Token);

				// Assert
				foreach (var database in allDatabases.Except(sdDatabaseName))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
					{
						DatabaseAssertionsHelper.AssertSqlUsersExist(
						adminConnection,
						dbReaderLogin,
						dbWriterLogin,
						dbRestrictedReaderLogin,
						dbRestrictedWriterLogin,
						sqlLoginNameReader,
						sqlLoginNameDeveloper,
						sqlLoginNameBackupOperator,
						sqlLoginNameHrmStaff);

						DatabaseAssertionsHelper.AssertRolesExist(
						adminConnection,
						DbRoleTypes.CwHRMStaffRole,
						DbRoleTypes.CwReaderRole,
						DbRoleTypes.CwRestrictedReaderRole,
						DbRoleTypes.CwRestrictedWriterRole,
						DbRoleTypes.CwUnrestrictedWriterRole);
					}
				}

				using (((ICurrentDbControl)adminConnection).UseDatabase(sdDatabaseName))
				{
					DatabaseAssertionsHelper.AssertPrincipalsMissing(
						adminConnection,
						dbReaderLogin,
						dbWriterLogin,
						dbRestrictedReaderLogin,
						dbRestrictedWriterLogin,
						sqlLoginNameReader,
						sqlLoginNameDeveloper,
						sqlLoginNameBackupOperator,
						sqlLoginNameHrmStaff,
						DbRoleTypes.CwHRMStaffRole,
						DbRoleTypes.CwReaderRole,
						DbRoleTypes.CwRestrictedReaderRole,
						DbRoleTypes.CwRestrictedWriterRole,
						DbRoleTypes.CwUnrestrictedWriterRole);
				}
			}
		}

		[TestCase(DatabaseType.Main, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException: Main database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException: SD database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException: User repository database")]
		[TestCase(DatabaseType.Audit, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException: Edw database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException: Exclusive ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException: Shared ref database")]
		public void TestBuildDatabaseSecurityIssuesDeveloperReportOnceAndAnErrorLogIfSynchroniserThrowsNonCriticalException(DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var sqlDbSecuritySynchroniserProviderMock = new Mock<ISqlDbSecuritySynchroniserProvider>();
			var serverSynchroniserMock = new Mock<IDifferenceSynchroniser>();
			var databaseSynchroniserMock = new Mock<IDifferenceSynchroniser>();

			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetServerSynchroniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(serverSynchroniserMock.Object);
			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetDatabaseSyncrhoniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(databaseSynchroniserMock.Object);

			serverSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Returns(true);

			var exception = new Exception("Exception message");
			databaseSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Throws(exception);

			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, sqlDbSecuritySynchroniserProviderMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				loggerMock.Verify(l => l.Log(IntegrationLogging.LogType.Error, $"Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised."));

				errorReporterMock.Verify(
					errorReporter =>
					errorReporter.ReportDeveloperExceptionOrHandleSilently(
						It.Is<string>(message => message == $"Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised."),
						It.IsAny<string>(),
						exception),
					Times.Once(),
					$"There should be a report once issued with the key: 'Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised.'.");
			}
		}

		[TestCase(DatabaseType.Main, TestName = "TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException: Main database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException: SD database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException: User repository database")]
		[TestCase(DatabaseType.Audit, TestName = "TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException: Edw database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException: Exclusive ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException: Shared ref database")]
		public void TestBuildDatabaseSecurityLogsProposedValuesIfSynchroniserThrowsNonCriticalException(DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var sqlDbSecuritySynchroniserProviderMock = new Mock<ISqlDbSecuritySynchroniserProvider>();
			var serverSynchroniserMock = new Mock<IDifferenceSynchroniser>();
			var databaseSynchroniserMock = new Mock<IDifferenceSynchroniser>();

			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetServerSynchroniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(serverSynchroniserMock.Object);
			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetDatabaseSyncrhoniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(databaseSynchroniserMock.Object);

			serverSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Returns(true);

			var exception = new Exception("Exception message");
			databaseSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Throws(exception);

			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, sqlDbSecuritySynchroniserProviderMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert

				loggerMock.VerifyCalled(
					IntegrationLogging.LogType.Debug,
					$"Database '{databaseName}' permissions proposed values should be included in the Debug log",
					Times.Once(),
					$"Expected permissions for database '{databaseName}'");

				loggerMock.VerifyCalled(
					IntegrationLogging.LogType.Debug,
					$"Database '{databaseName}' principals and memberships proposed values should be included in the Debug log",
					Times.Once(),
					$"Expected principals and memberships for database '{databaseName}'");
			}
		}

		[Test]
		public void TestBuildSecurityDoesNotLogsProposedValuesIfSynchroniserDoesNotThrowException()
		{
			// Arrange
			var sqlDbSecuritySynchroniserProviderMock = new Mock<ISqlDbSecuritySynchroniserProvider>();
			var serverSynchroniserMock = new Mock<IDifferenceSynchroniser>();
			var databaseSynchroniserMock = new Mock<IDifferenceSynchroniser>();

			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetServerSynchroniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(serverSynchroniserMock.Object);
			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetDatabaseSyncrhoniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(databaseSynchroniserMock.Object);
			serverSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Returns(It.IsAny<bool>());

			databaseSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Returns(It.IsAny<bool>());

			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, sqlDbSecuritySynchroniserProviderMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				loggerMock.VerifyCalled(
					It.IsAny<IntegrationLogging.LogType>(),
					"Proposed server principal and memberships values should not be included in the log",
					Times.Never(),
					$"Expected principals and memberships on server '{Db.ServerName}' for system with main database '{Db.DatabaseName}'");

				loggerMock.VerifyCalled(
					It.IsAny<IntegrationLogging.LogType>(),
					"Proposed server permissions values should not be included in the log",
					Times.Never(),
					$"Expected permissions on server '{Db.ServerName}' for system with main database '{Db.DatabaseName}'");

				foreach (var databaseName in Helper.Databases())
				{
					loggerMock.VerifyCalled(
						It.IsAny<IntegrationLogging.LogType>(),
						$"Proposed database '{databaseName}' principals and memberships values should not be included in the log",
						Times.Never(),
						$"Expected principals and memberships for database '{databaseName}'");

					loggerMock.VerifyCalled(
						It.IsAny<IntegrationLogging.LogType>(),
						$"Proposed database '{databaseName}' permissions values should not be included in the log",
						Times.Never(),
						$"Expected permissions for database '{databaseName}'");
				}
			}
		}

		[Test]
		public void TestBuildSecurityForAllDatabasesIssuesDeveloperReportOnceAndAnErrorLogIfSyncrhoniserThrowsNonCriticalException()
		{
			// Arrange
			var sqlDbSecuritySynchroniserProviderMock = new Mock<ISqlDbSecuritySynchroniserProvider>();
			var serverSynchroniserMock = new Mock<IDifferenceSynchroniser>();
			var databaseSynchroniserMock = new Mock<IDifferenceSynchroniser>();

			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetServerSynchroniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(serverSynchroniserMock.Object);
			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetDatabaseSyncrhoniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(databaseSynchroniserMock.Object);

			serverSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Returns(true);

			var exception = new Exception("Exception message");
			databaseSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Throws(exception);

			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, sqlDbSecuritySynchroniserProviderMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();

			ErrorReporter.Clear();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildSecurityForAllDatabases(adminConnection, cancellationTokenSource.Token);

				// Assert
				foreach (var databaseName in Helper.Databases())
				{
					loggerMock.Verify(l => l.Log(IntegrationLogging.LogType.Error, $"Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised."));

					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.ReportDeveloperExceptionOrHandleSilently(
							It.Is<string>(message => message == $"Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised."),
							It.IsAny<string>(),
							exception),
						Times.Once(),
						$"There should be a report once issued with the key: 'Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised.'.");
				}
			}
		}

		[Test]
		public void TestBuildSecurityForAllDatabasesLogsProposedValuesIfSyncrhoniserThrowsNonCriticalException()
		{
			// Arrange
			var sqlDbSecuritySynchroniserProviderMock = new Mock<ISqlDbSecuritySynchroniserProvider>();
			var serverSynchroniserMock = new Mock<IDifferenceSynchroniser>();
			var databaseSynchroniserMock = new Mock<IDifferenceSynchroniser>();

			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetServerSynchroniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(serverSynchroniserMock.Object);
			sqlDbSecuritySynchroniserProviderMock
				.Setup(provider => provider.GetDatabaseSyncrhoniser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<ILogger>()))
				.Returns(databaseSynchroniserMock.Object);

			serverSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Returns(true);

			var exception = new Exception("Exception message");
			databaseSynchroniserMock.Setup(synchroniser => synchroniser.Synchronise(It.IsAny<SqlConnection>(), false))
				.Throws(exception);

			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, sqlDbSecuritySynchroniserProviderMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			var errorReporterMock = new Mock<IErrorReporter>();

			ErrorReporter.Clear();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildSecurityForAllDatabases(adminConnection, cancellationTokenSource.Token);

				// Assert
				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					loggerMock.VerifyCalled(
						IntegrationLogging.LogType.Debug,
						$"Database '{databaseName}' permissions proposed values should be included in the log",
						Times.Once(),
						$"Expected permissions for database '{databaseName}'");

					loggerMock.VerifyCalled(
						IntegrationLogging.LogType.Debug,
						$"Database '{databaseName}' principals and memberships proposed values should be included in the log",
						Times.Once(),
						$"Expected principals and memberships for database '{databaseName}'");
				}

				loggerMock.VerifyCalled(
					IntegrationLogging.LogType.Debug,
					$"Database '{Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)}' permissions proposed values should be included in the log",
					Times.Once(),
					$"Expected permissions for database '{Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)}'");
			}
		}

		#region Timer log tests

		[Test]
		public void TestBuildSecurityIssuesLogToReportTimeToGetProposedValues()
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			// Act
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				loggerMock.Verify(
					l => l.Log(
						IntegrationLogging.LogType.Information,
						It.Is<string>(message => message.StartsWith($"Getting proposed values for server '{Db.ServerName}' with main database name '{Db.DatabaseName}' took"))));

				foreach (var databaseName in new[]
				{
					Db.DatabaseName,
					Helper.DatabaseNameFromDatabaseType(DatabaseType.SD),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef),
				})
				{
					loggerMock.Verify(
						l => l.Log(
							IntegrationLogging.LogType.Information,
							It.Is<string>(message => message.StartsWith($"Getting proposed values for database '{databaseName}' took"))),
						$"There should be an information log issued for database '{databaseName}', about the time it took to get proposed values for that database.");
				}
			}
		}

		[Test]
		public void TestBuildSecurityIssuesLogToReportTotalTimeToBuildSecurityPerServerAndDatabase()
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			// Act
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				loggerMock.Verify(
					l => l.Log(
						IntegrationLogging.LogType.Information,
						It.Is<string>(message => message.StartsWith($"Building Sql security for server '{Db.ServerName}' took"))));

				foreach (var databaseName in new[]
				{
					Db.DatabaseName,
					Helper.DatabaseNameFromDatabaseType(DatabaseType.SD),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef),
					Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef),
				})
				{
					loggerMock.Verify(
						l => l.Log(
							IntegrationLogging.LogType.Information,
							It.Is<string>(message => message.StartsWith($"Building Sql security for database '{databaseName}' took"))),
						$"There should be an information log issued for database '{databaseName}', about the time it took to build security for that database.");
				}
			}
		}

		#endregion Timer log tests

		[TestCase(DatabaseType.Main, TestName = "TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce: Main database")]
		[TestCase(DatabaseType.Audit, TestName = "TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce: EDW database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce: User repository database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce: SD database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce: Exclusive Ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce: Shared Ref database")]
		public void TestBuildSecurityProvidesServerInfoSynchronisationInformationInTheLogWhenDatabaseIssuesDeveloperReportOnce(DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var errorReporterMock = new Mock<IErrorReporter>();
			var userThatCannotBeDropped = $"{Db.DatabaseName}_UserThatCannotBeDropped";

			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				try
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{userThatCannotBeDropped}] WITH PASSWORD=N'SOmerANDOM[]pASSW1223D';
DROP TABLE IF EXISTS Test;
DROP USER IF EXISTS [{userThatCannotBeDropped}];
CREATE USER [{userThatCannotBeDropped}] FROM LOGIN [{userThatCannotBeDropped}];
CREATE TABLE Test (val int)
ALTER AUTHORIZATION ON OBJECT::dbo.Test TO [{userThatCannotBeDropped}];
");
					var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

					ErrorReporter.Clear();

					using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
					{
						// Act
						sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

						// Assert
						errorReporterMock.Verify(
							errorReporter =>
							errorReporter.ReportDeveloperExceptionOrHandleSilently(
								It.Is<string>(key => key == $"Sql security for database '{databaseName}' on server '{Db.ServerName}' is not synchronised."),
								It.Is<string>(message => message.Contains($"Building Sql security for server '{Db.ServerName}' with main database name '{Db.DatabaseName}'.")),
								null),
							Times.Once());
					}
				}
				finally
				{
					adminConnection.ExecuteNonQuery("DROP TABLE IF EXISTS dbo.Test");
				}
			}
		}

		[Test]
		public void TestNoExceptionIsThrownButDeveloperReportOnceIssuedIfUserThatShouldBeDroppedIsLoggedInToSql()
		{
			// Arrange
			using (var adminConnection = Db.NewAdminConnection())
			{
				var loginName = "StaffName";
				var password = "boo";
				var loggerMock = new Mock<IntegrationLogging.ILogger>();
				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

				var staffSqlLoginName = "EnterpriseDbUser_" + Db.DatabaseName + "_" + loginName;
				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.SqlMasterDb))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {staffSqlLoginName.QuoteName()} WITH PASSWORD = N'{password}', DEFAULT_DATABASE = {Db.DatabaseName.QuoteName()}, CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF;
");
				}

				adminConnection.ExecuteNonQuery($@"
CREATE USER {staffSqlLoginName.QuoteName()} FROM LOGIN {staffSqlLoginName.QuoteName()};
GRANT CONNECT TO {staffSqlLoginName.QuoteName()};
");

				var errorReporterMock = new Mock<IErrorReporter>();
				ErrorReporter.Clear();
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					//LOGIN USING STAFF CREDENTIALS
					using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, staffSqlLoginName, password))
					{
						connection.ExecuteNonQuery("SELECT 'HI'");

						// Act/Assert
						Assert.That(
							() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token),
							Throws.Nothing,
							"No exception should be thrown");

						errorReporterMock.Verify(
							errorReporter =>
							errorReporter.ReportDeveloperExceptionOrHandleSilently(
								$"Synchronization of server '{Db.ServerName}' with main database name '{Db.DatabaseName}' incomplete. There were more commands generated after synchronization.",
								It.IsAny<string>(),
								It.IsAny<Exception>()),
							Times.Once,
							"There should be report once reporting that differences still present.");
					}
				}
			}
		}

		[Test]
		public void TestBuildSecurityDoesNotCauseErrorsWhenDbDatabaseNameCaseIsIncorrect()
		{
			// Arrange
			try
			{
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(System.Environment.MachineName, Helper.MainDatabaseNameOutsideTestCase.ToUpper());

				var errorReporterMock = new Mock<IErrorReporter>();
				var loggerMock = new Mock<IntegrationLogging.ILogger>();

				Assume.That(
					adminConnection.Exists(
						$"FROM sys.databases WHERE name = @databaseName COLLATE {Db.DatabaseCaseSensitiveCollation}",
						cmd => cmd.AddParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128, Helper.MainDatabaseNameOutsideTestCase)),
					Is.True,
					$"Database with name '{Helper.MainDatabaseNameOutsideTestCase}' should exist (case sensitive).");

				Assume.That(
					adminConnection.Exists(
						$"FROM sys.databases WHERE name = @databaseName COLLATE {Db.DatabaseCaseSensitiveCollation}",
						cmd => cmd.AddParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128, Db.DatabaseName)),
					Is.False,
					$"Database with name '{Db.DatabaseName}' should not exist (case sensitive).");

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

				ErrorReporter.Clear();

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					// Act/Assert
					Assert.That(
						() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token),
						Throws.Nothing);

					loggerMock.Verify(
						logger => logger.Log(
							IntegrationLogging.LogType.Error,
							It.IsAny<string>(),
							It.IsAny<Exception>()),
						Times.Never);

					loggerMock.Verify(
						logger => logger.Log(
							IntegrationLogging.LogType.Error,
							It.IsAny<string>(),
							null),
						Times.Never);

					loggerMock.Verify(
						logger => logger.Log(
							IntegrationLogging.LogType.Warning,
							It.IsAny<string>(),
							It.IsAny<Exception>()),
						Times.Never);

					loggerMock.Verify(
						logger => logger.Log(
							IntegrationLogging.LogType.Warning,
							It.IsAny<string>(),
							null),
						Times.Never);

					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.Report(
							It.IsAny<string>(),
							It.IsAny<string>(),
							null),
						Times.Never());

					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.Report(
							It.IsAny<string>(),
							It.IsAny<string>(),
							It.IsAny<Exception>()),
						Times.Never());

					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.ReportDeveloperExceptionOrHandleSilently(
							It.IsAny<string>(),
							It.IsAny<string>(),
							null),
						Times.Never());

					errorReporterMock.Verify(
						errorReporter =>
						errorReporter.ReportDeveloperExceptionOrHandleSilently(
							It.IsAny<string>(),
							It.IsAny<string>(),
							It.IsAny<Exception>()),
						Times.Never());
				}
			}
			finally
			{
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(System.Environment.MachineName, Helper.MainDatabaseNameOutsideTestCase);
			}
		}

		[TestCase(DatabaseType.Main, TestName = "TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect: Main database")]
		[TestCase(DatabaseType.Audit, TestName = "TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect: EDW database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect: User repository database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect: SD database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect: Exclusive Ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect: Shared Ref database")]
		public void TestBuildDatabaseSecurityDoesNotCauseErrorsWhenDatabaseNameCaseIsIncorrect(DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType).ToUpper();
			var errorReporterMock = new Mock<IErrorReporter>();
			var loggerMock = new Mock<IntegrationLogging.ILogger>();

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {dbReaderLogin.QuoteName()} WITH PASSWORD = N'{testReaderCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin)};
CREATE LOGIN {dbWriterLogin.QuoteName()} WITH PASSWORD = N'{testWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin)};
CREATE LOGIN {dbRestrictedReaderLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin)};
CREATE LOGIN {dbRestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin)};
CREATE LOGIN {dbUnrestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testUnrestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin)};");

			Assume.That(
				adminConnection.Exists(
					$"FROM sys.databases WHERE name = @databaseName COLLATE {Db.DatabaseCaseSensitiveCollation}",
					cmd => cmd.AddParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128, Helper.DatabaseNameFromDatabaseType(databaseType))),
				Is.True,
				$"Database with name '{Helper.DatabaseNameFromDatabaseType(databaseType)}' should exist (case sensitive).");

			Assume.That(
				adminConnection.Exists(
					$"FROM sys.databases WHERE name = @databaseName COLLATE {Db.DatabaseCaseSensitiveCollation}",
					cmd => cmd.AddParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128, databaseName)),
				Is.False,
				$"Database with name '{databaseName}' should not exist (case sensitive).");

			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			ErrorReporter.Clear();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act/Assert
				Assert.That(
					() => sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false),
					Throws.Nothing);

				loggerMock.Verify(
					logger => logger.Log(
						IntegrationLogging.LogType.Error,
						It.IsAny<string>(),
						It.IsAny<Exception>()),
					Times.Never);

				loggerMock.Verify(
					logger => logger.Log(
						IntegrationLogging.LogType.Error,
						It.IsAny<string>(),
						null),
					Times.Never);

				loggerMock.Verify(
					logger => logger.Log(
						IntegrationLogging.LogType.Warning,
						It.IsAny<string>(),
						It.IsAny<Exception>()),
					Times.Never);

				loggerMock.Verify(
					logger => logger.Log(
						IntegrationLogging.LogType.Warning,
						It.IsAny<string>(),
						null),
					Times.Never);

				errorReporterMock.Verify(
					errorReporter =>
					errorReporter.Report(
						It.IsAny<string>(),
						It.IsAny<string>(),
						null),
					Times.Never());

				errorReporterMock.Verify(
					errorReporter =>
					errorReporter.Report(
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<Exception>()),
					Times.Never());

				errorReporterMock.Verify(
					errorReporter =>
					errorReporter.ReportDeveloperExceptionOrHandleSilently(
						It.IsAny<string>(),
						It.IsAny<string>(),
						null),
					Times.Never());

				errorReporterMock.Verify(
					errorReporter =>
					errorReporter.ReportDeveloperExceptionOrHandleSilently(
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<Exception>()),
					Times.Never());
			}
		}

		[Test]
		public void TestDatabaseRoleNamesAreCreatedInCamelCase()
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			var roleNameSql = $"SELECT name FROM sys.database_principals WHERE name = @roleName COLLATE {Db.DatabaseCaseSensitiveCollation}";
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						adminConnection.ExecuteNonQuery(@"
DROP ROLE IF EXISTS cwReaderRole;
DROP ROLE IF EXISTS cwRestrictedReaderRole;
DROP ROLE IF EXISTS cwHRMStaffRole;
DROP ROLE IF EXISTS cwRestrictedWriterRole;
DROP ROLE IF EXISTS cwUnrestrictedWriterRole;
");
					}
				}

				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwReaderRole")), Is.Null);
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwReaderRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedReaderRole")), Is.Null);
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedReaderRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedWriterRole")), Is.Null);
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedWriterRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwUnrestrictedWriterRole")), Is.Null);
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwHRMStaffRole")), Is.Null);
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwHRMStaffRole")), Is.Null);

				// Act

				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwReaderRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwReaderRole")), Is.EqualTo("cwReaderRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedReaderRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedReaderRole")), Is.EqualTo("cwRestrictedReaderRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedWriterRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedWriterRole")), Is.EqualTo("cwRestrictedWriterRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwUnrestrictedWriterRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole")), Is.EqualTo("cwUnrestrictedWriterRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwHRMStaffRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwHRMStaffRole")), Is.EqualTo("cwHRMStaffRole"));
					}
				}
			}
		}

		[Test]
		public void TestDatabaseRoleNamesAreChangedToCamelCase()
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			var roleNameSql = $"SELECT name FROM sys.database_principals WHERE name = @roleName COLLATE {Db.DatabaseCaseSensitiveCollation}";
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						adminConnection.ExecuteNonQuery(@"
DROP ROLE IF EXISTS cwReaderRole;
DROP ROLE IF EXISTS cwRestrictedReaderRole;
DROP ROLE IF EXISTS cwRestrictedWriterRole;
DROP ROLE IF EXISTS cwUnrestrictedWriterRole;
DROP ROLE IF EXISTS cwHRMStaffRole;

CREATE ROLE CwReaderRole;
CREATE ROLE CwRestrictedReaderRole;
CREATE ROLE CwRestrictedWriterRole;
CREATE ROLE CwUnrestrictedWriterRole;
CREATE ROLE CwHRMStaffRole;
");
					}
				}

				Assume.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwReaderRole")), Is.EqualTo("CwReaderRole"));
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwReaderRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedReaderRole")), Is.EqualTo("CwRestrictedReaderRole"));
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedReaderRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedWriterRole")), Is.EqualTo("CwRestrictedWriterRole"));
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedWriterRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwUnrestrictedWriterRole")), Is.EqualTo("CwUnrestrictedWriterRole"));
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole")), Is.Null);

				Assume.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwHRMStaffRole")), Is.EqualTo("CwHRMStaffRole"));
				Assume.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwHRMStaffRole")), Is.Null);

				// Act

				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwReaderRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwReaderRole")), Is.EqualTo("cwReaderRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedReaderRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedReaderRole")), Is.EqualTo("cwRestrictedReaderRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwRestrictedWriterRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwRestrictedWriterRole")), Is.EqualTo("cwRestrictedWriterRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwUnrestrictedWriterRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwUnrestrictedWriterRole")), Is.EqualTo("cwUnrestrictedWriterRole"));

						Assert.That(adminConnection.ExecuteScalar(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "CwHRMStaffRole")), Is.Null);
						Assert.That(adminConnection.ExecuteScalar<string>(roleNameSql, cmd => cmd.AddParameter("@roleName", SqlDbType.NVarChar, 128, "cwHRMStaffRole")), Is.EqualTo("cwHRMStaffRole"));
					}
				}
			}
		}

		[Test]
		public void TestStaffSqlLoginAndUserNameChangesCaseWhenStaffRecordLoginNameChangesCase()
		{
			// Arrange
			var originalStaffName1 = "_Tst_Name1";
			var originalStaffName2 = "_Tst_Name2";
			var staffName1WithSomeLettersChangedToLowerCase = "_Tst_name1";
			var staffName2WithSomeLettersChangedToUpperCase = "_Tst_NaMe2";

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				originalStaffName1,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.CwRestrictedReaderRole });

			var sqlLoginName1 = Helper.GetEnterpriseLoginFullName(originalStaffName1, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				originalStaffName2,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.CwRestrictedReaderRole });

			var sqlLoginName2 = Helper.GetEnterpriseLoginFullName(originalStaffName2, Db.DatabaseName);

			var sqlLoginName1ExpectedAfterChange = Helper.GetEnterpriseLoginFullName(staffName1WithSomeLettersChangedToLowerCase, Db.DatabaseName);
			var sqlLoginName2ExpectedAfterChange = Helper.GetEnterpriseLoginFullName(staffName2WithSomeLettersChangedToUpperCase, Db.DatabaseName);

			AssertionsHelper.AssumeStaffExists(adminConnection, originalStaffName1);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, originalStaffName1), Is.GreaterThan(0), $"Staff member '{originalStaffName1}' should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName1);

			AssertionsHelper.AssumeStaffExists(adminConnection, originalStaffName2);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, originalStaffName2), Is.GreaterThan(0), $"Staff member '{originalStaffName2}' should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName2);

			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				var sqlCaseSensitiveLoginComparison = "SELECT name FROM sys.server_principals WHERE name = @principalName COLLATE Latin1_General_CS_AS AND is_disabled = 0";
				var sqlCaseSensitiveUserComparison = "SELECT name FROM sys.database_principals WHERE name = @principalName COLLATE Latin1_General_CS_AS";

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);
				var sqlLoginName1FromServer = adminConnection.ExecuteScalar<string>(
					sqlCaseSensitiveLoginComparison,
					cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName1));
				Assume.That(sqlLoginName1FromServer, Is.EqualTo(sqlLoginName1));

				var sqlLoginName2FromServer = adminConnection.ExecuteScalar<string>(
					sqlCaseSensitiveLoginComparison,
					cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName2));
				Assume.That(sqlLoginName2FromServer, Is.EqualTo(sqlLoginName2));

				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						var sqlUserName1FromServer = adminConnection.ExecuteScalar<string>(
							sqlCaseSensitiveUserComparison,
							cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName1));
						Assume.That(sqlUserName1FromServer, Is.EqualTo(sqlLoginName1));

						var sqlUserName2FromServer = adminConnection.ExecuteScalar<string>(
							sqlCaseSensitiveUserComparison,
							cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName2));
						Assume.That(sqlUserName2FromServer, Is.EqualTo(sqlLoginName2));
					}
				}

				adminConnection.ExecuteNonQuery(
					$@"
UPDATE GlbStaff SET GS_LoginName = @newStaffName1 WHERE GS_LoginName = @staffName1;
UPDATE GlbStaff SET GS_LoginName = @newStaffName2 WHERE GS_LoginName = @staffName2;
",
					cmd =>
					{
						cmd.AddParameter("@staffName1", System.Data.SqlDbType.NVarChar, 128, originalStaffName1);
						cmd.AddParameter("@newStaffName1", System.Data.SqlDbType.NVarChar, 128, staffName1WithSomeLettersChangedToLowerCase);

						cmd.AddParameter("@staffName2", System.Data.SqlDbType.NVarChar, 128, originalStaffName2);
						cmd.AddParameter("@newStaffName2", System.Data.SqlDbType.NVarChar, 128, staffName2WithSomeLettersChangedToUpperCase);
					});

				// Act
				sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				var sqlLoginName1FromServerAfterChange = adminConnection.ExecuteScalar<string>(
					sqlCaseSensitiveLoginComparison,
					cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName1ExpectedAfterChange));
				Assert.That(sqlLoginName1FromServerAfterChange, Is.EqualTo(sqlLoginName1ExpectedAfterChange));

				var sqlLoginName2FromServerAfterChange = adminConnection.ExecuteScalar<string>(
					sqlCaseSensitiveLoginComparison,
					cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName2ExpectedAfterChange));
				Assert.That(sqlLoginName2FromServerAfterChange, Is.EqualTo(sqlLoginName2ExpectedAfterChange));

				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						var sqlUserName1FromServer = adminConnection.ExecuteScalar<string>(
							sqlCaseSensitiveUserComparison,
							cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName1ExpectedAfterChange));
						Assert.That(sqlUserName1FromServer, Is.EqualTo(sqlLoginName1ExpectedAfterChange));

						var sqlUserName2FromServer = adminConnection.ExecuteScalar<string>(
							sqlCaseSensitiveUserComparison,
							cmd => cmd.AddParameter("@principalName", SqlDbType.NVarChar, 128, sqlLoginName2ExpectedAfterChange));
						Assert.That(sqlUserName2FromServer, Is.EqualTo(sqlLoginName2ExpectedAfterChange));
					}
				}
			}
		}

		[Test]
		public void TestBuildSecurityWithTrialRunTrueLogsButDoesNotMakeChanges()
		{
			// Arrange
			var singleRefDbName = Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef);
			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var allDatabases = Helper.Databases();
			var staffLoginReader = "TestReader";
			var staffLoginDeveloper = "TestDeveloper";
			var staffLoginBackupOperator = "TestBackupOperator";
			var staffLoginHrmStaff = "TestHrmStaff";

			var sqlLoginNameReader = Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName);
			var sqlLoginNameDeveloper = Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName);
			var sqlLoginNameBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName);
			var sqlLoginNameHrmStaff = Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

			foreach (var database in allDatabases.Except(Db.DatabaseName))
			{
				adminConnection.ExecuteNonQuery($@"
DROP DATABASE IF EXISTS {database.QuoteName()};
CREATE DATABASE {database.QuoteName()};
");
			}

			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

			foreach (var databaseName in allDatabases.Except(singleRefDbName))
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}
			}

			using (((ICurrentDbControl)adminConnection).UseDatabase(singleRefDbName))
			{
				var findGuestPermissions = @"
FROM
	sys.database_permissions     AS dp
	JOIN sys.database_principals AS grantee ON dp.grantee_principal_id = grantee.principal_id
		AND grantee.name = N'guest'
";

				Assume.That(adminConnection.Exists(findGuestPermissions), Is.False, "There should be no permissions granted to guest principal before building security.");
			}

			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token, trialRun: true);

				// Assert
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, "There should be log message for server starting building sql security.", Times.Once(), $"Building Sql security for server '{Db.ServerName}' with main database name '{Db.DatabaseName}'.");
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, "There should be log message for server completed building sql security.", Times.Once(), $"Building Sql security for server '{Db.ServerName}' with main database name '{Db.DatabaseName}' took");
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, "Server proposed principals and memberships should be reported.", Times.Once(), $"Synchronizer debug information for server '{Db.ServerName}' with main database name '{Db.DatabaseName}'");

				AssertionsHelper.AssertPrincipalMissing(adminConnection, dbReaderLogin);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, dbWriterLogin);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedReaderLogin);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedWriterLogin);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameReader);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameDeveloper);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameHrmStaff);

				foreach (var databaseName in allDatabases.Except(singleRefDbName))
				{
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, $"There should be log message for database '{databaseName}' starting building sql security.", Times.Once(), $"Building Sql security for database '{databaseName}'.");
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, $"There should be log message for database '{databaseName}' completed building sql security.", Times.Once(), $"Building Sql security for database '{databaseName}' took");
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, $"Database proposed principals and memberships should be reported for database {databaseName}.", Times.Once(), $"Expected principals and memberships for database '{databaseName}'");
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, $"Database proposed permissions should be reported for database {databaseName}.", Times.Once(), $"Expected permissions for database '{databaseName}'");

					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbReaderLogin);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbWriterLogin);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedReaderLogin);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedWriterLogin);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameReader);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameDeveloper);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameBackupOperator);
						DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginNameHrmStaff);
					}
				}

				using (((ICurrentDbControl)adminConnection).UseDatabase(singleRefDbName))
				{
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, $"There should be log message for database '{singleRefDbName}'.", Times.Once(), $"Building Sql security for database '{singleRefDbName}'.");
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, $"There should be log message for database '{singleRefDbName}' completed building sql security.", Times.Once(), $"Building Sql security for database '{singleRefDbName}' took");

					loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, $"Database proposed permissions should be reported for database {singleRefDbName}.", Times.Once(), $"Expected permissions for database '{singleRefDbName}'");

					var findGuestPermissions = @"
FROM
	sys.database_permissions     AS dp
	JOIN sys.database_principals AS grantee ON dp.grantee_principal_id = grantee.principal_id
		AND grantee.name = N'guest'
";

					Assert.That(adminConnection.Exists(findGuestPermissions), Is.False, "There should still be no permissions granted to guest principal before building security.");
				}
			}
		}

		[Test]
		public void TestBuildSecurityUsesMainDatabaseNameCaseAsOnSqlServerWhenBuildingLoginsAndUsersNames()
		{
			// Arrange
			try
			{
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(System.Environment.MachineName, Helper.MainDatabaseNameOutsideTestCase.ToUpper());

				var staffLoginReader = "TestReader";
				var staffLoginDeveloper = "TestDeveloper";
				var staffLoginBackupOperator = "TestBackupOperator";
				var staffLoginHrmStaff = "TestHrmStaff";

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginReader, null, null, adminConnection, new[] { DbRoleTypes.CwRestrictedReaderRole });
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloper, null, null, adminConnection, new[] { DbRoleTypes.DbDataWriterRole });
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole });
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginHrmStaff, null, null, adminConnection, new[] { DbRoleTypes.CwHRMStaffRole });

				var errorReporterMock = new Mock<IErrorReporter>();
				var loggerMock = new Mock<IntegrationLogging.ILogger>();

				Assume.That(
					adminConnection.Exists(
						$"FROM sys.databases WHERE name = @databaseName COLLATE {Db.DatabaseCaseSensitiveCollation}",
						cmd => cmd.AddParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128, Helper.MainDatabaseNameOutsideTestCase)),
					Is.True,
					$"Database with name '{Helper.MainDatabaseNameOutsideTestCase}' should exist (case sensitive).");

				Assume.That(
					adminConnection.Exists(
						$"FROM sys.databases WHERE name = @databaseName COLLATE {Db.DatabaseCaseSensitiveCollation}",
						cmd => cmd.AddParameter("@databaseName", System.Data.SqlDbType.NVarChar, 128, Db.DatabaseName)),
					Is.False,
					$"Database with name '{Db.DatabaseName}' should not exist (case sensitive).");

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

				ErrorReporter.Clear();

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
				{
					// Act
					sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

					// Assert
					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_RestrictedReaderLogin", "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_RestrictedReaderLogin", "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_RestrictedWriterLogin", "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_RestrictedWriterLogin", "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_UnrestrictedWriterLogin", "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_UnrestrictedWriterLogin", "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_CargoWiseReaderLogin", "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_CargoWiseReaderLogin", "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_CargoWiseWriterLogin", "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_CargoWiseWriterLogin", "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginReader, Helper.MainDatabaseNameOutsideTestCase), "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName), "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Helper.MainDatabaseNameOutsideTestCase), "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName), "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Helper.MainDatabaseNameOutsideTestCase), "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName), "S");

					AssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Helper.MainDatabaseNameOutsideTestCase), "S");
					AssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName), "S");

					foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
					{
						using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
						{
							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_RestrictedReaderLogin", "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_RestrictedReaderLogin", "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_RestrictedWriterLogin", "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_RestrictedWriterLogin", "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_UnrestrictedWriterLogin", "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_UnrestrictedWriterLogin", "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_CargoWiseReaderLogin", "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_CargoWiseReaderLogin", "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, $"{Helper.MainDatabaseNameOutsideTestCase}_CargoWiseWriterLogin", "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, $"{Db.DatabaseName}_CargoWiseWriterLogin", "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginReader, Helper.MainDatabaseNameOutsideTestCase), "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginReader, Db.DatabaseName), "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Helper.MainDatabaseNameOutsideTestCase), "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginDeveloper, Db.DatabaseName), "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Helper.MainDatabaseNameOutsideTestCase), "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginBackupOperator, Db.DatabaseName), "S");

							DatabaseAssertionsHelper.AssertPrincipalExistsCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Helper.MainDatabaseNameOutsideTestCase), "S");
							DatabaseAssertionsHelper.AssertPrincipalMissingCaseSensitive(adminConnection, Helper.GetEnterpriseLoginFullName(staffLoginHrmStaff, Db.DatabaseName), "S");
						}
					}
				}
			}
			finally
			{
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(System.Environment.MachineName, Helper.MainDatabaseNameOutsideTestCase);
			}
		}

		#region Offline

		[TestCase(DatabaseType.Audit, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: EDW database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: User repository database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: SD database")]
		[TestCase(DatabaseType.SingleSharedRef, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Single Shared Ref database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Exclusive Ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Shared Ref database")]
		[TestCase(DatabaseType.SharedRef, DatabaseType.ExclusiveRef, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Shared Ref database and Exclusive Ref database")]
		[TestCase(DatabaseType.SharedRef, DatabaseType.SingleSharedRef, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Shared Ref database and Single Shared Ref database")]
		[TestCase(DatabaseType.SharedRef, DatabaseType.SD, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Shared Ref database and SD database")]
		[TestCase(DatabaseType.UserRepository, DatabaseType.Audit, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: User repository database and Audi database")]
		[TestCase(DatabaseType.Audit, DatabaseType.EDW, TestName = "TestBuildSecuritySynchronisesAllOnlineDatabases: Audit database and EDW database")]
		public void TestBuildSecuritySynchronisesAllOnlineDatabases(params DatabaseType[] databaseTypeOffline)
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();

			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);
			ErrorReporter.Clear();

			var errorReporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				var offlineDatabases = new List<string>();
				foreach (var databaseTypeOffLine in databaseTypeOffline)
				{
					var databaseName = Helper.DatabaseNameFromDatabaseType(databaseTypeOffLine);
					adminConnection.ExecuteNonQuery($"ALTER DATABASE {databaseName.QuoteName()} SET OFFLINE");

					offlineDatabases.Add(databaseName);
				}

				try
				{
					// Act/Assert
					Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

					errorReporterMock.VerifyNoOtherCalls();

					foreach (var databaseName in Helper.Databases().Except(offlineDatabases))
					{
						loggerMock.VerifyCalled(
							IntegrationLogging.LogType.Information,
							$"Sql security should have been completed for database '{databaseName}'.",
							Times.Once(),
							$"Building Sql security for database '{databaseName}' took");
					}

					foreach (var databaseName in offlineDatabases)
					{
						loggerMock.VerifyCalled(
							IntegrationLogging.LogType.Information,
							$"Sql security should not have been completed for database '{databaseName}'.",
							Times.Never(),
							$"Building Sql security for database '{databaseName}' took");

						loggerMock.VerifyCalled(
							IntegrationLogging.LogType.Error,
							typeof(SqlException),
							$"There should be an error log regarding failing to build Sql Security for databse '{databaseName}'.",
							Times.Once(),
							$"Failed to build Sql security for database '{databaseName}'.");
					}
				}
				finally
				{
					foreach (var databaseName in offlineDatabases)
					{
						adminConnection.ExecuteNonQuery($"ALTER DATABASE {databaseName.QuoteName()} SET ONLINE");
					}
				}
			}
		}

		[TestCase(DatabaseType.Audit, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Audit database")]
		[TestCase(DatabaseType.EDW, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: EDW database")]
		[TestCase(DatabaseType.UserRepository, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: User repository database")]
		[TestCase(DatabaseType.SD, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: SD database")]
		[TestCase(DatabaseType.SingleSharedRef, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Single Shared Ref database")]
		[TestCase(DatabaseType.ExclusiveRef, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Exclusive Ref database")]
		[TestCase(DatabaseType.SharedRef, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Shared Ref database")]
		[TestCase(DatabaseType.SharedRef, DatabaseType.ExclusiveRef, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Shared Ref database and Exclusive Ref database")]
		[TestCase(DatabaseType.SharedRef, DatabaseType.SingleSharedRef, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Shared Ref database and Single Shared Ref database")]
		[TestCase(DatabaseType.SharedRef, DatabaseType.SD, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Shared Ref database and SD database")]
		[TestCase(DatabaseType.UserRepository, DatabaseType.Audit, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: User repository database and Audi database")]
		[TestCase(DatabaseType.Audit, DatabaseType.EDW, TestName = "TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases: Audit database and EDW database")]
		public void TestBuildSecurityForAllDatabasesSynchronisesAllOnlineDatabases(params DatabaseType[] databaseTypeOffline)
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();

			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);
			ErrorReporter.Clear();

			var errorReporterMock = new Mock<IErrorReporter>();

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {dbReaderLogin.QuoteName()} WITH PASSWORD = N'{testReaderCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbReaderLogin)};
CREATE LOGIN {dbWriterLogin.QuoteName()} WITH PASSWORD = N'{testWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbWriterLogin)};
CREATE LOGIN {dbRestrictedReaderLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedReaderLogin)};
CREATE LOGIN {dbRestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testRestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbRestrictedWriterLogin)};
CREATE LOGIN {dbUnrestrictedWriterLogin.QuoteName()} WITH PASSWORD = N'{testUnrestrictedWriterCredentials.Password}', SID = {SqlServerLoginUtilities.ComputeSqlLoginSid(dbUnrestrictedWriterLogin)};
");

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				var offlineDatabases = new List<string>();
				foreach (var databaseTypeOffLine in databaseTypeOffline)
				{
					var databaseName = Helper.DatabaseNameFromDatabaseType(databaseTypeOffLine);
					adminConnection.ExecuteNonQuery($"ALTER DATABASE {databaseName.QuoteName()} SET OFFLINE");

					offlineDatabases.Add(databaseName);
				}

				try
				{
					// Act/Assert
					Assert.That(() => sqlSecurityManager.BuildSecurityForAllDatabases(adminConnection, cancellationTokenSource.Token), Throws.Nothing);

					errorReporterMock.VerifyNoOtherCalls();

					foreach (var databaseName in Helper.Databases().Except(offlineDatabases))
					{
						loggerMock.VerifyCalled(
							IntegrationLogging.LogType.Information,
							$"Sql security should have been completed for database '{databaseName}'.",
							Times.Once(),
							$"Building Sql security for database '{databaseName}' took");
					}

					foreach (var databaseName in offlineDatabases)
					{
						loggerMock.VerifyCalled(
							IntegrationLogging.LogType.Information,
							$"Sql security should not have been completed for database '{databaseName}'.",
							Times.Never(),
							$"Building Sql security for database '{databaseName}' took");

						loggerMock.VerifyCalled(
							IntegrationLogging.LogType.Error,
							typeof(SqlException),
							$"There should be an error log regarding failing to build Sql Security for databse '{databaseName}'.",
							Times.Once(),
							$"Failed to build Sql security for database '{databaseName}'.");
					}
				}
				finally
				{
					foreach (var databaseName in offlineDatabases)
					{
						adminConnection.ExecuteNonQuery($"ALTER DATABASE {databaseName.QuoteName()} SET ONLINE");
					}
				}
			}
		}

		#endregion offline

		#region backup operator and developer

		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestBuildSecurityCreatesAllLoginsAndUsersForAllDatabasesForAStaffMemberWithBackupOperatorAndDeveloperRole: Self-hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestBuildSecurityCreatesAllLoginsAndUsersForAllDatabasesForAStaffMemberWithBackupOperatorAndDeveloperRole: Self-hosted locked")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestBuildSecurityCreatesAllLoginsAndUsersForAllDatabasesForAStaffMemberWithBackupOperatorAndDeveloperRole: Wise clound hosted shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestBuildSecurityCreatesAllLoginsAndUsersForAllDatabasesForAStaffMemberWithBackupOperatorAndDeveloperRole: Wise cloud hosted dedicated")]
		public void TestBuildSecurityCreatesAllLoginsAndUsersForAllDatabasesForAStaffMemberWithBackupOperatorAndDeveloperRole(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var staffLoginDeveloperAndBackupOperator = "TestDeveloperAndBackupOperator";

			var sqlLoginNameDeveloperAndBackupOperator = Helper.GetEnterpriseLoginFullName(staffLoginDeveloperAndBackupOperator, Db.DatabaseName);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(staffLoginDeveloperAndBackupOperator, null, null, adminConnection, new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole });

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloperAndBackupOperator);

			foreach (var databaseName in Helper.Databases())
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameDeveloperAndBackupOperator);
				}
			}

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (Helper.CreateRefDbSynonyms(adminConnection, Db.DatabaseName))
			{
				// Act
				sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameDeveloperAndBackupOperator, "S");

				foreach (var databaseName in Helper.Databases().Except(Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef)))
				{
					using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
					{
						DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameDeveloperAndBackupOperator, "S");
					}
				}
			}
		}

		#endregion backup operator and developer

		#region Implementation

		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Helper.MainDatabaseNameOutsideTestCase);
		CancellationTokenSource cancellationTokenSource;

		[SetUp]
		public void SetUp()
		{
			adminConnection = Db.NewAdminConnection();
			Helper.DropExtraDatabases(adminConnection);
			Helper.EnsureExtraDatabasesWithSchemas(adminConnection);
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);

			if (!adminConnection.IsDbWriteable(Db.DatabaseName))
			{
				adminConnection.AlterDbWriteableState(Db.DatabaseName, writeable: true);
			}

			foreach (var databaseName in Helper.Databases())
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					Helper.DropDatabasePrincipals(adminConnection);
				}
			}

			Helper.CleanUpGlbTables(adminConnection);
			CleanUpTstSchemaTypeAndFunction();

			cancellationTokenSource = new CancellationTokenSource(180000);
		}

		[TearDown]
		public void TearDown()
		{
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);

			Helper.DropExtraDatabases(adminConnection);

			if (!adminConnection.IsDbWriteable(Db.DatabaseName))
			{
				adminConnection.AlterDbWriteableState(Db.DatabaseName, writeable: true);
			}

			Helper.CleanUpGlbTables(adminConnection);

			CleanUpTstSchemaTypeAndFunction();

			adminConnection?.Dispose();
		}

		void CleanUpTstSchemaTypeAndFunction()
		{
			adminConnection.ExecuteNonQuery($@"
DROP TYPE IF EXISTS {schemaNameWithSingleQuote.QuoteName()}.{typeNameWithSingleQuote.QuoteName()};
DROP TYPE IF EXISTS dbo.{typeNameWithSingleQuote.QuoteName()};

DROP FUNCTION IF EXISTS {schemaNameWithSingleQuote.QuoteName()}.{functionNameWithSingleQuote.QuoteName()};
DROP FUNCTION IF EXISTS dbo.{functionNameWithSingleQuote.QuoteName()};
");
			adminConnection.ExecuteNonQuery($@"
DROP SCHEMA IF EXISTS {schemaNameWithSingleQuote.QuoteName()};
");
		}

		AdminConnection adminConnection;
		readonly string typeNameWithSingleQuote = "_Tst_Name's with single quote";
		readonly string functionNameWithSingleQuote = "_Tst_FunctionName's with single quote";
		readonly string schemaNameWithSingleQuote = "_Tst_Name's with single quote";

		static IEnumerable<TestCaseData> DatabasesTestCaseSource()
		{
			yield return new TestCaseData(new List<string>()
			{
				Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit),
				Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW),
				Helper.DatabaseNameFromDatabaseType(DatabaseType.SD),
				Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository),
				Helper.DatabaseNameFromDatabaseType(DatabaseType.ExclusiveRef),
				Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef),
			})
				.SetName("{m}: All test databases");

			yield return new TestCaseData(new List<string>()
			{
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD002",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD102",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD233",
			})
				.SetName("{m}: Multiple SD databases");

			yield return new TestCaseData(new List<string>()
			{
				Helper.DatabaseNameFromDatabaseType(DatabaseType.SharedRef),
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Ent_CA",
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Cmr_US",
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Trf_AU",
			})
				.SetName("{m}: Multiple Ref databases");

			yield return new TestCaseData(new List<string>()
			{
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Ent_CA",
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Cmr_US",
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Trf_AU",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD002",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD102",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD233",
			})
				.SetName("{m}: Multiple Ref and SD databases");

			yield return new TestCaseData(new List<string>()
			{
				Helper.DatabaseNameFromDatabaseType(DatabaseType.Audit),
				Helper.DatabaseNameFromDatabaseType(DatabaseType.EDW),
				Helper.DatabaseNameFromDatabaseType(DatabaseType.UserRepository),
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Ent_CA",
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Cmr_US",
				$"{Helper.MainDatabaseNameOutsideTestCase}_{RefDbTableNameResolver.RefDbAffix}_Trf_AU",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD002",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD102",
				$"{Helper.MainDatabaseNameOutsideTestCase}_SD233",
			})
				.SetName("{m}: Test databases with multiple Ref and SD databases");
		}

		#endregion Implementation
	}
}
