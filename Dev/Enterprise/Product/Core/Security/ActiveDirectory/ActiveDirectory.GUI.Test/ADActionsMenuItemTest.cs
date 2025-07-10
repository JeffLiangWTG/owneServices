using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class ADActionsMenuItemTest : TestCaseWithFactoryAndMocks
	{
		public void TestCreateMenu_Group_WhenNotSyncingStaff()
		{
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;

			using (var form = new ADActionsTestForm(Factory.New<GlbGroup>()))
			{
				form.ShowActionsMenuItem();

				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Groups are not synchronized with Active Directory", form.ADMenuItem.MenuItems[0].Text);
			}
		}

		public void TestCreateMenu_WhenInactiveNotLinkedOrADIntegrationNotEnabled()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			var staff = Factory.New<GlbStaff>();
			Assert(!staff.IsADIntegrationEnabled);
			AssertNull(new ADActionsMenuItemProvider().GetFormMenuItem(staff));

			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			staff.GS_IsActive = false;
			AssertNull(new ADActionsMenuItemProvider().GetFormMenuItem(staff));

			staff.GS_IsActive = true;
			AssertNotNull(new ADActionsMenuItemProvider().GetFormMenuItem(staff));

			staff.GS_IsActive = false;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			AssertNotNull(new ADActionsMenuItemProvider().GetFormMenuItem(staff));
		}

		public void TestOpenMenuForNowInactiveStaff()
		{
			var staff = Factory.New<GlbStaff>();
			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				staff.GS_IsActive = false;
				form.ShowActionsMenuItem();

				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Only active or linked staff can be synchronized with AD.", form.ADMenuItem.MenuItems[0].Text);
			}
		}

		public void TestOpenMenuForActiveUnlinkedStaff()
		{
			var staff = Factory.New<GlbStaff>();
			using (var form = new ADActionsTestForm(staff))
			{
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Should have a placeholder menu item to ensure we have a child menu item list before it is populated on open.", "-", form.ADMenuItem.MenuItems[0].Text);

				form.ShowActionsMenuItem();
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);
			}
		}

		public void TestOpenMenuForActiveLinkedStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(3, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);
				AssertEquals("Disconnect From Active Directory", form.ADMenuItem.MenuItems[1].Text);
				AssertEquals("Unlock Account", form.ADMenuItem.MenuItems[2].Text);
			}
		}

		public void TestOpenMenuForLinkedAndActiveButCantLoginStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staff.GS_IsActive = true;
			staff.GS_CanLogin = false;

			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();

				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);
			}
		}

		public void TestOpenMenuForActiveUnlinkedGroup()
		{
			var group = Factory.New<GlbGroup>();
			using (var form = new ADActionsTestForm(group))
			{
				form.ShowActionsMenuItem();
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);
			}
		}

		public void TestOpenMenuForActiveLinkedGroup()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			using (var form = new ADActionsTestForm(group))
			{
				form.ShowActionsMenuItem();
				AssertEquals(2, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);
				AssertEquals("Disconnect From Active Directory", form.ADMenuItem.MenuItems[1].Text);
			}
		}

		public void TestOpenMenuForNowInactiveGroup()
		{
			var group = Factory.New<GlbGroup>();
			using (var form = new ADActionsTestForm(group))
			{
				form.ShowActionsMenuItem();
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				group.GG_IsActive = false;
				form.ShowActionsMenuItem();
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Only non-system, active or linked groups can be synchronized with AD.", form.ADMenuItem.MenuItems[0].Text);
			}
		}

		public void TestOpenMenuForSystemGroup()
		{
			var sysGroup = Factory.New<GlbGroup>();
			sysGroup.GG_IsSystemDefined = true;
			AssertNotNull(sysGroup);
			using (var form = new ADActionsTestForm(sysGroup))
			{
				form.ShowActionsMenuItem();
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Only non-system, active or linked groups can be synchronized with AD.", form.ADMenuItem.MenuItems[0].Text);
			}
		}

		public void TestSynchronise_ShouldRequireBusinessEntityToBeSavedFirst_NotInDatabase()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(3, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				form.ADMenuItem.MenuItems[0].PerformClick();
				AssertEquals("Please save this record first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSynchronise_ShouldRequireBusinessEntityToBeSavedFirst_HasChanges()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			using (var form = new ADActionsTestForm(staff))
			{
				staff.GS_FullName = "Martin Freeman";

				form.ShowActionsMenuItem();
				AssertEquals(3, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				form.ADMenuItem.MenuItems[0].PerformClick();
				AssertEquals("Please save this record first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSynchronise()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var adUser = new Mock<IADUser>();
			ADEntityProviderSubstitution.ADUser = adUser.Object;
			adUser.Setup(u => u.Synchronise(null)).Returns(true);

			var syncDirector = new Mock<ISynchronisationDirector>();
			var syncDirectorProvider = new SynchronisationDirectorProviderForTest();
			syncDirectorProvider.SyncDirectorOverride = syncDirector.Object;

			using (ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProvider))
			{
				using (var form = new ADActionsTestForm(staff))
				{
					form.ShowActionsMenuItem();
					AssertEquals(3, form.ADMenuItem.MenuItems.Count);
					AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

					form.ADMenuItem.MenuItems[0].PerformClick();
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				}
				adUser.Verify(u => u.Synchronise(null), Times.Once);
				adUser.Verify(u => u.CommitChanges(), Times.Once);
			}
			syncDirector.Verify(s => s.SyncUsersToRoboticGroupIfRequired(new[] { adUser.Object }), Times.Once);
			syncDirector.Verify(s => s.Save(), Times.Never);
		}

		public void TestSynchroniseStaff_CannotLogin_Linked()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_CanLogin = false;
			Factory.Save();

			var adUser = new Mock<IADUser>();
			ADEntityProviderSubstitution.ADUser = adUser.Object;
			adUser.Setup(u => u.Synchronise(null)).Returns(true);

			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(3, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				form.ADMenuItem.MenuItems[0].PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			adUser.Verify(u => u.Synchronise(null), Times.Once);
		}

		public void TestSynchroniseStaff_CannotLogin_NotLinked()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staff.GS_CanLogin = false;
			Factory.Save();

			var adUser = new Mock<IADUser>();
			ADEntityProviderSubstitution.ADUser = adUser.Object;
			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(1, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				var expectedWarning = string.Format($@"You are about to link and synchronize a Staff record without the 'Can Login' attribute to Active Directory.");

				// Press cancel won't continue sycn
				adUser.Verify(u => u.Synchronise(It.IsAny<SyncMode?>()), Times.Never);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.ADMenuItem.MenuItems[0].PerformClick();
				AssertEquals(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not be linked", false, staff.IsADLinked);

				// Press Yes will continue sync
				adUser.Reset();
				adUser.Setup(u => u.Synchronise(null)).Callback(() =>
				{
					staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();
				}).Returns(true);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddOKAnswer();

				form.ADMenuItem.MenuItems[0].PerformClick();
				AssertEquals(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should be linked", true, staff.IsADLinked);

				adUser.Verify(u => u.Synchronise(null));
			}
		}
		public void TestSynchroniseStaff_CannotLogin_NotLinked_PrefixRequired()
		{
			ActiveDirectoryRegistry.Instance.UserLoginPrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "720s.");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "mclaren";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staff.GS_CanLogin = false;
			Factory.Save();

			var adUser = new Mock<IADUser>();
			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.ADMenuItem.MenuItems[0].PerformClick();
				var expectedWarning = "Login Name 'mclaren' must begin with '720s.'";
				AssertEquals(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should be linked", false, staff.IsADLinked);
			}
			adUser.Verify(u => u.Synchronise(It.IsAny<SyncMode?>()), Times.Never);
		}

		public void TestSynchronise_ShouldHandleExceptionFromSyncUserToRPAGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var adUser = new Mock<IADUser>();
			ADEntityProviderSubstitution.ADUser = adUser.Object;
			adUser.Setup(u => u.Synchronise(null)).Returns(true);

			var syncDirector = new Mock<ISynchronisationDirector>();
			var syncDirectorProvider = new SynchronisationDirectorProviderForTest();
			syncDirectorProvider.SyncDirectorOverride = syncDirector.Object;

			var exception = new NoDomainPrivilegeException("RPA error");
			syncDirector.Setup(s => s.SyncUsersToRoboticGroupIfRequired(It.IsAny<IEnumerable<IADEntity>>())).Throws(exception);

			using (ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProvider))
			{
				using (var form = new ADActionsTestForm(staff))
				{
					form.ShowActionsMenuItem();
					AssertEquals(3, form.ADMenuItem.MenuItems.Count);
					AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

					AssertNoExceptionThrown("No exception should be thrown out", () => form.ADMenuItem.MenuItems[0].PerformClick());
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(exception.Message, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			adUser.Verify(u => u.Synchronise(null), Times.Once);
			adUser.Verify(u => u.CommitChanges(), Times.Once);
			syncDirector.Verify(s => s.SyncUsersToRoboticGroupIfRequired(new[] { adUser.Object }), Times.Once);
			syncDirector.Verify(s => s.Save(), Times.Never);
		}

		public void TestSynchronise_ShouldHandleInvalidOperationException()
		{
			Synchronise_ShouldHandleInvalidException(new InvalidOperationException("bla"), "bla");
		}

		void Synchronise_ShouldHandleInvalidException(Exception exception, string expectedMessage)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var adUser = new Mock<IADUser>();
			ADEntityProviderSubstitution.ADUser = adUser.Object;
			adUser.Setup(u => u.Synchronise(null)).Throws(exception);

			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(3, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				AssertNoExceptionThrown("No exception should be thrown out", () => form.ADMenuItem.MenuItems[0].PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSynchronise_ShouldHandleInvalidOUException()
		{
			Synchronise_ShouldHandleInvalidException(new InvalidOUException(TestConstants.InvalidOU), string.Format("The Organizational Unit {0} is invalid, please review the setting in Registry items: System -> Staff -> Active Directory -> Domain Credentials Collection", TestConstants.InvalidOU));
		}

		void MockAndSynchronise(GlbStaff staff)
		{
			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(3, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Synchronize", form.ADMenuItem.MenuItems[0].Text);

				form.ADMenuItem.MenuItems[0].PerformClick();
			}
		}

		public void TestSynchronise_WithoutRight()
		{
			var loginWithoutRight = Helper.CreateStaffWithSecurityRights(false, "StaffViewHomeAddressDetails");
			AssertNotNull(loginWithoutRight);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				MockAndSynchronise(staff);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Staff and Resources -> View -> View Other Staff Details -> View Home Address Details of Other Staff Members", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSynchronise_WithRight()
		{
			var loginWithRight = Helper.CreateStaffWithSecurityRights(true, "StaffViewHomeAddressDetails");
			AssertNotNull(loginWithRight);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				MockAndSynchronise(staff);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestSynchronise_WhenAValidationErrorOccurred()
		{
			ActiveDirectoryRegistry.Instance.GroupNamePrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AD.");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
			group.GG_Desc = "JohnLock";
			Factory.Save();

			var adGroup = new Mock<IADEntity>();
			ADEntityProviderSubstitution.ADGroup = adGroup.Object;

			using (var form = new GroupFormForTest(group))
			{
				form.Show();
				Application.DoEvents();

				var saveResult = form.ValidateAndSave();
				AssertEquals(ContinueWithSave.No, saveResult);

				group.GG_Desc = "AD.JohnLock";
				saveResult = form.ValidateAndSave();
				AssertEquals(ContinueWithSave.Yes, saveResult);

				AssertEquals(false, group.HasChanges);

				new ADActionsMenuItem(group).Synchronise();
				AssertNotEquals("Please save this record first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			adGroup.Verify(g => g.Synchronise(null), Times.Once);
		}

		public void TestDisconnectFromAD_WithoutRight()
		{
			var loginWithoutRight = Helper.CreateStaffWithSecurityRights(false, "StaffModify");
			AssertNotNull(loginWithoutRight);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithoutRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				MockAndDisconnectFromAD(staff);
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

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			using (EnvProxy.Instance.SetTemporaryUserContext(loginWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				MockAndDisconnectFromAD(staff);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		void MockAndDisconnectFromAD(GlbStaff staff)
		{
			using (var form = new ADActionsTestForm(staff))
			{
				form.ShowActionsMenuItem();
				AssertEquals(3, form.ADMenuItem.MenuItems.Count);
				AssertEquals("Disconnect From Active Directory", form.ADMenuItem.MenuItems[1].Text);

				form.ADMenuItem.MenuItems[1].PerformClick();
			}
		}

		class GroupFormForTest : GlbGroupForm
		{
			internal GroupFormForTest(GlbGroup group)
				: base(group)
			{
			}

			internal new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}

			internal void ShowErrorsDialog()
			{
				base.ShowErrorsDialog();
			}
		}

		class ADActionsTestForm : ZForm
		{
			internal ADActionsTestForm(IADLinkedEntity dataSource)
				: base(dataSource)
			{
				ADMenuItem = (ZMenuItem)new ADActionsMenuItemProvider().GetFormMenuItem(dataSource);
				ZFormMenuStrategy.AddActionsMenuItem(this, ADMenuItem);
			}

			internal ZMenuItem ADMenuItem { get; private set; }

			internal void ShowActionsMenuItem()
			{
				Show();
				ActionsMenuItem.PerformSelect();
				((ADActionsMenuItem)ActionsMenuItem.MenuItems.FindByText("Active Directory")).OnPopup();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
		}
	}
}
