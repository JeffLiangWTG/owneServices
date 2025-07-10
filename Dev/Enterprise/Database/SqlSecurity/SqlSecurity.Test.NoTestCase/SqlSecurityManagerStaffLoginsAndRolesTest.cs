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
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using IntegrationLogging = Enterprise.Integration;

[assembly: UsesConstants(typeof(TestConstants))]
namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	[Property("DAT:CapabilityRequirements", "SQL2019")]
	class SqlSecurityManagerStaffLoginsAndRoleTest
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
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithSingleQuote);

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLoginWithDoubleSingleQuote);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLoginWithDoubleSingleQuote), Is.GreaterThan(0), $"Staff member '{staffLoginWithDoubleSingleQuote}' should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginNameWithDoubleSingleQuote);

			var errorReporterMock = new Mock<IErrorReporter>();
			ErrorReporter.Clear();

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				// Act
				// Assert
				Assert.That(() => sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false), Throws.Nothing);

				errorReporterMock.Verify(errorReporter => errorReporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
				errorReporterMock.Verify(errorReporter => errorReporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameWithSingleQuote, "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginNameWithDoubleSingleQuote, "S");
			}
		}

		[TestCaseSource(nameof(StaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInWiseCloudHostedEnvironmentTestCaseSource))]
		[TestCaseSource(nameof(StaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInSelfHostedEnvironmentTestCaseSource))]
		public void TestStaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesIfAdIntegrationDisabled(DatabaseTestMode databaseTestMode, IEnumerable<string> staffDbRols, Dictionary<string, string[]> expectedPermissios)
		{
			// Arrange
			var staffLogin = Guid.NewGuid().ToString();

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				staffDbRols.ToArray());

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			var sqlLoginPasswordHashInStaffRecord = adminConnection.ExecuteScalar<byte[]>(
				"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLogin, GlbStaffSchema.GS_LoginName));

			AssertionsHelper.AssumeStaffExists(adminConnection, staffLogin);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, staffLogin), Is.GreaterThan(0), $"Staff member '{staffLogin}' should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(false, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);

				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginName, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, sqlLoginName);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, sqlLoginName, expectedPermissios);

				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, sqlLoginName, Db.DatabaseName);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(adminConnection, sqlLoginName);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(adminConnection, sqlLoginName);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, sqlLoginName, "us_english");
				AssertionsHelper.AssertHashMatchesLoginPasswordHash(
					adminConnection,
					$"Password hash for the created sql login '{sqlLoginName}' must match GS_SqlLoginPasswordHash value in the correspondign staff record '{staffLogin}'.",
					sqlLoginName, sqlLoginPasswordHashInStaffRecord);
			}
		}

		#endregion not AD integrated

		#region AD integrated

		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestStaffDbUserCreatedFromWindows_HavingShortenedADSamAccountName_AreNotDropped: self hosted locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestStaffDbUserCreatedFromWindows_HavingShortenedADSamAccountName_AreNotDropped: self hosted open")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestStaffDbUserCreatedFromWindows_HavingShortenedADSamAccountName_AreNotDropped: hosted in Wise cloud shared")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestStaffDbUserCreatedFromWindows_HavingShortenedADSamAccountName_AreNotDropped: hosted in Wise could dedicated")]
		public void TestStaffDbLoginShouldBeCreatedFromWindowsShortenedADSamAccountName(DatabaseTestMode databaseTestMode)
		{
			// Arrange
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

				AssertionsHelper.AssumePrincipalMissing(adminConnection, Helper.ADTestUserLongNameB.NameWithDomain);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000);

				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, Helper.ADTestUserLongNameB.NameWithDomain);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, Helper.ADTestUserLongNameB.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCaseSource(nameof(StaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInWiseCloudHostedEnvironmentTestCaseSource))]
		public void TestStaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInWiseCloudHostedEnvironmentIfAdIntegrationEnabled(DatabaseTestMode databaseTestMode, IEnumerable<string> staffDbRols, Dictionary<string, string[]> expectedPermissios)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffDbRols.ToArray());

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);

				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginName, "U");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, windowsLoginName);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, windowsLoginName, expectedPermissios);

				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, windowsLoginName, Db.DatabaseName);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, windowsLoginName, "us_english");

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginName, "S");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, sqlLoginName);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, sqlLoginName, expectedPermissios);

				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, sqlLoginName, Db.DatabaseName);
				AssertionsHelper.AssertSqlLoginExpirationPolicyIsDisabled(adminConnection, sqlLoginName);
				AssertionsHelper.AssertSqlLoginPasswordPolicyIsDisabled(adminConnection, sqlLoginName);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, sqlLoginName, "us_english");
			}
		}

		[TestCaseSource(nameof(StaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInSelfHostedEnvironmentTestCaseSource))]
		public void TestStaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInSelfHostedEnvironmentIfAdIntegrationEnabled(DatabaseTestMode databaseTestMode, IEnumerable<string> staffDbRols, Dictionary<string, string[]> expectedPermissios)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffDbRols.ToArray());

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);
				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);

				AssertionsHelper.AssumePrincipalMissing(adminConnection, sqlLoginName);
				AssertionsHelper.AssumePrincipalMissing(adminConnection, windowsLoginName);

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert

				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, windowsLoginName, "U");
				AssertionsHelper.AssertServerPrincipalHasNoRoles(adminConnection, windowsLoginName);
				AssertionsHelper.AssertServerPrincipalHasServerPermissionsOnly(adminConnection, windowsLoginName, expectedPermissios);

				AssertionsHelper.AssertSqlLoginDefaultDatabaseEquals(adminConnection, windowsLoginName, Db.DatabaseName);
				AssertionsHelper.AssertSqlLoginLanguageEquals(adminConnection, windowsLoginName, "us_english");

				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginName);
			}
		}

		#endregion AD integrated

		[TestCase(DatabaseTestMode.SelfHostedOpen, false, TestName = "TestBuildServerSecurityLogsExpectedServerPrincipalsMembershipsAndPermissionsWhenTrialRun: Self hosted open, with no AD integration")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, true, TestName = "TestBuildServerSecurityLogsExpectedServerPrincipalsMembershipsAndPermissionsWhenTrialRun: Self hosted open, with AD integration")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, false, TestName = "TestBuildServerSecurityLogsExpectedServerPrincipalsMembershipsAndPermissionsWhenTrialRun: Self hosted locked, with no AD integration")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, true, TestName = "TestBuildServerSecurityLogsExpectedServerPrincipalsMembershipsAndPermissionsWhenTrialRun: Self hosted locked, with AD integration")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestBuildServerSecurityLogsExpectedServerPrincipalsMembershipsAndPermissionsWhenTrialRun: Hosted in Wise cloud, with no AD integration")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestBuildServerSecurityLogsExpectedServerPrincipalsMembershipsAndPermissionsWhenTrialRun: Hosted in Wise cloud, with AD integration")]
		public void TestBuildServerSecurityLogsExpectedServerPrincipalsMembershipsAndPermissionsWhenTrialRun(DatabaseTestMode databaseTestMode, bool isAdIntegrated)
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var staffRoles = new[]
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
				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection, staffRoles.ToArray());

				var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
				var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");
				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);

				var localSqlLoginName = sqlLoginName.Replace("\\", "\\\\");
				var localWindowsLoginName = windowsLoginName.Replace("\\", "\\\\");

				var expectedPrincipals = new List<string>()
				{
					$"\"MemberName\":\"{dbReaderLogin}\",\"DefaultDatabaseName\":\"{Db.SqlMasterDb}\",\"DefaultLanguageName\":\"us_english\"",
					$"\"MemberName\":\"{dbWriterLogin}\",\"DefaultDatabaseName\":\"{Db.SqlMasterDb}\",\"DefaultLanguageName\":\"us_english\"",
					$"\"MemberName\":\"{dbRestrictedReaderLogin}\",\"DefaultDatabaseName\":\"{Db.SqlMasterDb}\",\"DefaultLanguageName\":\"us_english\"",
					$"\"MemberName\":\"{dbRestrictedWriterLogin}\",\"DefaultDatabaseName\":\"{Db.SqlMasterDb}\",\"DefaultLanguageName\":\"us_english\"",
					$"\"MemberName\":\"{dbUnrestrictedWriterLogin}\",\"DefaultDatabaseName\":\"{Db.SqlMasterDb}\",\"DefaultLanguageName\":\"us_english\"",
				}
				.AppendIf(
					databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || !isAdIntegrated,
					$"\"MemberName\":\"{localSqlLoginName}\",\"DefaultDatabaseName\":\"{Db.DatabaseName}\",\"DefaultLanguageName\":\"us_english\"")
				.AppendIf(
					isAdIntegrated,
					$"\"MemberName\":\"{localWindowsLoginName}\",\"DefaultDatabaseName\":\"{Db.DatabaseName}\",\"DefaultLanguageName\":\"us_english\"");

				var expectedPermissions = new List<string>()
				{
					$"GRANT CONNECT SQL TO [{dbReaderLogin}]",
					$"GRANT CONNECT SQL TO [{dbWriterLogin}]",
					$"GRANT CONNECT SQL TO [{dbRestrictedReaderLogin}]",
					$"GRANT CONNECT SQL TO [{dbRestrictedWriterLogin}]",
					$"GRANT CONNECT SQL TO [{dbUnrestrictedWriterLogin}]",
				}.AppendIf(databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer || !isAdIntegrated,
					$"GRANT CONNECT SQL TO [{sqlLoginName}]")
				.AppendIf(
					isAdIntegrated,
					$"GRANT CONNECT SQL TO [{windowsLoginName}]")
				.AppendIf(
					databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer,
					$"DENY VIEW ANY DATABASE TO [{sqlLoginName}]")
				.AppendIf(
					databaseTestMode == DatabaseTestMode.HostedInWiseCloudSharedServer && isAdIntegrated,
					$"DENY VIEW ANY DATABASE TO [{windowsLoginName}]")
				.AppendIf(
					(databaseTestMode == DatabaseTestMode.SelfHostedLocked || databaseTestMode == DatabaseTestMode.SelfHostedOpen)
					&& !isAdIntegrated,
					$"GRANT ALTER TRACE TO [{sqlLoginName}]",
					$"GRANT VIEW SERVER STATE TO [{sqlLoginName}]",
					$"GRANT ALTER ANY EVENT SESSION TO [{sqlLoginName}]",
					$"GRANT VIEW ANY DEFINITION TO [{sqlLoginName}]")
				.AppendIf(
					(databaseTestMode == DatabaseTestMode.SelfHostedLocked || databaseTestMode == DatabaseTestMode.SelfHostedOpen)
					&& isAdIntegrated,
					$"GRANT ALTER TRACE TO [{windowsLoginName}]",
					$"GRANT VIEW SERVER STATE TO [{windowsLoginName}]",
					$"GRANT ALTER ANY EVENT SESSION TO [{windowsLoginName}]",
					$"GRANT VIEW ANY DEFINITION TO [{windowsLoginName}]");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);

				// Assert
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, "There should be log message for server starting building sql security.", Times.Once(), $"Building Sql security for server '{Db.ServerName}' with main database name '{Db.DatabaseName}'.");
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, "There should be log message for server completed building sql security.", Times.Once(), $"Building Sql security for server '{Db.ServerName}' with main database name '{Db.DatabaseName}' took");
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, "Server proposed principals and memberships should be reported.", Times.Once(), $"Synchronizer debug information for server '{Db.ServerName}' with main database name '{Db.DatabaseName}'");
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Debug, "Should contain information about expected server principals and memberships.", Times.Once(), expectedPrincipals.ToArray());
				loggerMock.VerifyCalled(IntegrationLogging.LogType.Information, "Should contain information about expected server permissions.", Times.Once(), expectedPermissions.ToArray());
			}
		}

		#region Filter testing

		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeAndUserPrefix), new object[] { "OtherPrefix_", "Other User prefix name" })]
		[TestCaseSource(nameof(CombinationsOfDatabaseTestModeAndUserPrefix), new object[] { "", "Empty User prefix name" })]
		public void TestADLoginStartingFromDomainNameSlashNotFollowedByUserPreffixNameAndNotMatchingStaffIsNotDroppedWhenAdIntegrated(DatabaseTestMode databaseTestMode, string userPrefixName)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, userPrefixName, Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				AssertionsHelper.AssumeStaffMissing(adminConnection, TestConstants.ADTestUserAccount.Name);
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCaseSource(nameof(DatabaseTestModes))]
		public void TestADLoginStartingFromDomainNameSlashNotFollowedByUserPreffixNameAndMatchingActiveStaffWithNoDbRolesIsDroppedWhenAdIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection);

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, "OtherPrefix_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.Zero, $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist but not belong to any group with database access role(s).");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			}
		}

		[TestCaseSource(nameof(DatabaseTestModes))]
		public void TestADLoginStartingFromDomainNameSlashNotFollowedByUserPreffixNameAndMatchingInactiveStaffWithNoDbRolesIsNotDroppedWhenAdIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection);

			adminConnection.ExecuteNonQuery($@"
UPDATE dbo.GlbStaff SET GS_IsActive = 0, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_LoginName = N'{TestConstants.ADTestUserAccount.Name}'
");

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, "OtherPrefix_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.Zero, $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist but not belong to any group with database access role(s).");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCaseSource(nameof(DatabaseTestModes))]
		public void TestADLoginStartingFromDomainNameSlashUserPreffixNameAndNotMatchingStaffIsNotDroppedWhenNotADIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: false, "ADTest_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				AssertionsHelper.AssumeStaffMissing(adminConnection, TestConstants.ADTestAdminAccount.Name);
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCaseSource(nameof(DatabaseTestModes))]
		public void TestADLoginStartingFromDomainNameSlashUserPreffixNameButNotMatchingStaffIsDroppedWhenAdIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, "ADTest_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");

				AssertionsHelper.AssumeStaffMissing(adminConnection, TestConstants.ADTestAdminAccount.Name);
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			}
		}

		[TestCaseSource(nameof(DatabaseTestModes))]
		public void TestADLoginStartingFromDomainNameSlashUserPreffixNameMatchingStaffWithNoDbRolesIsDroppedWhenAdIntegrationEnabled(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection);

			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, "ADTest_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.Zero, $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist but not belong to any group with database access role(s).");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, TestName = "TestADLoginStartingFromDomainNameSlashUserPreffixNameMatchingStaffWithDbRolesIsNotDropped: Wise cloud shared dedicated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, TestName = "TestADLoginStartingFromDomainNameSlashUserPreffixNameMatchingStaffWithDbRolesIsNotDropped: Wise cloud shared hosted")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestADLoginStartingFromDomainNameSlashUserPreffixNameMatchingStaffWithDbRolesIsNotDropped: Self hosted open")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestADLoginStartingFromDomainNameSlashUserPreffixNameMatchingStaffWithDbRolesIsNotDropped: Self hosted locked")]
		public void TestADLoginStartingFromDomainNameSlashUserPreffixNameMatchingStaffWithDbRolesIsNotDroppedWnenAdIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(true, "ADTest_", Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection,
					DbRoleTypes.DbBackupOperatorRole);

				adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}')
	CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name}' should exist and belong to a group with database access role(s).");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				// Act
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedLocked, true, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Self hosted locked, AD integrated")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, true, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Self hosted open, AD integrated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise Cloud shared, AD integrated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise Cloud dedicated, AD integrated")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, false, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Self hosted locked, AD integration disabled")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, false, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Self hosted open, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise Cloud shared, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped: Wise Cloud dedicated, AD integration disabled")]
		public void TestLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreButNotMatchingStaffWithDbRolesAreDropped(DatabaseTestMode databaseTestMode, bool isAdIntegrated)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				isAdIntegrated ? TestConstants.Domain : null,
				isAdIntegrated ? Guid.Parse(TestConstants.ADTestUserAccount.Guid) : null,
				adminConnection);

			var sqlLoginNotMatchingStaff = Helper.GetEnterpriseLoginFullName("BobbyTheLoser", Db.DatabaseName);
			var sqlLoginMatchingStaffWithNoRoles = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);

			adminConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{sqlLoginMatchingStaffWithNoRoles}')
	CREATE LOGIN [{sqlLoginMatchingStaffWithNoRoles}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{sqlLoginNotMatchingStaff}')
	CREATE LOGIN [{sqlLoginNotMatchingStaff}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, sqlLoginMatchingStaffWithNoRoles), Is.Zero, $"Staff member '{TestConstants.ADTestUserAccount.Name} should exist but not belong to any group with database access role(s).");
			AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginMatchingStaffWithNoRoles}", "S");

			AssertionsHelper.AssumeStaffMissing(adminConnection, "BobbyTheLoser");
			AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginNotMatchingStaff}", "S");

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isAdIntegrated, It.IsAny<string>(), Helper.TestDomainCredentials)))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginMatchingStaffWithNoRoles}", "S");
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginNotMatchingStaff}", "S");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, $"{sqlLoginMatchingStaffWithNoRoles}");
				AssertionsHelper.AssertPrincipalMissing(adminConnection, $"{sqlLoginNotMatchingStaff}");
			}
		}

		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Wise Cloud shared, AD integrated")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Wise Cloud dedicated, AD integrated")]
		[TestCase(DatabaseTestMode.SelfHostedLocked, false, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Self hosted locked, AD integration disabled")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, false, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Self hosted open, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Wise Cloud shared, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped: Wise Cloud dedicated, AD integration disabled")]
		public void TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreNotDropped(DatabaseTestMode databaseTestMode, bool isAdIntegrated)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				isAdIntegrated ? TestConstants.Domain : null,
				isAdIntegrated ? Guid.Parse(TestConstants.ADTestUserAccount.Guid) : null,
				adminConnection,
				DbRoleTypes.CwRestrictedReaderRole);

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
			var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{windowsLoginName}] FROM WINDOWS;
