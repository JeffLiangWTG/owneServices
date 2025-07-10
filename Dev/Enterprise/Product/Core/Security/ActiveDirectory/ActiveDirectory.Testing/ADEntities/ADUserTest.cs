using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class ADUserTest : ADEntityTest
	{
		public void TestRequireDirectoryEntry_ShouldFallBackToLoginNameWhenNotFoundByGUID()
		{
			var adGUID = Guid.NewGuid();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Benedict.Cumberbatch";
			staff.GS_ActiveDirectoryObjectGuid = adGUID;

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Benedict.Cumberbatch");

			directorySearcherMock.Setup(s => s.FindUser(adGUID, string.Empty)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindUser("Benedict.Cumberbatch", string.Empty)).Returns(directoryEntry);

			var adUser = new ADUser(staff);
			AssertEquals(directoryEntry, adUser.GetDirectoryEntry(false));

			directorySearcherMock.Verify(s => s.FindUser(adGUID, string.Empty), Times.Once);
			directorySearcherMock.Verify(s => s.FindUser("Benedict.Cumberbatch", string.Empty), Times.Once);
		}

		public void TestChangeStaffPassword_DirectoryServicesExceptionThrown_ShouldRethrowWithTheRightMessage()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "coffeepot";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var adUser = new ADUser(staff);
			ADEntityProviderSubstitution.ADUser = adUser;

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), string.Empty)).Throws(new DirectoryServicesException("Blah"));

			AssertExceptionThrown<InvalidOperationException>("Unexpected Exception", $"Active Directory User '{staff.GS_LoginName}' is missing, please contact your system administrator.", () => staff.ResetPassword("MaiPassword"));
		}

		public void TestChangeStaffPassword_WrappedDirectoryServicesCOMExceptionThrown_ShouldDisplayMessage()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var directoryEntry = new Mock<IUserDirectoryEntry>();

			var adUser = new ADUser(staff);
			ADEntityProviderSubstitution.ADUser = adUser;

			directoryEntry.SetupGet(x => x.CanUpdate).Returns(true);
			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), string.Empty)).Returns(directoryEntry.Object);
			directoryEntry.Setup(x => x.SetPassword(It.IsAny<string>(), It.IsAny<bool>())).Throws(new TargetInvocationException(new DirectoryServicesCOMException("Blah")));

			staff.ResetPassword("MaiPassword");

			AssertEquals(@"An error occurred while communicating with the Active Directory controller. Please contact your System Administrator.

Error message is as follow:
Blah", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSynchroniseWithNoADRecord_ShouldDeactivate()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Morgoth";
			staff.GS_IsActive = true;
			var adUser = new ADUser(staff);

			directorySearcherMock.Setup(s => s.FindUser("Morgoth", string.Empty)).Returns((IUserDirectoryEntry)null);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());
			AssertEquals(false, adUser.EnterpriseEntity.GS_IsActive);
		}

		public void TestSynchroniseActiveAndLinkedStaffWithNoADRecord_ShouldReport()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			var adUserGuid = ZGuid.NewZGuid();
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Morgoth";
			staff.GS_ActiveDirectoryObjectGuid = adUserGuid;

			var adUser = new ADUser(staff);

			directorySearcherMock.Setup(s => s.FindUser("Morgoth", string.Empty)).Returns((IUserDirectoryEntry)null);

			var expectedMessage = string.Format(@"Cannot synchronize '{0}'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit {1} of domain {2} set in the registry item: {3}.
If '{0}' was created or activated recently please try again later.",
				staff.GS_LoginName,
				TestConstants.ValidOU,
				TestConstants.Domain,
				((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual
			);

			UnitTestUserNotification.Instance.ClearMessages();
			AssertEquals("Sync should fail", false, ((IADUser)adUser).Synchronise());

			AssertEquals("Unexpected error message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, adUser.EnterpriseEntity.GS_IsActive);
			AssertEquals("Staff should remain linked", adUserGuid, adUser.EnterpriseEntity.GS_ActiveDirectoryObjectGuid);
		}

		public void TestSynchroniseInactiveAndLinkedStaffWithNoADRecord_ShouldNotReport()
		{
			var adUserGuid = ZGuid.NewZGuid();
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Morgoth";
			staff.GS_IsActive = false;
			staff.GS_ActiveDirectoryObjectGuid = adUserGuid;

			var adUser = new ADUser(staff);

			directorySearcherMock.Setup(s => s.FindUser("Morgoth", string.Empty)).Returns((IUserDirectoryEntry)null);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());
			AssertEquals(false, adUser.EnterpriseEntity.GS_IsActive);
			AssertEquals("Staff should remain linked", adUserGuid, adUser.EnterpriseEntity.GS_ActiveDirectoryObjectGuid);
		}

		public void TestSynchroniseShouldHandleCOMException()
		{
			var adUserGuid = Guid.NewGuid();
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Morgoth";
			staff.GS_ActiveDirectoryObjectGuid = adUserGuid;

			var adUser = new ADUser(staff);
			//Construct a COMException with the expected ErrorCode to be caught
#if NETFRAMEWORK
			var flags = BindingFlags.Instance | BindingFlags.NonPublic;
			var hresultFieldInfo = typeof(COMException).GetField("_HResult", flags);
#else
			var flags = BindingFlags.Instance | BindingFlags.Public;
			var hresultFieldInfo = typeof(COMException).GetProperty("HResult", flags);
#endif
			var comExMessage = "-2147016646 LDAP_SERVER_DOWN: The server is not operational.";
			var comEx = new COMException(comExMessage);
			hresultFieldInfo.SetValue(comEx, -2147016646);

			directorySearcherMock.Setup(s => s.FindUser(adUserGuid, string.Empty)).Throws(comEx);

			AssertEquals("Sync should fail", false, ((IADUser)adUser).Synchronise());
			AssertEquals("COMException should be handled and re-thrown as DirectoryServicesException and handled", comExMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Staff should remain active", true, adUser.EnterpriseEntity.GS_IsActive);
			AssertEquals("Staff should remain linked", adUserGuid, adUser.EnterpriseEntity.GS_ActiveDirectoryObjectGuid);
		}

		public void TestSynchronise_LongLoginName_Unmatched()
		{
			AssertSynchronise_Unmatched("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrst");
		}

		public void TestSynchronise_LongLoginName_Unmatched_TruncatedSAMAccountAlreadyExists()
		{
			AssertSynchronise_Unmatched_NewSAMAccountsAlreadyExists("abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqr04", "abcdefghijklmnopqrst", "abcdefghijklmnopqr01", "abcdefghijklmnopqr02", "abcdefghijklmnopqr03");
		}

		public void TestSynchronise_LongLoginName_Matched()
		{
			AssertSynchronise_Matched("abcdefghijklmnopqrstuvwxyz");
		}

		public void TestSynchronise_LoginNameEndsWithDot_Unmatched()
		{
			AssertSynchronise_Unmatched("Leila.", "Leila_");
		}

		public void TestSynchronise_LoginNameEndsWithDot_Unmatched_NewSAMAccountsAlreadyExists()
		{
			AssertSynchronise_Unmatched_NewSAMAccountsAlreadyExists("Leila.", "Leil04", "Leila_", "Leil01", "Leil02", "Leil03");
		}

		public void TestSynchronise_LoginNameContainsAtSign_Unmatched()
		{
			AssertSynchronise_Unmatched("@Leila", "_Leila");
		}

		public void TestSynchronise_LoginNameContainsAtSign_Unmatched_NewSAMAccountsAlreadyExists()
		{
			AssertSynchronise_Unmatched_NewSAMAccountsAlreadyExists("@Leila", "_Lei04", "_Leila", "_Lei01", "_Lei02", "_Lei03");
		}

		public void TestSynchronise_LoginNameEndsWithDot_Matched()
		{
			AssertSynchronise_Matched("Leila.");
		}

		public void TestSynchronise_LoginNameContainsAtSign_Matched()
		{
			AssertSynchronise_Matched("Leila@");
		}

		void AssertSynchronise_Matched(string loginName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = loginName;
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			var adUser = new ADUser(staff);
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(loginName);

			directorySearcherMock.Setup(s => s.FindUser(loginName, string.Empty)).Returns(directoryEntry);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());

			//Unmatched AD causes creation of a new AD user, and will not get error
			Assert("Matched AD should get no error", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Matched AD should be activated", true, adUser.EnterpriseEntity.GS_IsActive);
			AssertEquals("Should be linked", directoryEntry.Guid, adUser.EnterpriseEntity.GS_ActiveDirectoryObjectGuid);
		}

		void AssertSynchronise_Unmatched_NewSAMAccountsAlreadyExists(string loginName, string expectedUserName, params string[] usersAlreadyExist)
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = loginName;
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			var adUser = new ADUser(staff);
			var mockedOU = new Mock<IOrganisationalUnit>();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(staff.GS_LoginName);
			var otherDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser("otheruser");

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);
			directorySearcherMock.Setup(s => s.FindUser(loginName, string.Empty)).Returns((IUserDirectoryEntry)null);
			foreach (var existingUser in usersAlreadyExist)
			{
				directorySearcherMock.Setup(s => s.FindUser(existingUser, string.Empty)).Returns(otherDirectoryEntry);
			}
			mockedOU.Setup(x => x.CreateNewChild(loginName, loginName, expectedUserName, DirectoryObjectType.User, directorySearcherMock.Object)).Returns(directoryEntry);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());

			//Unmatched AD causes creation of a new AD user, and will not get error
			Assert("Matched AD should get no error", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Matched AD should be activated", true, adUser.EnterpriseEntity.GS_IsActive);
			AssertEquals("Should be created and linked", directoryEntry.Guid, adUser.EnterpriseEntity.GS_ActiveDirectoryObjectGuid);

			foreach (var existingUser in usersAlreadyExist)
			{
				mockedOU.Verify(x => x.CreateNewChild(loginName, loginName, existingUser, DirectoryObjectType.User, directorySearcherMock.Object), Times.Never);
			}
		}

		void AssertSynchronise_Unmatched(string loginName, string usersAlreadyExist)
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = loginName;
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			var adUser = new ADUser(staff);
			var mockedOU = new Mock<IOrganisationalUnit>();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(staff.GS_LoginName);
			var otherDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser("otheruser");

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);
			directorySearcherMock.Setup(s => s.FindUser(loginName, string.Empty)).Returns((IUserDirectoryEntry)null);
			mockedOU.Setup(x => x.CreateNewChild(loginName, loginName, usersAlreadyExist, DirectoryObjectType.User, directorySearcherMock.Object)).Returns(directoryEntry);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());

			//Unmatched AD causes creation of a new AD user, and will not get error
			Assert("Matched AD should get no error", UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("Matched AD should be activated", true, adUser.EnterpriseEntity.GS_IsActive);
			AssertEquals("Should be created and linked", directoryEntry.Guid, adUser.EnterpriseEntity.GS_ActiveDirectoryObjectGuid);
		}

		public void TestGetDirectoryEntry_ShouldUseObjectGuidWhenPresent()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "ungoliant";
			var guid = Guid.NewGuid();
			staff.GS_ActiveDirectoryObjectGuid = guid;

			var adUser = new ADUser(staff);
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("ungoliant");

			directorySearcherMock.Setup(s => s.FindUser(guid, string.Empty)).Returns(directoryEntry);

			AssertEquals(directoryEntry, adUser.GetDirectoryEntry());
		}

		public void TestSynchronise_UnlinkedUserWithMatchedADRecordOutsideOU()
		{
			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection { domainCredentials });

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Unlinkie";
			staff.GS_FullName = "Un Lin Kie";
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			var adUser = new ADUser(staff);
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Unlinkie", fullName: "Pinkie Pine");

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);

			var expectedString = string.Format(@"Cannot create {0} in domain {1}, the same object already exists and may be located outside the Organizational Unit {2} set in the registry item: {3}",
				staff.GS_LoginName,
				domainCredentials.DomainName,
				domainCredentials.UserOrganisationalUnit,
				((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual);
			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());
			AssertEquals(expectedString, UnitTestUserNotification.Instance.LastMessage.Text);

			//Unlinked user with match outside the OU should not be linked, disabled and not sync
			AssertEquals(false, staff.IsADLinked);
			AssertEquals(false, staff.GS_IsActive);
			AssertEquals("Un Lin Kie", staff.GS_FullName);
		}

		public void TestSynchronise_PasswordDoesNotMatchPolicyException()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Frankie";
			staff.GS_FullName = "Funcky";
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			var adUser = new ADUser(staff);
			ADEntityProviderSubstitution.ADUser = adUser;

			var directoryEntry = new Mock<IUserDirectoryEntry>();
			directoryEntry.Setup(x => x.SetPassword(It.IsAny<string>(), It.IsAny<bool>())).Throws(new PasswordDoesNotMatchPolicyException("Password does not match policy."));

			var organisationalUnit = new Mock<IOrganisationalUnit>();
			organisationalUnit.Setup(x => x.CreateNewChild(staff.GS_LoginName, staff.GS_LoginName, staff.GS_LoginName, DirectoryObjectType.User, directorySearcherMock.Object)).
				Returns(directoryEntry.Object);

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);

			// Sync with ADUser.Syncrhonise
			AssertEquals("Sync with ADUser should pass", true, ((IADUser)adUser).Synchronise());
			AssertEquals("DefaultPasswordFailsToMeetDomainPolicy should be set", true, ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value[0].DefaultPasswordFailsToMeetDomainPolicy);
			// Should remain active and unlinked
			AssertEquals(false, staff.IsADLinked);
			AssertEquals(true, staff.GS_IsActive);
			AssertEquals("Funcky", staff.GS_FullName);

			// Sync with GlbStaff.SynchroniseWithAD
			UnitTestUserNotification.Instance.ClearMessages();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value[0].DefaultPasswordFailsToMeetDomainPolicy = false;
			var expectedMessage = @"The Domain Credentials Collection registry is using a password that does not meet the current password policy requirements of domain: sand.wtg.zone. Please contact your system administrator to correct this registry setting.
