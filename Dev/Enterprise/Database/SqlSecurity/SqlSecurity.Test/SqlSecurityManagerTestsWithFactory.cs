using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity.Test
{
	[UseSnapshotProtection]
	class SqlSecurityManagerTestsWithFactory : TestCase
	{
		[UseSnapshotProtection]
		class TestBuildingSecurityThrowsAnExceptionWhenADIntegratedStaffMemberFailsToBeMappedDueToDatabaseUpgradeException : TestCase
		{
			public void TestBuildSecurity()
			{
				// Arrange
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					"Test",
					TestConstants.Domain,
					Guid.NewGuid(),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				AssertionsHelper.AssumeStaffExists(adminConnection, "Test");
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, "Test"), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				if (adminConnection.Exists($"FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteEscapedName('\'')}'"))
				{
					adminConnection.ExecuteNonQuery($@"
DROP LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				}

				AssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				AssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000);

				var adRegisteryMock = new Mock<IADRegistry>();
				adRegisteryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				adRegisteryMock.Setup(adRegistry => adRegistry.DomainCredentialsCollection).Returns(new List<IDomainCredentials>() { Helper.TestDomainCredentials });
				ObjectFactory.Substitute(adRegisteryMock.Object);

				AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

				var adEntityProvider = new Mock<IADEntityProvider>();

				var adUserIncorrect = new Mock<IADUser>();
				adUserIncorrect.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adUserIncorrect.SetupGet(a => a.SAMAccountName).Throws<DatabaseUpgradedException>();

				var adTestUser = new Mock<IADUser>();
				adTestUser.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adTestUser.SetupGet(a => a.SAMAccountName).Returns(TestConstants.ADTestUserAccount.Name);
				adTestUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);

				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == "Test"))).Returns(adUserIncorrect.Object);
				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name))).Returns(adTestUser.Object);

				ObjectFactory.Substitute(adEntityProvider.Object);

				var staffCollection = new GlbStaffCollection(factory);
				var staffTest = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == "Test");
				var staffADTestUser = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name);

				AssertExceptionThrown<Exception>(() => staffTest.GetDownLevelLogonName());
				AssertNoExceptionThrown(() => staffADTestUser.GetDownLevelLogonName());

				var loggerMock = new Mock<ILogger>();

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Db.DatabaseName, allowTransaction: true);

				// Act
				// Assert
				var ex = AssertExceptionThrown<AggregateException>(
					() =>
					sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token));

				Assert("There must be a DatabaseUpgradedException exception.", ex.Flatten().InnerExceptions.Any(e => e is DatabaseUpgradedException));
				
				AssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");
				AssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);

				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000);
			}

			public void TestBuildServerSecurity()
			{
				// Arrange
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					"Test",
					TestConstants.Domain,
					Guid.NewGuid(),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				AssertionsHelper.AssumeStaffExists(adminConnection, "Test");
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, "Test"), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				if (adminConnection.Exists($"FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteEscapedName('\'')}'"))
				{
					adminConnection.ExecuteNonQuery($@"
DROP LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				}

				AssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				AssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);

				var adRegisteryMock = new Mock<IADRegistry>();
				adRegisteryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				adRegisteryMock.Setup(adRegistry => adRegistry.DomainCredentialsCollection).Returns(new List<IDomainCredentials>() { Helper.TestDomainCredentials });
				ObjectFactory.Substitute(adRegisteryMock.Object);

				AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

				var adEntityProvider = new Mock<IADEntityProvider>();

				var adUserIncorrect = new Mock<IADUser>();
				adUserIncorrect.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adUserIncorrect.SetupGet(a => a.SAMAccountName).Throws<DatabaseUpgradedException>();

				var adTestUser = new Mock<IADUser>();
				adTestUser.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adTestUser.SetupGet(a => a.SAMAccountName).Returns(TestConstants.ADTestUserAccount.Name);
				adTestUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);

				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == "Test"))).Returns(adUserIncorrect.Object);
				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name))).Returns(adTestUser.Object);

				ObjectFactory.Substitute(adEntityProvider.Object);

				var staffCollection = new GlbStaffCollection(factory);
				var staffTest = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == "Test");
				var staffADTestUser = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name);

				AssertExceptionThrown<Exception>(() => staffTest.GetDownLevelLogonName());
				AssertNoExceptionThrown(() => staffADTestUser.GetDownLevelLogonName());

				var loggerMock = new Mock<ILogger>();

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Db.DatabaseName, allowTransaction: true);

				// Act
				// Assert
				var ex = AssertExceptionThrown<AggregateException>(
					() =>
					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false));

				Assert("There must be a DatabaseUpgradedException exception.", ex.Flatten().InnerExceptions.Any(e => e is DatabaseUpgradedException));

				AssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");
				AssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			}

			public void TestBuildDatabaseSecurity()
			{
				// Arrange
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					"Test",
					TestConstants.Domain,
					Guid.NewGuid(),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				AssertionsHelper.AssumeStaffExists(adminConnection, "Test");
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, "Test"), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				if (!adminConnection.Exists($"FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteEscapedName('\'')}'"))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				}

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000);

				var adRegisteryMock = new Mock<IADRegistry>();
				adRegisteryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				adRegisteryMock.Setup(adRegistry => adRegistry.DomainCredentialsCollection).Returns(new List<IDomainCredentials>() { Helper.TestDomainCredentials });
				ObjectFactory.Substitute(adRegisteryMock.Object);

				AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

				var adEntityProvider = new Mock<IADEntityProvider>();

				var adUserIncorrect = new Mock<IADUser>();
				adUserIncorrect.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adUserIncorrect.SetupGet(a => a.SAMAccountName).Throws<DatabaseUpgradedException>();

				var adTestUser = new Mock<IADUser>();
				adTestUser.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adTestUser.SetupGet(a => a.SAMAccountName).Returns(TestConstants.ADTestUserAccount.Name);
				adTestUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);

				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == "Test"))).Returns(adUserIncorrect.Object);
				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name))).Returns(adTestUser.Object);

				ObjectFactory.Substitute(adEntityProvider.Object);

				var staffCollection = new GlbStaffCollection(factory);
				var staffTest = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == "Test");
				var staffADTestUser = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name);

				AssertExceptionThrown<Exception>(() => staffTest.GetDownLevelLogonName());
				AssertNoExceptionThrown(() => staffADTestUser.GetDownLevelLogonName());

				var loggerMock = new Mock<ILogger>();

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Db.DatabaseName, allowTransaction: true);

				// Act
				// Assert
				var ex = AssertExceptionThrown<AggregateException>(
					() =>
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, Db.DatabaseName, trialRun: false));

				Assert("There must be a DatabaseUpgradedException exception.", ex.Flatten().InnerExceptions.Any(e => e is DatabaseUpgradedException));

				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000);
			}

			#region Implementation

			TransactionScope transactionScope;
			BusinessObjectFactory factory;
			IDisposable dropUserRepository;
			IDisposable resetIsTestForTest;
			AdminConnection adminConnection;
			CancellationTokenSource cancellationTokenSource;

			protected override void SetUp()
			{
				base.SetUp();
				using (var connection = Db.NewAdminConnection())
				{
					// make sure we have all databases created and that we may need for testing BEFORE transaction scope open
					var userRepositoryDb = $"{Db.DatabaseName}{DbUserRepository.RepositoryDbSuffix}";
					if (!connection.DatabaseExists(userRepositoryDb))
					{
						dropUserRepository = new DisposableAction(() =>
						{
							AdoTestUtils.DropDbIfExists(connection, userRepositoryDb);
						});

						(new DbUserRepository()).CreateRepositoryDatabase();
					}
				}

				resetIsTestForTest = Globals.TemporaryOverrideForIsTest(true);

				transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(5), TransactionScopeAsyncFlowOption.Enabled);
				adminConnection = Db.NewAdminConnection();
				Db.ConnectionOverrideForTest = adminConnection;

				factory = new BusinessObjectFactory(adminConnection);
				cancellationTokenSource = new CancellationTokenSource(180000);
			}

			protected override void TearDown()
			{
				transactionScope?.Dispose();
				Db.ConnectionOverrideForTest = null;

				dropUserRepository?.Dispose();
				resetIsTestForTest?.Dispose();
				adminConnection?.Dispose();
				base.TearDown();
			}

			#endregion Implementation
		}

		[UseSnapshotProtection]
		class TestADIntegratedStaffMemberThatCouldNotBeMappedToADAccountDoesNotCauseExceptionButIssuesLogMessage : TestCase
		{
			public void TestBuildSecurity()
			{
				// Arrange
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					"Test",
					TestConstants.Domain,
					Guid.NewGuid(),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				AssertionsHelper.AssumeStaffExists(adminConnection, "Test");
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, "Test"), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				if (adminConnection.Exists($"FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteEscapedName('\'')}'"))
				{
					adminConnection.ExecuteNonQuery($@"
DROP LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				}

				AssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				AssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000);

				var adRegisteryMock = new Mock<IADRegistry>();
				adRegisteryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				adRegisteryMock.Setup(adRegistry => adRegistry.DomainCredentialsCollection).Returns(new List<IDomainCredentials>() { Helper.TestDomainCredentials });
				ObjectFactory.Substitute(adRegisteryMock.Object);

				AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

				var adEntityProvider = new Mock<IADEntityProvider>();

				var adUserIncorrect = new Mock<IADUser>();
				adUserIncorrect.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adUserIncorrect.SetupGet(a => a.SAMAccountName).Throws<CargoWise.ActiveDirectory.DirectoryServicesException>();

				var adTestUser = new Mock<IADUser>();
				adTestUser.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adTestUser.SetupGet(a => a.SAMAccountName).Returns(TestConstants.ADTestUserAccount.Name);
				adTestUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);

				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == "Test"))).Returns(adUserIncorrect.Object);
				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name))).Returns(adTestUser.Object);

				ObjectFactory.Substitute(adEntityProvider.Object);

				var staffCollection = new GlbStaffCollection(factory);
				var staffTest = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == "Test");
				var staffADTestUser = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name);

				AssertExceptionThrown<CargoWise.ActiveDirectory.DirectoryServicesException>(() => staffTest.GetDownLevelLogonName());
				AssertNoExceptionThrown(() => staffADTestUser.GetDownLevelLogonName());

				var loggerMock = new Mock<ILogger>();

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Db.DatabaseName, allowTransaction: true);

				// Act
				// Assert
				AssertNoExceptionThrown(
					() =>
					sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token));

				loggerMock.Verify(logger => logger.Log(IntegrationLogging.LogType.Warning, $"Failed to get full AD name for staff member with login 'Test'", It.IsAny<Exception>()));
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
				AssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");

				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");
			}

			public void TestBuildServerSecurity()
			{
				// Arrange
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					"Test",
					TestConstants.Domain,
					Guid.NewGuid(),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				AssertionsHelper.AssumeStaffExists(adminConnection, "Test");
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, "Test"), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				if (adminConnection.Exists($"FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteEscapedName('\'')}'"))
				{
					adminConnection.ExecuteNonQuery($@"
DROP LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}];
");
				}

				AssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				AssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);

				var adRegisteryMock = new Mock<IADRegistry>();
				adRegisteryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				adRegisteryMock.Setup(adRegistry => adRegistry.DomainCredentialsCollection).Returns(new List<IDomainCredentials>() { Helper.TestDomainCredentials });
				ObjectFactory.Substitute(adRegisteryMock.Object);

				AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

				var adEntityProvider = new Mock<IADEntityProvider>();

				var adUserIncorrect = new Mock<IADUser>();
				adUserIncorrect.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adUserIncorrect.SetupGet(a => a.SAMAccountName).Throws<CargoWise.ActiveDirectory.DirectoryServicesException>();

				var adTestUser = new Mock<IADUser>();
				adTestUser.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adTestUser.SetupGet(a => a.SAMAccountName).Returns(TestConstants.ADTestUserAccount.Name);
				adTestUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);

				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == "Test"))).Returns(adUserIncorrect.Object);
				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name))).Returns(adTestUser.Object);

				ObjectFactory.Substitute(adEntityProvider.Object);

				var staffCollection = new GlbStaffCollection(factory);
				var staffTest = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == "Test");
				var staffADTestUser = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name);

				AssertExceptionThrown<CargoWise.ActiveDirectory.DirectoryServicesException>(() => staffTest.GetDownLevelLogonName());
				AssertNoExceptionThrown(() => staffADTestUser.GetDownLevelLogonName());

				var loggerMock = new Mock<ILogger>();

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Db.DatabaseName, allowTransaction: true);

				// Act
				// Assert
				AssertNoExceptionThrown(
					() =>
					sqlSecurityManager.BuildServerSecurity(adminConnection, trialRun: false));

				loggerMock.Verify(logger => logger.Log(IntegrationLogging.LogType.Warning, $"Failed to get full AD name for staff member with login 'Test'", It.IsAny<Exception>()));
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
				AssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");
			}

			public void TestBuildDatabaseSecurity()
			{
				// Arrange
				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					TestConstants.ADTestUserAccount.Name,
					TestConstants.Domain,
					Guid.Parse(TestConstants.ADTestUserAccount.Guid),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				Helper.CreateGlbStaffWithDatabaseAccessRolesInDatabase(
					"Test",
					TestConstants.Domain,
					Guid.NewGuid(),
					adminConnection,
					DbRoleTypes.CwRestrictedReaderRole);

				AssertionsHelper.AssumeStaffExists(adminConnection, "Test");
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, "Test"), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				AssertionsHelper.AssumeStaffExists(adminConnection, TestConstants.ADTestUserAccount.Name);
				Assume.That(Helper.GetStaffRolesNumber(adminConnection, TestConstants.ADTestUserAccount.Name), Is.GreaterThan(0), $"Staff member 'Test' should exist and belong to a group with database access role(s).");

				if (!adminConnection.Exists($"FROM sys.server_principals WHERE name = N'{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000.QuoteEscapedName('\'')}'"))
				{
					adminConnection.ExecuteNonQuery($@"
CREATE LOGIN [{TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000}] FROM WINDOWS;
");
				}

				AssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				AssertionsHelper.AssumePrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");

				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, "Sand\\Test");
				DatabaseAssertionsHelper.AssumePrincipalMissing(adminConnection, TestConstants.ADTestAdminAccount.NameWithDomainPreWindows2000);

				var adRegisteryMock = new Mock<IADRegistry>();
				adRegisteryMock.SetupGet(a => a.IsIntegrationEnabled).Returns(true);
				adRegisteryMock.Setup(adRegistry => adRegistry.DomainCredentialsCollection).Returns(new List<IDomainCredentials>() { Helper.TestDomainCredentials });
				ObjectFactory.Substitute(adRegisteryMock.Object);

				AssertEquals(true, ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled);

				var adEntityProvider = new Mock<IADEntityProvider>();

				var adUserIncorrect = new Mock<IADUser>();
				adUserIncorrect.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adUserIncorrect.SetupGet(a => a.SAMAccountName).Throws<CargoWise.ActiveDirectory.DirectoryServicesException>();

				var adTestUser = new Mock<IADUser>();
				adTestUser.SetupGet(a => a.DomainNetBiosName).Returns(TestConstants.DomainPreWin2000);
				adTestUser.SetupGet(a => a.SAMAccountName).Returns(TestConstants.ADTestUserAccount.Name);
				adTestUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);

				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == "Test"))).Returns(adUserIncorrect.Object);
				adEntityProvider.Setup(a => a.GetADUser(It.Is<GlbStaff>(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name))).Returns(adTestUser.Object);

				ObjectFactory.Substitute(adEntityProvider.Object);

				var staffCollection = new GlbStaffCollection(factory);
				var staffTest = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == "Test");
				var staffADTestUser = staffCollection.FirstOrDefault(staff => staff.GS_LoginName == TestConstants.ADTestUserAccount.Name);

				AssertExceptionThrown<CargoWise.ActiveDirectory.DirectoryServicesException>(() => staffTest.GetDownLevelLogonName());
				AssertNoExceptionThrown(() => staffADTestUser.GetDownLevelLogonName());

				var loggerMock = new Mock<ILogger>();

				var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Db.DatabaseName, allowTransaction: true);

				// Act
				// Assert
				AssertNoExceptionThrown(
					() =>
					sqlSecurityManager.BuildDatabaseSecurity(adminConnection, Db.DatabaseName, trialRun: false));

				loggerMock.Verify(logger => logger.Log(IntegrationLogging.LogType.Warning, $"Failed to get full AD name for staff member with login 'Test'", It.IsAny<CargoWise.ActiveDirectory.DirectoryServicesException>()));
				AssertionsHelper.AssertPrincipalExistsAndIsNotDisabled(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
				AssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");

				DatabaseAssertionsHelper.AssertPrincipalExists(adminConnection, TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000, "U");
				DatabaseAssertionsHelper.AssertPrincipalMissing(adminConnection, "Sand\\Test");
			}

			#region Implementation

			TransactionScope transactionScope;
			BusinessObjectFactory factory;
			IDisposable dropUserRepository;
			IDisposable resetIsTestForTest;
			AdminConnection adminConnection;
			CancellationTokenSource cancellationTokenSource;

			protected override void SetUp()
			{
				base.SetUp();
				using (var connection = Db.NewAdminConnection())
				{
					// make sure we have all databases created and that we may need for testing BEFORE transaction scope open
					var userRepositoryDb = $"{Db.DatabaseName}{DbUserRepository.RepositoryDbSuffix}";
					if (!connection.DatabaseExists(userRepositoryDb))
					{
						dropUserRepository = new DisposableAction(() =>
						{
							AdoTestUtils.DropDbIfExists(connection, userRepositoryDb);
						});

						(new DbUserRepository()).CreateRepositoryDatabase();
					}
				}

				resetIsTestForTest = Globals.TemporaryOverrideForIsTest(true);

				transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(5), TransactionScopeAsyncFlowOption.Enabled);
				adminConnection = Db.NewAdminConnection();
				Db.ConnectionOverrideForTest = adminConnection;

				factory = new BusinessObjectFactory(adminConnection);
				cancellationTokenSource = new CancellationTokenSource(180000);
			}

			protected override void TearDown()
			{
				transactionScope?.Dispose();
				Db.ConnectionOverrideForTest = null;

				dropUserRepository?.Dispose();
				resetIsTestForTest?.Dispose();
				adminConnection?.Dispose();
				base.TearDown();
			}

			#endregion Implementation
		}

		[ExpectNoExceptions]
		public void TestBuildSecurityDoesNotCorruptDisposerForDbConnection()
		{
			const int timeoutSeconds = 120;

			using (new DisposableAction(() => AsyncHelper.WaitAllActiveTasksForTest()))
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				var task = AsyncHelper.RunTask(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var adminConnection = Db.NewAdminConnection())
					{
						AssertNoExceptionThrown(() => Db.Connection.EnsureIsOpen());

						var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName);
						sqlSecurityManager.BuildSecurity(adminConnection, cancellationTokenSource.Token);

						AssertNoExceptionThrown(() => Db.Connection.EnsureIsOpen());
					}
					return true;
				}, cancellationTokenSource.Token, "BuildSecurity with Db disposer");

				var isTaskCompleted = task.Wait(TimeSpan.FromSeconds(timeoutSeconds));
				cancellationTokenSource.Cancel();

				Assert($"SqlSecurityManager.BuildSecurity timed out for {timeoutSeconds} seconds", isTaskCompleted);
			}
		}
	}
}
