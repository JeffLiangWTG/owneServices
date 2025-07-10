using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	[Property("DAT:CapabilityRequirements", "SQL2019")]
	class SqlSecurityManagerStaffUsersAndRoleTest : SqlSecurityTestFixture
	{
		#region not AD integrated

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
			DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithSingleQuote);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLoginWithDoubleSingleQuote);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLoginWithDoubleSingleQuote), Is.GreaterThan(0), $"Staff member '{staffLoginWithDoubleSingleQuote}' should exist and belong to a group with database access role(s).");
			DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithDoubleSingleQuote);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginNameWithSingleQuote.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
CREATE LOGIN {sqlLoginNameWithDoubleSingleQuote.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildDatabaseSecurity(adminConnection, Helper.MainDatabaseNameOutsideTestCase, trialRun: false), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameWithSingleQuote, "S");
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginNameWithDoubleSingleQuote, "S");
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		public void TestStaffSqlUserIsCreatedWithCorrectPermissionsAndPropertiesWhenAdIntegrationDisabledHostedInWiseCloud(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var staffLogin = Guid.NewGuid().ToString();
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				staffUserRoles.ToArray());

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLogin);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLogin), Is.GreaterThan(0), $"Staff member '{staffLogin}' should exist and belong to a group with database access role(s).");

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");
				DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole }).ToArray()); // data writer role is currently only for User Repository database, db backup operator role is not allowed in hosted environment

				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, "CONNECT");
				DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, sqlLoginName);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0); //* 0 for Database *//
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		public void TestStaffSqlUserIsCreatedWithCorrectPermissionsAndPropertiesInBIDatabasesWhenAdIntegrationDisabledHostedInWiseCloud(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var staffLogin = Guid.NewGuid().ToString();
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				staffUserRoles.ToArray());

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLogin);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLogin), Is.GreaterThan(0), $"Staff member '{staffLogin}' should exist and belong to a group with database access role(s).");

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");
				DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole }).ToArray()); // data writer role is currently only for User Repository database, db backup operator role is not allowed in hosted environment

				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, "CONNECT");
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnOtherPrincipals(adminConnection, dbUnrestrictedWriterLogin);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0); //* 0 for Database *//
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		public void TestStaffSqlUserIsCreatedWithCorrectPermissionsAndPropertiesInUserRepositoryWhenAdIntegrationDisabledHostedInWiseCloud(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var staffLogin = Guid.NewGuid().ToString();
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				staffUserRoles.ToArray());

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLogin);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLogin), Is.GreaterThan(0), $"Staff member '{staffLogin}' should exist and belong to a group with database access role(s).");

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");
				DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbBackupOperatorRole }).ToArray());

				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, StaffUser.Permissions.ByRolesAndDatabaseTypeOnDatabase(DatabaseType.UserRepository, staffUserRoles).ToArray());
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, sqlLoginName, "dbo", StaffUser.Permissions.ByRolesAndDatabaseTypeOnDboSchema(DatabaseType.UserRepository, staffUserRoles).ToArray());
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, sqlLoginName, DbSecurity.SqlHrmSchema);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, sqlLoginName, "OrderTracking");
				DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, sqlLoginName);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0, 3); //* 0 for Database, 3 for Schema *//
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedLocked })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.SelfHostedOpen })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.SelfHostedOpen })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedOpen })]
		public void TestStaffSqlUserIsCreatedWithCorrectPermissionsAndPropertiesWhenAdIntegrationDisabledSelfHosted(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var staffLogin = Guid.NewGuid().ToString();
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				staffUserRoles.ToArray());

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLogin);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLogin), Is.GreaterThan(0), $"Staff member '{staffLogin}' should exist and belong to a group with database access role(s).");

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");
				DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole }).ToArray()); // data writer role is currently only for User Repository database

				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, "CONNECT");
				DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, sqlLoginName);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0); //* 0 for Database *//
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.SelfHostedLocked })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.SelfHostedOpen })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.SelfHostedOpen })]
		public void TestStaffSqlUserIsCreatedWithCorrectPermissionsAndPropertiesInBIDatabasesWhenAdIntegrationDisabledSelfHosted(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var staffLogin = Guid.NewGuid().ToString();
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				staffUserRoles.ToArray());

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLogin);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLogin), Is.GreaterThan(0), $"Staff member '{staffLogin}' should exist and belong to a group with database access role(s).");

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");
				DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole }).ToArray()); // data writer role is currently only for User Repository database

				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, "CONNECT");
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnOtherPrincipals(adminConnection, dbUnrestrictedWriterLogin);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0); //* 0 for Database *//
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.SelfHostedOpen })]
		public void TestStaffSqlUserIsCreatedWithCorrectPermissionsAndPropertiesInUserRepositoryWhenAdIntegrationDisabledSelfHosted(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var staffLogin = Guid.NewGuid().ToString();
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				staffUserRoles.ToArray());

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLogin);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLogin), Is.GreaterThan(0), $"Staff member '{staffLogin}' should exist and belong to a group with database access role(s).");

			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");
				DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.ToArray());

				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, StaffUser.Permissions.ByRolesAndDatabaseTypeOnDatabase(DatabaseType.UserRepository, staffUserRoles).ToArray());
				DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, sqlLoginName, "dbo", StaffUser.Permissions.ByRolesAndDatabaseTypeOnDboSchema(DatabaseType.UserRepository, staffUserRoles).ToArray());
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, sqlLoginName, DbSecurity.SqlHrmSchema);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, sqlLoginName, "OrderTracking");
				DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, sqlLoginName);
				DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0, 3); //* 0 for Database, 3 for Schema *//
			}
		}

		#endregion not AD integrated

		#region AD integrated

		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Main })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SD })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.UserRepository })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.Audit })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.EDW })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.ExclusiveRef })]
		[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.DatabaseTestModeDataBaseTypeCombinations), new object[] { DatabaseType.SharedRef })]
		public void TestStaffDbUserShouldBeCreatedFromWindowsShortenedADSamAccountName(DatabaseTestMode databaseTestMode, DatabaseType databaseType)
		{
			// Arrange
			var sqlLoginB = Helper.GetEnterpriseLoginFullName(Helper.ADTestUserLongNameB.Name, Db.DatabaseName);
			var sqlLoginC = Helper.GetEnterpriseLoginFullName(Helper.ADTestUserLongNameC.Name, Db.DatabaseName);

			adminConnection.ExecuteNonQuery($@"
	CREATE LOGIN {sqlLoginB.QuoteName()} WITH PASSWORD = N'SOMErandom123[])Pwd';
	CREATE LOGIN {sqlLoginC.QuoteName()} WITH PASSWORD = N'SOMErandom123[])Pwd';

	CREATE LOGIN {Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
	CREATE LOGIN {Helper.ADTestUserLongNameC.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
");
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					Helper.ADTestUserLongNameB.Name,
					TestConstants.Domain,
					Guid.Parse(Helper.ADTestUserLongNameB.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					Helper.ADTestUserLongNameC.Name,
					TestConstants.Domain,
					Guid.Parse(Helper.ADTestUserLongNameC.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, Helper.ADTestUserLongNameC.NameWithDomainPreWindows2000);

					var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000, "U");
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, Helper.ADTestUserLongNameC.NameWithDomainPreWindows2000, "U");
				}
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		public void TestStaffSqlUsersIsCreatedWithCorrectPermissionsAndPropertiesIfAdIntegrationEnabledHostedInCargoWiseCloud(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffUserRoles.ToArray());

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginName, "U");
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");

					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, windowsLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole }).ToArray()); // data writer role is currently only for User Repository database, db backup operator role is not allowed in hosted environment
					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole }).ToArray()); // data writer role is currently only for User Repository database, db backup operator role is not allowed in hosted environment

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, windowsLoginName, "CONNECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, "CONNECT");

					DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, sqlLoginName, windowsLoginName);

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, windowsLoginName, 0, 4); //* 0 for Database, 4 for Principal*//
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0); //* 0 for Database *//
				}
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.HostedInWiseCloudSharedServer })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		public void TestStaffSqlUsersIsCreatedWithCorrectPermissionsAndPropertiesInBIDatabasesIfAdIntegrationEnabledHostedInCargoWiseCloud(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffUserRoles.ToArray());

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginName, "U");
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");

					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, windowsLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole }).ToArray()); // data writer role is currently only for User Repository database, db backup operator role is not allowed in hosted environment
					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole }).ToArray()); // data writer role is currently only for User Repository database, db backup operator role is not allowed in hosted environment

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, windowsLoginName, "CONNECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, "CONNECT");

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnOtherPrincipals(adminConnection, dbUnrestrictedWriterLogin);

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, windowsLoginName, 0, 4); //* 0 for Database, 4 for Principal*//
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0); //* 0 for Database *//
				}
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedLocked })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Main, DatabaseTestMode.SelfHostedOpen })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.SD, DatabaseTestMode.SelfHostedOpen })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.ExclusiveRef, DatabaseTestMode.SelfHostedOpen })]
		public void TestStaffWindowsUsersIsCreatedWithCorrectPermissionsAndPropertiesIfAdIntegrationEnabledSelfHosted(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffUserRoles.ToArray());

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{windowsLoginName}] FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginName, "U");
					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, windowsLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole }).ToArray()); // data writer role is currently only for User Repository database

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, windowsLoginName, "CONNECT");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, windowsLoginName);
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, windowsLoginName, 0, 4); //* 0 for Database, 4 for Principal*//

					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginName);
				}
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.SelfHostedLocked })]

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.EDW, DatabaseTestMode.SelfHostedOpen })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.Audit, DatabaseTestMode.SelfHostedOpen })]
		public void TestStaffWindowsUsersIsCreatedWithCorrectPermissionsAndPropertiesInBIDatabasesIfAdIntegrationEnabledSelfHosted(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffUserRoles.ToArray());

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{windowsLoginName}] FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'123[]somepaSSWORD'
");

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginName, "U");
					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, windowsLoginName, staffUserRoles.Except(new[] { DbRoleTypes.DbDataWriterRole }).ToArray()); // data writer role is currently only for User Repository database

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, windowsLoginName, "CONNECT");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnOtherPrincipals(adminConnection, dbUnrestrictedWriterLogin);
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, windowsLoginName, 0, 4); //* 0 for Database, 4 for Principal*//

					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginName);
				}
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.HostedInWiseCloudDedicatedServer })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.HostedInWiseCloudSharedServer })]
		public void TestStaffSqlUsersIsCreatedWithCorrectPermissionsAndPropertiesInUserRepositoryIfAdIntegrationEnabledHostedInCargoWiseCloud(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffUserRoles.ToArray());

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginName, "U");
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, sqlLoginName, "S");

					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, windowsLoginName, staffUserRoles.Except(DbRoleTypes.DbBackupOperatorRole).ToArray()); // db backup operator role is not allowed in hosted environment
					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, sqlLoginName, staffUserRoles.Except(DbRoleTypes.DbBackupOperatorRole).ToArray()); // db backup operator role is not allowed in hosted environment

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, windowsLoginName, StaffUser.Permissions.ByRolesAndDatabaseTypeOnDatabase(databaseType, staffUserRoles).ToArray());
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, sqlLoginName, StaffUser.Permissions.ByRolesAndDatabaseTypeOnDatabase(databaseType, staffUserRoles).ToArray());

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, windowsLoginName, "dbo", StaffUser.Permissions.ByRolesAndDatabaseTypeOnDboSchema(databaseType, staffUserRoles).ToArray());
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, sqlLoginName, "dbo", StaffUser.Permissions.ByRolesAndDatabaseTypeOnDboSchema(databaseType, staffUserRoles).ToArray());

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, windowsLoginName, "hrm");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, sqlLoginName, "hrm");

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, windowsLoginName, "OrderTracking");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, sqlLoginName, "OrderTracking");

					DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, windowsLoginName, sqlLoginName);

					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, windowsLoginName, 0, 3); //* 0 for Database, 3 for Schema *//
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, sqlLoginName, 0, 3); //* 0 for Database, 3 for Schema*//
				}
			}
		}

		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.SelfHostedLocked })]
		[TestCaseSource(nameof(CombinationsOfStaffUserRolesSetAndDatabaseTestMode), new object[] { DatabaseType.UserRepository, DatabaseTestMode.SelfHostedOpen })]
		public void TestStaffSqlUsersIsCreatedWithCorrectPermissionsAndPropertiesInUserRepositoryIfAdIntegrationEnabledSelfHosted(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string[] staffUserRoles)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffUserRoles.ToArray());

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
					DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

					// Assert
					DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, windowsLoginName, "U");
					DatabaseAssertionsHelper.AssertPrincipalIsAMemeberOfRoles(adminConnection, windowsLoginName, staffUserRoles.ToArray()); // data writer role is currently only for User Repository database so should be included

					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnDatabase(adminConnection, windowsLoginName, StaffUser.Permissions.ByRolesAndDatabaseTypeOnDatabase(databaseType, staffUserRoles).ToArray());
					DatabaseAssertionsHelper.AssertPrincipalHasGrantPermissionsOnSchema(adminConnection, windowsLoginName, "dbo", StaffUser.Permissions.ByRolesAndDatabaseTypeOnDboSchema(databaseType, staffUserRoles).ToArray());
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, windowsLoginName, "hrm");
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnSchema(adminConnection, windowsLoginName, "OrderTracking");
					DatabaseAssertionsHelper.AssertPrincipalHasGrantImpersonatePermissionsOnUsersAndNoOtherPermissionsOnPrincipals(adminConnection, dbUnrestrictedWriterLogin, windowsLoginName);
					DatabaseAssertionsHelper.AssertPrincipalHasNoPermissionsOnClassesOtherThanExcluded(adminConnection, windowsLoginName, 0, 3); //* 0 for Database, 3 for Schema *//

					DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginName);
				}
			}
		}

		#endregion AD integrated

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "Self hosted locked, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "Self hosted open, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "Wise Cloud shared, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "Wise Cloud dedicated, AD integrated" })]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestBuildDatabaseSecurityLogsExpectedPrincipalsMemebershipsAndPermissionsWhenTrialRun: Wise cloud shared hosted, AD integrated")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestBuildDatabaseSecurityLogsExpectedPrincipalsMemebershipsAndPermissionsWhenTrialRun: Wise cloud hosted dedicated, AD integrated")]

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "Self hosted locked, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "Self hosted open, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "Wise Cloud shared, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "Wise Cloud dedicated, AD integration disabled" })]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestBuildDatabaseSecurityLogsExpectedPrincipalsMemebershipsAndPermissionsWhenTrialRun: Wise cloud shared hosted, AD integration disabled")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestBuildDatabaseSecurityLogsExpectedPrincipalsMemebershipsAndPermissionsWhenTrialRun: Wise cloud hosted dedicated, AD integration disabled")]
		public void TestBuildDatabaseSecurityLogsExpectedPrincipalsMemebershipsAndPermissionsWhenTrialRun(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool isAdIntegrated)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var loggerMock = new Mock<IntegrationLogging.ILogger>();

			var staffUserRoles = new[]
			{
				DbRoleTypes.CwRestrictedReaderRole,
				DbRoleTypes.DbDataWriterRole,
				DbRoleTypes.DbBackupOperatorRole,
				DbRoleTypes.CwHRMStaffRole,
			};

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: isAdIntegrated, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection, staffUserRoles.ToArray());

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					var expectedPrincipalsAndMemberships = new[]
					{
						$"{DbRoleTypes.CwReaderRole},R,db_datareader,",
						$"{DbRoleTypes.CwRestrictedReaderRole},R,,",
						$"{DbRoleTypes.CwRestrictedWriterRole},R,,",
						$"{DbRoleTypes.CwUnrestrictedWriterRole},R,,",
						$"{DbRoleTypes.CwHRMStaffRole},R,,",
						$"{dbReaderLogin},S,{DbRoleTypes.CwReaderRole},",
						$"{dbReaderLogin},S,db_datareader,",
						$"{dbWriterLogin},S,{DbRoleTypes.CwRestrictedWriterRole},",
						$"{dbRestrictedReaderLogin},S,{DbRoleTypes.CwRestrictedReaderRole},",
						$"{dbRestrictedWriterLogin},S,{DbRoleTypes.CwRestrictedWriterRole},",
						$"{dbUnrestrictedWriterLogin},S,{DbRoleTypes.CwUnrestrictedWriterRole},",
					}
					.AppendIf(
						!isAdIntegrated || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer,
						$"{sqlLoginName},S,{DbRoleTypes.CwRestrictedReaderRole},",
						$"{sqlLoginName},S,{DbRoleTypes.CwHRMStaffRole},"
						)
					.AppendIf(
						isAdIntegrated,
						$"{windowsLoginName},U,{DbRoleTypes.CwRestrictedReaderRole},",
						$"{windowsLoginName},U,{DbRoleTypes.CwHRMStaffRole},"
						)
					.AppendIf(
						(!isAdIntegrated || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer)
						&& databaseType == DatabaseType.UserRepository,
						$"{sqlLoginName},S,{DbRoleTypes.DbDataWriterRole},")
					.AppendIf(
						isAdIntegrated
						&& databaseType == DatabaseType.UserRepository,
						$"{windowsLoginName},U,{DbRoleTypes.DbDataWriterRole},")
					.AppendIf(
						!isAdIntegrated
						&& (databaseTestMode == DatabaseTestMode.SelfHostedOpen || databaseTestMode == DatabaseTestMode.SelfHostedLocked),
						$"{sqlLoginName},S,{DbRoleTypes.DbBackupOperatorRole},")
					.AppendIf(
						isAdIntegrated
						&& (databaseTestMode == DatabaseTestMode.SelfHostedOpen || databaseTestMode == DatabaseTestMode.SelfHostedLocked),
						$"{windowsLoginName},U,{DbRoleTypes.DbBackupOperatorRole},");

					var expectedPermissions = new[]
					{
						$"G,CONNECT,DATABASE,,{databaseName},,{dbReaderLogin},",
						$"G,CONNECT,DATABASE,,{databaseName},,{dbWriterLogin},",
						$"G,CONNECT,DATABASE,,{databaseName},,{dbRestrictedReaderLogin},",
						$"G,CONNECT,DATABASE,,{databaseName},,{dbRestrictedWriterLogin},",
						$"G,CONNECT,DATABASE,,{databaseName},,{dbUnrestrictedWriterLogin},",
						$"G,EXECUTE,DATABASE,,{databaseName},,{CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName)},",
						$"G,EXECUTE,DATABASE,,{databaseName},,{CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName)},",
						$"G,EXECUTE,DATABASE,,{databaseName},,{RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName)},",
						$"G,EXECUTE,DATABASE,,{databaseName},,{RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)},",
						$"G,EXECUTE,DATABASE,,{databaseName},,{UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)},",
						$"G,SHOWPLAN,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedReaderRole},",
						$"G,VIEW DEFINITION,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedReaderRole},",
						$"G,SELECT,OBJECT,sys,sql_expression_dependencies,,{DbRoleTypes.CwRestrictedReaderRole},",
						$"G,SHOWPLAN,DATABASE,,{databaseName},,{DbRoleTypes.CwReaderRole},",
						$"G,VIEW DEFINITION,DATABASE,,{databaseName},,{DbRoleTypes.CwReaderRole},",
						$"G,IMPERSONATE,USER,,{RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName)},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,IMPERSONATE,USER,,{CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName)},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,ALTER,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,CREATE SCHEMA,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,REFERENCES,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,SHOWPLAN,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,VIEW DATABASE STATE,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,VIEW DEFINITION,DATABASE,,{databaseName},,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,SELECT,OBJECT,sys,sql_expression_dependencies,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,DELETE,DATABASE,,{databaseName},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,EXECUTE,DATABASE,,{databaseName},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,INSERT,DATABASE,,{databaseName},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,SELECT,DATABASE,,{databaseName},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,UPDATE,DATABASE,,{databaseName},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,VIEW DATABASE STATE,DATABASE,,{databaseName},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,VIEW DEFINITION,DATABASE,,{databaseName},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,EXECUTE,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedReaderRole},",
						$"G,SELECT,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedReaderRole},",
						$"G,EXECUTE,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedReaderRole},",
						$"G,SELECT,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedReaderRole},",
						$"G,EXECUTE,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,SELECT,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,INSERT,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,DELETE,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,UPDATE,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,ALTER,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,CREATE SEQUENCE,SCHEMA,,dbo,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,EXECUTE,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,SELECT,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,INSERT,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,DELETE,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,UPDATE,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,ALTER,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,CREATE SEQUENCE,SCHEMA,,OrderTracking,,{DbRoleTypes.CwRestrictedWriterRole},",
						$"G,VIEW CHANGE TRACKING,SCHEMA,,dbo,,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,VIEW CHANGE TRACKING,SCHEMA,,{DbSecurity.SqlHrmSchema},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,VIEW CHANGE TRACKING,SCHEMA,,OrderTracking,,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,VIEW CHANGE TRACKING,SCHEMA,,{DbSecurity.SqlCdcSchema},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,VIEW CHANGE TRACKING,SCHEMA,,{DbSecurity.SqlStagingSchema},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,CREATE SEQUENCE,SCHEMA,,dbo,,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,CREATE SEQUENCE,SCHEMA,,{DbSecurity.SqlHrmSchema},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,CREATE SEQUENCE,SCHEMA,,OrderTracking,,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,CREATE SEQUENCE,SCHEMA,,{DbSecurity.SqlCdcSchema},,{DbRoleTypes.CwUnrestrictedWriterRole},",
						$"G,CREATE SEQUENCE,SCHEMA,,{DbSecurity.SqlStagingSchema},,{DbRoleTypes.CwUnrestrictedWriterRole},",
					}
					.AppendIf(
						databaseType == DatabaseType.Main,
						$"G,SELECT,SCHEMA,,hrm,,{DbRoleTypes.CwHRMStaffRole},")
					.AppendIf(
						!isAdIntegrated || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer,
						$"G,CONNECT,DATABASE,,{databaseName},,{sqlLoginName},")
					.AppendIf(
						isAdIntegrated,
						$"G,CONNECT,DATABASE,,{databaseName},,{windowsLoginName},")
					.AppendIf(
						(!isAdIntegrated || databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer)
						&& databaseType == DatabaseType.UserRepository,
						$"G,EXECUTE,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,SELECT,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,CREATE TYPE,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,CREATE SCHEMA,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,CREATE VIEW,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,CREATE FUNCTION,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,CREATE PROCEDURE,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,CREATE TABLE,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,INSERT,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,UPDATE,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,DELETE,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,REFERENCES,DATABASE,,{databaseName},,{sqlLoginName},",
						$"G,ALTER,SCHEMA,,dbo,,{sqlLoginName},")
					.AppendIf(
						isAdIntegrated
						&& databaseType == DatabaseType.UserRepository,
						$"G,EXECUTE,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,SELECT,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,CREATE TYPE,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,CREATE SCHEMA,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,CREATE VIEW,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,CREATE FUNCTION,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,CREATE PROCEDURE,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,CREATE TABLE,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,INSERT,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,UPDATE,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,DELETE,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,REFERENCES,DATABASE,,{databaseName},,{windowsLoginName},",
						$"G,ALTER,SCHEMA,,dbo,,{windowsLoginName},");

					// Act
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);

					// Assert
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, "Should contain database name information", Times.Once(), $"Building Sql security for database '{databaseName}'.");
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, "Should contain information about expected database principals and memberships.", Times.Once(), expectedPrincipalsAndMemberships.ToArray());
					loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, "Should contain information about expected database permissions.", Times.Once(), expectedPermissions.ToArray());
				}
			}
		}

		#region Filter testing

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "", "AD user with matching domain name and emtpy user prefix name when AD integrated, self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "", "AD user with matching domain name and emtpy user prefix name when AD integration disabled, self hosted open" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "OtherUserPrefix_", "AD user with matching domain name but not user prefix name when AD integrated, self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "OtherUserPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled, self hosted open" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "ADTest_", "AD user with matching domain name and user prefix name when AD integration disabled, self hosted open" })]

		[TestCaseSource(nameof(AllCombinationsOfSharedSingleRefDbDatabaseTestModeUserPrefixAndIsAdIntegratedFlag))]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name and empty user prefix name, AD integrated, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, "", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name and empty user prefix name, AD integrated, Wise cloud shared hosted, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "OtherUserPrefix_", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name but not user prefix name, AD integrated, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, "OtherUserPrefix_", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name but not user prefix name, AD integrated, Wise cloud shared hosted, Shared reference database")]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name and empty user prefix name when AD integration disabled, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, "", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name and empty user prefix name when AD integration disabled, Wise cloud shared hosted, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "OtherUserPrefix_", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name but not user prefix name when AD integration disabled, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, "OtherUserPrefix_", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name but not user prefix name when AD integration disabled, Wise cloud shared hosted, Shared reference database")]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "ADTest_", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name and user prefix name when AD integration disabled, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, "ADTest_", TestName = "TestWhenADUserNotMatchingStaffIsNotDropped: AD user with matching domain name and user prefix name when AD integration disabled, Wise cloud shared hosted, Shared reference database")]
		public void TestWhenADUserNotMatchingStaffIsNotDropped(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool adIntegrated, string userPrefixName)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			AssertionsHelper.AssumeStaffMissing(adminConnection, TestConstants.ADTestUserAccount.Name);

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			using (var productRegistrationMock = ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: adIntegrated, userPrefixName, Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE USER [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "", "AD user with matching domain name when user prefix name is emtpy, AD integration disabled and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "", "AD user with matching domain name when user prefix name is emtpy, AD integration disabled and Wise cloud shared hosted" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and Wise cloud shared hosted" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "", "AD user with matching domain name when user prefix name is emtpy, AD integration disabled and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and self hosted locked" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "OtherPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "OtherPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled and Wise cloud shared hosted" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and Wise cloud shared hosted" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "OtherPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and self hosted locked" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "ADTest_", " AD user with matching domain name and user prefix name when AD integration disabled and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "ADTest_", " AD user with matching domain name and user prefix name when AD integration disabled and Wise cloud shared hosted" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, Wise cloud shared hosted" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "ADTest_", " AD user with matching domain name and user prefix name when AD integration disabled and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, self hosted locked" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, self hosted open" })]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "ADTest_", TestName = "TestWhenADUserNotMatchingStaffIsDropped: AD user with matching domain name and user prefix name, AD integrated, Wise cloud hosted dedicated")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, "ADTest_", TestName = "TestWhenADUserNotMatchingStaffIsDropped: AD user with matching domain name and user prefix name, AD integrated, Wise cloud shared hosted")]
		public void TestWhenADUserNotMatchingStaffIsDropped(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool adIntegrated, string userPrefixName)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			AssertionsHelper.AssumeStaffMissing(adminConnection, TestConstants.ADTestUserAccount.Name);

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: adIntegrated, userPrefixName, Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				Assume.That(EnvProxy.IsHostedWithCargowise, Is.EqualTo(databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer));
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE USER [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			}
		}

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "", "AD user with matching domain name and emtpy user prefix name when AD integration disabled, self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "OtherUserPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled, self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "ADTest_", "AD user with matching domain name and user prefix name when AD integration disabled, self hosted open" })]

		[TestCaseSource(nameof(AllCombinationsOfSharedSingleRefDbDatabaseTestModeUserPrefixAndIsAdIntegratedFlag))]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsNotDropped: AD user with matching domain name and empty user prefix name when AD integration disabled, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, "", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsNotDropped: AD user with matching domain name and empty user prefix name when AD integration disabled, Wise cloud shared hosted, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "OtherUserPrefix_", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsNotDropped: AD user with matching domain name but not user prefix name when AD integration disabled, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, "OtherUserPrefix_", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsNotDropped: AD user with matching domain name but not user prefix name when AD integration disabled, Wise cloud shared hosted, Shared reference database")]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "ADTest_", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsNotDropped: AD user with matching domain name and user prefix name when AD integration disabled, Wise cloud hosted dedicated, Shared reference database")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, "ADTest_", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsNotDropped: AD user with matching domain name and user prefix name when AD integration disabled, Wise cloud shared hosted, Shared reference database")]
		public void TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsNotDropped(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool adIntegrated, string userPrefixName)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(TestConstants.ADTestUserAccount.Name, TestConstants.Domain, Guid.Parse(TestConstants.ADTestUserAccount.Guid), adminConnection);
			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assert.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.Zero, $"Staff member '{TestConstants.ADTestUserAccount.Name}' should have no database access group roles.");

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: adIntegrated, userPrefixName, Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE USER [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "", "AD user with matching domain name when user prefix name is emtpy, AD integration disabled and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "", "AD user with matching domain name when user prefix name is emtpy, AD integration disabled and Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "", "AD user with matching domain name when user prefix name is emtpy, AD integration disabled and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and self hosted open" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "OtherPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "OtherPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled and Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "OtherPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and self hosted open" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "ADTest_", " AD user with matching domain name and user prefix name when AD integration disabled and Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, Wise cloud hosted dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "ADTest_", " AD user with matching domain name and user prefix name when AD integration disabled and Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "ADTest_", " AD user with matching domain name and user prefix name when AD integration disabled and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, self hosted open" })]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsDropped: AD user with matching domain name and empty user prefix name, AD integrated, Wise cloud hosted dedicated")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, "", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsDropped: AD user with matching domain name and empty user prefix name, AD integrated, Wise cloud shared hosted")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "OtherUserPrefix_", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsDropped: AD user with matching domain name but not user prefix name, AD integrated, Wise cloud hosted dedicated")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, "OtherUserPrefix_", TestName = "TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsDropped: AD user with matching domain name but not user prefix name, AD integrated, Wise cloud shared hosted")]
		public void TestWhenADUserMatchingStaffWithNoDatabaseAccessGroupRolesIsDropped(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool adIntegrated, string userPrefixName)
		{
			// Arrange
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(TestConstants.ADTestUserAccount.Name, TestConstants.Domain, Guid.Parse(TestConstants.ADTestUserAccount.Guid), adminConnection);
			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assert.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.Zero, $"Staff member '{TestConstants.ADTestUserAccount.Name}' should have no database access group roles.");

			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: adIntegrated, userPrefixName, Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				Assume.That(EnvProxy.IsHostedWithCargowise, Is.EqualTo(databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer));
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE USER [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			}
		}

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and Wise cloud dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "", "AD user with matching domain name when user prefix name is emtpy, AD integration disabled and self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "", "AD user with matching domain name when user name prefix is empty, AD integrated and self hosted open" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and Wise cloud dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "OtherPrefix_", "AD user with matching domain name but not user prefix name when AD integration disabled and self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "OtherPrefix_", "AD user with matching domain name but not user prefix name, AD integrated and self hosted open" })]

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, Wise cloud dedicated" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, Wise cloud hosted shared" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "ADTest_", " AD user with matching domain name and user prefix name when AD integration disabled and self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "ADTest_", "AD user with matching domain name and user prefix name, AD integrated, self hosted open" })]

		[TestCaseSource(nameof(AllCombinationsOfSharedSingleRefDbDatabaseTestModeUserPrefixAndIsAdIntegratedFlag))]
		[TestCaseSource(nameof(AllCombinationsOfSharedRefDbWiseCloudHostedDatabaseTestModeUserPrefixAndIsAdIntegratedFlag))]
		public void TestWhenADUserMatchingStaffWithDatabaseAccessGroupRolesIsNotDropped(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool adIntegrated, string userNamePrefix)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection,
				new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.CwHRMStaffRole });

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assert.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should have some database access group roles.");

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: adIntegrated, userNamePrefix, Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				Assume.That(EnvProxy.IsHostedWithCargowise, Is.EqualTo(databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer));
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE USER [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");

				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, "ADTest_", "AD user with matching user prefix name, Wise cloud shared" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, "OtherUserPrefix_", "AD user with matching domain name but not user prefix name, Wise cloud shared" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, "", "AD user with matching domain name when user prefix name is empty, Wise cloud shared" })]

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, "ADTest_", "AD user with matching user prefix name, Wise cloud dedicated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, "OtherUserPrefix_", "AD user with matching domain name but not user prefix name, Wise cloud dedicated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, "", "AD user with matching domain name when user prefix name is empty, Wise cloud dedicated" })]

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.SelfHostedLocked, "ADTest_", "AD user with matching user prefix name, self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.SelfHostedLocked, "OtherUserPrefix_", "AD user with matching domain name but not user prefix name, self hosted locked" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix), new object[] { DatabaseTestMode.SelfHostedLocked, "", "AD user with matching domain name when user prefix name is empty, self hosted locked" })]
		public void TestWhenADUserMatchingStaffWithDatabaseAccessGroupRolesIsDroppedIfNotAdItegrated(DatabaseType databaseType, DatabaseTestMode databaseTestMode, string userNamePrefix)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection,
				new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.CwHRMStaffRole });

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assert.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should have some database access group roles.");

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, userNamePrefix, Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE USER [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			}
		}

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, true, "Self hosted locked, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, true, "Self hosted open, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "Wise Cloud shared, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "Wise Cloud dedicated, AD integrated" })]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise cloud shared hosted, AD integrated")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise cloud hosted dedicated, AD integrated")]

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "Self hosted locked, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "Self hosted open, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "Wise Cloud shared, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "Wise Cloud dedicated, AD integration disabled" })]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise cloud shared hosted, AD integration disabled")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise cloud hosted dedicated, AD integration disabled")]
		public void TestUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool adIntegrated)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				adIntegrated ? TestConstants.Domain : null,
				adIntegrated ? Guid.Parse(TestConstants.ADTestUserAccount.Guid) : null,
				adminConnection);

			var sqlLoginNotMatchingStaff = Helper.GetEnterpriseLoginFullName("BobbyTheLoser", Db.DatabaseName);
			var sqlLoginMatchingStaffWithNoRoles = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, sqlLoginMatchingStaffWithNoRoles), Is.Zero, $"Staff member '{TestConstants.ADTestUserAccount.Name} should exist but not belong to any group with database access role(s).");
			AssertionsHelper.AssumeStaffMissing(adminConnection, "BobbyTheLoser");

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: adIntegrated, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;