Synchronization with Active Directory will not work properly until this is fixed.";
			AssertExceptionThrown(typeof(PasswordDoesNotMatchPolicyException), expectedMessage, () => staff.SynchroniseWithAD());

			AssertEquals("DefaultPasswordFailsToMeetDomainPolicy should be set", true, ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value[0].DefaultPasswordFailsToMeetDomainPolicy);
			// Should remain active and unlinked
			AssertEquals(false, staff.IsADLinked);
			AssertEquals(true, staff.GS_IsActive);
			AssertEquals("Funcky", staff.GS_FullName);
		}

		public void TestSynchronise_LinkedUserWithMatchedADRecordOutsideOU()
		{
			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials));

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Linkie";
			staff.GS_FullName = "Linkie Thomas";
			var adUser = new ADUser(staff);
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Linkie", fullName: "Pinkie Pine");
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

			directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);

			var expectedMessage = string.Format(@"Cannot synchronize '{0}'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit {1} of domain {2} set in the registry item: {3}.
If '{0}' was created or activated recently please try again later.",
				staff.GS_LoginName,
				domainCredentials.UserOrganisationalUnit,
				domainCredentials.DomainName,
				((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual);
			UnitTestUserNotification.Instance.ClearMessages();
			AssertEquals("Sync should fail", false, ((IADUser)adUser).Synchronise());
			AssertEquals("Unexpected errror message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			//Linked user outside the OU should continue be linked, active but not sync
			AssertEquals(true, staff.IsADLinked);
			AssertEquals(true, staff.GS_IsActive);
			AssertEquals("Linkie Thomas", staff.GS_FullName);
		}

		public void TestSynchronise_LoginNameConflict()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "user1";
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("userX", path: "CN=userX");
			staff1.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "userX";
			staff2.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			Factory.Save();

			directoryEntry.SetLastModified(ZDateTime.UtcNow.ToDateTime());

			var adUser = new ADUser(staff1);

			directorySearcherMock.Setup(s => s.FindUser(staff1.GS_LoginName, "")).Returns(directoryEntry);

			var expectedError = @"Cannot synchronize Login Name 'user1' with Active Directory User's Logon Name 'userX' 
Staff Member's Login Name must be unique in the system";
			AssertEquals("Sync should fail", false, ((IADUser)adUser).Synchronise());
			AssertEquals("Unexpected errror", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("GS_LoginName", "user1", staff1.GS_LoginName);
			AssertEquals("HasChanges", false, staff1.HasChanges);
		}

		public void TestSynchronise_Manager()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var managerDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser("Gru", path: "CN=Gru,OU=ADUnitTesting,OU=Accounts,OU=root,DC=sand,DC=wtg,DC=zone");
			managerDirectoryEntry.SetLastModified(DateTime.UtcNow.AddDays(-1));

			var manager = Factory.NewWithValidTestData<GlbStaff>();
			manager.GS_Code = "GRU";
			manager.GS_LoginName = "Gru";
			manager.GS_ActiveDirectoryObjectGuid = managerDirectoryEntry.Guid;

			var staffDirectoryEntry = DummyDirectoryEntryWrapper.CreateUser("Bob", path: "CN=Bob,OU=ADUnitTesting,OU=Accounts,OU=root,DC=sand,DC=wtg,DC=zone");
			staffDirectoryEntry.SetLastModified(DateTime.UtcNow.AddDays(-1));

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_LoginName = "Bob";
			staff.GS_ActiveDirectoryObjectGuid = staffDirectoryEntry.Guid;

			StaffManagerTestHelper.AddManager(staff, manager, "DRM");
			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), "")).Returns(staffDirectoryEntry);
			directorySearcherMock.Setup(s => s.FindUser(manager.GS_ActiveDirectoryObjectGuid.ToGuid(), "")).Returns(managerDirectoryEntry);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var staffAdUser = new ADUser(staff);
				AssertEquals("CW Staff's manager is set", manager, staff.CurrentDRMManager);
				AssertEquals("AD User's manager is not set before sync", null, staffAdUser.Manager);
				AssertEquals("AD DirectoryEntry's manager is not set before sync", string.Empty, staffDirectoryEntry[ADAttributes.Manager]);

				((IADUser)staffAdUser).Synchronise();

				AssertEquals("AD DirectoryEntry's manager is set", managerDirectoryEntry[ADAttributes.DistinguishedName], staffDirectoryEntry[ADAttributes.Manager]);
				AssertEquals("AD User's manager is set", manager, staffAdUser.Manager);
			}
		}

		public void TestGetDirectoryEntry_ShouldUseLoginNameWhenNoObjectGuidPresent()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "ungoliant";

			var adUser = new ADUser(staff);
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("ungoliant");

			directorySearcherMock.Setup(s => s.FindUser("ungoliant", string.Empty)).Returns(directoryEntry);

			AssertEquals(directoryEntry, adUser.GetDirectoryEntry());
		}

		public void TestGetDirectoryEntry_ShouldUseLoginNameWhenNoObjectGuidPresent_DoesNotCrashWithInvalidOU()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = TestConstants.ADTestUserAccount.NameWithDomain;

			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = new DirectorySearcherWrapper(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, TestConstants.Domain);
			var adUser = new ADUser(staff);

			var directoryEntry = adUser.GetDirectoryEntry();
			AssertEquals("AD Test User Account", directoryEntry[ADAttributes.Name]);
		}

		public void TestGetDirectoryEntry_WhenPerformedOperationsUnderNonPrivilegedContext_ShouldThrow()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Fëanor";

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Fëanor");
			Assert(!directoryEntry.HasChanges);

			directorySearcherMock.Setup(s => s.FindUser("Fëanor", string.Empty)).Returns(directoryEntry);

			var adUser = new ADUser(staff);
			adUser.GetDirectoryEntry(false).UserPrincipalName = "Fëanor";

			Assert(adUser.GetDirectoryEntry(false).HasChanges);

			AssertExceptionThrown<InvalidOperationException>(() => adUser.GetDirectoryEntry(true));
		}

		public void TestGuid_ShouldComeFromStaffRecord()
		{
			var staff = Factory.New<GlbStaff>();
			var guid = ZGuid.NewZGuid();
			staff.GS_ActiveDirectoryObjectGuid = guid;
			var adUser = new ADUser(staff);
			AssertEquals(guid, adUser.Guid);
		}

		public void TestShortcutProperties()
		{
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("sauron@mordor.com");
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

			var adUser = new ADUser(staff);

			directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());

			AssertEquals("sauron@mordor.com", adUser.UserPrincipalName);
			AssertEquals("sauron", adUser.LoginName);
			AssertEquals("Lord Sauron", adUser.FullName);
			AssertEquals("1 Barad Dur Way", adUser.StreetAddress);
			AssertEquals("Gorgoroth", adUser.City);
			AssertEquals("Lord", adUser.Title);
			AssertEquals("Mordor", adUser.State);
			AssertEquals("1111", adUser.Postcode);
			AssertEquals("0294811111", adUser.WorkPhone);
			AssertEquals("0294811110", adUser.FaxNum);
			AssertEquals("0294811111", adUser.HomePhone);
			AssertEquals("0412345678", adUser.MobilePhone);
			AssertEquals("sauron@mordor.com", adUser.EmailAddress);
			AssertEquals("123456", adUser.Pager);
			AssertEquals("123", adUser.WorkExtension);
			AssertEquals(true, adUser.IsActive);
		}

		public override void TestGetPropertiesRequiringMissingDirectoryEntry_ShouldThrowInformativeException()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Sauron";
			var adUser = new ADUser(staff);

			directorySearcherMock.Setup(s => s.FindUser("Sauron", string.Empty)).Returns((IUserDirectoryEntry)null);

			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.UserPrincipalName; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.LoginName; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.FullName; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.StreetAddress; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.City; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.Title; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.State; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.Postcode; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.WorkPhone; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.FaxNum; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.HomePhone; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.MobilePhone; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.EmailAddress; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.Pager; });
			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adUser.IsActive; });
		}

		public void TestAccessPropertyOnWorkerThread_ShouldNotThrow()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = TestConstants.ADTestUserAccount.NameWithDomain;
			var adUser = new ADUser(staff);
			AssertNotNull(adUser.GetDirectoryEntry());
			AssertEquals(TestConstants.ADTestUserAccount.Guid, adUser.Guid.ToString());

			Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					AssertNoExceptionThrown(() => { var var = adUser.Guid; });
					AssertNoExceptionThrown(() => { var var = adUser.FullName; });
				}
			}).Wait();
		}

		public void TestPasswordMustChangeAtNextLogon()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Melkor";
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Melkor");

			directorySearcherMock.Setup(s => s.FindUser("Melkor", string.Empty)).Returns(directoryEntry);

			directoryEntry[ADAttributes.PasswordLastSet] = new ADLargeInteger(ADLargeIntegerHelper.MinDateTimeFromLargeInteger);
			var adUser = new ADUser(staff);
			Assert(adUser.PasswordMustChangeAtNextLogon);

			directoryEntry[ADAttributes.PasswordLastSet] = new ADLargeInteger(ZDateTime.UtcNow.ToDateTime());
			adUser = new ADUser(staff);
			Assert(!adUser.PasswordMustChangeAtNextLogon);
		}

		public void TestPasswordMustChangeAtNextLogon_Setter()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Melkor";
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Melkor");
			directoryEntry.PasswordMustChangeAtNextLogon = true;
			AssertEquals(0, directoryEntry[ADAttributes.PasswordLastSet]);

			var adUser = new ADUser(staff);
			((IADEntity)adUser).SetDirectoryEntry(directoryEntry);
			AssertEquals(ADLargeIntegerHelper.MinDateTimeFromLargeInteger, adUser.PasswordLastSet);

			directoryEntry.PasswordMustChangeAtNextLogon = false;
			AssertEquals(-1, directoryEntry[ADAttributes.PasswordLastSet]);
			adUser = new ADUser(staff);
			((IADEntity)adUser).SetDirectoryEntry(directoryEntry);
			AssertDateTimeWithinOneSecond("adUser.PasswordLastSet should be now", DateTime.UtcNow, adUser.PasswordLastSet);
		}

		public void TestGetNumberOfDaysTillPasswordExpiry()
		{
			const int passwordMaxAgeInDay = 5;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Melkor";

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Melkor");

			directorySearcherMock.Setup(s => s.FindUser("Melkor", string.Empty)).Returns(directoryEntry);

			var adUser = new ADUser(staff);
			directoryEntry.PasswordDoesntExpireUserAttribute = false;

			directoryEntry.PasswordExpirationDate = ZDateTime.Now.AddDays(passwordMaxAgeInDay).AddHours(-1).ToDateTime();
			AssertEquals(passwordMaxAgeInDay - 1, adUser.GetNumberOfDaysTillPasswordExpiry());
			directoryEntry.PasswordExpirationDate = ZDateTime.Now.AddMinutes(-1).ToDateTime();
			AssertEquals(0, adUser.GetNumberOfDaysTillPasswordExpiry());
			directoryEntry.PasswordExpirationDate = ZDateTime.Now.AddDays(-10).ToDateTime();
			AssertEquals(0, adUser.GetNumberOfDaysTillPasswordExpiry());
			//Invalid date should return 0
			directoryEntry.PasswordExpirationDate = default;
			AssertEquals(0, adUser.GetNumberOfDaysTillPasswordExpiry());
			//when AD's PasswordDoesntExpire is true, PasswordExpirationDate returns 30/12/1899 but we need to be able to pick that up
			directoryEntry.PasswordDoesntExpireUserAttribute = true;
			AssertEquals(int.MaxValue, adUser.GetNumberOfDaysTillPasswordExpiry());

			//When the user attribute PasswordDoesntExpire is false, but group policy is set to never expire, PasswordExpirationDate will return 1/1/1970, so it should return the max value
			directoryEntry.PasswordDoesntExpireUserAttribute = false;
			directoryEntry.PasswordExpirationDate = new DateTime(1970, 1, 1);
			AssertEquals(int.MaxValue, adUser.GetNumberOfDaysTillPasswordExpiry());
		}

		[ExpectNoExceptions]
		public void TestSetPassword()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Melkor";
			var directoryEntry = new Mock<IUserDirectoryEntry>();

			directorySearcherMock.Setup(s => s.FindUser("Melkor", string.Empty)).Returns(directoryEntry.Object);
			directoryEntry.SetupGet(x => x.Guid).Returns(Guid.NewGuid());

			var adUser = new ADUser(staff);
			adUser.ChangePassword("p@55w3rd", "N3Wp@55w3rd");

			directoryEntry.Verify(x => x.ChangePassword("p@55w3rd", "N3Wp@55w3rd"), Times.Once);
			directoryEntry.Verify(x => x.CommitChanges(), Times.Once);
		}

		public void TestIsValidPassword()
		{
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = TestConstants.ADTestUserAccountNoOURight.NameWithDomain;

			var adUser = new ADUser(staff);
			AssertEquals(true, adUser.IsPasswordValid(TestConstants.ADTestUserAccountNoOURight.Password));
			AssertEquals(false, adUser.IsPasswordValid("x"));
		}

		[ExpectNoExceptions]
		public void TestIsValidPassword_EmptyPassword()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Melkor";
			var adUser = new ADUser(staff);

			AssertExceptionThrown<ArgumentException>(() => adUser.IsPasswordValid(null));
		}

		void AssertRenamUserNameAndCommonName(string newUsername, string expectedSAMAccountName, string expectedCommonName, string[] usersAlreadyExist, string[] usersCommonNameAlreadyExist)
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "John";
			staff.GS_DomainName = TestConstants.Domain;
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(staff.GS_LoginName, path: "CN=" + staff.GS_LoginName);

			directorySearcherMock.Reset();
			directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUserByCommonName(staff.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry);

			if (usersAlreadyExist != null)
			{
				foreach (var existingUser in usersAlreadyExist)
				{
					directorySearcherMock.Setup(s => s.FindUser(existingUser, string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateUser("otheruser"));
				}
			}

			if (usersCommonNameAlreadyExist != null)
			{
				foreach (var existingUser in usersCommonNameAlreadyExist)
				{
					directorySearcherMock.Setup(s => s.FindUserByCommonName(existingUser, TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser("otheruser"));
				}
			}

			var adUser = new ADUser(staff);
			adUser.UserPrincipalName = staff.GS_LoginName + '@' + staff.GS_DomainName;
			adUser.SAMAccountName = staff.GS_LoginName;
			adUser.CommitChanges();

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());
			adUser.RenameLoginName(newUsername);
			adUser.CommitChanges();

			AssertEquals("Unexpected LoginName", newUsername, adUser.LoginName);
			AssertEquals("Unexpected User Principal Name", newUsername + '@' + staff.GS_DomainName, adUser.UserPrincipalName);
			AssertEquals("Unexpected SAMAccountName", expectedSAMAccountName, adUser.SAMAccountName);
			AssertEquals("Unexpected CommonName", expectedCommonName, directoryEntry["name"]);
		}

		public void TestRename()
		{
			AssertRenamUserNameAndCommonName("MelQoooor", "MelQoooor", "MelQoooor", null, null);
		}

		public void TestRename_LongCommonName()
		{
			var existingUserNamelist = new string[] { "IHaveAVeryVeryLongNa" };
			var existingCommonNamelist = new string[] { "IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNa" };

			// no conflict
			AssertRenamUserNameAndCommonName("IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongName",
				   "IHaveAVeryVeryLongNa",
				   "IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNa",
				   null,
				   null);

			// conflict in both
			AssertRenamUserNameAndCommonName("IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongName",
				"IHaveAVeryVeryLong01",
				"IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLong01",
				existingUserNamelist,
				existingCommonNamelist);

			// no conflict in username but conflict in common name
			AssertRenamUserNameAndCommonName("IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongName",
				   "IHaveAVeryVeryLongNa",
				   "IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLong01",
				   null,
				   existingCommonNamelist);

			// conflict in username but no conflict in common name
			AssertRenamUserNameAndCommonName("IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongName",
				   "IHaveAVeryVeryLong01",
				   "IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNa",
				   existingUserNamelist,
				   null);
		}

		public void TestRename_UserNameEndsWithDot()
		{
			AssertRenamUserNameAndCommonName("Leila.", "Leil01", "Leila.", new[] { "Leila_" }, null);
		}

		public void TestRename_LongUserNameEndsWithDot()
		{
			AssertRenamUserNameAndCommonName("abcdefghijklmnopqrstuvwxyz.", "abcdefghijklmnopqrst", "abcdefghijklmnopqrstuvwxyz.", null, null);
		}

		public void TestRename_LongUserNameEndsWithDotAndContainsAtSign()
		{
			AssertRenamUserNameAndCommonName("abcdefghijk@lmnopqrstuvwxyz.", "abcdefghijk_lmnopqrs", "abcdefghijk@lmnopqrstuvwxyz.", null, null);
		}

		public void TestRename_LongUserNameContainsDotAt20thCharacter()
		{
			AssertRenamUserNameAndCommonName("abcdefghijklmnopqrs.uvwxyz", "abcdefghijklmnopqrs_", "abcdefghijklmnopqrs.uvwxyz", null, null);
		}

		public void TestRename_OneLetterUserNameToEndsWithDot()
		{
			AssertRenamUserNameAndCommonName("a.", "a_", "a.", null, null);
		}

		public void TestRename_OneLetterUserNameToEndsWithDot_NewSAMAccountsAlreadyExists()
		{
			AssertRenamUserNameAndCommonName("a.", "a10", "a.", new[] { "a_", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9" }, null);
		}

		public void TestRename_LongUserNameContainsAtSign()
		{
			AssertRenamUserNameAndCommonName("abcdefghij@klmnopqrstuvwxyz", "abcdefghij_klmnopqrs", "abcdefghij@klmnopqrstuvwxyz", null, null);
		}

		public void TestRename_ThreeLettersUserNameContainsAtSign()
		{
			AssertRenamUserNameAndCommonName("A@B", "A01", "A@B", new[] { "A_B" }, null);
		}

		public void TestRename_UserNameContainsAtSigns()
		{
			AssertRenamUserNameAndCommonName("jer@min@kok", "jer_min_kok", "jer@min@kok", null, null);
		}

		public void TestRename_UserNameContainsAtSigns_NewSAMAccountNameExists()
		{
			AssertRenamUserNameAndCommonName("jer@min@kok", "jer_min_k01", "jer@min@kok", new[] { "jer_min_kok" }, null);
		}

		public void TestRename_LongUserName_NoUniqueFound()
		{
			var existingUserNamelist = new List<string>();
			existingUserNamelist.Add("IHaveAVeryVeryLongNa");
			for (int i = 1; i < 100; i++)
			{
				existingUserNamelist.Add("IHaveAVeryVeryLong" + i.ToString().PadLeft(2, '0'));
			}
			AssertRenamUserNameAndCommonName("IHaveAVeryVeryLongName", "John", "IHaveAVeryVeryLongName", existingUserNamelist.ToArray(), null);
		}

		public void TestRename_LongUserName_NoUniqueFoundForCommonName()
		{
			var existingUserNamelist = new List<string>();
			existingUserNamelist.Add("IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongNa");
			for (int i = 1; i < 100; i++)
			{
				existingUserNamelist.Add("IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLong" + i.ToString().PadLeft(2, '0'));
			}
			AssertRenamUserNameAndCommonName("IHaveAVeryVeryLongNameIHaveAVeryVeryLongNameIHaveAVeryVeryLongName", "IHaveAVeryVeryLongNa", "John", null, existingUserNamelist.ToArray());
		}

		public void TestRename_ShortUserName_NoUniqueFound()
		{
			var existingUserNamelist = new List<string>();
			existingUserNamelist.Add("A_");
			for (int i = 1; i < 100; i++)
			{
				existingUserNamelist.Add("A" + i.ToString());
			}
			AssertRenamUserNameAndCommonName("A.", "John", "A.", existingUserNamelist.ToArray(), null);
		}

		public void TestCommitChanges_WhenNoDirectoryEntry()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "this.is.a.rare.name";

			var adUser = new ADUser(staff);
			AssertNull(adUser.GetDirectoryEntry());
			AssertNoExceptionThrown(() => adUser.CommitChanges());
		}

		public void TestCommitChanges_WhenNoChangesMade()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = TestConstants.ADTestUserAccount.NameWithDomain;

			var adUser = new ADUser(staff);
			AssertNotNull(adUser.GetDirectoryEntry());
			Assert(!adUser.GetDirectoryEntry().HasChanges);
			AssertNoExceptionThrown(() => adUser.CommitChanges());
		}

		public void TestSynchronise_WhenWiseCloudAccessSecurityGroupRegistryisOn()
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.WiseCloudAccessSecurityGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "TestGroup1", "TestGroup2", "TestGroup3", "TestGroup4" });
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var adGroup1 = DummyDirectoryEntryWrapper.CreateGroup("TestGroup1");
			var adGroup2 = DummyDirectoryEntryWrapper.CreateGroup("TestGroup2");

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Leila";
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var adUser = new ADUser(staff);
			var mockedOU = new Mock<IOrganisationalUnit>();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Leila");

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);
			mockedOU.Setup(x => x.CreateNewChild("Leila", "Leila", "Leila", DirectoryObjectType.User, directorySearcherMock.Object)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser("Leila", string.Empty)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindGroup("TestGroup1", string.Empty)).Returns(adGroup1);
			directorySearcherMock.Setup(s => s.FindGroup("TestGroup2", string.Empty)).Returns(adGroup2);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());

			AssertEquals(1, adGroup1.GetMembers().Count());
			AssertEquals(1, adGroup2.GetMembers().Count());

			AssertCollectionContains(directoryEntry, adGroup1.GetMembers());
			AssertCollectionContains(directoryEntry, adGroup2.GetMembers());

			AssertEquals("Could not find the following WiseCloud Access Security group(s) in domain sand.wtg.zone: TestGroup3, TestGroup4", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSynchronise_SelfHostedIgnoreWiseCloudAccessSecurityGroupRegistry()
		{
			EnvProxy.SetHostedLocationForTest(string.Empty);

			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.WiseCloudAccessSecurityGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "TestGroup1", "TestGroup2" });
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var adGroup1 = DummyDirectoryEntryWrapper.CreateGroup("TestGroup1");

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Leila";
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var adUser = new ADUser(staff);
			var mockedOU = new Mock<IOrganisationalUnit>();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Leila");

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);
			mockedOU.Setup(x => x.CreateNewChild("Leila", "Leila", "Leila", DirectoryObjectType.User, directorySearcherMock.Object)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser("Leila", string.Empty)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindGroup("TestGroup1", string.Empty)).Returns(adGroup1);

			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());
			AssertEquals("Staff has linked", staff.GS_ActiveDirectoryObjectGuid, directoryEntry.Guid);
			AssertEquals(0, adGroup1.GetMembers().Count());
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCommitChanges_CanHandleException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var directoryEntry = new Mock<IUserDirectoryEntry>();

			var adUser = new ADUser(staff);

			//Construct a DirectoryServicesCOMException with the expected ErrorCode to be caught
