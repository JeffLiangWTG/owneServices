using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BlazorWinFormsInterop;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.SqlSecurity;
using Enterprise.Startup.Login;
using Enterprise.Startup.Tools;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using Form = System.Windows.Forms.Form;

#if !WINZOR && !NET8_0_OR_GREATER
using Enterprise.ZArchitecture;
#endif

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(MainForm))]
	internal class MainFormTestCase : BasherTest
	{
		#region Font

		[RequiresSTA]
		public void TestMainFormUsesSystemFont()
		{
			using (var form = new TestMainForm())
			{
				form.DoInitializeAfterLogin();

				var tahoma = "Tahoma";
				EnvProxy.Instance.Registry.SystemFontRegItem = tahoma;
				AssertEquals(tahoma, OFont.NormalFontName);
				var times = "Times New Roman";
				EnvProxy.Instance.Registry.SystemFontRegItem = times;
				AssertEquals(times, OFont.NormalFontName);
			}
		}

		#endregion

		#region Nav Bar Tool Strip

		[RequiresSTA]
		public void TestNavBarToolStrip()
		{
			using (var form = new TestMainForm())
			{
				var toolStrip = (ZToolStrip)form.Controls.Find("NavBarToolStrip", true)[0];

				var settingsMenu = (ZToolStripMenuItem)toolStrip.Items.Find("SettingsToolStripMenuItem", true)[0];
				AssertEquals("&Options", settingsMenu.Text);
				AssertEquals(true, settingsMenu.DropDownItems.Count > 0);

				var testingMenu = (ZToolStripMenuItem)toolStrip.Items.Find("TestingToolStripMenuItem", true)[0];
				AssertEquals("Te&sting", testingMenu.Text);
				AssertEquals(true, testingMenu.DropDownItems.Count > 0);
				ZToolStripMenuItem testsIHaveNotRecentlyRun = testingMenu.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "Tests I have not recently run");
				AssertEquals(true, testsIHaveNotRecentlyRun.DropDownItems.Count == 0);
				testingMenu.ShowDropDown();
				AssertEquals(true, testingMenu.DropDownItems.Count > 0);
				AssertEquals(true, testsIHaveNotRecentlyRun.DropDownItems.Count > 0);

				var helpMenu = (ZToolStripMenuItem)toolStrip.Items.Find("HelpToolStripMenuItem", true)[0];
				AssertEquals("&Help", helpMenu.Text);
				AssertEquals(true, helpMenu.DropDownItems.Count > 0);
			}
		}

		[RequiresSTA]
		public void TestNavBarToolStrip_OnlyVisibleForJump()
		{
			using (var form = new TestMainForm())
			{
				form.Show();

				var toolStrip = (ZToolStrip)form.Controls.Find("NavBarToolStrip", true)[0];

				form.SwitchCategory(ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Operations.Name]);
				AssertEquals(false, toolStrip.Visible);

				form.SwitchCategory(ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Admin.Name]);
				AssertEquals(false, toolStrip.Visible);

				form.SwitchCategory(ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Manage.Name]);
				AssertEquals(false, toolStrip.Visible);

				form.SwitchCategory(ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name]);
				AssertEquals(true, toolStrip.Visible);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestNavBarToolStripShortcutsAreUnique()
		{
			using (var form = new TestMainForm())
			{
				var allShortcuts = new Dictionary<Keys, string>();
				var toolStrip = (ZToolStrip)form.Controls.Find("NavBarToolStrip", true)[0];
				foreach (ToolStripItem item in toolStrip.Items)
				{
					CheckShortcutKeys(item, allShortcuts);
				}
			}

			void CheckShortcutKeys(ToolStripItem item, Dictionary<Keys, string> allShortcuts)
			{
				if (item is ToolStripMenuItem menu)
				{
					if (menu.ShortcutKeys != Keys.None)
					{
						if (allShortcuts.ContainsKey(menu.ShortcutKeys))
						{
							Fail($"Duplicate shortcut \"{menu.ShortcutKeys}\" for \"{allShortcuts[menu.ShortcutKeys]}\" and \"{item.Text}\"");
						}
						else
						{
							allShortcuts.Add(menu.ShortcutKeys, menu.Text);
						}
					}

					if (menu is ToolStripDropDownItem dropDown && !menu.Text.Replace("&", "").Contains("Test Failures") && !menu.Text.Contains("not recently run"))
					{
						dropDown.ShowDropDown();
						foreach (ToolStripItem child in dropDown.DropDownItems)
						{
							CheckShortcutKeys(child, allShortcuts);
						}
					}
				}
			}
		}

		#endregion Nav Bar Tool Strip

		#region Menu Items

		[RequiresSTA]
		public void TestSecurityOverrideTokenMenuItem()
		{
			var mockIOIDCConfig = new Mock<IOIDCConfig>();

			mockIOIDCConfig.Setup(mockIOIDCConfig => mockIOIDCConfig.IsOIDCEnabled).Returns(false);

			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSecurityOverrideTokenMenuItem(false);
			}

			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSecurityOverrideTokenMenuItem(false);
			}

			mockIOIDCConfig.Setup(mockIOIDCConfig => mockIOIDCConfig.IsOIDCEnabled).Returns(true);

			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSecurityOverrideTokenMenuItem(false);
			}

			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSecurityOverrideTokenMenuItem(true);
			}
		}

		void AssertSecurityOverrideTokenMenuItem(bool shouldShow)
		{
			MainForm.UnloadForLogin();
			ZFormActivityLogger.Instance.DisableActivityLogger();
			MainForm.DoInitializeAfterLogin();

			var foundItem = MainForm.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "Security Override Token");
			AssertEquals($"Security Override Token menu item should show: {shouldShow}", shouldShow, foundItem != null);

			var loginForm = ZApplication.GetOpenForms().OfType<GlbStaffForm>().FirstOrDefault();
			loginForm?.Dispose();
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestRecreateDbSynonymMenuItem()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "user";
			staff.GS_IsController = true;
			staff.RunPreSaveValidation();
			Factory.Save();

			TemporaryUserContext userContext = new TemporaryUserContext { StaffLoginName = "user" };

			using (userContext.Set())
			using (TestMainForm mainForm = new TestMainFormWithHotkeyOverride())
			{
				var dbAdminMenu = mainForm.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "Database Administration");
				AssertNotNull("Help > Database Administration menu item", dbAdminMenu);

				var foundItem = dbAdminMenu.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "Recreate Database Synonyms");
				AssertNotNull("Recreate Database Synonyms menu item", foundItem);
				foundItem.PerformClick();
				var message = ((UnitTestUserNotification)Globals.Message).LastMessage.ToString();
				AssertContains("Question Synonyms were successfully recreated for the following databases:", message);

				var loginForm = ZApplication.GetOpenForms().OfType<GlbStaffForm>().FirstOrDefault();
				loginForm?.Dispose();
			}
		}

		[RequiresSTA]
		public void TestChangePasswordMenuItem()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new TestMainForm())
			{
				form.Show();
				AssertEquals(true, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Change Password"));
				form.Close();
			}

			// Non-AD, DisableADPasswordChange
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			using (var form = new TestMainForm())
			{
				form.Show();
				AssertEquals(true, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Change Password"));
				form.Close();
			}

			// AD, DisableADPasswordChange
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			using (var form = new TestMainForm())
			{
				form.Show();
				AssertEquals("Change password should be hidden when AD is On and DisableADPasswordChange is set", false, form.SettingsMenuButton.DropDownItems.Cast<ToolStripItem>().Any(m => m.Text == "&Change Password"));
				form.Close();
			}
		}

		[RequiresSTA]
		public void TestMenuStructure()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			Registry.Business.GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address");

			using (var form = new TestMainForm())
			{
				form.Show();

				var menuStructure = new StringBuilder(@"&Options
	Purge Data (Admin Users)
	-
	&Change Password
	&Login as New User
	Fast User Switching (Debug Only)
	Change Company/&Branch/Dept. (Manually Close Forms)
		&Close Open Forms
		&Save and Close Open Forms
		&Manually Close Forms");
#if WINZOR
				menuStructure.AppendLine();
				menuStructure.AppendLine("	-");
				menuStructure.Append("	&Settings");

#endif
				menuStructure.AppendLine(@"
	-
	E&xit
	Exit and &remember open forms");

				AssertMenuStructure(form.SettingsMenuButton, menuStructure.ToString());
				var helpMenuTextBuilder = new StringBuilder();
				helpMenuTextBuilder.AppendLine(@"&Help
	My Account
	WiseTech Academy
		My Learning
		Content && Support
		Update Notes Portal
	BorderWise
	CargoWise Web Portals
	-
	&Training Mode
	New eRequest
	Manage eRequests Online
	Unregister Product
	-
	STL Billing
		Collect Retrospective Data
		Dynamic Collector Definitions
	-
	Database Administration
		Execute Configure-Server Procedure
		SQL Server System Configurations
		Customize User Repository Objects
		Recreate Database Synonyms
		Reference Data
		Restore Reader Role Permissions
		Restore Database Logins
		Synchronize Staff Database Logins
		Output SQL security build info
		Reset Always On Cache
	Diagnostics
		Capture Business Object Stack Trace
		Send/receive test email...
		Send/receive test message via eHub...
		Send/receive test message via Direct xT...
		Test connection with xT server...
		Print test Air Waybill...
		Print test Shipment Cover Sheet...
		Print test printer scaling page...
		Compact Heap and Garbage Collect
		Run Barcode Parsing Rules
		Cartonization Algorithm Diagnostics
		Get && Verify B2C Access Token
		Run Wedge Scanner Diagnostics
		Performance
		Profile Performance (dotTrace)
			Sampling
		Profile Memory (dotMemory)
		Thread Monitor (Hang Debugging)
		Show Query Stack Trace
		Show Loaded Fetch Hints
		Show Memory Usage
		Database and S3 Storage Info
		Document Sections
		Address Validation Message Monitor
		De-duplication Result Monitor
		Denied Party Screening Monitor
		Security Checkpoint Monitor
		Trace Monitor
		Test Hardware Token Signature
		Deliberately Leak Forms
		Hot Key Monitor");
#if !WINZOR
				helpMenuTextBuilder.AppendLine(@"
	Developer Feature Override");
#endif
				helpMenuTextBuilder.AppendLine(@"
	Hot Key Help
	About");
				AssertMenuStructure(form.HelpMenuButton, helpMenuTextBuilder.ToString());
			}
		}

		void AssertMenuStructure(ZToolStripMenuItem menuItem, string expected)
		{
			var stringBuilder = new StringBuilder();
			AddItemToString(menuItem, stringBuilder);
			var sbString = stringBuilder.ToString();
			AssertMultilineASCIIEquals("Expected menu structure", expected, sbString);
		}

		void AddItemToString(ToolStripMenuItem item, StringBuilder stringBuilder, int indent = 0)
		{
			var indentString = new string('\t', indent);
			stringBuilder.AppendLine(indentString + item.Text);
			indent++;
			foreach (var childItem in item.DropDownItems)
			{
				var separator = childItem as ToolStripSeparator;
				if (separator != null)
				{
					stringBuilder.AppendLine(new string('\t', indent) + "-");
				}
				else
				{
					AddItemToString((ToolStripMenuItem)childItem, stringBuilder, indent);
				}
			}
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestSetSqlPasswordMenuItem()
		{
			using (RunNonTransactioned())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var initialStaff = Factory.New<GlbStaff>();
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				try
				{
					initialStaff.GS_LoginName = "First Controller Staff";
					initialStaff.GS_IsActive = true;
					initialStaff.GS_IsController = true;
					Factory.Save();

					using ((new TemporaryUserContext() { StaffLoginName = User.SupportUserName }).Set())
					using (var form = new TestMainFormWithHotkeyOverride())
					{
						AssertHelpSubMenuFound(form, false, "Set SQL password");

						var loginForm = ZApplication.GetOpenForms().OfType<GlbStaffForm>().FirstOrDefault();
						loginForm?.Dispose();
					}

					ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: false, isDatabaseDeveloper: false, isReadOnlyDBUser: false, isBackupOperator: false, expectedIsVisible: false);
					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: false, isReadOnlyDBUser: false, isBackupOperator: false, expectedIsVisible: false);
					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: true, isReadOnlyDBUser: false, isBackupOperator: false, expectedIsVisible: false);
					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: false, isReadOnlyDBUser: true, isBackupOperator: false, expectedIsVisible: false);
					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: false, isReadOnlyDBUser: false, isBackupOperator: true, expectedIsVisible: false);

					EnvProxy.SetHostedLocationForTest("SYD");

					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: false, isReadOnlyDBUser: false, isBackupOperator: false, expectedIsVisible: false);
					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: true, isReadOnlyDBUser: false, isBackupOperator: false, expectedIsVisible: true);
					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: false, isReadOnlyDBUser: true, isBackupOperator: false, expectedIsVisible: true);
					AssertSetPasswordMenuIsVisibleFor(staff: staff, isActive: true, isController: true, isDatabaseDeveloper: false, isReadOnlyDBUser: false, isBackupOperator: true, expectedIsVisible: true);
				}
				finally
				{
					initialStaff.Delete();
					staff.Delete();
					Factory.Save();
				}
			}
		}

		void AssertSetPasswordMenuIsVisibleFor(GlbStaff staff, bool isActive, bool isController, bool isDatabaseDeveloper, bool isReadOnlyDBUser, bool isBackupOperator, bool expectedIsVisible)
		{
			var menuToTest = "Set SQL password";
			staff.GS_LoginName = "~12345~";
			staff.GS_IsActive = isActive;
			staff.GS_IsController = isController;
			staff.IsDatabaseDeveloper = isDatabaseDeveloper;
			staff.IsReadOnlyDBUser = isReadOnlyDBUser;
			staff.IsBackupOperator = isBackupOperator;

			Factory.Save();

			using ((new TemporaryUserContext() { StaffLoginName = "~12345~" }).Set())
			using (var form = new TestMainFormWithHotkeyOverride())
			{
				AssertHelpSubMenuFound(form, expectedIsVisible, menuToTest);

				var loginForm = ZApplication.GetOpenForms().OfType<GlbStaffForm>().FirstOrDefault();
				loginForm?.Dispose();
			}
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestDatabaseAdminHelpMenuItem()
		{
			using (RunNonTransactioned())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var initialStaff = Factory.New<GlbStaff>();
				var staff = Factory.NewWithValidTestData<GlbStaff>();

				try
				{
					initialStaff.GS_LoginName = "First Controller Staff";
					initialStaff.GS_IsActive = true;
					initialStaff.GS_IsController = true;
					Factory.Save();

					using ((new TemporaryUserContext() { StaffLoginName = User.SupportUserName }).Set())
					using (var form = new TestMainFormWithHotkeyOverride())
					{
						AssertHelpSubMenuFound(form, true, "Database Administration");
					}

					staff.GS_LoginName = "~12345~";
					staff.GS_IsController = false;
					staff.GS_IsDeveloper = false;
					staff.IsDatabaseDeveloper = false;
					Factory.Save();

					using ((new TemporaryUserContext() { StaffLoginName = "~12345~" }).Set())
					using (var form = new TestMainFormWithHotkeyOverride())
					{
						AssertHelpSubMenuFound(form, false, "Database Administration");
					}

					staff.GS_IsController = true;
					Factory.Save();

					using ((new TemporaryUserContext() { StaffLoginName = "~12345~" }).Set())
					using (var form = new TestMainFormWithHotkeyOverride())
					{
						AssertHelpSubMenuFound(form, true, "Database Administration");
						var staffForm = ZApplication.GetOpenForms().OfType<GlbStaffForm>().FirstOrDefault();
						staffForm?.Dispose();
					}

					staff.GS_IsController = false;
					staff.IsDatabaseDeveloper = true;
					Factory.Save();

					using ((new TemporaryUserContext() { StaffLoginName = "~12345~" }).Set())
					using (var form = new TestMainFormWithHotkeyOverride())
					{
						AssertHelpSubMenuFound(form, true, "Database Administration");
					}

					staff.GS_IsDeveloper = true;
					Factory.Save();

					using ((new TemporaryUserContext() { StaffLoginName = "~12345~" }).Set())
					using (var form = new TestMainFormWithHotkeyOverride())
					{
						AssertHelpSubMenuFound(form, true, "Database Administration");
					}
				}
				finally
				{
					initialStaff.Delete();
					staff.Delete();
					Factory.Save();
				}
			}
		}

		void AssertHelpSubMenuFound(MainForm form, bool expected, params string[] menuTexts)
		{
			var helpMenuItems = form.HelpMenuButton.DropDownItems;

			foreach (string menuText in menuTexts)
			{
				AssertEquals("[" + menuText + "] menu item found?", expected, (helpMenuItems[menuText] != null));
			}
		}

		[RequiresSTA]
		public void TestServiceRequestMenuItem()
		{
			var helpMenuItem = MainForm.HelpMenuButton;

			bool found = false;
			foreach (var item in helpMenuItem.DropDownItems.OfType<ZToolStripMenuItem>())
			{
				if (item.Text.Equals("New eRequest") && item.ShortcutKeys == Keys.F1)
				{
					found = true;
					break;
				}
			}

			Assert("Service Request Menu Item Found", found);
		}

		[RequiresSTA]
		public void TestSecondaryThreadException()
		{
			try
			{
				StartupUserControl.ThrowExceptionInGetNews = true;
				using (var mainForm = new TestMainForm())
				{
					Fail();
				}
			}
			catch (Exception e)
			{
				AssertEquals("Forced exception in GetNews", e.Message);
			}
			finally
			{
				StartupUserControl.ThrowExceptionInGetNews = false;
			}
		}

		[RequiresSTA]
		public void TestBorderWiseMenuItem_ShouldExists()
		{
			using (var mainForm = new TestMainFormWithHotkeyOverride())
			{
				var foundItem = mainForm.HelpMenuButton.DropDownItems["BorderWise"];
				AssertNotNull(foundItem);
			}
		}

		[RequiresSTA]
		public void TestUpdateNotesPortalMenuItem_ShouldExists()
		{
			using (var mainForm = new TestMainFormWithHotkeyOverride())
			{
				var academy = (ZToolStripMenuItem)mainForm.HelpMenuButton.DropDownItems[ZHelpMenu.WiseTechAcademyName];
				var foundItem = academy.DropDownItems[ZHelpMenu.ReleaseNotesName];
				AssertNotNull(foundItem);
			}
		}

		[RequiresSTA]
		public void TestCargoWiseWebPortalsMenuItem_ShouldExistsIfCargoWiseWebPortalsIsConfigured()
		{
			Registry.Business.GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address");

			using (var mainForm = new TestMainFormWithHotkeyOverride())
			{
				var foundItem = mainForm.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "CargoWise Web Portals");

				AssertNotNull(foundItem);
			}
		}

		[RequiresSTA]
		public void TestCargoWiseWebPortalsMenuItem_ShouldNotExistsIfCargoWiseWebPortalsIsNotConfigured()
		{
			Registry.Business.GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null);

			using (var mainForm = new TestMainFormWithHotkeyOverride())
			{
				var foundItem = mainForm.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "CargoWise Web Portals");

				AssertNull(foundItem);
			}
		}

		[RequiresSTA]
		public void TestERequestWebPortalMenuItem()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "user";
			staff.GS_IsSystemAccount = true;
			staff.RunPreSaveValidation();
			Factory.Save();

			TemporaryUserContext userContext = new TemporaryUserContext() { StaffLoginName = "user" };
			using (userContext.Set())
			using (TestMainForm mainForm2 = new TestMainFormWithHotkeyOverride())
			{
				var foundItem = mainForm2.HelpMenuButton.DropDownItems["Manage eRequests Online"];
				AssertNotNull("eRequest Portal help menu option", foundItem);

				foundItem.PerformClick();
				AssertAccessDeniedMessage(ZHelpMenu.eRequestPortalName, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				staff.GS_IsSystemAccount = false;
				staff.Factory.Save();

				foundItem.PerformClick();
				AssertAccessDeniedMessage(ZHelpMenu.eRequestPortalName, false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[RequiresSTA]
		public void TestUserPortalAccess()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "sysuser";
			staff1.GS_IsSystemAccount = true;
			staff1.RunPreSaveValidation();
			Factory.Save();

			TemporaryUserContext t = new TemporaryUserContext() { StaffLoginName = "sysuser" };
			using (t.Set())
			using (MainForm form = new TestMainFormWithHotkeyOverride())
			{
				form.Show();
				form.HelpMenuButton.DropDownItems["My Account"].PerformClick();
				AssertAccessDeniedMessage(ZHelpMenu.CargoWiseWebName, true);
				AssertInformationMessage(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "nonsysuser";
			staff2.GS_IsSystemAccount = false;
			Factory.Save();

			t = new TemporaryUserContext() { StaffLoginName = "nonsysuser" };
			using (t.Set())
			using (MainForm form = new TestMainFormWithHotkeyOverride())
			{
				WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				form.Show();
				var onlineItem = form.HelpMenuButton.DropDownItems[ZHelpMenu.CargoWiseWebName];
				onlineItem.PerformClick();
				AssertAccessDeniedMessage(ZHelpMenu.CargoWiseWebName, false);
				AssertInformationMessage(true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				staff2.GS_EmailAddress = "nonsysuser@online.me";
				staff2.Factory.Save();
				onlineItem.PerformClick();
				AssertAccessDeniedMessage(ZHelpMenu.CargoWiseWebName, false);
				AssertInformationMessage(false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[RequiresSTA]
		public void TestHelpDiagnosticsProfilePerformanceMenuItem()
		{
			GlbStaff testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = "user";
			Factory.Save();

			using ((new TemporaryUserContext() { StaffLoginName = "user" }).Set())
			using (var mainForm1 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm1.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingPerformanceMenuItem = diagnosticsMenuItem.DropDownItems["Profile Performance (dotTrace)"];
				AssertNotNull("Profile Performance menu item should exist if not in a hosted environment", profilingPerformanceMenuItem);
			}

			using ((new TemporaryUserContext() { StaffLoginName = User.SupportUserName }).Set())
			using (var mainForm2 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm2.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingPerformanceMenuItem = diagnosticsMenuItem.DropDownItems["Profile Performance (dotTrace)"];
				AssertNotNull("Profile Performance menu item should exist for support users", profilingPerformanceMenuItem);
			}

			EnvProxy.SetHostedLocationForTest("IDK");
			SystemDataRegistry.Instance.AllowNonSupportDiagnosticsProfiling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using ((new TemporaryUserContext() { StaffLoginName = "user" }).Set())
			using (var mainForm1 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm1.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingPerformanceMenuItem = diagnosticsMenuItem.DropDownItems["Profile Performance (dotTrace)"];
				AssertNotNull("Profile Performance menu item should exist for non-support users in a hosted environment if AllowDiagnosticsProfiling is enabled", profilingPerformanceMenuItem);
			}

			EnvProxy.SetHostedLocationForTest("IDK");
			SystemDataRegistry.Instance.AllowNonSupportDiagnosticsProfiling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using ((new TemporaryUserContext() { StaffLoginName = "user" }).Set())
			using (var mainForm1 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm1.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingPerformanceMenuItem = diagnosticsMenuItem.DropDownItems["Profile Performance (dotTrace)"];
				AssertNull("Profile Performance menu item should not exist for non-support users in a hosted environment if AllowDiagnosticsProfiling is disabled", profilingPerformanceMenuItem);
			}
		}

		[RequiresSTA]
		public void TestHelpDiagnosticsProfileMemoryMenuItem()
		{
			GlbStaff testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = "user";
			Factory.Save();

			using ((new TemporaryUserContext() { StaffLoginName = "user" }).Set())
			using (var mainForm1 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm1.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingMemoryMenuItem = diagnosticsMenuItem.DropDownItems["Profile Memory (dotMemory)"];
				AssertNotNull("Profile Memory menu item should exist if not in a hosted environment", profilingMemoryMenuItem);
			}

			using ((new TemporaryUserContext() { StaffLoginName = User.SupportUserName }).Set())
			using (var mainForm2 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm2.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingMemoryMenuItem = diagnosticsMenuItem.DropDownItems["Profile Memory (dotMemory)"];
				AssertNotNull("Profile Memory menu item should exist for support users", profilingMemoryMenuItem);
			}

			EnvProxy.SetHostedLocationForTest("IDK");
			SystemDataRegistry.Instance.AllowNonSupportDiagnosticsProfiling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using ((new TemporaryUserContext() { StaffLoginName = "user" }).Set())
			using (var mainForm1 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm1.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingMemoryMenuItem = diagnosticsMenuItem.DropDownItems["Profile Memory (dotMemory)"];
				AssertNotNull("Profile Memory menu item should exist for non-support users in a hosted environment if AllowDiagnosticsProfiling is enabled", profilingMemoryMenuItem);
			}

			EnvProxy.SetHostedLocationForTest("IDK");
			SystemDataRegistry.Instance.AllowNonSupportDiagnosticsProfiling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using ((new TemporaryUserContext() { StaffLoginName = "user" }).Set())
			using (var mainForm1 = new TestMainFormWithHotkeyOverride())
			{
				var diagnosticsMenuItem = (ZToolStripMenuItem)mainForm1.HelpMenuButton.DropDownItems["Diagnostics"];
				var profilingMemoryMenuItem = diagnosticsMenuItem.DropDownItems["Profile Memory (dotMemory)"];
				AssertNull("Profile Memory menu item should not exist for non-support users in a hosted environment if AllowDiagnosticsProfiling is disabled", profilingMemoryMenuItem);
			}
		}

		void AssertAccessDeniedMessage(string menuItem, bool isShown)
		{
			AssertEquals("Access Denied message should have been shown", isShown,
				UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(string.Format("System users do not have access to {0}.", menuItem)));
		}

		void AssertInformationMessage(bool isShown)
		{
			AssertEquals("Information Message should have been shown", isShown,
				UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(string.Format("Email address is required to log you into {0}. Please update your staff profile details and try again.", ZHelpMenu.CargoWiseWebName)));
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestMainFormLoadsWithHelpMenuEvenIfRefDbDoesNotExist()
		{
			var sqlText = FormattableString.Invariant($@"
					SELECT 'DROP SYNONYM [' + sch.name + '].[' + syn.name + '];CREATE SYNONYM [' + sch.name + '].[' + syn.name + '] FOR [*Not-A-Db*].[*Not-A-Schema*].[*Not-An-Object*];'
					FROM sys.synonyms syn
					INNER JOIN sys.schemas sch ON syn.schema_id = sch.schema_id
					WHERE syn.name like '{RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix}%'");

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				foreach (var invalidateSynonymSql in DataUtils.GetListOfValuesFromQuery(connection, sqlText))
				{
					connection.ExecuteNonQuery(invalidateSynonymSql);
				}
			}

			using (var form = new MainForm())
			{
				AssertNoExceptionThrown(() => form.DoInitializeAfterLogin());
				var dbAdminMenu = form.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "Database Administration");
				AssertNotNull("Help > Database Administration menu item", dbAdminMenu);
				AssertNotNull("Help > Database Administration > Recreate Database Synonyms menu item", dbAdminMenu.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "Recreate Database Synonyms"));
			}
		}

		[RequiresSTA]
		public void TestRegisterHotKeysAfterLogin()
		{
			using (var mainForm = new TestMainFormWithHotkeyOverride())
			{
				Assert("Should register hot keys after login", mainForm.Hotkeys.Descriptions.Any());
				Assert("Should register global hot keys after login", mainForm.GlobalHotkeysForTest.Descriptions.Any());

				mainForm.InitialiseHotKeyMessageFilter(); // Should not trigger Error ReportOnce.
			}
		}

		#endregion Menu Items

		#region UpdatingToolbarDeleteButton

		[RequiresSTA]
		public void TestToolBarDeleteButtonEmbeddedModule()
		{
			using (MainForm mainForm = new TestMainForm())
			using (DummyFilterGridModuleWithCancellableBizO module = new DummyFilterGridModuleWithCancellableBizO(mainForm))
			{
				mainForm.Controls.Add(module.EmbeddedControl);
				mainForm.Text = "Delete button test";
				mainForm.Show();

				Assert("Precondition: Grid has BizO that implements ICancellable", typeof(ICancellable).IsAssignableFrom(module.GridCollection.TypeOfElements));

				var cancellableBizO = Factory.New<Core.GUI.Testing.ZFormTest.DummyCancellableWhichCanBeCancelled>();
				var cancellableBizO2 = Factory.New<Core.GUI.Testing.ZFormTest.DummyCancellableWhichCanBeCancelled>();

				Factory.Save();

				mainForm.OpenZEmbeddedModule(module);

				ToolStripItem deleteItem = mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Delete);
				AssertNotNull("Main toolbar strip item with name 'Delete' is present", deleteItem);
				AssertNull("Main toolbar strip does NOT contain 'Deactivate' item", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Deactivate));
				AssertNull("Main toolbar strip does NOT contain 'Activate' item", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Activate));

				ZDisplayGrid grid = (ZDisplayGrid)module.DisplayGrid;
				IDummyFilterControl filterControl = (IDummyFilterControl)module.EmbeddedControl;
				filterControl.FindButton.PerformButtonClick();

				grid.Select(0);
				grid.CurrentRowIndex = 0;
				grid.Select(1);
				grid.CurrentRowIndex = 1;

				AssertEquals("deleteItem has caption 'Deactivate' since BizO is cancellable", deleteItem.Text, ZFilterGridModule.DeleteButtonCaptions.Deactivate);
				AssertNotNull("Main toolbar strip contains 'Deactivate' item", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Deactivate));
				AssertNull("Main toolbar strip does NOT contain item named 'Activate'", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Activate));
				AssertNull("Main toolbar strip does NOT contain item named 'Delete'", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Delete));

				(grid.List[1] as ICancellable).IsCancelled = true;

				grid.Select(0);
				grid.CurrentRowIndex = 0;
				grid.Select(1);
				grid.CurrentRowIndex = 1;

				AssertEquals("deleteItem has caption 'Activate' since BizO is cancellable and cancelled", deleteItem.Text, ZFilterGridModule.DeleteButtonCaptions.Activate);
				AssertNull("Main toolbar strip does NOT contain item named 'Deactivate'", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Deactivate));
				AssertNotNull("Main toolbar strip contains item named 'Activate'", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Activate));
				AssertNull("Main toolbar strip does NOT contain item named 'Delete'", mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Delete));
			}
		}

		[RequiresSTA]
		public void TestToolBarDeleteButtonEmbeddedModule_DeleteDeactivateActivate()
		{
			using (MainForm mainForm = new TestMainForm())
			using (DummyFilterGridModuleWithCancellableBizO module = new DummyFilterGridModuleWithCancellableBizO(mainForm))
			{
				mainForm.Controls.Add(module.EmbeddedControl);
				mainForm.Text = "Delete button test";
				mainForm.Show();

				var cancellableBizO = Factory.New<Core.GUI.Testing.ZFormTest.DummyCancellableWhichCanBeCancelled>();
				var cancellableBizO2 = Factory.New<Core.GUI.Testing.ZFormTest.DummyCancellableWhichCanBeCancelled>();

				Factory.Save();

				mainForm.OpenZEmbeddedModule(module);

				ToolStripItem deleteItem = mainForm.FindMainFormToolbarStripItemByText(ZFilterGridModule.DeleteButtonCaptions.Delete);
				ZDisplayGrid grid = (ZDisplayGrid)module.DisplayGrid;
				IDummyFilterControl filterControl = (IDummyFilterControl)module.EmbeddedControl;
				filterControl.FindButton.PerformButtonClick();

				grid.Select(0);
				grid.CurrentRowIndex = 0;
				grid.Select(1);
				grid.CurrentRowIndex = 1;

				AssertDeleteButton(
					toolStripItem: deleteItem,
					module: module,
					expectedToolTipText: "Deactivates the selected item after viewing its details read-only (shortcut Del)",
					expectedImageIndex: Icons.GetImageIndex(IconTypes.ClearButtonActive),
					expectedImage: Icons.GetImage(IconTypes.ClearButtonActive),
					expectedIconTypes: IconTypes.ClearButtonActive);

				(grid.List[1] as ICancellable).IsCancelled = true;

				grid.Select(0);
				grid.CurrentRowIndex = 0;
				grid.Select(1);
				grid.CurrentRowIndex = 1;

				AssertDeleteButton(
					toolStripItem: deleteItem,
					module: module,
					expectedToolTipText: "Activates the selected item",
					expectedImageIndex: Icons.GetImageIndex(IconTypes.BlackWhite_Tick),
					expectedImage: Icons.GetImage(IconTypes.BlackWhite_Tick),
					expectedIconTypes: IconTypes.BlackWhite_Tick);
			}
		}

		[RequiresSTA]
		public void TestToolBarDeleteButtonPopupModule()
		{
			using (DummyFilterGridModuleWithCancellableBizO module = new DummyFilterGridModuleWithCancellableBizO())
			using (EmbeddedModulePopup embeddedModulePopup = new EmbeddedModulePopup(module))
			{
				embeddedModulePopup.Controls.Add(module.EmbeddedControl);
				embeddedModulePopup.Text = "Delete button test";
				embeddedModulePopup.Show();

				Assert("Precondition: Grid has BizO that implements ICancellable", typeof(ICancellable).IsAssignableFrom(module.GridCollection.TypeOfElements));

				var cancellableBizO = Factory.New<Core.GUI.Testing.ZFormTest.DummyCancellableWhichCanBeCancelled>();
				var cancellableBizO2 = Factory.New<Core.GUI.Testing.ZFormTest.DummyCancellableWhichCanBeCancelled>();

				Factory.Save();

				embeddedModulePopup.Show();

				ToolStripItem button = embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Deactivate);
				AssertNull("Main toolbar strip item with name 'Delete' is present", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Delete));
				AssertNotNull("Main toolbar strip does NOT contain 'Deactivate' item", button);
				AssertNull("Main toolbar strip does NOT contain 'Activate' item", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Activate));

				ZDisplayGrid grid = (ZDisplayGrid)module.DisplayGrid;
				IDummyFilterControl filterControl = (IDummyFilterControl)module.EmbeddedControl;
				filterControl.FindButton.PerformButtonClick();

				grid.Select(0);
				grid.CurrentRowIndex = 0;
				grid.Select(1);
				grid.CurrentRowIndex = 1;

				AssertEquals("deleteItem has caption 'Deactivate' since BizO is cancellable", button.Text, ZFilterGridModule.DeleteButtonCaptions.Deactivate);
				AssertNotNull("Main toolbar strip contains 'Deactivate' item", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Deactivate));
				AssertNull("Main toolbar strip does NOT contain item named 'Activate'", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Activate));
				AssertNull("Main toolbar strip does NOT contain item named 'Delete'", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Delete));

				(grid.List[1] as ICancellable).IsCancelled = true;

				grid.Select(0);
				grid.CurrentRowIndex = 0;
				grid.Select(1);
				grid.CurrentRowIndex = 1;

				AssertEquals("deleteItem has caption 'Activate' since BizO is cancellable and cancelled", button.Text, ZFilterGridModule.DeleteButtonCaptions.Activate);
				AssertNull("Main toolbar strip does NOT contain item named 'Deactivate'", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Deactivate));
				AssertNotNull("Main toolbar strip contains item named 'Activate'", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Activate));
				AssertNull("Main toolbar strip does NOT contain item named 'Delete'", embeddedModulePopup.FindToolBarButtonByText(ZFilterGridModule.DeleteButtonCaptions.Delete));
			}
		}

		#endregion UpdatingToolbarDeleteButton

		#region Size and Position

		[RequiresSTA]
		public void TestMinimumSize()
		{
			using (var form = new TestMainForm())
			{
				form.Width = 10;
				form.Height = 10;
				form.Show();

				AssertEquals("Min Width is 1366 - do not change without discussing with Zubin, Brett or Richard W", ControlDpiScalingHelper.ScaleToCurrentDpiX(1366), form.Width);
				AssertEquals("Min Height is 730 - do not change without discussing with Zubin, Brett or Richard W", ControlDpiScalingHelper.ScaleToCurrentDpiY(730), form.Height);
			}
		}

		[RequiresSTA]
		public void TestNavigationControlWidth()
		{
			using (var form = new TestMainForm())
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull(form.NavigationBar);
				AssertEquals("Navigation Bar Width", ControlDpiScalingHelper.ScaleToCurrentDpiY(289), form.NavigationBar.Width);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestMinimiseWorks()
		{
			MainForm.Show();
			MainForm.WindowState = FormWindowState.Minimized;
		}

		[RequiresSTA]
		public void TestFixLocation()
		{
			MainForm.Show();
			MainForm.Location = new Point(-999, -999);
			MainForm.FixLocation(null, null);
			var mainBounds = CachedScreenInfo.Instance.BoundsInfos[0];
			AssertEquals(new Point(mainBounds.Left + mainBounds.Width / 2 - MainForm.Width / 2, mainBounds.Top + mainBounds.Height / 2 - MainForm.Height / 2), MainForm.Location);

			MainForm.Location = new Point(30, 30);
			MainForm.FixLocation(null, null);
			AssertEquals(new Point(30, 30), MainForm.Location);

			MainForm.Location = new Point(50000, 50000);
			MainForm.FixLocation(null, null);
			AssertEquals(new Point(mainBounds.Left + mainBounds.Width / 2 - MainForm.Width / 2, mainBounds.Top + mainBounds.Height / 2 - MainForm.Height / 2), MainForm.Location);
		}

		#endregion Size and Position

		#region Open Module

		[RequiresSTA]
		public void TestPreviousModuleDisposedWhenStartupScreenShownOrNewModuleShown()
		{
			using (var form = new TestMainForm())
			{
				form.Show();
				form.OpenZModule(new MainFormModule(DummyModuleIDs.Dummy));
				var previousModule = form.previousEmbeddedModule;
				form.ShowStartUpScreen();
				AssertEquals(true, ((ZEmbeddedModule)previousModule).IsDisposed);

				form.OpenZModule(new MainFormModule(DummyModuleIDs.Dummy));
				previousModule = form.previousEmbeddedModule;
				form.OpenZModule(new MainFormModule(DummyModuleIDs.Dummy));
				AssertEquals(true, ((ZEmbeddedModule)previousModule).IsDisposed);
			}
		}

		[RequiresSTA]
		public void TestStartupUserControlCached()
		{
			using (var form = new TestMainForm())
			{
				form.ByPassSecurityLicenceCheck = true;

				form.Show();
				form.ShowStartUpScreen();
				var originalStartupControl = form.startupControl;
				AssertEquals(true, originalStartupControl.Visible);

				var module = new MainFormModule(DummyModuleIDs.Dummy);
				form.OpenModule(module, false);
				AssertEquals(false, originalStartupControl.Visible);

				form.ShowStartUpScreen();
				AssertEquals(originalStartupControl, form.startupControl);
				AssertEquals(true, originalStartupControl.Visible);
				AssertEquals(false, originalStartupControl.IsDisposed);

				form.UnloadForLogin();
				form.ShowStartUpScreen();
				AssertNotEquals(originalStartupControl, form.startupControl);
			}
		}

		[RequiresSTA]
		public void TestStartupUserControl_AllowAutoLogin()
		{
			using (var form = new TestMainForm())
			{
				form.ByPassSecurityLicenceCheck = true;

				form.Show();
				form.ShowStartUpScreen();
				AssertEquals(true, form.startupControl.AllowAutoLogin);

				form.UnloadForLogin();
				AssertEquals(false, form.startupControl.AllowAutoLogin);
			}
		}

		[RequiresSTA]
		public void TestStartupPositionIsCorrect()
		{
			using (var form = new TestMainForm())
			{
				AssertEquals(FormStartPosition.CenterScreen, form.StartPosition);
			}
		}

		[RequiresSTA]
		public void TestOpenModuleWhenHasUnReadItemsMandatoryToReadOrNot()
		{
			NonDeveloperStaff.Factory.Save();
			using (CurrentUserChanger.SwitchToNewUserTemporarily("nondeveloper"))
			{
				try
				{
					MainForm.Show();
					MainForm.ByPassSecurityLicenceCheck = true;
					MainForm.ByPassHasUnReadItemsCheck = false;

					var layoutConfig = new NewsSectionCollection();
					var topLeft = layoutConfig.AddNew();
					topLeft.SectionID = NewsSectionTypeList.Codes.ClientAnnouncements;
					topLeft.LayoutPanelID = "Top-Left";
					topLeft.MandatoryToRead = true;
					SystemDataRegistry.Instance.NewsSectionLayouts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, layoutConfig);

					var bottomLeft = layoutConfig.AddNew();
					bottomLeft.SectionID = NewsSectionTypeList.Codes.WiseLearningUpdates;
					bottomLeft.LayoutPanelID = "Bottom-Left";
					bottomLeft.MandatoryToRead = false;

					var startupControl = MainForm.startupControl;
					startupControl.UnReadItems.Clear();
					startupControl.UnReadItems.Add("key1", new string[] { topLeft.SectionID, topLeft.SectionName });
					startupControl.UnReadItems.Add("key2", new string[] { bottomLeft.SectionID, bottomLeft.SectionName });

					Env.Security.IgnoreMandatoryToRead.IsAllowed = true;
					MainFormModule mainFormModule = new MainFormModule(DummyModuleIDs.Dummy);
					MainForm.OpenModule(mainFormModule, true);
					AssertNotNull(ZCurrentModules.Instance.GetCurrentModule(DummyModuleIDs.Dummy));
					ZCurrentModules.Instance.GetCurrentModule(DummyModuleIDs.Dummy).Dispose();

					Env.Security.IgnoreMandatoryToRead.IsAllowed = false;
					MainForm.OpenModule(mainFormModule, true);
					AssertNull(ZCurrentModules.Instance.GetCurrentModule(DummyModuleIDs.Dummy));
					AssertContains("warning message", "There are unread message(s) mandatory to read in following sections:", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("section name", topLeft.SectionName, UnitTestUserNotification.Instance.LastMessage.Text);

					startupControl.UnReadItems.Remove("key1");
					MainForm.OpenModule(mainFormModule, true);
					AssertNotNull(ZCurrentModules.Instance.GetCurrentModule(DummyModuleIDs.Dummy));
					ZCurrentModules.Instance.GetCurrentModule(DummyModuleIDs.Dummy).Dispose();
				}
				finally
				{
					OpenedFormCache.GetInstance().CloseAllCachedForms();
					MainForm.ByPassSecurityLicenceCheck = false;
				}
			}
		}

		[RequiresSTA]
		public void TestCheckLicenceAndSecurityPermissionsWhileOpeningModules()
		{
			NonDeveloperStaff.Factory.Save();
			using (CurrentUserChanger.SwitchToNewUserTemporarily("nondeveloper"))
			{
				try
				{
					MainForm.Show();
					var licence = Env.Licence;
					var core = licence.Core;
					var forwarder = licence.Forwarder;
					var docManager = licence.DocManager;
					core.ForceLogout();
					forwarder.ForceLogout();
					docManager.ForceLogout();

					AssertEquals("IsLoggedIn to Core", false, core.IsLoggedIn);

					MainFormModule receivablesMainFormModule = new MainFormModule(ModuleIDs.AREnquiry);
					MainFormModule payablesMainFormModule = new MainFormModule(ModuleIDs.APEnquiry);
					MainFormModule containersMainFormModule = new MainFormModule(ModuleIDs.Containers);
					MainFormModule shipmentMainFormModule = new MainFormModule(ModuleIDs.JobShipment);
					MainFormModule consolMainFormModule = new MainFormModule(ModuleIDs.JobConsol);
					MainFormModule docManagerMainFormModule = new MainFormModule(ModuleIDs.DocumentAllocation);
					MainFormModule matchTransactionMainFormModule = new MainFormModule(ModuleIDs.ZARMatching);
					Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)receivablesMainFormModule.SecurityCheckpoint).IsAllowed = true;
					Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)payablesMainFormModule.SecurityCheckpoint).IsAllowed = true;
					Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)containersMainFormModule.SecurityCheckpoint).IsAllowed = true;
					Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)shipmentMainFormModule.SecurityCheckpoint).IsAllowed = true;
					Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)consolMainFormModule.SecurityCheckpoint).IsAllowed = true;
					Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)docManagerMainFormModule.SecurityCheckpoint).IsAllowed = true;
					Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)matchTransactionMainFormModule.SecurityCheckpoint).IsAllowed = true;
					docManagerMainFormModule.SecurityCheckpoint.IsAllowed = true;
					Env.Security.MaintainShipmentNew.IsAllowed = true;

					MainForm.OpenModule(receivablesMainFormModule, true);
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);

					//Login to an embedded ZModule
					MainForm.OpenModule(payablesMainFormModule, true);
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);

					//Login to an embedded ZModule
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);
					MainForm.OpenModule(shipmentMainFormModule, true);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);

					//Login to another embedded ZModule
					MainForm.OpenModule(consolMainFormModule, true);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);

					//Login to another embedded ZModule
					MainForm.OpenModule(containersMainFormModule, true);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);

					//Login to a ZPopup Module
					AssertEquals("IsLoggedIn DocManager", false, docManager.IsLoggedIn);
					MainForm.OpenModule(docManagerMainFormModule, true);
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn DocManager", true, docManager.IsLoggedIn);

					OpenedFormCache.GetInstance().CloseAllCachedForms();
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn DocManager", false, docManager.IsLoggedIn);

					//Login to an ZModule
					MainForm.OpenModule(matchTransactionMainFormModule, true);
					AssertEquals("IsLoggedIn Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);

					OpenedFormCache.GetInstance().CloseAllCachedForms();
					AssertEquals("IsLoggedIn Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);

					//Login to an embedded OModule that doesn't have a licence
					core.AllowUsageForTest = false;
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					MainForm.OpenModule(receivablesMainFormModule, true);
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);

					AssertEquals("Licence error form shown", typeof(LicenceErrorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					//Login to an embedded ZModule
					core.AllowUsageForTest = true;
					MainForm.OpenModule(receivablesMainFormModule, true);
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);

					//Login to an embedded ZModule that doesn't have a licence
					forwarder.AllowUsageForTest = false;
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					MainForm.OpenModule(shipmentMainFormModule, true);
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);
					AssertEquals("Licence error form shown", typeof(LicenceErrorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					//Login to an embedded ZModule that doesn't have a licence
					forwarder.AllowUsageForTest = false;
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					MainForm.OpenModule(containersMainFormModule, true);
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);
					AssertEquals("Licence error form shown", typeof(LicenceErrorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					//Login to a Z popup module that doesn't have a licence
					docManager.AllowUsageForTest = false;
					AssertEquals("IsLoggedIn DocManager", false, docManager.IsLoggedIn);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					MainForm.OpenModule(docManagerMainFormModule, true);
					AssertEquals("IsLoggedIn Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn DocManager", false, docManager.IsLoggedIn);
					AssertEquals("Licence error form shown", typeof(LicenceErrorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					//Login to an embedded ZModule
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);
					forwarder.AllowUsageForTest = true;
					MainForm.OpenModule(shipmentMainFormModule, true);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);

					//Login to a O popup module that doesn't have a licence
					core.AllowUsageForTest = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					MainForm.OpenModule(matchTransactionMainFormModule, true);
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("Licence error form shown", typeof(LicenceErrorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					//Open a shipment form and log into an O module
					using (ZFilterGridModule module = (ZFilterGridModule)shipmentMainFormModule.CreateZModule())
					{
						module.FormActionMenu.FindByText("&New").PerformClick();
					}

					AssertEquals("IsLoggedIn to Core", false, core.IsLoggedIn);
					core.AllowUsageForTest = true;
					MainForm.OpenModule(receivablesMainFormModule, true);
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);

					OpenedFormCache.GetInstance().CloseAllCachedForms();
					AssertEquals("IsLoggedIn to Core", true, core.IsLoggedIn);
					AssertEquals("IsLoggedIn Forwarder", false, forwarder.IsLoggedIn);

					//Open a receivables form and log into the shipment module
					forwarder.AllowUsageForTest = true;
					MainForm.OpenModule(shipmentMainFormModule, true);
					AssertEquals("IsLoggedIn Forwarder", true, forwarder.IsLoggedIn);
					AssertEquals("IsLoggedIn Core", false, core.IsLoggedIn);
				}
				finally
				{
					OpenedFormCache.GetInstance().CloseAllCachedForms();
				}
			}
		}

		[RequiresSTA]
		public void TestHeadingShowsDetails()
		{
			MainFormModule testForm = new MainFormModule(ModuleIDs.BookingsReports);
			MainForm.OpenModule(testForm, true);
			Application.DoEvents();

			var headingLabel = MainForm.Controls.Find("ModuleHeadingLabel", true)[0];

			AssertEquals("Heading for this module should be Reports (Booking)", "Reports (Booking)", headingLabel.Text);
		}

		[RequiresSTA]
		public void TestHeadingShowsDetails_WhenModuleDescriptionHasSpecialSymbols()
		{
			var testForm = new MainFormModule(ModuleIDs.SalesMgrReports);
			MainForm.OpenModule(testForm, true);
			Application.DoEvents();

			var headingLabel = MainForm.Controls.Find("ModuleHeadingLabel", true)[0];

			AssertEquals("Heading for this module should be Reports (Sales & Marketing)", "Reports (Sales & Marketing)", headingLabel.Text);
		}

		[RequiresSTA]
		public void TestOpenModuleUpdatesZCurrentModules()
		{
			using (ZModule moduleToResetCurrentModule = ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				ZCurrentModules.Instance.SetCurrentModule(moduleToResetCurrentModule);
			}
			AssertEquals("Precondition: There should be no 'current' module for the test", null, ZCurrentModules.Instance.GetCurrentModule(ModuleIDs.Organisation));

			MainFormModule testModule = new MainFormModule(ModuleIDs.Organisation);
			MainForm.OpenModule(testModule, true);
			using (ZModule currentModule = ZCurrentModules.Instance.GetCurrentModule(ModuleIDs.Organisation))
			{
				AssertEquals("ZCurrentModules should be updated with the most recent open of the module", testModule.ID, currentModule.ID.ToString());
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestOpenModuleDoesNothingAfterDisposing()
		{
			TestMainForm form = new TestMainForm();
			form.Show();
			Application.DoEvents();
			form.Dispose();
			form.OpenModule(new MainFormModule(ModuleIDs.Organisation), true);
		}

		[RequiresSTA]
		public void TestCurrentModuleLicenceCheckPointName()
		{
			AssertEquals(string.Empty, MainForm.CurrentModuleLicenceCheckPointName);

			MainFormModule testModule = new MainFormModule(ModuleIDs.Organisation);
			MainForm.OpenModule(testModule, true);

			AssertEquals("COR", MainForm.CurrentModuleLicenceCheckPointName);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[RequiresSTA]
		public void TestUserRepositoryObjectsAllowMultiAccess()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "DBdeveloper";
				staff.IsDatabaseDeveloper = true;
				new DbUserManager().SetPasswordForStaff(staff, "P@$$w0rd!");
				Factory.Save();

				var sqlLoginName = $"EnterpriseDbUser_{Db.DatabaseName}_DBdeveloper";
				try
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName);
						sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);
					}

					using (var tempConnection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName + DbUserRepository.RepositoryDbSuffix, sqlLoginName, "P@$$w0rd!"))
					{
						AssertNoExceptionThrown("PRE-CONDITON: Can connect to user repository.", () => tempConnection.EnsureIsOpen());
					}

					using (var mainForm = new TestMainFormWithUserRepositoryFormList())
					using (new TemporaryUserContext() { StaffLoginName = "DBdeveloper" }.Set())
					{
						Assert("DBdeveloper has been configured for database developer access", GlbStaff.CurrentUser.IsDatabaseDeveloper);

						var item = mainForm.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Database Administration").DropDownItems.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Customize User Repository Objects");
						AssertNoExceptionThrown("", () => item.PerformClick());
						AssertNoExceptionThrown("", () => item.PerformClick());

						AssertEquals(string.Format("Login failed for user '{0}'.", sqlLoginName), UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					staff.Delete();
					Factory.Save();

					using (var adminConnection = Db.NewAdminConnection())
					{
						var sqlSecurityManager = new SqlSecurityManager(Mock.Of<ILogger>(), Db.DatabaseName);
						sqlSecurityManager.BuildSecurity(adminConnection, CancellationToken.None);
					}
				}
			}
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestUserRepositoryObjectsAccessIsDenied_CurrentUser_NotConfiguredAsDatabaseDeveloper()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "User1";
			staff.IsDatabaseDeveloper = false;
			Factory.Save();

			using (var mainForm = new TestMainFormWithUserRepositoryFormList())
			using (new TemporaryUserContext() { StaffLoginName = "User1" }.Set())
			{
				Assert("User1 is not configured for database developer access", !GlbStaff.CurrentUser.IsDatabaseDeveloper);

				var item = mainForm.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Database Administration").DropDownItems.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Customize User Repository Objects");
				AssertNoExceptionThrown("Customize User Repository Objects", () => item.PerformClick());
				AssertEquals("Your staff record is not configured for database developer access. Contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestOpenZEmbeddedModuleWithoutNavigationBar()
		{
			using (var form = new TestMainForm())
			{
				form.Show();
				form.NavigationBar = null;
				form.OpenZModule(new MainFormModule(DummyModuleIDs.Dummy));
			}
		}

		#endregion Open Module

		#region Form Events

		[RequiresSTA]
		public void TestOnClosing_PendingActivityLogsSaved()
		{
			TestCaseHelper.ClearTable(StmActivityLogSchema.Constants.TableName);
			MainForm.Show();
			Application.DoEvents();

			FormUserStatistics stats = new FormUserStatistics();
			stats.NotifyFormShownUtc("Hello", "Test", ZDateTime.UtcNow.ToDateTime());
			stats.NotifyFormClosed(Guid.Empty, "");

			FormUserStatistics stats2 = new FormUserStatistics();
			stats2.NotifyFormShownUtc("Hello2", "Test2", ZDateTime.UtcNow.ToDateTime());

			ZFormActivityLogger.Instance.StatLogs.Clear();
			ZFormActivityLogger.Instance.StatLogs.Add(stats);
			ZFormActivityLogger.Instance.StatLogs.Add(stats2);
			Assert("Stat Log is waiting to be written", ZFormActivityLogger.Instance.StatLogs.Count >= 2);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MainForm.Close();
			AssertEquals("BOTH Stat Logs - even not yet closed window - is written instantly", 0, ZFormActivityLogger.Instance.StatLogs.Count);
			AssertEquals("Form should be closed", true, MainForm.IsDisposed);
		}

		[RequiresSTA]
		public void TestOnClosing_ActivityLogsWereNotSaved()
		{
			TestCaseHelper.ClearTable(StmActivityLogSchema.Constants.TableName);
			MainForm.Show();
			MainForm.NavigationBar.IsShown = false;
			Application.DoEvents();

			var stats = new FormUserStatistics();
			stats.NotifyFormShownUtc("Hello", "Test", ZDateTime.UtcNow.ToDateTime());
			stats.NotifyFormClosed(Guid.Empty, "");

			var stats2 = new FormUserStatistics();
			stats2.NotifyFormShownUtc("Hello2", "Test2", ZDateTime.UtcNow.ToDateTime());

			ZFormActivityLogger.Instance.StatLogs.Clear();
			ZFormActivityLogger.Instance.StatLogs.Add(stats);
			ZFormActivityLogger.Instance.StatLogs.Add(stats2);
			AssertEquals("Stat Log is waiting to be written", 2, ZFormActivityLogger.Instance.StatLogs.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			MainForm.Close();
			AssertEquals("Stat Logs are not removed", 2, ZFormActivityLogger.Instance.StatLogs.Count);
		}

#if !WINZOR && !NET8_0_OR_GREATER // Enable it for NET8 after WI00756415 is fixed

		public void TestOnClosing_BackgroundAppDomainWorkerJobsShown()
		{
			MainForm.Show();
			Application.DoEvents();

			BackgroundAppDomainWorker.QueueWorkItem("Test Work Item 1", new CrossAppDomainDelegate(delegate
			{ Thread.Sleep(100); }));
			BackgroundAppDomainWorker.QueueWorkItem("Test Work Item 2", new CrossAppDomainDelegate(delegate
			{ Thread.Sleep(100); }));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MainForm.Close();
			AssertEquals("Form should not close when jobs are in progress", false, MainForm.IsDisposed);
			AssertEquals(@$"
You cannot exit {BrandingFactory.Instance.ProductName} as the following background jobs are still running:

Test Work Item 1
Test Work Item 2

Please try again later.
", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			for (int i = 0; i < 5000 && BackgroundAppDomainWorker.WorkItemsInProgress.Length > 0; i++)
			{
				Thread.Sleep(10);
			}
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MainForm.Close();
			AssertEquals("Form should close when no jobs in progress", true, MainForm.IsDisposed);
			AssertEquals("Form should close when no jobs in progress", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

#endif

		[RequiresSTA]
		public void TestOnClosing_WhenNavBarHidden_ExitConfirmationShown()
		{
			var testUserNotifications = (UnitTestUserNotification)Globals.Message;

			MainForm.Show();
			MainForm.NavigationBar.IsShown = false;
			Application.DoEvents();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			MainForm.Close();
			AssertEquals("ExitConfirmationControl", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, MainForm.IsDisposed);
			ZFormModaliser.LastFormShownDialogForTest = null;
			MainForm.NavigationBar.IsShown = false;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			MainForm.Close();
			AssertEquals("ExitConfirmationControl", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, MainForm.IsDisposed);
			ZFormModaliser.LastFormShownDialogForTest = null;
			MainForm.NavigationBar.IsShown = false;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			MainForm.Close();
			AssertEquals("ExitConfirmationControl", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, MainForm.IsDisposed);
			ZFormModaliser.LastFormShownDialogForTest = null;
		}

		[RequiresSTA]
		public void TestOnClosing_NoNavBar_ExitConfirmationNotShown()
		{
			MainForm.Show();
			Application.DoEvents();

			MainForm.NavigationBar = null;
			MainForm.Close();
			Application.DoEvents();
			AssertEquals(true, MainForm.IsDisposed);
		}

		[RequiresSTA]
		public void TestOnClosing_HomeScreen_ExitConfirmationNotShown()
		{
			MainForm.Show();
			Application.DoEvents();

			MainForm.NavigationBar.IsShown = true;
			MainForm.Close();
			Application.DoEvents();
			AssertEquals(true, MainForm.IsDisposed);
		}

		[RequiresSTA]
		public void TestExitWithLocationNotLoggedIn()
		{
			using (var form = new TestMainForm())
			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserContext.User, Guid.Empty, Guid.Empty)))
			{
				AssertNoExceptionThrown(() => form.ExitMenuItem_Click(null, null));
			}
		}

		#endregion Form Events

		#region Login as new user

		[RequiresSTA]
		public void TestFormTextIsRefreshedWhenLoggedOnAsNewUser()
		{
			Guid initialBranchPK = Env.CurrentBranch.PK;

			GlbBranch newBranch = new BusinessObjectFactory().New<GlbBranch>();
			newBranch.GB_BranchName = "GahBlahBlahDuh";
			newBranch.GB_GC = Env.CurrentCompany.PK;
			newBranch.Factory.Save();

			Env.Registry.GlobalFormTopCaption = "";
			Env.Registry.ShowDatabaseName = false;
			Env.Registry.ShowBranchName = false;
			Env.Registry.ShowCompanyName = false;
			Env.Registry.ShowDepartmentName = false;
			Env.Registry.ShowUserName = false;

			Env.Registry.GlobalFormTopCaption = "ShowThis";
			Env.Registry.ShowDatabaseName = true;
			Env.Registry.ShowBranchName = true;
			Env.Registry.ShowCompanyName = true;
			Env.Registry.ShowDepartmentName = true;
			Env.Registry.ShowUserName = true;

			MainForm.UnloadForLogin();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				MainForm.DoInitializeAfterLogin();
				AssertEquals("Form.Text", BrandingFactory.Instance.ProductBrandingName + " - ShowThis - DB: " + Db.DatabaseName + " - Branch: GahBlahBlahDuh - Company: " + Env.CurrentCompany.Name +
					" - Department: " + Env.CurrentDepartment.Description + " - User: " + Env.CurrentUser.FullName, MainForm.TextIncludingSuffix);
			}
		}

		[RequiresSTA]
		public void TestFormMinimumSizeDoesnotChangedWhenLoggedOnAsNewUser()
		{
			Guid initialBranchPK = Env.CurrentBranch.PK;

			GlbBranch newBranch = new BusinessObjectFactory().New<GlbBranch>();
			newBranch.GB_BranchName = "GahBlahBlahDuh";
			newBranch.GB_GC = Env.CurrentCompany.PK;
			newBranch.Factory.Save();

			Env.Registry.GlobalFormTopCaption = "";
			Env.Registry.ShowDatabaseName = false;
			Env.Registry.ShowBranchName = false;
			Env.Registry.ShowCompanyName = false;
			Env.Registry.ShowDepartmentName = false;
			Env.Registry.ShowUserName = false;

			Env.Registry.GlobalFormTopCaption = "ShowThis";
			Env.Registry.ShowDatabaseName = true;
			Env.Registry.ShowBranchName = true;
			Env.Registry.ShowCompanyName = true;
			Env.Registry.ShowDepartmentName = true;
			Env.Registry.ShowUserName = true;

			var originMinSize = MainForm.MinimumSize;

			MainForm.Show();
			MainForm.UnloadForLogin();
			MainForm.WindowState = FormWindowState.Maximized;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				MainForm.DoInitializeAfterLogin();
				AssertEquals(originMinSize, MainForm.MinimumSize);
			}
		}

		public void TestReportWhenUnknownFormExistDuringSwitchLogin()
		{
			AssertEquals(0, OpenedFormCache.GetInstance().Count);
			ErrorReporter.Clear();

			using (var form = new ZForm())
			{
				form.Show();

				try
				{
					MainForm.GetFileLoginMenuItem().PerformClick();
				}
				finally
				{
					AssertEquals("There is an unknown form still open when users switch login. Please confirm whether this form should be ignored or instantiated with a valid Text.", ErrorReporter.LastMessageReported);
					Assert(ErrorReporter.LastExceptionReported is NotImplementedException);
					AssertEquals($"A form of type 'Enterprise.ZArchitecture.GUI.ZForm' without Text still open when users switch their login.", ErrorReporter.LastExceptionReported.Message);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestNonVisibleFormsDoNotShowInMessageDuringSwitchLogin()
		{
			AssertEquals(0, OpenedFormCache.GetInstance().Count);
			ErrorReporter.Clear();

			using (var form = new ZForm())
			using (var invisibleForm = new ZForm())
			using (var balloon = new BalloonWindow())
			using (var calendar = new ZPopupCalendar())
			{
				form.Text = "Test Form";
				invisibleForm.Text = "Invisible Form";
				balloon.Text = "Skipped Form With Text";
				calendar.Popup(new Point(), DateTime.Now, form, ZDateTimePickerFormat.Short);

				form.Show();
				balloon.Show();
				calendar.Show();

				balloon.Hide();
				calendar.Hide();

				try
				{
					MainForm.GetFileLoginMenuItem().PerformClick();
				}
				finally
				{
					AssertEquals("Message should only mention one form opened", "Question Please close the following windows before logging in as another user or before switching the login Company, Branch and Department:" + System.Environment.NewLine + "Test Form\r\n", ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());
					AssertNull("No exception should be thrown for a form with a null or empty Text property", ErrorReporter.LastExceptionReported);
					ErrorReporter.Clear();
				}
			}
		}

		[RequiresSTA]
		public void TestCanLogInAsNewUserWithBalloonWindowOpened()
		{
			AssertEquals(0, OpenedFormCache.GetInstance().Count);

			using (var balloonWindow = new BalloonWindow())
			{
				AssertNoExceptionThrown(() =>
				{
					balloonWindow.Show();
					MainForm.GetFileLoginMenuItem().PerformClick();
					AssertEquals("no new messages", 1, ((UnitTestUserNotification)Globals.Message).PreviousMessages.Length);
				});
			}
		}

		[RequiresSTA]
		public void TestGetOpenedFormsNameShouldReportErrorWhenTheTextOfFormIsEmpty()
		{
			AssertEquals(0, OpenedFormCache.GetInstance().Count);
			using (var formWithoutText = new ZForm())
			{
				formWithoutText.Show();
				MainForm.GetFileLoginMenuItem().PerformClick();
				AssertEquals($"{formWithoutText.GetType().FullName} without Text still opens when switching login", ErrorReporter.LastKeyReported);
			}
			ErrorReporter.Clear();
		}

		[RequiresSTA]
		public void TestCannotLogInAsNewUserWithOpenFormsOrModules()
		{
			AssertEquals(0, OpenedFormCache.GetInstance().Count);
			var form1 = new ZForm();
			form1.Text = "TestForm 1";
			form1.Show();
			var form2 = new ZForm();
			form2.Text = "TestForm 1";
			form2.Show();
			var form3 = new ZForm();
			form3.Text = "TestForm 2";
			form3.Show();

			var baseExpectMessageBuilder = new StringBuilder();

			var baseExpectMessage = "Question Please close the following windows before logging in as another user or before switching the login Company, Branch and Department:" + System.Environment.NewLine;

			baseExpectMessageBuilder.AppendLine("TestForm 1");
			baseExpectMessageBuilder.AppendLine("TestForm 1");
			baseExpectMessageBuilder.AppendLine("TestForm 2");
			var expectMessage = baseExpectMessage + baseExpectMessageBuilder.ToString();

			MainForm.GetFileLoginMenuItem().PerformClick();
			AssertEquals("Should shows the 3 opened forms name with correct order.", expectMessage, ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());
			form1.Dispose();
			form2.Dispose();
			form3.Dispose();

			((UnitTestUserNotification)Globals.Message).ClearMessagesAndAnswers();
			MainForm.GetFileLoginMenuItem().PerformClick();
			AssertEquals("no new messages", 1, ((UnitTestUserNotification)Globals.Message).PreviousMessages.Length);
			MainForm.DoInitializeAfterLogin();

			var module = new ZFilterModuleForTest().ShowPopup() as ZForm;
			baseExpectMessageBuilder = new StringBuilder();
			baseExpectMessageBuilder.AppendLine(module?.Text);
			expectMessage = baseExpectMessage + baseExpectMessageBuilder.ToString();

			MainForm.GetFileLoginMenuItem().PerformClick();
			AssertEquals(expectMessage, ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());
			module?.Dispose();
			AssertEquals(0, OpenedFormCache.GetInstance().AdditionalFormsCount);
			((UnitTestUserNotification)Globals.Message).ClearMessagesAndAnswers();
			MainForm.GetFileLoginMenuItem().PerformClick();
			AssertEquals("no new messages", 1, ((UnitTestUserNotification)Globals.Message).PreviousMessages.Length);
		}

		class TestClientHookWithHasCompanySpecificOverrides : TestClientHook
		{
			public override bool HasCompanySpecificOverrides => true;
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestClickLoginMenuItemWithOpenFormsOrModulesWithClientHookDoesntThrowException()
		{
			AssertNoExceptionWhenClickingMenuItem(MainForm.GetFileLoginMenuItem());
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestClickChangeCompanyMenuItemWithOpenFormsOrModulesWithClientHookDoesntThrowException()
		{
			AssertNoExceptionWhenClickingMenuItem(MainForm.ChangeBranchDepartmentMenuItem);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		[RequiresSTA]
		public void TestLoginMenuItemResetsTheIsAuthenticated()
		{
			// Arrange
			var newBranch = new BusinessObjectFactory { RefreshEnabled = false }.New<GlbBranch>();
			newBranch.GB_BranchName = "GahBlahBlahDuh";
			newBranch.GB_GC = Env.CurrentCompany.PK;
			newBranch.Factory.Save();

			var initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext(Env.CurrentUser, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK, loginAuthenticationInfo: LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser)));

			try
			{
				AssertEquals(true, Env.IsAuthenticated);

				// Act
				MainForm.GetFileLoginMenuItem().PerformClick();

				// Assert
				AssertEquals(false, Env.IsAuthenticated);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		void AssertNoExceptionWhenClickingMenuItem(ToolStripMenuItem menuItem)
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientHookWithHasCompanySpecificOverrides()))
			{
				ClientHookLoader.Instance.ClientHook.Initialise(true);

				AssertEquals(0, OpenedFormCache.GetInstance().Count);
				var form = new ZForm();
				form.Text = "TestForm";
				form.Show();
				menuItem.PerformClick();

				var baseExpectMessageBuilder = new StringBuilder();
				baseExpectMessageBuilder.AppendLine(form.Text);

				AssertEquals("Question Please close the following windows before logging in as another user or before switching the login Company, Branch and Department:" + System.Environment.NewLine + baseExpectMessageBuilder.ToString(), ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());
				form.Dispose();
				AssertEquals(0, OpenedFormCache.GetInstance().Count);
				((UnitTestUserNotification)Globals.Message).ClearMessagesAndAnswers();
				MainForm.GetFileLoginMenuItem().PerformClick();
				AssertEquals("Question Some company specific functionality may work incorrectly after switching between two different companies. You must log out and in again after changing companies.", ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());
			}
		}

		#region ZFilterModuleForTest

		class ZFilterModuleForTest : ZFilterGridModule
		{
			public ZQuery GetExportQuery()
			{
				return ExportQuery;
			}

			#region Abstract Members Implementation Dummies

			protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
			{
				get { throw new NotImplementedException(); }
			}

			public override SecurityCheckpoint SecurityCheckpoint
			{
				get { return EnvProxy.Instance.Security.None as SecurityCheckpoint; }
			}

			public override ModuleIdentifier ID
			{
				get { return DummyModuleIDs.Dummy; }
			}

			protected override FilterBusinessObject GetNewFilterBusinessObject()
			{
				return new DummyFilterStripBusinessObjectOverride();
			}

			protected override IFilterControl GetNewFilterControl()
			{
				return new DummyFilterControl(GetNewGridCollection(), (FilterStripBusinessObject)GetNewFilterBusinessObject());
			}

			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				return new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			}

			protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				throw new NotImplementedException();
			}

			public class DummyFilterStripBusinessObjectOverride : DummyFilterStripBusinessObject
			{
				public DummyFilterStripBusinessObjectOverride()
					: base()
				{
				}

				public class Schema
				{
					public const string TableName = "DummyFilterStripBusinessObjectOverride";
				}
			}

			#endregion Abstract Members Implementation Dummies

			public void AddImportDataMenuItems()
			{
				AddImportDataMenuItem("with license", null);
				AddImportDataMenuItem("without license", null, false);
			}

			public void AddImportFromCSVDataMenuItem(bool incLicence)
			{
				AddImportFromCSVDataMenuItem(null, incLicence);
			}

			public FilterModuleMenuItemDescriptorCollection ImportMenuItems_Exposed
			{
				get
				{
					return base.ImportMenuItems;
				}
			}
		}

		#endregion ZFilterModuleForTest

		[RequiresSTA]
		public void TestLoggingInAsNewUserRefreshesAdminMenuItems()
		{
			var controller = Factory.New<GlbStaff>();
			controller.GS_LoginName = "Controller";
			controller.GS_IsController = true;

			var nonController = Factory.New<GlbStaff>();
			nonController.GS_LoginName = "Non.Controller";
			nonController.GS_IsController = false;

			var operationalUser = Factory.New<GlbStaff>();
			operationalUser.GS_LoginName = "Operational";
			operationalUser.GS_IsOperational = true;
			operationalUser.GS_IsController = false;

			var nonOperationalUser = Factory.New<GlbStaff>();
			nonOperationalUser.GS_LoginName = "NonOperational";
			nonOperationalUser.GS_IsOperational = false;
			nonOperationalUser.GS_IsController = false;

			Factory.Save();

			LoginAndAssertMenuItemCount(controller.GS_LoginName, hasPurge: true, hasDbAdmin: true);
			LoginAndAssertMenuItemCount(nonController.GS_LoginName, hasPurge: false, hasDbAdmin: false);
			LoginAndAssertMenuItemCount(User.SupportUserName, hasPurge: true, hasDbAdmin: true);
			LoginAndAssertMenuItemCount(operationalUser.GS_LoginName, hasPurge: false, hasDbAdmin: false);
			LoginAndAssertMenuItemCount(nonOperationalUser.GS_LoginName, hasPurge: true, hasDbAdmin: false);
		}

		void LoginAndAssertMenuItemCount(string loginName, bool hasPurge, bool hasDbAdmin)
		{
			MainForm.UnloadForLogin();
			ZFormActivityLogger.Instance.DisableActivityLogger();
			using (Env.SetTemporaryUserContext(loginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				MainForm.DoInitializeAfterLogin();

				FindAndAssertMenuItem(MainForm.SettingsMenuButton.DropDownItems, "Purge Data (Admin Users)", hasPurge, loginName);
				FindAndAssertMenuItem(MainForm.HelpMenuButton.DropDownItems, "Database Administration", hasDbAdmin, loginName);

				var loginForm = ZApplication.GetOpenForms().OfType<GlbStaffForm>().FirstOrDefault();
				loginForm?.Dispose();
			}
		}

		void FindAndAssertMenuItem(ToolStripItemCollection menuItems, string expectedItem, bool shouldFindItem, string loginName)
		{
			var foundItem = menuItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == expectedItem);
			Assert(string.Format("Item [{0}] should{1} be found for user [{2}]", expectedItem, shouldFindItem ? "" : " NOT", loginName),
				shouldFindItem ? foundItem != null : foundItem == null);
		}
#if !WINZOR
		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestLogInAsNewUserRestartCargoWiseForHybridUser()
		{
			var programRestartMock = new Mock<IProgramRestarter>();
			// Create Hybrid User
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);
			var featureTest = Factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_GG_Group = group.PK;
			featureTest.SFT_FeatureName = StmFeatureTest.WinzorFeatureCode;
			Factory.Save();
			var mockWinformListener = new Mock<IWinFormsListener>();
			// UnloadForLogin Sometime activate the ZFormActivityLogger. Other tests using it might me flaky
			MainForm.UnloadForLogin();
			using (ObjectFactory.Substitute(mockWinformListener.Object))
			using (ObjectFactory.Substitute(programRestartMock.Object))
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				MainForm.DoInitializeAfterLogin();
				AssertEquals(true, Env.IsAuthenticated);
				MainForm.GetFileLoginMenuItem().PerformClick();
				programRestartMock.Verify(r => r.ShutdownEnterpriseWithMessage(It.IsAny<string>()), Times.Once());
				Assert("hybrid user should be logged out after restart", !Env.IsAuthenticated);
			}
			ZFormActivityLogger.Instance.DisableActivityLogger();
		}
