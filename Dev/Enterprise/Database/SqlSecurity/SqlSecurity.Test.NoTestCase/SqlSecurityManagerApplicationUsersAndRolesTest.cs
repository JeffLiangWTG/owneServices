using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DataProtection;
using Moq;
using NUnit.Framework;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	class SqlSecurityManagerApplicationUsersAndRolesTest : SqlSecurityTestFixture
	{
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRolesForMainDatabase: self hosted locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRolesForMainDatabase: self hosted open")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRolesForMainDatabase: Wise cloud shared hosted")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRolesForMainDatabase: Wise cloud dedicated hosted")]
		public void TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRolesForMainDatabase(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (((ICurrentDbControl)adminConnection).UseDatabase(Db.DatabaseName))
			{
				Helper.DropDatabasePrincipals(adminConnection);
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);

				using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
				using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
				{
					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, Db.DatabaseName, trialRun: true);

					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbReaderLogin);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbWriterLogin);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedReaderLogin);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedWriterLogin);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);

					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, Db.DatabaseName, trialRun: false);

					// Assert
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

					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwHRMStaffRole, "R");
					DatabaseAssertionsHelper.AssertPrincipalHasNoRoles(adminConnection, DbRoleTypes.CwHRMStaffRole);
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlHrmSchema, "SELECT");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, "dbo");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, "OrderTracking");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlCdcSchema);
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlStagingSchema);

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwHRMStaffRole, 3); /* 3 for schema */
				}
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, DatabaseType.SharedRef, TestName = "TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRoles: Shared ref db, Wise cloud hosted dedicated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, DatabaseType.SharedRef, TestName = "TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRoles: Shared ref db, Wise cloud shared hosted")]
		public void TestApplicationUsersAndRolesAreCreatedWithCorrectPermissionsAndRoles(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				Helper.DropDatabasePrincipals(adminConnection);

				var randomSchemaName = $"RandomSchema_{databaseType}";
				Helper.EnsureSchemasForDatabase(adminConnection, databaseName, randomSchemaName);

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbReaderLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbWriterLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedReaderLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbRestrictedWriterLogin);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);

				using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
				using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
				{
					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbReaderLogin);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbWriterLogin);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedReaderLogin);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, dbRestrictedWriterLogin);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, dbUnrestrictedWriterLogin);

					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwReaderRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedReaderRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwRestrictedWriterRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole);
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, DbRoleTypes.CwHRMStaffRole);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
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

					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwReaderRole, "R");
					DatabaseAssertionsHelper.AssertPrincipalHasRoles(adminConnection, DbRoleTypes.CwReaderRole, "db_datareader");

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, DbRoleTypes.CwReaderRole, "SHOWPLAN", "VIEW DEFINITION");

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwReaderRole, excludedClasses: 0); /* 0 for database */

					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "R");
					DatabaseAssertionsHelper.AssertPrincipalHasNoRoles(adminConnection, DbRoleTypes.CwRestrictedReaderRole);

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "SHOWPLAN", "VIEW DEFINITION");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "dbo", "EXECUTE", "SELECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, "OrderTracking", "EXECUTE", "SELECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedReaderRole, randomSchemaName, "EXECUTE", "SELECT");

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
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwRestrictedWriterRole, randomSchemaName, "DELETE", "EXECUTE", "INSERT", "SELECT", "UPDATE", "ALTER", "CREATE SEQUENCE");

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
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, randomSchemaName, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, DbSecurity.SqlCdcSchema, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, DbSecurity.SqlStagingSchema, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, DbSecurity.SqlHrmSchema, "VIEW CHANGE TRACKING", "CREATE SEQUENCE");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, DbRoleTypes.CwUnrestrictedWriterRole, 0, 3); /* 0 for database, 3 for schema */

					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, DbRoleTypes.CwHRMStaffRole, "R");
					DatabaseAssertionsHelper.AssertPrincipalHasNoRoles(adminConnection, DbRoleTypes.CwHRMStaffRole);
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlHrmSchema, "SELECT");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, "dbo");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, "OrderTracking");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, randomSchemaName);
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlCdcSchema);
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, DbRoleTypes.CwHRMStaffRole, DbSecurity.SqlStagingSchema);
				}
			}
		}

		#region Filter tests

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Main })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, DatabaseType.SharedRef, TestName = "TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped: Shared ref db, Wise cloud hosted dedicated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, DatabaseType.SharedRef, TestName = "TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped: Shared ref db, Wise cloud shared hosted")]
		public void TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreDropped(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				Helper.DropLoginIfExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S");
				Helper.DropLoginIfExists(adminConnection, $"{Db.DatabaseName}LoginWithoutUnderscoreToKeep", "S");

				adminConnection.ExecuteNonQuery($@"
DROP USER IF EXISTS [{Db.DatabaseName}_LoginWithUnderscoreToDrop];
");
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				try
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{Db.DatabaseName}_LoginWithUnderscoreToDrop] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
CREATE USER [{Db.DatabaseName}_LoginWithUnderscoreToDrop] FROM LOGIN [{Db.DatabaseName}_LoginWithUnderscoreToDrop];
					
					");

					DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S");

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S");
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop");
				}
				finally
				{
					Helper.DropLoginIfExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscoreToDrop", "S");

					adminConnection.ExecuteNonQuery($@"
DROP USER IF EXISTS [{Db.DatabaseName}_LoginWithUnderscoreToDrop];
");
				}
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreNotDroppedInSingleSharedRefDb: Hosted in Wise cloud shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreNotDroppedInSingleSharedRefDb: Hosted in Wise dedicateed server")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreNotDroppedInSingleSharedRefDb: Self hosted locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreNotDroppedInSingleSharedRefDb: Self hosted open")]
		public void TestUsersStartingFromDbNameUnderscoreButNotMatchingApplicationLoginNamesAreNotDroppedInSingleSharedRefDb(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(DatabaseType.SingleSharedRef);

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				Helper.DropLoginIfExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscore", "S");

				adminConnection.ExecuteNonQuery($@"
DROP USER IF EXISTS [{Db.DatabaseName}_LoginWithUnderscore];
");
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				try
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{Db.DatabaseName}_LoginWithUnderscore] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
CREATE USER [{Db.DatabaseName}_LoginWithUnderscore] FROM LOGIN [{Db.DatabaseName}_LoginWithUnderscore];
					
					");

					DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscore", "S");

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscore", "S");
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscore", "S");
				}
				finally
				{
					Helper.DropLoginIfExists(adminConnection, $"{Db.DatabaseName}_LoginWithUnderscore", "S");

					adminConnection.ExecuteNonQuery($@"
DROP USER IF EXISTS [{Db.DatabaseName}_LoginWithUnderscore];
");
				}
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Main })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]
		public void TestRolesStartingFromcwAndEndingWithRoleButNotMatchingApplicationRoleNamesAreDroppedInExclusiveDbs(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				adminConnection.ExecuteNonQuery($@"
DROP ROLE IF EXISTS [cwStartsWithCwEndsWithRole];
");

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				try
				{
					adminConnection.ExecuteNonQuery($@"
CREATE ROLE cwStartsWithCwEndsWithRole;
					
					");

					DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, "cwStartsWithCwEndsWithRole", "R");

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, "cwStartsWithCwEndsWithRole", "R");

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, "cwReaderRoleEndsWithRole");
				}
				finally
				{
					adminConnection.ExecuteNonQuery($@"
DROP ROLE IF EXISTS [cwStartsWithCwEndsWithRole];
");
				}
			}
		}

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SingleSharedRef })]

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, DatabaseType.SharedRef, TestName = "TestRolesStartingFromcwAndEndingWithRoleButNotMatchingApplicationRoleNamesAreNotDroppedInSharedDbs: Shared ref db, Wise cloud hosted dedicated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, DatabaseType.SharedRef, TestName = "TestRolesStartingFromcwAndEndingWithRoleButNotMatchingApplicationRoleNamesAreNotDroppedInSharedDbs: Shared ref db, Wise cloud shared hosted")]
		public void TestRolesStartingFromcwAndEndingWithRoleButNotMatchingApplicationRoleNamesAreNotDroppedInSharedDbs(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			{
				adminConnection.ExecuteNonQuery($@"
DROP ROLE IF EXISTS [cwStartsWithCwEndsWithRole];
");

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				try
				{
					adminConnection.ExecuteNonQuery($@"
CREATE ROLE cwStartsWithCwEndsWithRole;
					
					");

					DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, "cwStartsWithCwEndsWithRole", "R");

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, "cwStartsWithCwEndsWithRole", "R");

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, "cwStartsWithCwEndsWithRole", "R");
				}
				finally
				{
					adminConnection.ExecuteNonQuery($@"
DROP ROLE IF EXISTS [cwStartsWithCwEndsWithRole];
");
				}
			}
		}

		#endregion Filter tests

		#region Implementation

		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

		AdminConnection adminConnection;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			adminConnection = Db.NewAdminConnection();
			Helper.EnsureExtraDatabasesWithSchemas(adminConnection);
		}

		[SetUp]
		public void SetUp()
		{
			CreateServerTestEntities(adminConnection, Db.DatabaseName);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			adminConnection?.Dispose();
		}

		#endregion Implementation
	}
}