CREATE LOGIN [{sqlLoginMatchingStaffWithNoRoles}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{sqlLoginMatchingStaffWithNoRoles}')
	CREATE USER [{sqlLoginMatchingStaffWithNoRoles}] FROM LOGIN [{sqlLoginMatchingStaffWithNoRoles}];

CREATE LOGIN [{sqlLoginNotMatchingStaff}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{sqlLoginNotMatchingStaff}')
	CREATE USER [{sqlLoginNotMatchingStaff}] FROM LOGIN [{sqlLoginNotMatchingStaff}];
");

				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, $"{sqlLoginMatchingStaffWithNoRoles}", "S");
				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, $"{sqlLoginNotMatchingStaff}", "S");

				Assume.That(EnvProxy.IsHostedWithCargowise, Is.EqualTo(databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer));

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{sqlLoginMatchingStaffWithNoRoles}", "S");
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{sqlLoginNotMatchingStaff}", "S");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, $"{sqlLoginMatchingStaffWithNoRoles}");
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, $"{sqlLoginNotMatchingStaff}");
			}
		}

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, true, "Wise Cloud shared, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, "Wise Cloud dedicated, AD integrated" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedLocked, false, "Self hosted locked, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.SelfHostedOpen, false, "Self hosted open, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudSharedServer, false, "Wise Cloud shared, AD integration disabled" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag), new object[] { DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, "Wise Cloud dedicated, AD integration disabled" })]

		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, Wise cloud shared hosted, AD integrated")]
		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, Wise cloud hosted dedicated, AD integated")]
		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.SelfHostedLocked, true, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, self hosted locked, AD integrated")]
		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.SelfHostedOpen, true, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, self hosted open, AD integated")]

		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, Wise cloud shared hosed, AD integration disabled")]
		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, Wise cloud hosted dedicated, AD integation disabled")]
		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.SelfHostedLocked, false, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, self hosted locked, AD integration disabled")]
		[TestCase(DatabaseType.SingleSharedRef, DatabaseTestMode.SelfHostedOpen, false, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: SharedSingleRefDb, self hosted open, AD integation disabled")]

		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Shared ref db, Wise cloud shared hosted, AD integrated")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Shared ref db, Wise cloud hosted dedicated, AD integrated")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Shared ref db, Wise cloud hosted dedicated, AD integation disabled")]
		[TestCase(DatabaseType.SharedRef, DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Shared ref db, Wise cloud shared hosed, AD integration disabled")]
		public void TestWhenUsersStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped(DatabaseType databaseType, DatabaseTestMode databaseTestMode, bool adIntegrated)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				adIntegrated ? TestConstants.Domain : null,
				adIntegrated ? Guid.Parse(TestConstants.ADTestUserAccount.Guid) : null,
				adminConnection,
				DbRoleTypes.CwRestrictedReaderRole);

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name} should exist and belong to a group with database access role(s).");

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: adIntegrated, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteName()} FROM WINDOWS;
CREATE LOGIN [{sqlLoginName}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{sqlLoginName}')
	CREATE USER [{sqlLoginName}] FROM LOGIN [{sqlLoginName}];
");

				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, $"{sqlLoginName}", "S");

				Assume.That(EnvProxy.IsHostedWithCargowise, Is.EqualTo(databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || databaseTestMode == DatabaseTestMode.HostedInWiseCloudDedicatedServer));

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{sqlLoginName}", "S");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{sqlLoginName}", "S");
			}
		}

		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDataBaseType), new object[] { DatabaseTestMode.SelfHostedOpen, "Self hosted open" })]
		[TestCaseSource(nameof(CombinationsOfExclusiveDatabaseTestModeDataBaseType), new object[] { DatabaseTestMode.SelfHostedLocked, "Self hosted locked" })]
		public void TestWhenUserStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesIsDroppedWhenSelfHostedADIntegrated(DatabaseType databaseType, DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var databaseName = Helper.DatabaseNameFromDatabaseType(databaseType);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection,
				DbRoleTypes.CwRestrictedReaderRole);

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name} should exist and belong to a group with database access role(s).");

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
			var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

			// Act
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
			{
				adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{windowsLoginName}] FROM WINDOWS;