#endif

		#endregion Login as new user

		#region Change Company/Branch/Dept.

		[RequiresSTA]
		public void TestChangeBranchDepartmentMenuItem()
		{
			ZToolStripMenuItem GetMenuItem(MainForm mainForm) => mainForm.ChangeBranchDepartmentMenuItem;
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true, hasError: true);

			MainForm.SaveChangeBranchDepartmentSettingIfNeeded("C");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true, isInDatabase: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true, hasError: true);

			MainForm.SaveChangeBranchDepartmentSettingIfNeeded("S");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true, isInDatabase: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true, hasError: true);
		}

		[RequiresSTA]
		public void TestChangeBranchDepartmentDefault()
		{
			ZToolStripMenuItem GetMenuItem(MainForm mainForm) => mainForm.ChangeBranchDepartmentMenuItem.DropDownItems.OfType<ZToolStripMenuItem>().Single(m => m.Text == "&Manually Close Forms");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, mainMenuItemText: "Change Company/&Branch/Dept. (Manually Close Forms)");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true, hasError: true);
		}

		[RequiresSTA]
		public void TestChangeBranchDepartmentClose()
		{
			ZToolStripMenuItem GetMenuItem(MainForm mainForm) => mainForm.ChangeBranchDepartmentMenuItem.DropDownItems.OfType<ZToolStripMenuItem>().Single(m => m.Text == "&Close Open Forms");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, mainMenuItemText: "Change Company/&Branch/Dept. (Close Forms)");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true, isInDatabase: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true, hasError: true);
		}

		[RequiresSTA]
		public void TestChangeBranchDepartmentSaveAndClose()
		{
			ZToolStripMenuItem GetMenuItem(MainForm mainForm) => mainForm.ChangeBranchDepartmentMenuItem.DropDownItems.OfType<ZToolStripMenuItem>().Single(m => m.Text == "&Save and Close Open Forms");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, mainMenuItemText: "Change Company/&Branch/Dept. (Save/Close Forms)");
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true, isInDatabase: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: false, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true);
			AssertChangeBranchDepartmentClick(GetMenuItem, shouldPrompt: true, hasForm: true, hasBusinessObject: true, isInDatabase: true, hasChanges: true, hasError: true);
		}

		void AssertChangeBranchDepartmentClick(Func<MainForm, ZToolStripMenuItem> getMenuItem, bool shouldPrompt, bool hasForm = false, bool hasBusinessObject = false, bool isInDatabase = false, bool hasChanges = false, bool hasError = false, string mainMenuItemText = null)
		{
			if (!hasForm && (hasBusinessObject || isInDatabase || hasChanges || hasError)
				|| (!hasBusinessObject && (isInDatabase || hasChanges || hasError))
				|| (!isInDatabase && (hasChanges || hasError))
				|| (!hasChanges && hasError))
			{
				throw new InvalidOperationException("Invalid flags combination");
			}

			using var testMainForm = new TestMainForm();
			StmData businessObject = null;
			ZForm openedForm = null;
			if (hasForm)
			{
				if (hasBusinessObject)
				{
					businessObject = Factory.NewWithValidTestData<StmData>();
					if (isInDatabase)
					{
						Factory.Save();
					}
					businessObject.HasChanges = hasChanges;
					if (hasError)
					{
						// SD_Name validation will add an error if it is not english character
						businessObject.SD_Name = "\u9999";
					}
				}

				openedForm = new ZForm(businessObject);
				openedForm.Text = "TestForm 1";
				openedForm.Show();
			}

			var menuItem = getMenuItem(testMainForm);
			menuItem.PerformClick();

			if (mainMenuItemText is not null)
			{
				AssertEquals(mainMenuItemText, testMainForm.ChangeBranchDepartmentMenuItem.Text);
			}

			if (menuItem != testMainForm.ChangeBranchDepartmentMenuItem)
			{
				AssertChangeBranchDepartmentShortcutKeys(testMainForm, menuItem);
			}

			string assertMessage;
			if (hasForm)
			{
				if (isInDatabase)
				{
					assertMessage = hasBusinessObject ? $"business object {(hasChanges ? "has" : "has no")} changes {(hasError ? "with" : "without")} error." : "form has no business object.";
				}
				else
				{
					assertMessage = "new business object is not saved.";
				}
			}
			else
			{
				assertMessage = "there is no open form.";
			}

			if (shouldPrompt)
			{
				string expectedPromptMessage = "Please close the following windows before logging in as another user or before switching the login Company, Branch and Department:" + System.Environment.NewLine + "TestForm 1\r\n";
				AssertEquals($"Prompt should be shown when {assertMessage}", expectedPromptMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertEquals($"No prompt should be shown when {assertMessage}", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				var loginLocationControl = testMainForm.BaseWorkspaceAreaPanel.Controls.OfType<LoginLocationControl>().SingleOrDefault();
				AssertNotNull(loginLocationControl);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			businessObject?.Delete();
			Factory.Save();
			openedForm?.Dispose();
		}

		void AssertChangeBranchDepartmentShortcutKeys(MainForm mainForm, ZToolStripMenuItem menuItem)
		{
			foreach (ZToolStripMenuItem dropDownItem in mainForm.ChangeBranchDepartmentMenuItem.DropDownItems)
			{
				if (dropDownItem == menuItem)
				{
					AssertEquals("ShortcutKeys should be set for the preferred menu item", Keys.Control | Keys.Shift | Keys.B, dropDownItem.ShortcutKeys);
				}
				else
				{
					AssertEquals("ShortcutKeys should be cleared for other menu items", Keys.None, dropDownItem.ShortcutKeys);
				}
			}
		}

		#endregion  Change Company/Branch/Dept.

		#region Show In System Tray

#if !WINZOR

		[RequiresSTA]
		public void TestShowInSystemTray()
		{
			var savedShowInSystemTray = ((WinFormsEnvironment)Env.Instance).ShowInSystemTray;
			((WinFormsEnvironment)Env.Instance).ShowInSystemTray = true;
			try
			{
				MainForm.Show();
				Application.DoEvents();

				AssertEquals("Notify icon should be visible", true, MainForm.notifyIcon.Visible);
				AssertEquals("Notify icon should have form text", MainForm.Text, MainForm.notifyIcon.Text);
				MainForm.WindowState = FormWindowState.Minimized;
				MainForm.OnDeactivate(EventArgs.Empty);
				Application.DoEvents();
				AssertEquals("A balloon notifying the user of issues with this feature should be shown on first minimize", true, MainForm.ShowNotifyIconBalloonCalled);
				AssertEquals("Form should be minimized", FormWindowState.Minimized, MainForm.WindowState);
				AssertEquals("Form should exist in OpenForms, because it is an open form", true, new ArrayList(Application.OpenForms).Contains(MainForm));

				AssertEquals("When minimised, it shouldn't show in the taskbar", false, MainForm.ShowInTaskbar);
				MainForm.WindowState = FormWindowState.Normal;
				MainForm.OnActivated(EventArgs.Empty);
				Application.DoEvents();
				AssertEquals("Form should be restored", FormWindowState.Normal, MainForm.WindowState);
				AssertEquals("When normal, it should show in the task bar", true, MainForm.ShowInTaskbar);
				AssertEquals("Form should exist in OpenForms, because it is an open form", true, new ArrayList(Application.OpenForms).Contains(MainForm));

				MainForm.ShowNotifyIconBalloonCalled = false;
				MainForm.WindowState = FormWindowState.Minimized;
				MainForm.OnActivated(EventArgs.Empty);
				AssertEquals("The balloon should not show a second time", false, MainForm.ShowNotifyIconBalloonCalled);
				Application.DoEvents();

				MainForm.WindowState = FormWindowState.Normal;
				MainForm.OnActivated(EventArgs.Empty);
				Application.DoEvents();
				AssertEquals("Form should be restored, otherwise this test may leak memory", FormWindowState.Normal, MainForm.WindowState);
			}
			finally
			{
				((WinFormsEnvironment)Env.Instance).ShowInSystemTray = savedShowInSystemTray;
			}
		}

#endif

		#endregion Show In System Tray

		#region Favorites and Recent

		[RequiresSTA]
		public void TestStartupCategoryIsJump()
		{
			using (var form = new TestMainForm())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Startup category is Jump", ModuleTreeLoaderConstant.Category.Jump.Name, form.NavigationBar.navigationViewModel.SelectedCategory.Name);
			}
		}

		[RequiresSTA]
		public void TestFavorites()
		{
			using (var form = new TestMainForm())
			{
				var oldMainForm = StartupOpenMainFormTask.MainFormInstance;
				StartupOpenMainFormTask.MainFormInstance = form;
				try
				{
					form.Show();
					Application.DoEvents();

					var favoriteShortcut1 = new LinkWrapper("Dummy", Guid.Empty, string.Empty, string.Empty);
					var mainModule1 = new LinkMainFormModule(favoriteShortcut1);

					var favoriteShortcut2 = new LinkWrapper("Dummy", Guid.NewGuid(), "http://pen", "I have a pen");
					var mainModule2 = new LinkMainFormModule(favoriteShortcut2);

					var favoriteShortcut3 = new LinkWrapper("Dummy2", Guid.NewGuid(), "http://pineapple", "I have a pineapple");
					var mainModule3 = new LinkMainFormModule(favoriteShortcut3);

					Assert("Added successfully", form.FavoriteProvider.AddToFavorites(favoriteShortcut1));
					AssertFavoriteExistance(form, favoriteShortcut1, true);

					Assert("Did not add duplicate item", !form.FavoriteProvider.AddToFavorites(favoriteShortcut1));

					Assert("mainModule1 removed from favorite", !form.AddOrRemoveFromFavorites(mainModule1));
					AssertFavoriteExistance(form, favoriteShortcut1, false);

					Assert("mainModule1 added to favorite", form.AddOrRemoveFromFavorites(mainModule1));
					AssertFavoriteExistance(form, favoriteShortcut1, true);

					form.FavoriteProvider.DeleteFromFavorites(favoriteShortcut1);
					AssertFavoriteExistance(form, favoriteShortcut1, false);

					Assert("Added successfully", form.FavoriteProvider.AddToFavorites(favoriteShortcut1));
					AssertFavoriteExistance(form, favoriteShortcut1, true);

					Assert("Added successfully", form.FavoriteProvider.AddToFavorites(favoriteShortcut2));
					AssertFavoriteExistance(form, favoriteShortcut2, true);

					Assert("Added successfully", form.FavoriteProvider.AddToFavorites(favoriteShortcut3));
					AssertFavoriteExistance(form, favoriteShortcut3, true);

					form.RemoveSingleLink(mainModule1);
					AssertFavoriteExistance(form, favoriteShortcut1, false);

					form.RemoveSingleLink(mainModule2);
					AssertFavoriteExistance(form, favoriteShortcut2, false);

					form.RemoveSingleLink(mainModule3);
					AssertFavoriteExistance(form, favoriteShortcut3, false);
				}
				finally
				{
					StartupOpenMainFormTask.MainFormInstance = oldMainForm;
				}
			}
		}

		[RequiresSTA]
		public void TestRemovingFromFavoritesAddsBackToRecent()
		{
			using (var form = new TestMainForm())
			{
				var oldMainForm = StartupOpenMainFormTask.MainFormInstance;
				StartupOpenMainFormTask.MainFormInstance = form;
				try
				{
					form.Show();
					Application.DoEvents();

					var favoriteModuleShortcut = new LinkWrapper("Dummy", Guid.Empty, string.Empty, string.Empty);
					var favoriteItemShortcut = new LinkWrapper("Dummy", Guid.NewGuid(), "http:", "description");

					var mainModule = new LinkMainFormModule(favoriteModuleShortcut);
					var mainModuleItem = new LinkMainFormModule(favoriteItemShortcut);

					Assert("Added module successfully", form.FavoriteProvider.AddToFavorites(favoriteModuleShortcut));
					AssertFavoriteExistance(form, favoriteModuleShortcut, true);
					AssertRecentModuleExistance(form, favoriteModuleShortcut, false);

					Assert("Added item successfully", form.FavoriteProvider.AddToFavorites(favoriteItemShortcut));
					AssertFavoriteExistance(form, favoriteItemShortcut, true);
					AssertRecentItemExistance(form, favoriteItemShortcut, false, false);

					Assert("mainModule removed from favorite", !form.AddOrRemoveFromFavorites(mainModule));
					AssertFavoriteExistance(form, favoriteModuleShortcut, false);
					AssertRecentModuleExistance(form, favoriteModuleShortcut, true);

					Assert("mainModuleItem removed from favorite", !form.AddOrRemoveFromFavorites(mainModuleItem));
					AssertFavoriteExistance(form, favoriteItemShortcut, false);
					AssertRecentItemExistance(form, favoriteItemShortcut, true, true);
				}
				finally
				{
					StartupOpenMainFormTask.MainFormInstance = oldMainForm;
				}
			}
		}

		[RequiresSTA]
		public void TestRecentItems()
		{
			using (var form = new TestMainForm())
			{
				var oldMainForm = StartupOpenMainFormTask.MainFormInstance;
				StartupOpenMainFormTask.MainFormInstance = form;
				try
				{
					form.Show();
					Application.DoEvents();

					var recentShortcut1 = new LinkWrapper("Dummy", Guid.NewGuid(), "http://", "description");
					var recentShortcut2 = new LinkWrapper("Dummy", Guid.NewGuid(), "http://pen", "I have a pen");
					var recentShortcut3 = new LinkWrapper("Dummy2", Guid.NewGuid(), "http://pineapple", "I have a pineapple");

					form.FavoriteProvider.AddToRecentItems(recentShortcut1);
					AssertRecentItemExistance(form, recentShortcut1, true, true);

					form.FavoriteProvider.AddToRecentItems(recentShortcut2);
					AssertRecentItemExistance(form, recentShortcut2, true, true);

					form.FavoriteProvider.AddToRecentItems(recentShortcut3);
					AssertRecentItemExistance(form, recentShortcut3, true, true);

					Assert("recentShortcut1 added to favorite", form.AddOrRemoveFromFavorites(new LinkMainFormModule(recentShortcut1)));
					AssertFavoriteExistance(form, recentShortcut1, true);
					AssertRecentItemExistance(form, recentShortcut1, true, true);

					Assert("recentShortcut1 removed from favorite", !form.AddOrRemoveFromFavorites(new LinkMainFormModule(recentShortcut1)));
					AssertFavoriteExistance(form, recentShortcut1, false);
					AssertRecentItemExistance(form, recentShortcut1, true, true);
					form.RemoveSingleLink(new LinkMainFormModule(recentShortcut1));
					AssertRecentItemExistance(form, recentShortcut1, false, true);

					form.RemoveSingleLink(new LinkMainFormModule(recentShortcut2));
					AssertRecentItemExistance(form, recentShortcut2, false, true);

					form.RemoveSingleLink(new LinkMainFormModule(recentShortcut3));
					AssertRecentItemExistance(form, recentShortcut3, false, true);
				}
				finally
				{
					StartupOpenMainFormTask.MainFormInstance = oldMainForm;
				}
			}
		}

		[RequiresSTA]
		public void TestRecentModules()
		{
			using (var form = new TestMainForm())
			{
				var oldMainForm = StartupOpenMainFormTask.MainFormInstance;
				StartupOpenMainFormTask.MainFormInstance = form;
				try
				{
					form.Show();
					Application.DoEvents();

					var recentShortcut = new LinkWrapper("Dummy", Guid.Empty, string.Empty, string.Empty);

					form.UpdateRecentModules(new LinkMainFormModule(recentShortcut));
					AssertRecentModuleExistance(form, recentShortcut, true);

					Assert("recentShortcut added to favorite", form.AddOrRemoveFromFavorites(new LinkMainFormModule(recentShortcut)));
					AssertFavoriteExistance(form, recentShortcut, true);
					AssertRecentModuleExistance(form, recentShortcut, false);
				}
				finally
				{
					StartupOpenMainFormTask.MainFormInstance = oldMainForm;
				}
			}
		}

		[RequiresSTA]
		public void TestRecentModuleWithSecurityDenied()
		{
			using (var form = new TestMainForm())
			{
				var oldMainForm = StartupOpenMainFormTask.MainFormInstance;
				StartupOpenMainFormTask.MainFormInstance = form;
				try
				{
					form.Show();
					Application.DoEvents();

					var favoriteShortcut = new LinkWrapper("Dummy", Guid.Empty, string.Empty, string.Empty);
					RecentItemManager.Instance.AddToFavoriteModules(favoriteShortcut);
					var favoriteModule = new LinkMainFormModule(favoriteShortcut);
					var original = Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)favoriteModule.SecurityCheckpoint);
					original.IsAllowed = false;

					form.OpenFavorite(0, true);
					AssertNull("Recent item should not open because security not allowed", form.LastModuleInNewWindow);
				}
				finally
				{
					StartupOpenMainFormTask.MainFormInstance = oldMainForm;
				}
			}
		}

		[RequiresSTA]
		public void TestRecentModuleWithSecurityAllowed()
		{
			using (var form = new TestMainForm())
			{
				var oldMainForm = StartupOpenMainFormTask.MainFormInstance;
				StartupOpenMainFormTask.MainFormInstance = form;
				try
				{
					form.Show();
					Application.DoEvents();

					var favoriteShortcut = new LinkWrapper("Dummy", Guid.Empty, string.Empty, string.Empty);
					RecentItemManager.Instance.AddToFavoriteModules(favoriteShortcut);
					var favoriteModule = new LinkMainFormModule(favoriteShortcut);
					var original = Env.Security.FindOrCreateAccessModuleCheckPoint((SecurityCheckpoint)favoriteModule.SecurityCheckpoint);
					original.IsAllowed = true;

					form.OpenFavorite(0, true);
					AssertNotNull("Recent item should open", form.LastModuleInNewWindow);

					form.LastModuleInNewWindow.Dispose();
				}
				finally
				{
					StartupOpenMainFormTask.MainFormInstance = oldMainForm;
				}
			}
		}

		[RequiresSTA]
		public void TestFavoritesFull()
		{
			using (var form = new TestMainForm())
			{
				var oldMainForm = StartupOpenMainFormTask.MainFormInstance;
				StartupOpenMainFormTask.MainFormInstance = form;
				try
				{
					form.Show();
					Application.DoEvents();
					RecentItemManager.Instance.FavoriteModules.CleanUp();
					while (RecentItemManager.Instance.FavoriteModules.Count < RecentItemManager.Instance.MaximumNumberOfFavorites)
					{
						var favoriteShortcut1 = new LinkWrapper("WorkItem", Guid.NewGuid(), string.Empty, string.Empty);
						var mainModule1 = new LinkMainFormModule(favoriteShortcut1);
						form.AddOrRemoveFromFavorites(mainModule1);
					}
					var favoriteShortcut2 = new LinkWrapper("WorkItem", Guid.NewGuid(), string.Empty, string.Empty);
					var mainModule2 = new LinkMainFormModule(favoriteShortcut2);
					Assert("favorite is full", !form.AddOrRemoveFromFavorites(mainModule2));
				}
				finally
				{
					RecentItemManager.Instance.FavoriteModules.CleanUp();
					StartupOpenMainFormTask.MainFormInstance = oldMainForm;
				}
			}
		}

		#region Implementation

		void AssertFavoriteExistance(MainForm form, LinkWrapper shortcut, bool shouldExist)
		{
			var shouldExistMsg = shouldExist ? "should exist in Favorites but doesn't" : "shouldn't exist in Favorites but does";
			var assertMsg = $"Shortcut \"{shortcut.UniqueKey}\" {shouldExistMsg}";
			Assert(assertMsg, shouldExist == ((CargoWise.Main.Navigation.MenuSection)form.NavigationBar.navigationViewModel.Categories[0].Buttons[0]).Items.Any(i => i.Key == shortcut.UniqueKey));
			Assert("Exists in recent items manager", shouldExist == RecentItemManager.Instance.IsInFavoriteModules(shortcut));
			Assert("Exists according to MainForm", shouldExist == form.FavoriteProvider.IsInFavorites(shortcut));
		}

		void AssertRecentItemExistance(MainForm form, LinkWrapper shortcut, bool shouldExist, bool shouldExistInModuleSpecific)
		{
			var shouldExistMsg = shouldExist ? "should exist in Recent Items but doesn't" : "shouldn't exist in Recent Items but does";
			var assertMsg = $"Shortcut \"{shortcut.UniqueKey}\" {shouldExistMsg}";
			Assert(assertMsg, shouldExist == ((CargoWise.Main.Navigation.MenuSection)form.NavigationBar.navigationViewModel.Categories[0].Buttons[1]).Items.Any(i => i.Key == shortcut.UniqueKey));
			Assert("Exists in recent items manager", shouldExist == RecentItemManager.Instance.IsInRecentItems(string.Empty, shortcut));
			Assert("Exists in recent items manager (module specific)", shouldExistInModuleSpecific == RecentItemManager.Instance.IsInRecentItems(shortcut.ModuleName, shortcut));
		}

		void AssertRecentModuleExistance(MainForm form, LinkWrapper shortcut, bool shouldExist)
		{
			var shouldExistMsg = shouldExist ? "should exist in Recent Modules but doesn't" : "shouldn't exist in Recent Modules but does";
			var assertMsg = $"Shortcut \"{shortcut.UniqueKey}\" {shouldExistMsg}";
			Assert(assertMsg, shouldExist == ((CargoWise.Main.Navigation.MenuSection)form.NavigationBar.navigationViewModel.Categories[0].Buttons[2]).Items.Any(i => i.Key == shortcut.UniqueKey));
			Assert("Exists in recent items manager", shouldExist == RecentItemManager.Instance.IsInRecentModules(shortcut));
		}

		void AssertDeleteButton(ToolStripItem toolStripItem, ZEmbeddedModule module, string expectedToolTipText, int expectedImageIndex, Image expectedImage, IconTypes expectedIconTypes)
		{
			var assertMessage = "Delete-button must show correct {0} based on type (delete, deactivate, activate)";
			AssertEquals(string.Format(assertMessage, "tooltip-text"), expectedToolTipText, toolStripItem.ToolTipText);
			AssertEquals(string.Format(assertMessage, "Image"), expectedImage, toolStripItem.Image);

			var deleteCaptions = new[] {
					KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Delete),
					KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Activate),
					KMenuItem.StripAcceleratorKeys(ZFilterGridModule.DeleteButtonCaptions.Deactivate) };
			var toolbarButton = deleteCaptions.Select(c => (ZToolBarButton)module.ToolBarButtons.FindByText(c)).FirstOrDefault();
			AssertEquals(string.Format(assertMessage, "Image-index"), expectedImageIndex, toolbarButton.ImageIndex);
			AssertEquals(string.Format(assertMessage, "Active-icon-type"), expectedIconTypes, toolbarButton.ActiveIconType);
		}

		#endregion Implementation

		#endregion Favorites and Recent

		#region Theme

		[RequiresSTA]
		public void TestHomeButtonUsesTheme()
		{
			using (var form = new TestMainForm())
			{
				form.Show();
				Application.DoEvents();

				var colorTheme = SystemDataRegistry.Instance.ColorTheme;

				AssertEquals("Background Color", colorTheme.NavBarButtonColor1, form.HomeButton.BackColor);
				AssertEquals("Text Color", colorTheme.NavBarTextColor, form.HomeButton.ForeColor);
				AssertEquals("Hover Color", colorTheme.NavBarGroupSelected1, form.HomeButton.FlatAppearance.MouseOverBackColor);
			}
		}

		[RequiresSTA]
		public void TestPanelBackColorsAreSet()
		{
			using (var form = new TestMainForm())
			{
				AssertEquals("BaseWorkspaceAreaPanel colour is wrong", SystemColors.Control, form.BaseWorkspaceAreaPanel.BackColor);
				AssertEquals("RightWorkspaceAreaPanel colour is wrong", SystemColors.Control, form.RightWorkspaceAreaPanel.BackColor);
				AssertEquals("ZModulePanel colour is wrong", SystemColors.Control, form.ZModulePanel.BackColor);
			}
		}

		[RequiresSTA]
		public void TestTitleBarBackground_UsesColourTheme()
		{
			using (var form = new TestMainForm())
			using (SystemDataRegistry.Instance.SetColorThemeTemporarily(DefinedColorThemes.GrapeColorTheme))
			{
				AssertEquals("PRE: Registry has 'Grape' colour theme selected", "Grape", ((ColorTheme)SystemDataRegistry.Instance.ColorTheme).Name);

				form.Show();
				form.SetColorTheme();

				CombineAssertions("Main form should use title bar colour theme as set in the registry", () =>
				{
					AssertEquals(DefinedColorThemes.GrapeColorTheme.TitleBarBackground, form.TitleBar.BackColor);
					AssertEquals(DefinedColorThemes.GrapeColorTheme.TitleBarText, form.AppTitleText.ForeColor);
				});
			}
		}

		[RequiresSTA]
		public void TestColorThemeIsUpdated_WhenLoggedOn()
		{
			using (SystemDataRegistry.Instance.SetColorThemeTemporarily(DefinedColorThemes.GrapeColorTheme))
			{
				AssertEquals("Grape", ((ColorTheme)SystemDataRegistry.Instance.ColorTheme).Name);

				using (var form = new TestMainForm())
				{
					AssertEquals("Default", ((ColorTheme)SystemDataRegistry.Instance.ColorTheme).Name);
				}
			}
		}

		#endregion Theme

		#region Test Classes

		internal class TestMainForm : MainForm
		{
			public TestMainForm(bool isCWNext = false)
			{
				IsCWNext = isCWNext;

				try
				{
					this.DoInitializeAfterLogin();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					this.Dispose();
					throw;
				}
			}

#if !WINZOR

			public new ZNotifyIconEx notifyIcon
			{
				get { return base.notifyIcon; }
			}

#endif

			public ToolStripMenuItem GetFileLoginMenuItem()
			{
				return SettingsMenuButton.DropDownItems.OfType<ZToolStripMenuItem>().FirstOrDefault(x => x.Text == "&Login as New User");
			}

			public IDisposable PreviousEmbeddedModule
			{
				get { return base.previousEmbeddedModule; }
			}

			public new void OnActivated(EventArgs e)
			{
				base.OnActivated(e);
			}

			public new void OnDeactivate(EventArgs e)
			{
				base.OnDeactivate(e);
			}

#if !WINZOR

			public bool ShowNotifyIconBalloonCalled;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
			protected override void ShowNotifyIconBalloon(string title, string text, ZNotifyIconEx.NotifyInfoFlags type, int timeoutInMilliSeconds)
			{
				base.ShowNotifyIconBalloon(title, text, type, timeoutInMilliSeconds);
				ShowNotifyIconBalloonCalled = true;
			}

#endif

			internal override bool HasLicenceAndSecurityPermissions(MainFormModule module)
			{
				if (ByPassSecurityLicenceCheck)
				{
					return true;
				}
				else
				{
					return base.HasLicenceAndSecurityPermissions(module);
				}
			}

			public bool ByPassSecurityLicenceCheck;

			internal override bool HasUnReadItemsMandatoryToRead()
			{
				if (ByPassHasUnReadItemsCheck)
				{
					return false;
				}
				else
				{
					return base.HasUnReadItemsMandatoryToRead();
				}
			}

			public bool ByPassHasUnReadItemsCheck;

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					this.NavigationBar?.navigationViewModel?.SearchViewModel.WaitingForSearchToComplete();
				}
				base.Dispose(disposing);
			}
		}

		internal class TestMainFormWithOpenModuleInProgress : MainForm
		{
			public TestMainFormWithOpenModuleInProgress()
			{
				InitiateForm();
			}

			void InitiateForm()
			{
				openModuleInProgress = true;

				OpenStartUpModule();
				HasUnReadItemsMandatoryToRead();
			}
		}

		internal class TestMainFormWithHotkeyOverride : TestMainForm
		{
			internal HotkeyRegister GlobalHotkeysForTest => GlobalHotkeys;
			protected override HotkeyRegister GlobalHotkeys { get; } = new HotkeyRegister();
			protected internal new HotkeyRegister Hotkeys => base.Hotkeys;
		}

		internal class TestMainFormWithUserRepositoryFormList : TestMainForm
		{
			public List<UserRepositoryConsoleForm> OpenedUserRepositoryFormList = new List<UserRepositoryConsoleForm>();

			protected override void AddToOpenedUserRepositoryConsoleFormList(UserRepositoryConsoleForm form)
			{
				OpenedUserRepositoryFormList.Add(form);
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing && OpenedUserRepositoryFormList != null)
				{
					OpenedUserRepositoryFormList.ForEach(form => form.Dispose());
					OpenedUserRepositoryFormList.Clear();
				}
				base.Dispose(isDisposing);
			}
		}

		internal class TestMainFormOnSwitchUser : TestMainForm
		{
			public void SwitchUser()
			{
				OnSwitchUser_ClearGlowUserData();
			}

			void OnSwitchUser_ClearGlowUserData()
			{
				ObjectFactory.Get<ZArchitecture.GlowInterop.IGlowServiceClientFactory>().ClearUserData();
			}
		}

		#endregion Test Classes

		#region TestLateEvents

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestLateEvents()
		{
			MainFormModule module1 = new MainFormModule(ModuleIDs.AREnquiry);
			MainFormModule module2 = new MainFormModule(ModuleIDs.APEnquiry);
			using (TestMainForm form = new TestMainForm())
			{
				form.OpenModule(module1, true);
				form.DisposePreviousEmbeddedModuleDoingEvents += (sender, e) =>
					{
						((MainForm)sender).NavigationBar.OnTileLink_Click(sender, module2);
					};
				form.Close();
			}
		}

		#endregion TestLateEvents

		[RequiresSTA]
		public void TestHomeButtonImageScalingSize()
		{
			using (var form1 = new TestMainForm())
			{
				form1.Show();
				Application.DoEvents();

				AssertEquals("Default HomeButton Image should be 16 * 16", new Size(16, 16), form1.HomeButton.Image.Size);
			}

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(192, 192))
			using (var form2 = new TestMainForm())
			{
				form2.Show();
				Application.DoEvents();

				AssertEquals("when DPI is 192 the HomeButton Image should be 32 * 32", new Size(32, 32), form2.HomeButton.Image.Size);
			}
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestClickDiagnosticsMenuItem()
		{
			var itemNameMap = new Dictionary<string, string>
				{
					{ "Denied Party Screening Monitor", "DeniedPartyScreeningMonitoringForm" },
					{ "Trace Monitor", "TraceMonitorForm" },
					{ "De-duplication Result Monitor", "DeduplicationMonitoringForm" },
					{ "Security Checkpoint Monitor", "SecurityCheckpointMonitoringForm" },
					{ "Address Validation Message Monitor", "AddressValidationMessageMonitorForm" },
					{ "UXML Matching Diagnostic Tool", "UXMLMatchingDiagnosticToolForm" },
				};

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var userContext = new TemporaryUserContext { StaffLoginName = "CWSupport" };

			using (userContext.Set())
			using (var form = new TestMainForm())
			{
				foreach (var eachItem in itemNameMap)
				{
					var items = form.HelpMenuButton.DropDownItems.OfType<ZToolStripMenuItem>();
					var diagnosticsMenu = (ZToolStripMenuItem)form.HelpMenuButton.DropDownItems["Diagnostics"];
					AssertNotNull(diagnosticsMenu);

					var addressItem = diagnosticsMenu.DropDownItems[eachItem.Key];
					AssertNotNull(addressItem);

					addressItem.PerformClick();
					WaitForFormOpen();

					var openForms = ZApplication.GetOpenForms();

					AssertEquals(1, openForms.Length);
					AssertEquals(eachItem.Value, openForms[0].Name);

					AddressValidationMessageMonitorForm.MessageWriter = null;

					openForms[0].Invoke(new Action(() =>
					{
						openForms[0].Close();
						openForms[0].Dispose();
					}));
				}
			}
		}

		[RequiresSTA]
		public void TestClickThreadMonitorMenuTwice()
		{
			using (var form = new TestMainForm())
			{
				try
				{
					OpenThreadMonitor(form);
					var openForm = ZApplication.GetOpenForms().OfType<ThreadMonitorForm>().Single();
					openForm.WindowState = FormWindowState.Minimized;
					OpenThreadMonitor(form);
					if (ErrorReporter.TotalErrorCount > 0)
					{
						Fail(ErrorReporter.LastExceptionReported.InnerException.Message);
					}

					openForm = ZApplication.GetOpenForms().OfType<ThreadMonitorForm>().Single();
					AssertNotEquals(FormWindowState.Minimized, openForm.WindowState);
					openForm.Close();
					openForm.Dispose();
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}

			void OpenThreadMonitor(TestMainForm mainForm)
			{
				var diagnosticsMenu = (ZToolStripMenuItem)mainForm.HelpMenuButton.DropDownItems["Diagnostics"];
				var threadMonitor = diagnosticsMenu.DropDownItems["Thread Monitor (Hang Debugging)"];
				threadMonitor.PerformClick();
				WaitForFormOpen();
			}
		}

		void WaitForFormOpen()
		{
			for (var i = 0; i < 100; i++)
			{
				Thread.Sleep(100);
				Application.DoEvents();
				if (ZApplication.GetOpenForms().Length > 0)
				{
					break;
				}
			}
		}

		#region SetSQLPassword

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestSetSQLPassword()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var controllerStaff = Factory.New<GlbStaff>();
				var dbReaderStaff = Factory.NewWithValidTestData<GlbStaff>();
				try
				{
					controllerStaff.GS_LoginName = "First Controller Staff";
					controllerStaff.GS_IsActive = true;
					dbReaderStaff.GS_LoginName = "~12345~";
					Factory.Save();

					using ((new TemporaryUserContext() { StaffLoginName = dbReaderStaff.GS_LoginName }).Set())
					using (var form = new TestMainForm())
					{
						form.SetSQLPasswordMenuItem_Click(null, null);
						AssertEquals("Last shown form is correct type (no staff with DB access)", null, ZFormModaliser.LastFormShownDialogForTest);

						dbReaderStaff.IsDatabaseDeveloper = true;
						Factory.Save();
						form.SetSQLPasswordMenuItem_Click(null, null);
						AssertEquals("Last shown form is correct type for staff with DB access", typeof(SetSQLPasswordForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					}
				}
				finally
				{
					controllerStaff.Delete();
					dbReaderStaff.Delete();
					Factory.Save();
				}
			}
		}

		#endregion

		#region SQL Server System Configurations

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestExecuteConfigureServerProcedure_AllSuccess()
		{
			var utilsMock = new Mock<ServerConfigurationUtils>();
			utilsMock.Setup(x => x.EnsureServerIsConfiguredAndTryConfiguringOtherReplicas(It.IsAny<AdminConnection>(), out It.Ref<List<Exception>>.IsAny));
			using (var form = new TestMainForm())
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.ExecuteConfigureServerProcedure(utilsMock.Object);

				AssertEquals("PreviousMessages was question.", true, UnitTestUserNotification.Instance.PreviousMessages[1].WasQuestion);
				AssertEquals("The system will execute the Configure-Server procedure.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Last message was information.", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Configure-Server procedure executed successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestExecuteConfigureServerProcedure_SecondaryErrors()
		{
			var utilsMock = new Mock<ServerConfigurationUtils>();
			utilsMock.Setup(x => x.EnsureServerIsConfiguredAndTryConfiguringOtherReplicas(It.IsAny<AdminConnection>(), out It.Ref<List<Exception>>.IsAny))
				.Callback((AdminConnection connection, out List<Exception> exceptions) =>
				{
					exceptions = new List<Exception> {
						new InvalidOperationException("Something went wrong on the other server."),
						new InvalidOperationException("Something went wrong on the other other server.")
					};
				});
			using (var form = new TestMainForm())
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.ExecuteConfigureServerProcedure(utilsMock.Object);

				AssertEquals("PreviousMessages was question.", true, UnitTestUserNotification.Instance.PreviousMessages[1].WasQuestion);
				AssertEquals("The system will execute the Configure-Server procedure.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Last message was warning.", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("The Configure-Server procedure executed successfully on the primary server. However, the following errors occurred when attempting to configure the secondaries: \r\nSomething went wrong on the other server.\r\nSomething went wrong on the other other server.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestExecuteConfigureServerProcedure_fail()
		{
			var utilsMock = new Mock<ServerConfigurationUtils>();
			utilsMock.Setup(x => x.EnsureServerIsConfiguredAndTryConfiguringOtherReplicas(It.IsAny<AdminConnection>(), out It.Ref<List<Exception>>.IsAny))
				.Callback((AdminConnection connection, out List<Exception> exceptions) =>
				{
					throw new Exception("Something went wrong on the primary server.");
				});
			using (var form = new TestMainForm())
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.ExecuteConfigureServerProcedure(utilsMock.Object);

				AssertEquals("PreviousMessages was question.", true, UnitTestUserNotification.Instance.PreviousMessages[1].WasQuestion);
				AssertEquals("The system will execute the Configure-Server procedure.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Last message was error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Something went wrong on the primary server.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestSQLServerSystemConfigurations()
		{
			using (var form = new TestMainForm())
			{
				form.SqlServerSystemConfigurations_Click(null, null);
				AssertEquals("Last shown form is correct type for staff with DB access", typeof(SqlSystemConfigurationsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		#endregion

		#region Restore DbLogins

		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestRestoreDbLogins()
		{
			using (var testAdminConnection = Db.NewAdminConnection())
			{
				var sqlText = string.Format(
					CultureInfo.InvariantCulture,
					"REVOKE IMPERSONATE ON USER::[{0}] FROM [cwRestrictedWriterRole]; DROP USER IF EXISTS [{0}]; CREATE USER [{0}] FOR LOGIN [{0}]; " +
					"DROP USER IF EXISTS [{1}]; CREATE USER [{1}] FOR LOGIN [{1}]; ALTER ROLE [db_owner] ADD MEMBER[{1}]; " +
					"DROP USER IF EXISTS [{2}]; CREATE USER [{2}] FOR LOGIN [{2}]; ALTER ROLE [db_datareader] ADD MEMBER[{2}]; ALTER ROLE [cwUnrestrictedWriterRole] ADD MEMBER[{2}]; ",
					((IDbLoginRepair)testAdminConnection).RestrictedReaderDbLoginName,
					((IDbLoginRepair)testAdminConnection).RestrictedWriterDbLoginName,
					((IDbLoginRepair)testAdminConnection).UnrestrictedWriterDbLoginName);
				testAdminConnection.ExecuteNonQuery(sqlText);

				using (var form = new TestMainForm())
				{
					form.RestoreDbLoginsMenuItem_Click(null, null);
				}

				DatabaseLoginTest.AssertLoginIsEnabled(testAdminConnection, ((IDbLoginRepair)testAdminConnection).ReaderDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).ReaderDbLoginName, expected: true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).ReaderDbLoginName, "db_datareader", true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).ReaderDbLoginName, "cwReaderRole", true);

				DatabaseLoginTest.AssertLoginIsEnabled(testAdminConnection, ((IDbLoginRepair)testAdminConnection).RestrictedReaderDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).RestrictedReaderDbLoginName, expected: true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).RestrictedReaderDbLoginName, "cwRestrictedReaderRole", true);

				DatabaseLoginTest.AssertLoginIsEnabled(testAdminConnection, ((IDbLoginRepair)testAdminConnection).RestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).RestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).RestrictedWriterDbLoginName, "db_owner", false);
				DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).RestrictedWriterDbLoginName, "cwRestrictedWriterRole", true);

				DatabaseLoginTest.AssertLoginIsEnabled(testAdminConnection, ((IDbLoginRepair)testAdminConnection).UnrestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertLoginIsMappedToDatabase(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).UnrestrictedWriterDbLoginName, expected: true);
				DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).UnrestrictedWriterDbLoginName, "db_datareader", false);
				DatabaseLoginTest.AssertUserIsMemberOfRole(testAdminConnection, testAdminConnection.CurrentDatabase, ((IDbLoginRepair)testAdminConnection).UnrestrictedWriterDbLoginName, "cwUnrestrictedWriterRole", true);
			}
		}

		#endregion

		#region Reset Always On Cache

		[RequiresSTA]
		public void TestResetAlwaysOnCache()
		{
			// Arrange
			var registry = EnvProxy.Instance.Registry;
			registry.UseAlwaysOnReplicaCache = true;
			registry.AlwaysOnReplicaCachedInfos = new[] { new AlwaysOnReplicaInfo { ReplicaServerName = "testReplicaServerName1", AvailabilityMode = 1 } };
			registry.AvailabilityGroupInfo = new[] { "testGroupInfo" };
			Assert(registry.AlwaysOnReplicaCachedInfos.Length == 1);
			Assert(registry.AvailabilityGroupInfo.Length == 1);

			using (var form = new TestMainForm())
			{
				// Act
				form.ResetAlwaysOnCacheMenuItem_Click(null, null);

				// Assert
				Assert(registry.AlwaysOnReplicaCachedInfos.Length == 0);
			}
		}

		#endregion

		#region IPositionSaveProvider

		[RequiresSTA]
		public void TestIPositionSaveProvider()
		{
			using (TestMainForm testForm = new TestMainForm())
			{
				IPositionSaveProvider positionSaveProvider = testForm;
				Assert(positionSaveProvider.RememberFormPosition);
				Assert(positionSaveProvider.RememberFormSize);
			}
		}

		#endregion IPositionSaveProvider

		#region IDatabaseAndStroageInfoProvider

		public void TestDatabaseAndStroageInfoProvider()
		{
			const string bucketName = "TestBucketName";
			const string serviceUrl = "  TestServiceUrl  ";
			const int timeout = 30;

			using (SystemDataRegistry.Instance.DocManagerStorageBucketName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, bucketName))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceUrl))
			using (SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeout))
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var provider = new DatabaseAndS3InfoProvider();
				AssertEquals(Db.ServerName, provider.ServerAlias);
				AssertEquals(Db.Connection.ServerNameReportedByDatabase, provider.ServerMachine);
				AssertEquals(Db.DatabaseName, provider.MainDbName);
				AssertEquals(Db.Connection.SPID, provider.MainConnectionSpid);
				AssertEquals(bucketName, provider.S3BucketName);
				AssertEquals(serviceUrl.Trim(), provider.S3BucketServiceUrl);
				AssertEquals(timeout, provider.S3BucketTimeout);

				var bucketSize = new DbGroupSize
				{
					DbGroup = DbGroupEnum.S3Bucket,
					UsedSizeMb = 0,
					DiskSizeMb = 0
				};
				var expectedDbGroupSizes = new DbSizeInfoCollection().GetDbGroupSizes().Concat(new[] { bucketSize });
				Assert(expectedDbGroupSizes.ContainsSameElementsInAnyOrder(
					provider.DbGroupSizes,
					(a, b) => a.DbGroup == b.DbGroup && a.DiskSizeMb == b.DiskSizeMb && a.UsedSizeMb == b.UsedSizeMb));
			}
		}

		public void TestDatabaseAndStroageInfoProvider_DbGroupSizesWithoutS3Info()
		{
			var provider = new DatabaseAndS3InfoProvider();
			var expectedDbGroupSizes = new DbSizeInfoCollection().GetDbGroupSizes();
			Assert(expectedDbGroupSizes.ContainsSameElementsInAnyOrder(
				provider.DbGroupSizes,
				(a, b) => a.DbGroup == b.DbGroup && a.DiskSizeMb == b.DiskSizeMb && a.UsedSizeMb == b.UsedSizeMb));
		}

		[RequiresSTA]
		public void TestDatabaseAndStorageInfoProvider_RetrieveBucketSize()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (var form = new TestMainForm())
			{
				var diagnosticsMenu = (ZToolStripMenuItem)form.HelpMenuButton.DropDownItems["Diagnostics"];
				AssertNotNull(diagnosticsMenu);

				var addressItem = diagnosticsMenu.DropDownItems["Database and S3 Storage Info"];
				AssertNotNull(addressItem);

				var persisterMock = new Mock<IExternalPersister>();
				persisterMock.Setup(x => x.GetBucketSizeInMb()).Returns(100);
				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				persisterProviderMock.Setup(x => x.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
				ObjectFactory.Substitute(persisterProviderMock.Object);

				addressItem.PerformClick();

				var provider = ObjectFactory.Get<IDatabaseAndS3InfoProvider>();
				Assert(string.IsNullOrEmpty(provider.S3BucketSizeError));
				AssertEquals(100, provider.DbGroupSizes.Last().UsedSizeMb);
				AssertEquals(100, provider.DbGroupSizes.Last().DiskSizeMb);
			}
		}

		[RequiresSTA]
		public void TestDatabaseAndStorageInfoProvider_RetrieveBucketSizeWithError()
		{
			using (DocumentScanning.Business.DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (var form = new TestMainForm())
			{
				var diagnosticsMenu = (ZToolStripMenuItem)form.HelpMenuButton.DropDownItems["Diagnostics"];
				AssertNotNull(diagnosticsMenu);

				var addressItem = diagnosticsMenu.DropDownItems["Database and S3 Storage Info"];
				AssertNotNull(addressItem);

				addressItem.PerformClick();

				var provider = ObjectFactory.Get<IDatabaseAndS3InfoProvider>();
				AssertEquals("There was a network issue while retrieving bucket size. Please try again later. If this issue persists, please contact your system administrator to check the configuration. Error message: Service URL can not be empty.", provider.S3BucketSizeError);
			}
		}

		#endregion

		#region Implementation

		TestMainForm MainForm => fMainForm ?? (fMainForm = new TestMainForm());
		TestMainForm fMainForm;

		GlbStaff NonDeveloperStaff
		{
			get
			{
				if (fNonDeveloperStaff == null)
				{
					fNonDeveloperStaff = Factory.NewWithValidTestData<GlbStaff>();
					NonDeveloperStaff.GS_LoginName = "nondeveloper";
				}
				return fNonDeveloperStaff;
			}
		}

		GlbStaff fNonDeveloperStaff;

		protected override void TearDown()
		{
			fMainForm?.Dispose();
			base.TearDown();
		}

		public override Form GetFormToBash()
		{
			return MainForm;
		}

		public override bool AllowUntranslatableFormTitle() => true;

		#endregion Implementation
	}
}