#if NETFRAMEWORK
			var flags = BindingFlags.Instance | BindingFlags.NonPublic;
			var hresultFieldInfo = typeof(DirectoryServicesCOMException).GetField("_HResult", flags);
#else
			var flags = BindingFlags.Instance | BindingFlags.Public;
			var hresultFieldInfo = typeof(DirectoryServicesCOMException).GetProperty("HResult", flags);
#endif
			var innerEx = new DirectoryServicesCOMException("A device attached to the system is not functioning.");
			hresultFieldInfo.SetValue(innerEx, -2147024865);

			var ex = new DirectoryServicesException("Some message", innerEx);

			directoryEntry.SetupGet(x => x.Guid).Returns(staff.GS_ActiveDirectoryObjectGuid.ToGuid());
			directoryEntry.As<IDirectoryEntry>().SetupGet(x => x.Guid).Returns(staff.GS_ActiveDirectoryObjectGuid.ToGuid());
			directoryEntry.SetupGet(x => x.CanUpdate).Returns(true);
			directoryEntry.SetupGet(x => x.HasChanges).Returns(true);
			directoryEntry.As<IDirectoryEntry>().SetupGet(x => x.IsActive).Returns(true);

			directoryEntry.Setup(x => x.CommitChanges()).Throws(ex);

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), string.Empty)).Returns(directoryEntry.Object);

			adUser.GetDirectoryEntry(true); // This call is required to set retrievedWithWritePrivilege true, there is no other way to set it
			AssertEquals("Sync should pass", true, ((IADUser)adUser).Synchronise());
			AssertExceptionThrown<DirectoryServicesException>("UnexpectedMessage", @"Could not save user with username ''.
Please ensure that the username is not longer than 20 characters and does not contain any of these symbols: "" / \ [ ] : ; | = , + * ? < >",
() => adUser.CommitChanges());
			AssertEquals("Staff should not be disabled", true, adUser.EnterpriseEntity.GS_IsActive);
			AssertEquals("Staff should still be linked", true, adUser.EnterpriseEntity.IsADLinked);
		}

		public override void TestIsIdentityInConflict()
		{
			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("Elijah.Wood");
			var directoryEntry2 = DummyDirectoryEntryWrapper.CreateUser("frodo.baggins");

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Elijah.Wood";
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry1.Guid;

			directorySearcherMock.Setup(s => s.FindUser(directoryEntry1.Guid, string.Empty)).Returns(directoryEntry1);
			directorySearcherMock.Setup(s => s.FindUser("frodo.baggins", string.Empty)).Returns(directoryEntry2);

			var adUser = new ADUser(staff);
			Assert(!adUser.IsIdentityInConflict());
			staff.GS_LoginName = "frodo.baggins";
			Assert(adUser.IsIdentityInConflict());
		}

		public void TestCommitChangesDirectoryServicesCOMExceptionHandled()
		{
			string testExceptionMessage = "AD Test Error";

			var staff = Factory.New<GlbStaff>();
			var adUser = new ADUser(staff);
			ADEntityProviderSubstitution.ADUser = adUser;
			staff.GS_LoginName = "testUser";

			ActiveDirectoryRegistry.Instance.ReportHandledDirectoryExceptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			directorySearcherMock.Setup(s => s.FindUser("testUser", string.Empty)).Throws(new DirectoryServicesCOMException(testExceptionMessage));

			staff.SynchroniseWithAD();
			Assert(CargoWise.Common.ErrorReporter.LastExceptionReported.Message.Equals(testExceptionMessage));
			CargoWise.Common.ErrorReporter.Clear();
		}

		public void TestHandleObjectAlreadyExistsWithBetterMessage()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var mockedOU = new Mock<IOrganisationalUnit>();

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			staff.GS_LoginName = "testUser";
			var adUser = new ADUser(staff);

			//Construct a DirectoryServicesCOMException with the expected ErrorCode to be caught
