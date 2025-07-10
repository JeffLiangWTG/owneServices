using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class LoginPasswordListEditControlForRegistryTest : TestCaseWithFactory
	{
		public void TestCanShowPasswords_Developer()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "abc";
			user.GS_Code = "abc";
			user.GS_GB_HomeBranch = EnvProxy.Instance.CurrentBranch.PK;
			user.GS_IsDeveloper = true;
			user.GS_IsController = false;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.PK.ToGuid(), EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var loginPasswordControl = new LoginPasswordListEditControlForRegistryForTest())
				{
					Assert("Show passwords button should be enabled", loginPasswordControl.ButtonShowPasswordsExposedForTest.Enabled);

					loginPasswordControl.IsValidPasswordForTest = false;
					Assert("Should not allow to show passwords", !loginPasswordControl.CanShowPasswords());

					loginPasswordControl.IsValidPasswordForTest = true;
					Assert("Should allow to show passwords", loginPasswordControl.CanShowPasswords());
				}
			}
		}

		public void TestCanShowPasswords_Controller()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "abc";
			user.GS_Code = "abc";
			user.GS_GB_HomeBranch = EnvProxy.Instance.CurrentBranch.PK;
			user.GS_IsDeveloper = false;
			user.GS_IsController = true;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.PK.ToGuid(), EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var loginPasswordControl = new LoginPasswordListEditControlForRegistryForTest())
				{
					Assert("Show passwords button should be enabled", loginPasswordControl.ButtonShowPasswordsExposedForTest.Enabled);
					Assert("Should allow to show passwords", loginPasswordControl.CanShowPasswords());
				}
			}
		}

		public void TestCanShowPasswords_NormalUser()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "abc";
			user.GS_Code = "abc";
			user.GS_GB_HomeBranch = EnvProxy.Instance.CurrentBranch.PK;
			user.GS_IsDeveloper = false;
			user.GS_IsController = false;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.PK.ToGuid(), EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var loginPasswordControl = new LoginPasswordListEditControlForRegistryForTest())
				{
					Assert("Show passwords button should be disabled", !loginPasswordControl.ButtonShowPasswordsExposedForTest.Enabled);
					Assert("Should not allow to show passwords", !loginPasswordControl.CanShowPasswords());
				}
			}
		}

		public void TestShowHidePasswords()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "abc";
			user.GS_Code = "abc";
			user.GS_GB_HomeBranch = EnvProxy.Instance.CurrentBranch.PK;
			user.GS_IsDeveloper = false;
			user.GS_IsController = true;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.PK.ToGuid(), EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var loginPasswordControl = new LoginPasswordListEditControlForRegistryForTest())
				{
					Assert("Show passwords button should be enabled", loginPasswordControl.ButtonShowPasswordsExposedForTest.Enabled);
					Assert("Should allow to show passwords", loginPasswordControl.CanShowPasswords());

					AssertEquals("Should show password symbol", true, loginPasswordControl.DescriptionColumnStyleExposedForTest.TextBox.UseSystemPasswordChar);
					AssertEquals("Show Passwords", loginPasswordControl.ButtonShowPasswordsExposedForTest.Text);

					loginPasswordControl.ButtonShowPasswordsExposedForTest.PerformClick();

					AssertEquals("Should show password clear text", false, loginPasswordControl.DescriptionColumnStyleExposedForTest.TextBox.UseSystemPasswordChar);
					AssertEquals("Hide Passwords", loginPasswordControl.ButtonShowPasswordsExposedForTest.Text);

					loginPasswordControl.ButtonShowPasswordsExposedForTest.PerformClick();

					AssertEquals("Should show password symbol", true, loginPasswordControl.DescriptionColumnStyleExposedForTest.TextBox.UseSystemPasswordChar);
					AssertEquals("Show Passwords", loginPasswordControl.ButtonShowPasswordsExposedForTest.Text);
				}
			}
		}
	}
}
