using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class ModuleADActionsMenuItemTest : TestCaseWithFactoryAndMocks
	{
		public void TestCreateMenu_SetDomainNameInBulk()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection() {
				ADTestHelper.CreateDomainCredentials(domainName: "domain1"),
				ADTestHelper.CreateDomainCredentials(domainName: "domain2", isDefaultDomain: false)
			});

			var module = new Mock<ZFilterGridModule>();
			MenuItem menuItem;
			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { Factory.New<GlbStaff>(), Factory.New<GlbStaff>() });
			module.Setup(m => m.AllowEdit).Returns(true);

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals("Should show Set Domain Name when multi domains multi selections", true, menuItem.MenuItems.Cast<MenuItem>().Any(x => x.Text == "Set Domain Name"));
		}

		public void TestCreateMenu_WhenNoRowsSelected()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(Array.Empty<BusinessObject>());
			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(1, menuItem.MenuItems.Count);
			AssertEquals("Please select a record", menuItem.MenuItems[0].Text);
		}

		public void TestCreateMenu_WhenResourcesOnlyAreSelected()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var resource1 = Factory.New<GlbStaff>();
			resource1.GS_Code = "$R1";
			resource1.GS_IsActive = true;
			resource1.GS_IsResource = true;

			var resource2 = Factory.New<GlbStaff>();
			resource2.GS_Code = "$R2";
			resource2.GS_IsActive = false;
			resource2.GS_IsResource = true;

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { resource1, resource2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(1, menuItem.MenuItems.Count);
			AssertEquals("Only active or linked staff can be synchronized with AD.", menuItem.MenuItems[0].Text);
		}

		public void TestCreateMenu_Group_WhenNotSyncingStaff()
		{
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_IsActive = true;
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_IsActive = false;
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			AssertEquals(true, staff1.IsADLinked);
			AssertEquals(false, staff2.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff1, staff2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(1, menuItem.MenuItems.Count);
			AssertEquals("Set Domain Name", menuItem.MenuItems[0].Text);
		}

		public void TestGetMenuItem_IntegrationDisabled()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			var module = new Mock<ZFilterGridModule>();

			AssertNotNull("AD menu item should be shown regardless of AD Integration status", new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory));
			module.Object.Dispose();
		}

		public void TestGetMenuItem_IntegrationEnabled()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			var module = new Mock<ZFilterGridModule>();

			AssertNotNull("AD menu item should be shown regardless of AD Integration status", new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory));
			module.Object.Dispose();
		}

		public void TestGetMenuItem_ActiveLinkedStaff()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_IsActive = true;
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_IsActive = false;
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			AssertEquals(true, staff1.IsADLinked);
			AssertEquals(false, staff2.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff1, staff2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(4, menuItem.MenuItems.Count);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertEquals("Disconnect From Active Directory", menuItem.MenuItems[1].Text);
			AssertEquals("Unlock Account", menuItem.MenuItems[2].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[3].Text);
		}

		public void TestGetMenuItem_ActiveLinkedGroup()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = Factory.New<GlbGroup>();
			group1.GG_IsActive = true;
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			var group2 = Factory.New<GlbGroup>();
			group2.GG_IsActive = false;
			group2.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;

			AssertEquals(true, group1.IsADLinked);
			AssertEquals(false, group2.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { group1, group2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(3, menuItem.MenuItems.Count);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertEquals("Disconnect From Active Directory", menuItem.MenuItems[1].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[2].Text);
		}

		public void TestGetMenuItem_LinkedAndActiveButCantLoginStaff()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_CanLogin = false;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(2, menuItem.MenuItems.Count);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[1].Text);
		}

		public void TestGetMenuItem_NewActiveGroupCanSynchronize()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group = Factory.New<GlbGroup>();
			group.GG_IsActive = true;
			group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { group });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(2, menuItem.MenuItems.Count);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[1].Text);
		}

		public void TestGetMenuItem_InactiveUnlinkedStaff()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_IsActive = false;
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_IsActive = false;
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			AssertEquals(false, staff1.IsADLinked);
			AssertEquals(false, staff2.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff1, staff2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(2, menuItem.MenuItems.Count);
			AssertEquals("Only active or linked staff can be synchronized with AD.", menuItem.MenuItems[0].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[1].Text);
		}

		public void TestGetMenuItem_InactiveUnlinkedGroup()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = Factory.New<GlbGroup>();
			group1.GG_IsActive = false;
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
			var group2 = Factory.New<GlbGroup>();
			group2.GG_IsActive = false;
			group2.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;

			AssertEquals(false, group1.IsADLinked);
			AssertEquals(false, group2.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { group1, group2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(2, menuItem.MenuItems.Count);
			AssertEquals("Only non-system, active or linked groups can be synchronized with AD.", menuItem.MenuItems[0].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[1].Text);
		}

		public void TestGetMenuItem_SystemGroup()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var sysGroup = Factory.New<GlbGroup>();
			sysGroup.GG_IsSystemDefined = true;
			AssertNotNull(sysGroup);

			var module = new Mock<ZFilterGridModule>();
			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { sysGroup });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(1, menuItem.MenuItems.Count);
			AssertEquals("Only non-system, active or linked groups can be synchronized with AD.", menuItem.MenuItems[0].Text);
		}

		public void TestGetMenuItem_InactiveLinkedStaff()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_IsActive = false;
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_IsActive = false;
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			AssertEquals(true, staff1.IsADLinked);
			AssertEquals(false, staff2.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff1, staff2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(4, menuItem.MenuItems.Count);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertEquals("Disconnect From Active Directory", menuItem.MenuItems[1].Text);
			AssertEquals("Unlock Account", menuItem.MenuItems[2].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[3].Text);
		}

		public void TestGetMenuItem_InactiveLinkedGroup()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = Factory.New<GlbGroup>();
			group1.GG_IsActive = false;
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			var group2 = Factory.New<GlbGroup>();
			group2.GG_IsActive = false;
			group2.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;

			AssertEquals(true, group1.IsADLinked);
			AssertEquals(false, group2.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { group1, group2 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(3, menuItem.MenuItems.Count);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertEquals("Disconnect From Active Directory", menuItem.MenuItems[1].Text);
			AssertEquals("Set Domain Name", menuItem.MenuItems[2].Text);
		}

		public void TestSynchroniseStaffAndResources()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var resource = Factory.New<GlbStaff>();
			resource.GS_Code = "$R1";
			resource.GS_IsActive = true;
			resource.GS_IsResource = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var syncDirector = new Mock<ISynchronisationDirector>();
			var syncDirectorProvider = new SynchronisationDirectorProviderForTest();
			syncDirectorProvider.SyncDirectorOverride = syncDirector.Object;

			using (ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProvider))
			{
				var module = new Mock<ZFilterGridModule>();

				module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
				module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { resource, staff });

				var menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
				((ADActionsMenuItem)menuItem).OnPopup();
				module.Object.Dispose();

				AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
				AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
				AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());
			}

			syncDirector.Verify(s => s.Synchronise(null, null), Times.Once);
			syncDirector.Verify(s => s.Save(), Times.Once);
		}

		public void TestSynchroniseStaff_CannotLogin()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var staffCannotLogin = Factory.New<GlbStaff>();
			staffCannotLogin.GS_IsActive = true;
			staffCannotLogin.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staffCannotLogin.GS_CanLogin = false;

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff, staffCannotLogin });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());

			var expectedWarning = string.Format($@"You are about to link and synchronize a Staff record without the 'Can Login' attribute to Active Directory.");
			AssertEquals(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSynchroniseStaff_CannotLogin_PrefixRequired()
		{
			ActiveDirectoryRegistry.Instance.UserLoginPrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "720s.");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var staffCannotLogin = Factory.New<GlbStaff>();
			staffCannotLogin.GS_LoginName = "mclaren";
			staffCannotLogin.GS_IsActive = true;
			staffCannotLogin.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staffCannotLogin.GS_CanLogin = false;

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff, staffCannotLogin });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());

			var expectedWarning = "Login Name 'mclaren' must begin with '720s.'";
			AssertEquals(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSynchroniseGroup()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			var group1 = Factory.New<GlbGroup>();
			group1.GG_IsActive = true;
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			AssertEquals(true, group1.IsADLinked);

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { group1 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
			AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
			AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());
		}

		public void TestSynchronise_ShouldHandleZCannotSaveException()
		{
			var message = @"You are attempting to affect the last active non operational or controller staff member.";
			Synchronise_ShouldHandleException(new ZCannotSaveException(message, ""), message);
		}

		void Synchronise_ShouldHandleException(Exception exception, string expectedMessage)
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var syncDirector = new Mock<ISynchronisationDirector>();
			var syncDirectorProvider = new SynchronisationDirectorProviderForTest();
			syncDirectorProvider.SyncDirectorOverride = syncDirector.Object;

			using (ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProvider))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
				staff.GS_IsActive = true;
				staff.GS_IsController = true;

				Factory.Save();

				var module = new Mock<ZFilterGridModule>();
				MenuItem menuItem;

				syncDirector.Setup(s => s.Save()).Throws(exception);
				module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
				module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff });
				menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
				((ADActionsMenuItem)menuItem).OnPopup();
				module.Object.Dispose();

				AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
				AssertEquals("Synchronize", menuItem.MenuItems[0].Text);
				AssertNoExceptionThrown("No Exception Should Be Thrown Out", () => menuItem.MenuItems[0].PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				syncDirector.Verify(s => s.Synchronise(null, null), Times.Once);
				syncDirector.Verify(s => s.Save());
			}
		}

		public void TestSynchronise_ShouldHandleInvalidOUException()
		{
			Synchronise_ShouldHandleException(new InvalidOUException(TestConstants.InvalidOU), string.Format("The Organizational Unit {0} is invalid, please review the setting in Registry items: System -> Staff -> Active Directory -> Domain Credentials Collection", TestConstants.InvalidOU));
		}

		public void TestSynchronise_ShouldHandleExceptionFromSyncUserToRPAGroup()
		{
			var message = "Domain user does not have write privileges to the Robotic Group xxx";
			Synchronise_ShouldHandleException(new NoDomainPrivilegeException(message), message);
		}

		public void TestDisconnectFromAD_WithoutRight()
		{
			var loginWithoutRight = Helper.CreateStaffWithSecurityRights(false, "StaffModify");
			AssertNotNull(loginWithoutRight);
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				MockAndDisconnectFromAD(false);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Staff and Resources -> Edit -> Modify All", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDisconnectFromAD_WithRight()
		{
			var loginWithRight = Helper.CreateStaffWithSecurityRights(true, "StaffModify");
			AssertNotNull(loginWithRight);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				MockAndDisconnectFromAD(false);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestDisconnectLastUserFromADShowsErrorMessage()
		{
			Helper.PurgeAllStaff();
			AssertEquals("Should be no operational or controller staff in database after purge", 0, GetOpStaffCount());

			var user = Helper.CreateStaffWithSecurityRights(true, "StaffModify");
			AssertNotNull("Helper should return non-null user", user);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			using (EnvProxy.Instance.SetTemporaryUserContext(user.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				MockAndDisconnectFromAD(true);
				AssertEquals("Should have shown error message to the user", UnitTestUserNotification.Instance.LastMessage.Text, "You are attempting to affect the last active non operational or controller staff member.");
				AssertEquals("User created for this test should remain in the database at the end of the test", 1, GetOpStaffCount());
			}
		}

		public void TestDisconnectFromAD_NoConcurrencyException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_IsActive = true;
			var group = staff.Groups.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var staffInAnotherFactory = newFactory.Load<GlbStaff>(staff.PK);
			var groupInAnotherFactory = newFactory.Load<GlbGroup>(group.PK);
			staffInAnotherFactory.Groups.Remove(groupInAnotherFactory);
			newFactory.Save();

			var module = new Mock<ZFilterGridModule>();
			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff });

			var menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals("Disconnect From Active Directory", menuItem.MenuItems[1].Text);
			AssertNoExceptionThrown(() => menuItem.MenuItems[1].PerformClick());
			AssertEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
			Assert(!staff.GS_IsActive);
			AssertCollectionNotContains(group, staff.Groups);
		}

		public void TestStaffSetDomainNameInBulk_CheckSecurityRight()
		{
			var loginWithoutRight = Helper.CreateStaffWithSecurityRights(false, "StaffSetDomainInBulk");
			var loginWithRight = Helper.CreateStaffWithSecurityRights(true, "StaffSetDomainInBulk");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
			var setDomainNameMenuItem = menuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Set Domain Name");
			AssertNotNull(setDomainNameMenuItem);

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>("User without security right should throw exception", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Staff and Resources -> Edit -> Modify All -> Set Domain Name In Bulk",
					() => setDomainNameMenuItem.PerformClick());
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown("User with security right should not throw exception", () => setDomainNameMenuItem.PerformClick());
			}
		}

		public void TestGroupSetDomainNameInBulk_CheckSecurityRight()
		{
			var loginWithoutRight = Helper.CreateStaffWithSecurityRights(false, "GroupSetDomainInBulk");
			var loginWithRight = Helper.CreateStaffWithSecurityRights(true, "GroupSetDomainInBulk");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { group });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
			var setDomainNameMenuItem = menuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Set Domain Name");
			AssertNotNull(setDomainNameMenuItem);

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>("User without security right should throw exception", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Group -> Edit -> Set Domain Name In Bulk",
					() => setDomainNameMenuItem.PerformClick());
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown("User with security right should not throw exception", () => setDomainNameMenuItem.PerformClick());
			}
		}

		public void TestStaffSetDomainNameInBulk_CanChangeDomain()
		{
			AssertStaffSetDomainNameInBulk("domain2", "domain2");
		}

		public void TestStaffSetDomainNameInBulk_CanChangeDomain2()
		{
			AssertStaffSetDomainNameInBulk("domain1", "domain1");
		}

		public void TestStaffSetDomainNameInBulk_CanClearDomain()
		{
			AssertStaffSetDomainNameInBulk("(empty)", "");
		}

		void AssertStaffSetDomainNameInBulk(string newDomain, string expectedNewDomain)
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection() {
				ADTestHelper.CreateDomainCredentials(domainName: "domain1"),
				ADTestHelper.CreateDomainCredentials(domainName: "domain2", isDefaultDomain: false)
			});

			var loginWithRight = Helper.CreateStaffWithSecurityRights(true, "StaffSetDomainInBulk");

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.DomainName = "domain1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.DomainName = "domain2";

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.DomainName = "";

			Factory.Save();

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff1, staff2, staff3 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
			var setDomainNameMenuItem = menuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Set Domain Name");
			AssertNotNull(setDomainNameMenuItem);

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddUserResponse(newDomain);
				UnitTestUserNotification.Instance.AddOKAnswer();

				setDomainNameMenuItem.PerformClick();

				AssertEquals(expectedNewDomain, staff1.DomainName);
				AssertEquals(expectedNewDomain, staff2.DomainName);
				AssertEquals(expectedNewDomain, staff3.DomainName);
			}
		}

		public void TestGroupSetDomainNameInBulk_CanChangeDomain()
		{
			AssertGroupSetDomainNameInBulk("domain2", "domain2");
		}

		public void TestGroupSetDomainNameInBulk_CanChangeDomain2()
		{
			AssertGroupSetDomainNameInBulk("domain1", "domain1");
		}

		public void TestGroupSetDomainNameInBulk_CanClearDomain()
		{
			AssertGroupSetDomainNameInBulk("(empty)", "");
		}

		void AssertGroupSetDomainNameInBulk(string newDomain, string expectedNewDomain)
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection() {
				ADTestHelper.CreateDomainCredentials(domainName: "domain1"),
				ADTestHelper.CreateDomainCredentials(domainName: "domain2", isDefaultDomain: false)
			});

			var loginWithRight = Helper.CreateStaffWithSecurityRights(true, "GroupSetDomainInBulk");

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.DomainName = "domain1";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.DomainName = "domain2";

			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.DomainName = "";

			Factory.Save();

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbGroup);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { group1, group2, group3 });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertGreaterThanOrEqualTo(menuItem.MenuItems.Count, 1);
			var setDomainNameMenuItem = menuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Set Domain Name");
			AssertNotNull(setDomainNameMenuItem);

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddUserResponse(newDomain);
				UnitTestUserNotification.Instance.AddOKAnswer();

				setDomainNameMenuItem.PerformClick();

				AssertEquals(expectedNewDomain, group1.DomainName);
				AssertEquals(expectedNewDomain, group2.DomainName);
				AssertEquals(expectedNewDomain, group3.DomainName);
			}
		}

		int GetOpStaffCount()
		{
			var nonOpStaffCount = -1;
			var query = @"select count(GS_PK) from dbo.GlbStaff where GS_IsSystemAccount = 0 and GS_IsActive = 1 and (GS_IsOperational = @IsOpr OR GS_IsController = @IsController)";
			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameterBasedOnDbColumn("@IsOpr", false, GlbStaffSchema.GS_IsOperational);
				cmd.AddParameterBasedOnDbColumn("@IsController", true, GlbStaffSchema.GS_IsController);

				nonOpStaffCount = (int)cmd.ExecuteScalar();
			}
			return nonOpStaffCount;
		}

		void MockAndDisconnectFromAD(bool staffIsController)
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_IsActive = true;
			staff.GS_IsController = staffIsController;

			Factory.Save();

			var module = new Mock<ZFilterGridModule>();

			MenuItem menuItem;

			module.Setup(m => m.ID).Returns(ModuleIDs.GlbStaff);
			module.Setup(m => m.GetSelectedBusinessObjects()).Returns(new[] { staff });

			menuItem = (MenuItem)new ADActionsMenuItemProvider().GetModuleMenuItem(module.Object, Factory);
			((ADActionsMenuItem)menuItem).OnPopup();
			module.Object.Dispose();

			AssertEquals(4, menuItem.MenuItems.Count);
			AssertEquals("Disconnect From Active Directory", menuItem.MenuItems[1].Text);
			AssertNoExceptionThrown(() => menuItem.MenuItems[1].PerformClick());
		}
	}
}
