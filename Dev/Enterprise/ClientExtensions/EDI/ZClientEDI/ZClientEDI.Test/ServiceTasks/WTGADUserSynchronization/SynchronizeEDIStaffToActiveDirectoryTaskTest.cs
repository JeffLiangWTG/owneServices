using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.WTGADUserSynchronization;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ZClientEDI.Business;

namespace ZClientEDI.ServiceTasks.WTGADUserSynchronization.Test
{
	[TestedType(typeof(SynchronizeEDIStaffToActiveDirectoryTask))]
	class SynchronizeEDIStaffToActiveDirectoryTaskTestCase : ServiceTaskTestCase<SynchronizeEDIStaffToActiveDirectoryTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("6Hours", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestServiceTaskCode()
		{
			AssertEquals("ADE", SynchronizeEDIStaffToActiveDirectoryTask.Code);
		}

		public void TestServiceTaskThrowNoDomainPrivilegeException()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Throws(new NoDomainPrivilegeException("domain user credential is incorrect"));

			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "all_staff" }))
			using (EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WTGActiveDirectoryCredentials
			{
				DomainName = "fake.domain",
				DomainUserName = "Dexter",
				DomainUserPassword = "DasIstEinPasswort",
				OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
				IsEnabled = ZBool.True
			}))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);
				AssertContains("Should log error exception detail", "Domain user does not have read and write privileges in the Organizational Units set in Registry items: WiseTech Global Client Extensions -> OpenID Connect -> WTG Active Directory Credentials - cannot run ADE task.\r\nError Details: CargoWise.ActiveDirectory.NoDomainPrivilegeException: domain user credential is incorrect", log.ToString());
			}
		}

		public void TestDoNotCreateActiveDirectoryAccountIfIsIntegrationEnabledIsFalse()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);

			IUserDirectoryEntry createdUserDirectoryEntry = null;
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) =>
				{
					createdUserDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(name);
					return createdUserDirectoryEntry;
				});

			var log = InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals("Information|Synchronize EDI Staffs to Active Directory not started, WTG Active Directory Credentials is disabled or the security group value in Registry item WiseTech Global Client Extensions -> OpenID Connect -> WiseCloud Access Security Group for WTG is empty.\r\n", log.ToString());
			AssertNull("should not create any active directory account.", createdUserDirectoryEntry);
		}

		public void TestDoNotCreateActiveDirectoryAccountIfGroupRegistryIsEmpty()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);

			IUserDirectoryEntry createdUserDirectoryEntry = null;
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) =>
				{
					createdUserDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(name);
					return createdUserDirectoryEntry;
				});

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals("Information|Synchronize EDI Staffs to Active Directory not started, WTG Active Directory Credentials is disabled or the security group value in Registry item WiseTech Global Client Extensions -> OpenID Connect -> WiseCloud Access Security Group for WTG is empty.\r\n", log.ToString());
			}

			AssertNull("should not create any active directory account.", createdUserDirectoryEntry);
		}

		public void TestDoNotCreateActiveDirectoryAccountIfWTGActiveDirectoryCredentialsIsEmpty()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);

			IUserDirectoryEntry createdUserDirectoryEntry = null;
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) =>
				{
					createdUserDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(name);
					return createdUserDirectoryEntry;
				});

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "all_staff" }))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);
				AssertEquals("Information|Synchronize EDI Staffs to Active Directory not started, WTG Active Directory Credentials is disabled or the security group value in Registry item WiseTech Global Client Extensions -> OpenID Connect -> WiseCloud Access Security Group for WTG is empty.\r\n", log.ToString());
			}
			AssertNull("should not create any active directory account.", createdUserDirectoryEntry);
		}

		public void TestActiveDirectoryAccountHasSecurityGroupAssigned()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			directorySearcherMock.Setup(directorySearcher => directorySearcher.DomainName).Returns("sand.wtg.zone");

			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);
			IUserDirectoryEntry createdUserDirectoryEntry = null;
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) =>
				{
					createdUserDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(name);
					return createdUserDirectoryEntry;
				});

			var dummyGroup = DummyDirectoryEntryWrapper.CreateGroup("all_staff");

			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(() => dummyGroup);

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "all_staff" }))
			using (EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WTGActiveDirectoryCredentials() { IsEnabled = true, DomainName = "dummy", DomainUserName = "user", DomainUserPassword = "password", OrganizationalUnitPath = "root/Accounts/ADFunctionalTesting/JeffOU" }))
			{
				InitialiseAndRunTaskSchedule(serviceTask);
				AssertEquals("WTG.Test.User@sand.wtg.zone", createdUserDirectoryEntry.UserPrincipalName);
				Assert("user 'WTG.Test.User' should be added to the group 'all_staff'", dummyGroup.GetMembers().Contains(createdUserDirectoryEntry));
			}
		}

		public void TestEmailNotificationOnActiveDirectoryDeletedError()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_EmailAddress = "bar@test.com";
			staff.GS_IsActive = true;
			staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid = Guid.NewGuid();
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ADE";
			staff.Groups.Add(group);
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>())).Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) => null);
			var dummyGroup = DummyDirectoryEntryWrapper.CreateGroup("all_staff");

			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(() => dummyGroup);

			using (EDIDataRegistry.Instance.NotificationGroupForADETask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "all_staff" }))
			using (EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WTGActiveDirectoryCredentials() { IsEnabled = true, DomainName = "dummy", DomainUserName = "user", DomainUserPassword = "password", OrganizationalUnitPath = "root/Accounts/ADFunctionalTesting/JeffOU" }))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				InitialiseAndRunTaskSchedule(serviceTask);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("ADE - Synchronize EDI Staffs to Active Directory", email.Subject);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("bar@test.com", email.Recipients[0].Email);

				directorySearcherMock.Verify(s => s.FindOrganisationalUnit(It.IsAny<string>()), Times.Never);
				organizationalUnit.Verify(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()), Times.Never);
			}
		}

		public void TestActiveDirectoryAccountIsDisableWhenGlbStaffIsDeactivated()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			directorySearcherMock.Setup(directorySearcher => directorySearcher.DomainName).Returns("sand.wtg.zone");
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);

			IUserDirectoryEntry createdUserDirectoryEntry = null;
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) =>
				{
					createdUserDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(name, path: "LDAP://CN=WTG/OU=root/Accounts/ADFunctionalTesting/JeffOU");
					return createdUserDirectoryEntry;
				});

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "all_staff" }))
			using (EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WTGActiveDirectoryCredentials() { IsEnabled = true, DomainName = "dummy", DomainUserName = "user", DomainUserPassword = "password", OrganizationalUnitPath = "root/Accounts/ADFunctionalTesting/JeffOU" }))
			{
				InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals("WTG.Test.User@sand.wtg.zone", createdUserDirectoryEntry.UserPrincipalName);
				AssertEquals(true, createdUserDirectoryEntry.IsActive);

				directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => createdUserDirectoryEntry);
				staff.GS_IsActive = false;
				Factory.Save();
				InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals(false, createdUserDirectoryEntry.IsActive);
			}
		}

		[TestDate(2022, 11, 16)]
		[TestUtcOffset(0, 0, 0)]
		public void TestServiceTaskRun()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";

			var staff2 = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff2.GS_IsActive = false;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			directorySearcherMock.Setup(directorySearcher => directorySearcher.DomainName).Returns("sand.wtg.zone");

			var dummyGroup = DummyDirectoryEntryWrapper.CreateGroup("all_staff");
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(() => dummyGroup);

			IUserDirectoryEntry createdUserDirectoryEntry = null;
			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) =>
				{
					createdUserDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(name, path: "LDAP://CN=WTG/OU=root/Accounts/ADFunctionalTesting/JeffOU");
					return createdUserDirectoryEntry;
				});

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "all_staff" }))
			using (EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WTGActiveDirectoryCredentials() { IsEnabled = true, DomainName = "dummy", DomainUserName = "user", DomainUserPassword = "password", OrganizationalUnitPath = "root/Accounts/ADFunctionalTesting/JeffOU" }))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				AssertContains("Should only sync 1 user", "Information|Gathering user(s) to sync\r\nInformation|Synchronizing 1 user(s)\r\nInformation|100% - Completed\r\nInformation|Synchronize EDI Staffs to Active Directory finished", log.ToString());

				staff.ReloadSafe();
				AssertEquals(createdUserDirectoryEntry.Guid, staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid);
				AssertEquals("WTG.Test.User@sand.wtg.zone", createdUserDirectoryEntry.UserPrincipalName);
				AssertEquals("2022-11-16", EDIDataRegistry.Instance.LastSuccessfulSyncForEDIUTC.Value.ToString("yyyy-MM-dd"));
				AssertEquals("the AD user is added to AD group", 1, dummyGroup.GroupMembership.Count);
				staff.GS_LoginName = "Test.User2";
				Factory.Save();

				directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => createdUserDirectoryEntry);

				InitialiseAndRunTaskSchedule(serviceTask);

				staff.ReloadSafe();
				AssertEquals(createdUserDirectoryEntry.Guid, staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid);
				AssertEquals("WTG.Test.User2@sand.wtg.zone", createdUserDirectoryEntry.UserPrincipalName);

				staff.GS_LoginName = "Test.User";
				Factory.Save();

				createdUserDirectoryEntry.HasChanges = false;
				InitialiseAndRunTaskSchedule(serviceTask);

				staff.ReloadSafe();
				AssertEquals(createdUserDirectoryEntry.Guid, staff.StaffEx.GS9_EdiActiveDirectoryObjectGuid);
				AssertEquals("WTG.Test.User@sand.wtg.zone", createdUserDirectoryEntry.UserPrincipalName);
			}
		}

		[ExpectNoExceptions]
		public void TestActiveDirectoryAccountsOU()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => organizationalUnit.Object);
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<Guid>(), It.IsAny<string>())).Returns(() => null);
			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);

			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, DirectoryObjectType type, IDirectorySearcher searcher) =>
				{
					var createdUserDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser(name);
					return createdUserDirectoryEntry;
				});

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "g_allStaff" }))
			using (EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WTGActiveDirectoryCredentials() { IsEnabled = true, DomainName = "dummy", DomainUserName = "user", DomainUserPassword = "password", OrganizationalUnitPath = "root/Accounts/ADFunctionalTesting/JeffOU" }))
			{
				InitialiseAndRunTaskSchedule(serviceTask);
			}

			directorySearcherMock.Verify(s => s.FindOrganisationalUnit("root/Accounts/ADFunctionalTesting/JeffOU"), Times.Once);
		}

		public void TestActiveDirectoryAccountsWithInvalidOU()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_LoginName = "Test.User";
			staff.GS_IsActive = true;
			Factory.Save();

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			var organizationalUnit = new Mock<IOrganisationalUnit>();

			var serviceTask = new SynchronizeEDIStaffToActiveDirectoryTask(directorySearcherMock.Object);

			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcherMock.Object;
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(() => throw new InvalidOUException("invalidOU"));
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(() => null);

			organizationalUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
				.Returns((string name, string userPrincipalName, string sAMAccountName, DirectoryObjectType type, IDirectorySearcher searcher) => DummyDirectoryEntryWrapper.CreateUser(userPrincipalName));

			using (EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "g_allStaff" }))
			using (EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WTGActiveDirectoryCredentials
			{
				DomainName = "fake.domain",
				DomainUserName = "Dexter",
				DomainUserPassword = "DasIstEinPasswort",
				OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
				IsEnabled = ZBool.True
			}))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				AssertContains("The Organizational Unit invalidOU is invalid, please review the setting in Registry items: WiseTech Global Client Extensions -> OpenID Connect -> WTG Active Directory Credentials - cannot run ADE task.", log.ToString());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				GlbStaffSchema.Constants.TableName,
				null,
				GlbStaffSchema.Constants.GS_IsSystemAccount + "=N", GlbStaffSchema.Constants.GS_IsResource + "=N", GlbStaffSchema.Constants.GS_IsActive + "=Y"
			)
		};
	}
}