CREATE LOGIN [{sqlLoginName}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name} should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginName}", "S");

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: isAdIntegrated, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginName}", "S");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginName}", "S");
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedLocked, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreDroppedIfAdIntegrated: Self hosted locked, AD integration disabled")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, TestName = "TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreDroppedIfAdIntegrated: Self hosted open, AD integration disabled")]
		public void TestWhenLoginsStartingFromEnterpriseDbUserUnderscoreDbNameUnderscoreAndMatchingStaffWithDbRolesAreDroppedIfAdIntegrated(DatabaseTestMode databaseTestMode)
		{
			// Arrange
			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				TestConstants.ADTestUserAccount.Name,
				TestConstants.Domain,
				(Guid?)Guid.Parse(TestConstants.ADTestUserAccount.Guid),
				adminConnection,
				DbRoleTypes.CwRestrictedReaderRole);

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(TestConstants.ADTestUserAccount.Name, Db.DatabaseName);
			var windowsLoginName = TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000;

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{windowsLoginName}] FROM WINDOWS;
CREATE LOGIN [{sqlLoginName}] WITH PASSWORD = N'[]123ASGASDGASDG131dk;k;'
");

			AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
			Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member '{TestConstants.ADTestUserAccount.Name} should exist and belong to a group with database access role(s).");
			AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginName}", "S");

			// Act
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isADIntegrationEnabled: true, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			using (ObjectFactory.Substitute(Helper.GetADEntityProviderMock()))
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, $"{sqlLoginName}", "S");

				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, $"{sqlLoginName}");
			}
		}

		#endregion Filter testing

		#region password hash tests

		[TestCase(DatabaseTestMode.SelfHostedLocked, false, TestName = "TestStaffMemberSqlLoginPasswordHashMustMatchSqlPasswordHashStoredInStaffMemberRecord: Self-hosted, locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, false, TestName = "TestStaffMemberSqlLoginPasswordHashMustMatchSqlPasswordHashStoredInStaffMemberRecord: Self-hosted, open")]

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestStaffMemberSqlLoginPasswordHashMustMatchSqlPasswordHashStoredInStaffMemberRecord: Wise cloud hosted, decicated, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestStaffMemberSqlLoginPasswordHashMustMatchSqlPasswordHashStoredInStaffMemberRecord: Wise cloud hosted, shared, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestStaffMemberSqlLoginPasswordHashMustMatchSqlPasswordHashStoredInStaffMemberRecord: Wise cloud hosted, decicated, AD integration enabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestStaffMemberSqlLoginPasswordHashMustMatchSqlPasswordHashStoredInStaffMemberRecord: Wise cloud hosted, shared, AD integration enabled")]
		public void TestStaffMemberSqlLoginPasswordHashMustMatchSqlPasswordHashStoredInStaffMemberRecord(DatabaseTestMode databaseTestMode, bool isAdIntegrationEnabled)
		{
			// Arrange
			var staffLogin1 = Guid.NewGuid().ToString();

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin1,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.CwRestrictedReaderRole });

			var sqlLoginNameForStaff1WithIncorrectPasswordHash = Helper.GetEnterpriseLoginFullName(staffLogin1, Db.DatabaseName);

			var sqlLoginPasswordHashInStaffRecord1 = adminConnection.ExecuteScalar<byte[]>(
				"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLogin1, GlbStaffSchema.GS_LoginName));

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginNameForStaff1WithIncorrectPasswordHash.QuoteName()} WITH PASSWORD = N'SOM123pASSWORD[]', DEFAULT_DATABASE = {Helper.MainDatabaseNameOutsideTestCase.QuoteName()}, DEFAULT_LANGUAGE = us_english, CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
");

			Assume.That(
				adminConnection.ExecuteScalar<byte[]>(
					"SELECT password_hash FROM sys.sql_logins WHERE name = @loginName",
					cmd => cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, sqlLoginNameForStaff1WithIncorrectPasswordHash)),
				Is.Not.EqualTo(sqlLoginPasswordHashInStaffRecord1),
				"Password hash for the login should be different from password hash on staff record for staff 1");

			var staffLogin2 = Guid.NewGuid().ToString();

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin2,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.CwRestrictedReaderRole });

			var sqlLoginNameForStaff2WithCorrectPasswordHash = Helper.GetEnterpriseLoginFullName(staffLogin2, Db.DatabaseName);

			var sqlLoginPasswordHashInStaffRecord2 = adminConnection.ExecuteScalar<byte[]>(
				"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLogin2, GlbStaffSchema.GS_LoginName));

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginNameForStaff2WithCorrectPasswordHash.QuoteName()} WITH PASSWORD = {DataUtils.BytesToHexString(sqlLoginPasswordHashInStaffRecord2)} HASHED, DEFAULT_DATABASE = {Helper.MainDatabaseNameOutsideTestCase.QuoteName()}, DEFAULT_LANGUAGE = us_english, CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
");

			Assume.That(
				adminConnection.ExecuteScalar<byte[]>(
					"SELECT password_hash FROM sys.sql_logins WHERE name = @loginName",
					cmd => cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, sqlLoginNameForStaff2WithCorrectPasswordHash)),
				Is.EqualTo(sqlLoginPasswordHashInStaffRecord2),
				"Password hash for the login should match password hash on staff record for staff 2.");

			var sqlSecurityManager = new SqlSecurityManager(Mock.Of<IntegrationLogging.ILogger>(), Helper.MainDatabaseNameOutsideTestCase);

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isAdIntegrationEnabled, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				Assert.That(
					adminConnection.ExecuteScalar<byte[]>(
						"SELECT password_hash FROM sys.sql_logins WHERE name = @loginName",
						cmd => cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, sqlLoginNameForStaff1WithIncorrectPasswordHash)),
					Is.EqualTo(sqlLoginPasswordHashInStaffRecord1),
					"Password hash for the login should now match password hash on staff record for staff 1");

				Assert.That(
					adminConnection.ExecuteScalar<byte[]>(
						"SELECT password_hash FROM sys.sql_logins WHERE name = @loginName",
						cmd => cmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, 128, sqlLoginNameForStaff2WithCorrectPasswordHash)),
					Is.EqualTo(sqlLoginPasswordHashInStaffRecord2),
					"Password hash for the login should still match password hash on staff record for staff 2.");
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedLocked, false, TestName = "TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Self-hosted, locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, false, TestName = "TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Self-hosted, open")]

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, decicated, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, shared, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, decicated, AD integration enabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, shared, AD integration enabled")]
		public void TestWarningIsIssuedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord(DatabaseTestMode databaseTestMode, bool isAdIntegrationEnabled)
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var staffLogin = Guid.NewGuid().ToString();

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.CwRestrictedReaderRole });

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);

			adminConnection.ExecuteNonQuery(
				"UPDATE dbo.GlbStaff SET GS_SqlLoginPasswordHash = NULL, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLogin, GlbStaffSchema.GS_LoginName));

			Assume.That(
				adminConnection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", sqlLoginName, GlbStaffSchema.GS_LoginName)),
				Is.Null,
				$"Sql password hash should not be set for the staff member '{staffLogin}'.");

			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isAdIntegrationEnabled, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: true);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginName);
				loggerMock.Verify(logger => logger.Log(
					IntegrationLogging.LogType.Warning,
					$"Skipped synchronising SQL login for staff record {staffLogin}. The record is missing its SQL login password hash value. Consider setting SQL password via the following CW1 menu item: Help > Set SQL password"));
			}
		}

		[TestCase(DatabaseTestMode.SelfHostedLocked, false, TestName = "TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Self-hosted, locked")]
		[TestCase(DatabaseTestMode.SelfHostedOpen, false, TestName = "TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Self-hosted, open")]

		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, false, TestName = "TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, decicated, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, false, TestName = "TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, shared, AD integration disabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudDedicatedServer, true, TestName = "TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, decicated, AD integration enabled")]
		[TestCase(DatabaseTestMode.HostedInWiseCloudSharedServer, true, TestName = "TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord: Wise cloud hosted, shared, AD integration enabled")]
		public void TestSqlLoginIsDroppedForStaffMembersThatShouldHaveSqlLoginButMissingPasswordHashInStaffRecord(DatabaseTestMode databaseTestMode, bool isAdIntegrationEnabled)
		{
			// Arrange
			var loggerMock = new Mock<IntegrationLogging.ILogger>();
			var staffLogin = Guid.NewGuid().ToString();

			Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
				staffLogin,
				null,
				null,
				adminConnection,
				new[] { DbRoleTypes.CwRestrictedReaderRole });

			var sqlLoginName = Helper.GetEnterpriseLoginFullName(staffLogin, Db.DatabaseName);

			adminConnection.ExecuteNonQuery(
				"UPDATE dbo.GlbStaff SET GS_SqlLoginPasswordHash = NULL, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' WHERE GS_LoginName = @loginName",
				cmd => cmd.AddParameterBasedOnDbColumn("@loginName", staffLogin, GlbStaffSchema.GS_LoginName));

			adminConnection.ExecuteNonQuery($@"
CREATE LOGIN {sqlLoginName.QuoteName()} WITH PASSWORD = N'SOM123pASSWORD[]', DEFAULT_DATABASE = {Helper.MainDatabaseNameOutsideTestCase.QuoteName()}, DEFAULT_LANGUAGE = us_english, CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
");

			Assume.That(
				adminConnection.ExecuteScalar(
					"SELECT GS_SqlLoginPasswordHash FROM dbo.GlbStaff WHERE GS_LoginName = @loginName",
					cmd => cmd.AddParameterBasedOnDbColumn("@loginName", sqlLoginName, GlbStaffSchema.GS_LoginName)),
				Is.Null,
				$"Sql password hash should not be set for the staff member '{staffLogin}'.");

			AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, sqlLoginName, "S");

			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			// Act
			using (ObjectFactory.Substitute(Helper.GetDedicatedSqlServerInstanceDefinitionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetProductRegistractionMock(databaseTestMode)))
			using (ObjectFactory.Substitute(Helper.GetADRegistryMock(isAdIntegrationEnabled, It.IsAny<string>(), It.IsAny<DomainCredentials>())))
			{
				sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false);

				// Assert
				AssertionsHelper.AssertPrincipalMissing(adminConnection, sqlLoginName);
				loggerMock.Verify(logger => logger.Log(
					IntegrationLogging.LogType.Warning,
					$"Skipped synchronising SQL login for staff record {staffLogin}. The record is missing its SQL login password hash value. Consider setting SQL password via the following CW1 menu item: Help > Set SQL password"));
			}
		}

		#endregion password hash tests

		#region Implementation

		[SetUp]
		public void SetUp()
		{
			adminConnection = Db.NewAdminConnection();
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);
			Helper.CleanUpGlbTables(adminConnection);
		}

		[TearDown]
		public void TearDown()
		{
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);
			Helper.CleanUpGlbTables(adminConnection);
			adminConnection?.Dispose();
		}

		AdminConnection adminConnection;
		readonly string dbReaderLogin = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbWriterLogin = CargoWiseWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedReaderLogin = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbRestrictedWriterLogin = RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string dbUnrestrictedWriterLogin = UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName);

		static IEnumerable<TestCaseData> StaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInWiseCloudHostedEnvironmentTestCaseSource()
		{
			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } }, { "D", new[] { "VIEW ANY DATABASE" } } })
				.SetName($"{{m}}: Database developer and backup operator on shared hosted server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				new[] { DbRoleTypes.CwHRMStaffRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } }, { "D", new[] { "VIEW ANY DATABASE" } } })
				.SetName("{m}: Hrm staff on shared hosted server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				new[] { DbRoleTypes.CwHRMStaffRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } }, { "D", new[] { "VIEW ANY DATABASE" } } })
				.SetName("{m}: Hrm, Database developer, reader and backup operator on shared hosted server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				new[] { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } }, { "D", new[] { "VIEW ANY DATABASE" } } })
				.SetName("{m}: Database developer, reader and backup operator on shared hosted server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				new[] { DbRoleTypes.DbBackupOperatorRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } }, { "D", new[] { "VIEW ANY DATABASE" } } })
				.SetName("{m}: Backup operator on shared hosted server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				new[] { DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } }, { "D", new[] { "VIEW ANY DATABASE" } } })
				.SetName("{m}: Database developer on shared hosted server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudSharedServer,
				new[] { DbRoleTypes.CwRestrictedReaderRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } }, { "D", new[] { "VIEW ANY DATABASE" } } })
				.SetName("{m}: Database reader on shared hosted server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudDedicatedServer,
				new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
				.SetName($"{{m}}: Database developer and backup operator on {Helper.DatabaseTestModeDescription(DatabaseTestMode.HostedInWiseCloudDedicatedServer)} server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudDedicatedServer,
				new[] { DbRoleTypes.CwHRMStaffRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } } })
				.SetName($"{{m}}: Hrm staff on {Helper.DatabaseTestModeDescription(DatabaseTestMode.HostedInWiseCloudDedicatedServer)} server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudDedicatedServer,
				new[] { DbRoleTypes.CwHRMStaffRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
				.SetName($"{{m}}: Hrm, Database developer, reader and backup operator on {Helper.DatabaseTestModeDescription(DatabaseTestMode.HostedInWiseCloudDedicatedServer)} server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudDedicatedServer,
				new[] { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
				.SetName($"{{m}}: Database developer, reader and backup operator on {Helper.DatabaseTestModeDescription(DatabaseTestMode.HostedInWiseCloudDedicatedServer)} server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudDedicatedServer,
				new[] { DbRoleTypes.DbBackupOperatorRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } } })
				.SetName($"{{m}}: Backup operator on {Helper.DatabaseTestModeDescription(DatabaseTestMode.HostedInWiseCloudDedicatedServer)}server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudDedicatedServer,
				new[] { DbRoleTypes.DbDataWriterRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
				.SetName($"{{m}}: Database developer on {Helper.DatabaseTestModeDescription(DatabaseTestMode.HostedInWiseCloudDedicatedServer)} server");

			yield return new TestCaseData(
				DatabaseTestMode.HostedInWiseCloudDedicatedServer,
				new[] { DbRoleTypes.CwRestrictedReaderRole },
				new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } } })
				.SetName($"{{m}}: Database reader on {Helper.DatabaseTestModeDescription(DatabaseTestMode.HostedInWiseCloudDedicatedServer)} server");
		}

		static IEnumerable<TestCaseData> StaffSqlLoginIsCreatedWithCorrectPermissionsAndPropertiesInSelfHostedEnvironmentTestCaseSource()
		{
			foreach (var standAloneOption in new[] { DatabaseTestMode.SelfHostedLocked, DatabaseTestMode.SelfHostedOpen })
			{
				yield return new TestCaseData(
					standAloneOption,
					new[] { DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
					new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
					.SetName($"{{m}}: Database developer and backup operator on {Helper.DatabaseTestModeDescription(standAloneOption)} server");

				yield return new TestCaseData(
					standAloneOption,
					new[] { DbRoleTypes.CwHRMStaffRole },
					new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } } })
					.SetName($"{{m}}: Hrm staff on {Helper.DatabaseTestModeDescription(standAloneOption)} server");

				yield return new TestCaseData(
					standAloneOption,
					new[] { DbRoleTypes.CwHRMStaffRole, DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
					new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
					.SetName($"{{m}}: Hrm, Database developer, reader and backup operator on {Helper.DatabaseTestModeDescription(standAloneOption)} server");

				yield return new TestCaseData(
					standAloneOption,
					new[] { DbRoleTypes.CwRestrictedReaderRole, DbRoleTypes.DbBackupOperatorRole, DbRoleTypes.DbDataWriterRole },
					new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
					.SetName($"{{m}}: Database developer, reader and backup operator on {Helper.DatabaseTestModeDescription(standAloneOption)} server");

				yield return new TestCaseData(
					standAloneOption,
					new[] { DbRoleTypes.DbBackupOperatorRole },
					new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } } })
					.SetName($"{{m}}: Backup operator on {Helper.DatabaseTestModeDescription(standAloneOption)}server");

				yield return new TestCaseData(
					standAloneOption,
					new[] { DbRoleTypes.DbDataWriterRole },
					new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL", "ALTER TRACE", "VIEW SERVER STATE", "ALTER ANY EVENT SESSION", "VIEW ANY DEFINITION" } } })
					.SetName($"{{m}}: Database developer on {Helper.DatabaseTestModeDescription(standAloneOption)} server");

				yield return new TestCaseData(
					standAloneOption,
					new[] { DbRoleTypes.CwRestrictedReaderRole },
					new Dictionary<string, string[]> { { "G", new[] { "CONNECT SQL" } } })
					.SetName($"{{m}}: Database reader on {Helper.DatabaseTestModeDescription(standAloneOption)} server");
			}
		}

		static IEnumerable<TestCaseData> CombinationsOfDatabaseTestModeAndUserPrefix(string userPrefix, string description)
		{
			foreach (DatabaseTestMode databaseTestMode in Enum.GetValues(typeof(DatabaseTestMode)))
			{
				yield return new TestCaseData(databaseTestMode, userPrefix)
					.SetName($"{{m}}: {description} {Helper.DatabaseTestModeDescription(databaseTestMode)}");
			}
		}

		static IEnumerable<TestCaseData> DatabaseTestModes()
		{
			foreach (DatabaseTestMode databaseTestMode in Enum.GetValues(typeof(DatabaseTestMode)))
			{
				yield return new TestCaseData(databaseTestMode)
					.SetName($"{{m}}: {Helper.DatabaseTestModeDescription(databaseTestMode)}");
			}
		}

		#endregion Implementation

	}
}