CREATE LOGIN [{sqlLoginName}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
CREATE USER [{sqlLoginName}] FROM LOGIN [{sqlLoginName}];
");

				DatabaseAssertionsHelper.AssumePrincipalExists(adminConnection, $"{sqlLoginName}", "S");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: true);
				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, $"{sqlLoginName}", "S");

				sqlSecurityManager.BuildDatabaseSecurity(adminConnection, databaseName, trialRun: false);

				// Assert
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, $"{sqlLoginName}");
			}
		}

		#endregion Filter testing

		#region Implementation

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			adminConnection = Db.NewAdminConnection();
			Helper.EnsureExtraDatabasesWithSchemas(adminConnection);
		}

		[SetUp]
		public void SetUp()
		{
			CleanUp();
			CreateServerTestEntities(adminConnection, Db.DatabaseName);
		}

		[TearDown]
		public void TearDown()
		{
			CleanUp();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			adminConnection?.Dispose();
		}

		void CleanUp()
		{
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);
			Helper.CleanUpGlbTables(adminConnection);

			foreach (var database in Helper.Databases())
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(database))
				{
					Helper.DropDatabasePrincipals(adminConnection);
				}
			}
		}

		AdminConnection adminConnection;

		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

		static IEnumerable<TestCaseData> AllCombinationsOfSharedRefDbWiseCloudHostedDatabaseTestModeUserPrefixAndIsAdIntegratedFlag()
		{
			foreach (var databaseTestMode in new[] { DatabaseTestMode.HostedInWiseCloudSharedServer, DatabaseTestMode.HostedInWiseCloudDedicatedServer })
			{
				yield return new TestCaseData(
					DatabaseType.SharedRef,
					databaseTestMode,
					true,
					"")
				.SetName($"{{m}}: AD user with matching domain name and emtpy user prefix name in SharedRefDb database, AD integrated, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SharedRef,
					databaseTestMode,
					false,
					"")
				.SetName($"{{m}}: AD user with matching domain name and emtpy user prefix name in SharedRefDb database when AD integration disabled, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SharedRef,
					databaseTestMode,
					true,
					"OtherUserPrefix_")
				.SetName($"{{m}}: AD user with matching domain name but not user prefix name in SharedRefDb database, AD integrated, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SharedRef,
					databaseTestMode,
					false,
					"OtherUserPrefix_")
				.SetName($"{{m}}: AD user with matching domain name but not user prefix name, SharedRefDb database when AD integration disabled, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SharedRef,
					databaseTestMode,
					true,
					"ADTest_")
				.SetName($"{{m}}: AD user with matching domain name and user prefix name in SharedRefDb database, AD integrated, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SharedRef,
					databaseTestMode,
					false,
					"ADTest_")
				.SetName($"{{m}}: AD user with matching domain name and user prefix name in SharedRefDb database when AD integration disabled, {Helper.DatabaseTestModeDescription(databaseTestMode)}");
			}
		}

		static IEnumerable<TestCaseData> AllCombinationsOfSharedSingleRefDbDatabaseTestModeUserPrefixAndIsAdIntegratedFlag()
		{
			foreach (DatabaseTestMode databaseTestMode in Enum.GetValues(typeof(DatabaseTestMode)))
			{
				yield return new TestCaseData(
					DatabaseType.SingleSharedRef,
					databaseTestMode,
					true,
					"")
				.SetName($"{{m}}: AD user with matching domain name and emtpy user prefix name in SharedSingleRefDb database, AD integrated, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SingleSharedRef,
					databaseTestMode,
					false,
					"")
				.SetName($"{{m}}: AD user with matching domain name and emtpy user prefix name in SharedSingleRefDb database when AD integration disabled, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SingleSharedRef,
					databaseTestMode,
					true,
					"OtherUserPrefix_")
				.SetName($"{{m}}: AD user with matching domain name but not user prefix name in SharedSingleRefDb database, AD integrated, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SingleSharedRef,
					databaseTestMode,
					false,
					"OtherUserPrefix_")
				.SetName($"{{m}}: AD user with matching domain name but not user prefix name, SharedSingleRefDb database when AD integration disabled, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SingleSharedRef,
					databaseTestMode,
					true,
					"ADTest_")
				.SetName($"{{m}}: AD user with matching domain name and user prefix name in SharedSingleRefDb database, AD integrated, {Helper.DatabaseTestModeDescription(databaseTestMode)}");

				yield return new TestCaseData(
					DatabaseType.SingleSharedRef,
					databaseTestMode,
					false,
					"ADTest_")
				.SetName($"{{m}}: AD user with matching domain name and user prefix name in SharedSingleRefDb database when AD integration disabled, {Helper.DatabaseTestModeDescription(databaseTestMode)}");
			}
		}

		static IEnumerable<TestCaseData> CombinationsOfStaffUserRolesSetAndDatabaseTestMode(DatabaseType databaseType, DatabaseTestMode databaseTestMode)
		{
			var staffUserDescriptionRolesMap = new Dictionary<string, string[]>
			{
				{ "Hrm staff", new[] { DbRoleTypes.CwHRMStaffRole } },
				{ "Database developer", new[] { DbRoleTypes.DbDataWriterRole } },
				{ "Database reader", new[] { DbRoleTypes.CwRestrictedReaderRole } },
				{ "Database backup operator", new[] { DbRoleTypes.DbBackupOperatorRole } },
				{ "Hrm, database developer, reader and backup operator", new[] { DbRoleTypes.CwHRMStaffRole, DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole } },
				{ "Database developer, reader and backup operator", new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole } },
				{ "Database reader and backup operator", new[] { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole } },
				{ "Database developer and backup operator", new[] { DbRoleTypes.DbDataWriterRole, DbRoleTypes.DbBackupOperatorRole } },
			};

			foreach (var staffUserRoles in staffUserDescriptionRolesMap)
			{
				yield return new TestCaseData(
					databaseType,
					databaseTestMode,
					staffUserRoles.Value)
					.SetName($"{{m}}: {staffUserRoles.Key} in {databaseType} on {Helper.DatabaseTestModeDescription(databaseTestMode)} server");
			}
		}

		static IEnumerable<TestCaseData> CombinationsOfExclusiveDatabaseTestModeDataBaseType(DatabaseTestMode databaseTestMode, string description)
		{
			foreach (var databaseType in Helper.GetExclusiveDatabaseTypes())
			{
				yield return new TestCaseData(databaseType, databaseTestMode)
					.SetName($"{{m}}: {description}, {databaseType}");
			}
		}

		static IEnumerable<TestCaseData> CombinationsOfDatabaseTestModeExclusiveDatabaseTypeUserPrefixAndIsAdIntegratedFlag(DatabaseTestMode databaseTestMode, bool adIntegrated, string userPrefix, string description)
		{
			foreach (var databaseType in Helper.GetExclusiveDatabaseTypes())
			{
				yield return new TestCaseData(databaseType, databaseTestMode, adIntegrated, userPrefix)
					.SetName($"{{m}}: {description}, {databaseType} database");
			}
		}

		static IEnumerable<TestCaseData> CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndUserPrefix(DatabaseTestMode databaseTestMode, string userPrefix, string description)
		{
			foreach (var databaseType in Helper.GetExclusiveDatabaseTypes())
			{
				yield return new TestCaseData(databaseType, databaseTestMode, userPrefix)
					.SetName($"{{m}}: {description}, {databaseType} database");
			}
		}

		static IEnumerable<TestCaseData> CombinationsOfExclusiveDatabaseTestModeDatabaseTypeAndIsAdIntegratedFlag(DatabaseTestMode databaseTestMode, bool adIntegrated, string description)
		{
			foreach (var databaseType in Helper.GetExclusiveDatabaseTypes())
			{
				yield return new TestCaseData(databaseType, databaseTestMode, adIntegrated)
					.SetName($"{{m}}: {description}, {databaseType} database");
			}
		}

		#endregion Implementation

	}
}
