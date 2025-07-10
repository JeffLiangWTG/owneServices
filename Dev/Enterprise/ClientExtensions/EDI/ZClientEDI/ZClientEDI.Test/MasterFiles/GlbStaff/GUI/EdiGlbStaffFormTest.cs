using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EdiGlbStaffForm))]
	public class EdiGlbStaffFormTest : ZFormBasherTest
	{
		public void TestNameFields()
		{
			using (var form = GetFormToBash())
			{
				var friendlyNameBoundText = form.Controls.Find("GS_FriendlyNameBoundText", true);
				AssertEquals(0, friendlyNameBoundText.Length);
			}
		}

		[GuiTest]
		public override void TestMinimumSizeNotTooBig()
		{
			using (var testForm = GetFormToBash())
			{
				const int MinScreenWidthSupported = 1366;
				const int MinScreenHeightSupported = 811;
				const int TypicalTaskbarHeight = 43;
				int maxSizeWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported);
				int maxSizeHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported - TypicalTaskbarHeight);
				Assert("Form min size too wide (" + testForm.MinimumSize.Width + ") for the screen. Should be less than or equal to " + maxSizeWidth, testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height + ") for the screen. Should be less than or equal to " + maxSizeHeight, testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		[GuiTest]
		public void TestReadonlyByDefault()
		{
			//change
			using (var testForm = GetFormToBash())
			{
				testForm.Show();
				Application.DoEvents();

				var homeDepartmentFindBox = testForm.Controls.Find("HomeDepartmentFindBox", true).First() as ZGuidFindBox;
				var homeBranchFindBox = testForm.Controls.Find("HomeBranchGuidFindBox", true).First() as ZGuidFindBox;
				var titleTextBox = testForm.Controls.Find("GS_TitleTextBox", true).First() as ZTextBox;

				CombineAssertions(() =>
				{
					Assert("homeDepartmentFindBox", !homeDepartmentFindBox.Enabled);
					Assert("homeBranchFindBox", !homeBranchFindBox.Enabled);
					Assert("titleTextBox", !titleTextBox.Enabled);
				});
			}
		}

		[GuiTest]
		public void TestShowPasswordControlsWhenCurrentUserIsHrAndBusinessEntityIsNotController()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "HRGROUPUSER";
			var hrStaff = Factory.NewWithValidTestData<EDIGlbStaff>();
			hrStaff.Groups.Add(group);

			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_IsController = false;

			Factory.Save();

			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOIDCConfig()))
			using (Env.SetTemporaryUserContext(new UserContext(hrStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var testForm = new EdiGlbStaffForm(staff))
			{
				testForm.Show();

				testForm.GlbStaffTabControl.SelectedTab = testForm.PasswordTabPage;

				AssertEquals("ChangePasswordButton", true, testForm.GetControl<ZButton>("ChangePasswordButton", true).Visible);
				AssertEquals("PasswordOptionsGroupBox", true, testForm.GetControl<ZGroupBox>("PasswordOptionsGroupBox", true).Visible);
				AssertEquals("PasswordTabPage", "Password and Signature", testForm.GetControl<ZTabPage>("PasswordTabPage", true).Text);
			}
		}

		[GuiTest]
		public void TestHidePasswordControlsWhenCurrentUserIsHrAndBusinessEntityIsController()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "HRGROUPUSER";
			var hrStaff = Factory.NewWithValidTestData<EDIGlbStaff>();
			hrStaff.Groups.Add(group);

			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_IsController = true;

			Factory.Save();

			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOIDCConfig()))
			using (Env.SetTemporaryUserContext(new UserContext(hrStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var testForm = new EdiGlbStaffForm(staff))
			{
				testForm.Show();

				testForm.GlbStaffTabControl.SelectedTab = testForm.PasswordTabPage;

				AssertEquals("ChangePasswordButton", false, testForm.GetControl<ZButton>("ChangePasswordButton", true).Visible);
				AssertEquals("PasswordOptionsGroupBox", false, testForm.GetControl<ZGroupBox>("PasswordOptionsGroupBox", true).Visible);
				AssertEquals("PasswordTabPage", "Signature", testForm.GetControl<ZTabPage>("PasswordTabPage", true).Text);
			}
		}

		[GuiTest]
		public void TestShowPasswordControlsWhenOIDCIsDisableInEdi()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "HRGROUPUSER";
			var hrStaff = Factory.NewWithValidTestData<EDIGlbStaff>();
			hrStaff.Groups.Add(group);

			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_IsController = true;

			Factory.Save();

			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetOIDCConfig(false)))
			using (Env.SetTemporaryUserContext(new UserContext(hrStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var testForm = new EdiGlbStaffForm(staff))
			{
				testForm.Show();

				testForm.GlbStaffTabControl.SelectedTab = testForm.PasswordTabPage;

				AssertEquals("ChangePasswordButton", true, testForm.GetControl<ZButton>("ChangePasswordButton", true).Visible);
				AssertEquals("PasswordOptionsGroupBox", true, testForm.GetControl<ZGroupBox>("PasswordOptionsGroupBox", true).Visible);
				AssertEquals("PasswordTabPage", "Password and Signature", testForm.GetControl<ZTabPage>("PasswordTabPage", true).Text);
			}
		}

		[GuiTest]
		public void TestSendGitHubInviteActionMenu()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				var sendGitHubInviteMenu = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Send GitHub Invite");
				AssertNotNull(sendGitHubInviteMenu);
				Assert(sendGitHubInviteMenu.Visible);
			}
		}

		[GuiTest]
		public void TestSendGitHubInviteActionMenuIsBehindSecurityCheckPoint()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			var otherStaff = Factory.NewWithValidTestData<EDIGlbStaff>();

			// other staff without permission can't send invite
			using (Env.SetTemporaryUserContext(new UserContext(otherStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				EDISecurityCheckpoints.SendGitHubInviteForOthers.IsAllowed = false;
				using (var form = new EdiGlbStaffForm(staff))
				{
					form.Show();

					var sendGitHubInviteMenu = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Send GitHub Invite");
					Assert(!Env.CurrentUser.IsController);
					AssertNull("There should be no menu to Send GitHub Invite if the form is open for other staff and current user does not have SendGitHubInviteForOthers, SendGitHubInvite menu", sendGitHubInviteMenu);
				}
			}

			// other staff with permission can send invite
			using (Env.SetTemporaryUserContext(new UserContext(otherStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				EDISecurityCheckpoints.SendGitHubInviteForOthers.IsAllowed = true;
				using (var form = new EdiGlbStaffForm(staff))
				{
					form.Show();
					var sendGitHubInviteMenu = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Send GitHub Invite");
					AssertNotNull("There should be menu to Send GitHub Invite if the form is open for other staff and current user has SendGitHubInviteForOthers, SendGitHubInvite menu", sendGitHubInviteMenu);
					Assert(sendGitHubInviteMenu.Visible);
				}
			}

			// staff can send invite to self regardless of permission
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				EDISecurityCheckpoints.SendGitHubInviteForOthers.IsAllowed = false;
				using (var form = new EdiGlbStaffForm(staff))
				{
					form.Show();

					var sendGitHubInviteMenu = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Send GitHub Invite");

					AssertNotNull("There should be menu to Send GitHub Invite for self regardless of the permission SendGitHubInviteForOthers, SendGitHubInvite menu", sendGitHubInviteMenu);
					Assert(sendGitHubInviteMenu.Visible);
				}
			}
		}

		OIDCConfig GetOIDCConfig(bool isOIDCEnabled = true)
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = isOIDCEnabled,
				OIDCServerType = OIDCServerTypes.Azure,
				AuthorityURL = AuthorityUrl,
				ClientIdentifier = ClientId,
			};
			oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "testname", Identifier = "GlbStaff.GS_LoginName" });
			oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
			oidcConfig.IsVerified = true;
			return oidcConfig;
		}

		protected override Form GetFormToBashCore()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			Factory.Save();
			return new EdiGlbStaffForm(staff);
		}

		const string AuthorityUrl = "https://www.example.com";

		const string ClientId = "46546646-e627-46fb-afe4-e5ea9928740e";
	}
}
