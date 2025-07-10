using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Caching;
using System.Security.AccessControl;
using System.Security.Principal;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using Moq;
using NUnit.Framework;
using static Enterprise.Security.ActiveDirectory.DomainCredentialsValidation;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class DirectorySearcherFactoryTest : TransactionedTestCase
	{
		public void TestGetDirectorySearcher()
		{
			var domainCredentials = new DomainCredentials()
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccountNoOURight.Name,
				DomainUserPassword = TestConstants.ADTestUserAccountNoOURight.Password,
				IsDefaultDomain = true,
				UserOrganisationalUnit = TestConstants.ValidOU,
				GroupOrganisationalUnit = TestConstants.ValidOU,
				DefaultPassword = "Changeme1234"
			};

			var searcher = DirectorySearcherFactory.GetDirectorySearcher(domainCredentials);

			AssertEquals(TestConstants.Domain, searcher.DomainName);
			AssertEquals(TestConstants.ADTestUserAccountNoOURight.Name, searcher.UserName);
			AssertEquals(TestConstants.ADTestUserAccountNoOURight.Password, searcher.Password);

			var searcherWrapper = ((DirectorySearcherWrapper)searcher).GetDirectorySearcher("");
			var root = searcherWrapper.SearchRoot;

			AssertEquals("LDAP://" + TestConstants.Domain, root.Path);
			AssertEquals(TestConstants.ADTestUserAccountNoOURight.Name, root.Username);
		}

		public void TestGetDirectorySearcher_NullDomainCredentials()
		{
			using (new WindowsIdentityImpersonator(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, () =>
			{
				AssertEquals(0, ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.Count);

				var searcher = DirectorySearcherFactory.GetDirectorySearcher(null);

				AssertEquals(Domain.GetCurrentDomain().Name, searcher.DomainName);
				AssertEquals(null, searcher.UserName);
				AssertEquals(null, searcher.Password);
			}))
			{ }
		}

		public void TestGetDirectorySearcher_LoadFromCache()
		{
			var searcher1 = DirectorySearcherFactory.GetDirectorySearcher(new DomainCredentials() { DomainName = "domain1" });
			var searcher2 = DirectorySearcherFactory.GetDirectorySearcher(new DomainCredentials() { DomainName = "domain2" });
			var searcher3 = DirectorySearcherFactory.GetDirectorySearcher(new DomainCredentials() { DomainName = "domain1" });

			AssertSame("Searcher1 and 3 should be the same", searcher1, searcher3);
			AssertNotEquals("Searcher1 and 2 should not be the same", searcher1, searcher2);
		}

		public void TestClearCache()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			DirectorySearcherFactory.GetDirectorySearcher(ADTestHelper.CreateDomainCredentials(), false);
			DirectorySearcherFactory.GetDirectorySearcher(ADTestHelper.CreateDomainCredentials(), true);

			var cache = MemoryCache.Default;
			AssertEquals(true, cache.Contains("DirectorySearcherFactory-Read-" + TestConstants.Domain));
			AssertEquals(true, cache.Contains("DirectorySearcherFactory-ReadWrite-" + TestConstants.Domain));

			DirectorySearcherFactory.ClearCache();

			AssertEquals(false, cache.Contains("DirectorySearcherFactory-Read-" + TestConstants.Domain));
			AssertEquals(false, cache.Contains("DirectorySearcherFactory-ReadWrite-" + TestConstants.Domain));
		}

		readonly static byte[] currentSID = new byte[28] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
		readonly static byte[] oldSID = new byte[28] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 };
		readonly static byte[] groupSID = new byte[28] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3 };

		readonly SecurityIdentifier currentIdentifier = new SecurityIdentifier(currentSID, 0);
		readonly SecurityIdentifier oldIdentifier = new SecurityIdentifier(oldSID, 0);
		readonly SecurityIdentifier groupIdentifier = new SecurityIdentifier(groupSID, 0);

		static readonly Guid OrganisationalUnitTypeGuid_User = new Guid("bf967aba-0de6-11d0-a285-00aa003049e2");

		IDirectorySearcher TestUserACL_FullControl_Explicit(SecurityIdentifier identifier, AccessControlType accessControlType)
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(identifier, ActiveDirectoryRights.GenericAll, accessControlType, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);
			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);

			return directorySearcher;
		}

		public void TestUserACL_FullControl_Explicit_Denied()
		{
			var directorySearcher = TestUserACL_FullControl_Explicit(currentIdentifier, AccessControlType.Deny);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied by objectSid", false, isAllowed);

			directorySearcher = TestUserACL_FullControl_Explicit(oldIdentifier, AccessControlType.Deny);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied by sIDHistory", false, isAllowed);

			directorySearcher = TestUserACL_FullControl_Explicit(groupIdentifier, AccessControlType.Deny);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied by tokenGroups", false, isAllowed);
		}

		public void TestUserACL_FullControl_Explicit_Granted()
		{
			var directorySearcher = TestUserACL_FullControl_Explicit(currentIdentifier, AccessControlType.Allow);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has required permissions to sync, allowed by objectSid", true, isAllowed);

			directorySearcher = TestUserACL_FullControl_Explicit(oldIdentifier, AccessControlType.Allow);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has required permissions to sync, allowed by sIDHistory", true, isAllowed);

			directorySearcher = TestUserACL_FullControl_Explicit(groupIdentifier, AccessControlType.Allow);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has required permissions to sync, allowed by tokenGroups", true, isAllowed);
		}

		public void TestUserACL_FullControl_Explicit_Denied_OldIdentifier()
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(currentIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(oldIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Deny, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, allowed by objectSid but denied by sIDHistory", false, isAllowed);
		}

		public void TestUserACL_FullControl_Explicit_Denied_GroupIdentifier()
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(currentIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Allow, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(groupIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Deny, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, allowed by objectSid but denied by tokenGroups", false, isAllowed);
		}

		IDirectorySearcher TestUserACL_CreateChild_FullControlOnDescendent_Explicit(SecurityIdentifier identifier, AccessControlType accessControlType)
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(identifier, ActiveDirectoryRights.GenericAll, accessControlType, ActiveDirectorySecurityInheritance.Descendents, OrganisationalUnitTypeGuid_User);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(identifier, ActiveDirectoryRights.CreateChild, accessControlType, OrganisationalUnitTypeGuid_User, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);
			return directorySearcher;
		}

		IDirectorySearcher TestUserACL_AllowCreateChild_FullControlOnDescendent_Explicit(SecurityIdentifier identifier, AccessControlType accessControlType)
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(identifier, ActiveDirectoryRights.GenericAll, AccessControlType.Allow, ActiveDirectorySecurityInheritance.Descendents, OrganisationalUnitTypeGuid_User);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(identifier, ActiveDirectoryRights.CreateChild, accessControlType, OrganisationalUnitTypeGuid_User, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);
			return directorySearcher;
		}

		IDirectorySearcher TestUserACL_AllowFullControlOnDescendent_CreateChild_Explicit(SecurityIdentifier identifier, AccessControlType accessControlType)
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(identifier, ActiveDirectoryRights.GenericAll, accessControlType, ActiveDirectorySecurityInheritance.Descendents, OrganisationalUnitTypeGuid_User);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(identifier, ActiveDirectoryRights.CreateChild, AccessControlType.Allow, OrganisationalUnitTypeGuid_User, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);
			return directorySearcher;
		}

		public void TestUserACL_CreateChild_FullControlOnDescendent_Explicit_Denied()
		{
			var directorySearcher = TestUserACL_CreateChild_FullControlOnDescendent_Explicit(currentIdentifier, AccessControlType.Deny);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied by objectSid", false, isAllowed);

			directorySearcher = TestUserACL_CreateChild_FullControlOnDescendent_Explicit(oldIdentifier, AccessControlType.Deny);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied by sIDHistory", false, isAllowed);

			directorySearcher = TestUserACL_CreateChild_FullControlOnDescendent_Explicit(groupIdentifier, AccessControlType.Deny);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied by tokenGroups", false, isAllowed);

			directorySearcher = TestUserACL_AllowCreateChild_FullControlOnDescendent_Explicit(currentIdentifier, AccessControlType.Deny);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied FullControlOnDescendent by objectSid", false, isAllowed);

			directorySearcher = TestUserACL_AllowFullControlOnDescendent_CreateChild_Explicit(currentIdentifier, AccessControlType.Deny);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, denied CreateChild by objectSid", false, isAllowed);
		}

		public void TestUserACL_CreateChild_FullControlOnDescendent_Explicit_Granted()
		{
			var directorySearcher = TestUserACL_CreateChild_FullControlOnDescendent_Explicit(currentIdentifier, AccessControlType.Allow);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has required permissions to sync, allowed by objectSid", true, isAllowed);

			directorySearcher = TestUserACL_CreateChild_FullControlOnDescendent_Explicit(oldIdentifier, AccessControlType.Allow);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has required permissions to sync, allowed by sIDHistory", true, isAllowed);

			directorySearcher = TestUserACL_CreateChild_FullControlOnDescendent_Explicit(groupIdentifier, AccessControlType.Allow);
			isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has required permissions to sync, allowed by tokenGroups", true, isAllowed);
		}

		public void TestUserACL_CreateChild_FullControlOnDescendent_Explicit_Denied_OldIdentifier()
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(currentIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Allow, ActiveDirectorySecurityInheritance.Descendents, OrganisationalUnitTypeGuid_User);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(currentIdentifier, ActiveDirectoryRights.CreateChild, AccessControlType.Allow, OrganisationalUnitTypeGuid_User, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(oldIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Deny, ActiveDirectorySecurityInheritance.Descendents, OrganisationalUnitTypeGuid_User);
			activeDirectorySecurity.AddAccessRule(rule);

			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, allowed by objectSid but denied by sIDHistory", false, isAllowed);
		}

		public void TestUserACL_CreateChild_FullControlOnDescendent_Explicit_Denied_GroupIdentifier()
		{
			var activeDirectorySecurity = new ActiveDirectorySecurity();
			var rule = new ActiveDirectoryAccessRule(currentIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Allow, ActiveDirectorySecurityInheritance.Descendents, OrganisationalUnitTypeGuid_User);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(currentIdentifier, ActiveDirectoryRights.CreateChild, AccessControlType.Allow, OrganisationalUnitTypeGuid_User, ActiveDirectorySecurityInheritance.All);
			activeDirectorySecurity.AddAccessRule(rule);

			rule = new ActiveDirectoryAccessRule(groupIdentifier, ActiveDirectoryRights.GenericAll, AccessControlType.Deny, ActiveDirectorySecurityInheritance.Descendents, OrganisationalUnitTypeGuid_User);
			activeDirectorySecurity.AddAccessRule(rule);

			var directorySearcher = SetupActiveDirectory(activeDirectorySecurity);
			var isAllowed = ADAccessRuleChecker.HasRequiredPermissionsToSync(OrganisationalUnitType.User, directorySearcher, "");
			AssertEquals("User has NO required permissions to sync, allowed by objectSid but denied by tokenGroups", false, isAllowed);
		}

		IDirectorySearcher SetupActiveDirectory(ActiveDirectorySecurity activeDirectorySecurity)
		{
			var directoryEntry = new DirectoryEntry();
			directoryEntry.Properties["sIDHistory"].Insert(0, oldSID);
			directoryEntry.Properties["tokenGroups"].Insert(0, groupSID);
			var ouDE = new DirectoryEntry();
			ouDE.ObjectSecurity = activeDirectorySecurity;

			var directorySearcher = new Mock<IDirectorySearcher>();
			var userDirectoryEntry = (DirectoryEntryWrapper)DirectoryEntryWrapperProvider.Get(directoryEntry, directorySearcher.Object, DirectoryObjectType.User);
			var ouDirectoryEntry = (IOrganisationalUnit)DirectoryEntryWrapperProvider.Get(ouDE, directorySearcher.Object, DirectoryObjectType.OrganisationalUnit);

			var user = new Mock<IUserDirectoryEntry>();
			user.Setup(u => u.GetDirectoryEntry()).Returns((IUserDirectoryEntry)userDirectoryEntry);
			user.Setup(u => u["objectSid", 0]).Returns(currentSID);

			directorySearcher.Setup(m => m.FindUser(It.IsAny<string>(), "")).Returns(user.Object);
			directorySearcher.Setup(m => m.FindOrganisationalUnit(It.IsAny<string>())).Returns(ouDirectoryEntry);

			return directorySearcher.Object;
		}

		protected override void TearDown()
		{
			base.TearDown();

			DirectorySearcherFactory.ClearCache();
		}
	}
}