#if NETFRAMEWORK
			var flags = BindingFlags.Instance | BindingFlags.NonPublic;
			var hresultFieldInfo = typeof(DirectoryServicesCOMException).GetField("_HResult", flags);
#else
			var flags = BindingFlags.Instance | BindingFlags.Public;
			var hresultFieldInfo = typeof(DirectoryServicesCOMException).GetProperty("HResult", flags);
#endif
			var ex = new DirectoryServicesCOMException("The object already exists.");
			hresultFieldInfo.SetValue(ex, -2147019886);

			directorySearcherMock.Setup(s => s.FindUser("testUser", "")).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindUser("testUser", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(mockedOU.Object);
			mockedOU.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), directorySearcherMock.Object)).Throws(ex);

			AssertEquals("Sync should fail", false, ((IADUser)adUser).Synchronise());

			AssertEquals(@"An error occurred while communicating with the Active Directory controller. Please contact your System Administrator.

Error message is as follow:
The object already exists.
Please check for Active Directory objects with any of these values:
CN=testUser
UserPrincipleName=testUser
SAMAccountName=testUser", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Staff should not be disabled", true, staff.GS_IsActive);
			AssertEquals("Staff should not be linked", false, staff.IsADLinked);

			//Assert Error when call from dbo.GlbStaff
			staff.SynchroniseWithAD();
			AssertEquals(@"An error occurred while communicating with the Active Directory controller. Please contact your System Administrator.

Error message is as follow:
The object already exists.
Please check for Active Directory objects with any of these values:
CN=testUser
UserPrincipleName=testUser
SAMAccountName=testUser", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Staff should not be disabled", true, staff.GS_IsActive);
			AssertEquals("Staff should not be linked", false, staff.IsADLinked);
		}

		public void TestDomainNetBiosName()
		{
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var adUser = new ADUser(Factory.New<GlbStaff>());
			AssertEquals("SAND", adUser.DomainNetBiosName);

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
			{
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ADTestHelper.CreateDomainCredentialsCollection(
						domainName: "wtg.zone",
						domainUserName: TestConstants.ADTestUserAccount.NameWithDomain,
						domainUserPassword: TestConstants.ADTestUserAccount.Password));

				adUser = new ADUser(Factory.New<GlbStaff>());
				AssertEquals("CORP", adUser.DomainNetBiosName);
			}
		}

		public void TestLockedOut()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "pooh";
			var directoryEntry = new Mock<IUserDirectoryEntry>();

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, string.Empty)).Returns(directoryEntry.Object);
			directoryEntry.SetupGet(x => x.LockedOut).Returns(true);

			var adUser = new ADUser(staff);
			AssertEquals(true, adUser.LockedOut);
		}

		protected override IADLinkedEntity CreateEnterpriseEntity(string identity, string domainName, ZGuid activeDirectoryObjectGuid)
		{
			var staff = Helper.CreateStaff(identity);
			staff.GS_DomainName = domainName;
			staff.GS_ActiveDirectoryObjectGuid = activeDirectoryObjectGuid;
			return staff;
		}

		protected override IADEntity CreateADEntity(IADLinkedEntity enterpriseEntity)
		{
			return new ADUser((GlbStaff)enterpriseEntity);
		}

		public override void TestSync_WithRaceCondition()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			// Instance 1 create the staff and saved, but has not synced to AD
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var staff = factory1.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "jon";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			factory1.Save();

			// Instance 2 sync and link it to a new AD user on different AD site not yet accessible by instance 1
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var staffFromFactory2 = factory2.Load<GlbStaff>(staff.PK);
			staffFromFactory2.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid(); // pretend this is a new ad user that is not exist yet in site 1
			factory2.Save();

			// When sync from Instance 1, it should reload and relised it has been linked, and report error as it is not yet accessible.
			AssertEquals("Staff should not linked yet in the factory1 before sync", false, staff.IsADLinked);
			var adUser = new ADUser(staff);
			ADEntityProviderSubstitution.ADUser = adUser;
			staff.SynchroniseWithAD();

			var expectedMessage = @"Cannot synchronize 'jon'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'jon' was created or activated recently please try again later.";
			AssertEquals("Expecting error message when sync from Instance 1", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Staff should be linked", true, staff.IsADLinked);
			AssertEquals("Staff should be linked to the same AD object", staffFromFactory2.GS_ActiveDirectoryObjectGuid, staff.GS_ActiveDirectoryObjectGuid);
		}
	}

	[TestedType(typeof(ADUser))]
	class ADUser_NonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ADUser(Factory.NewWithValidTestData<GlbStaff>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			directorySearcherMock = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcherMock.Object;
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser("Test.User"));
		}
		Mock<IDirectorySearcher> directorySearcherMock;
	}
}
