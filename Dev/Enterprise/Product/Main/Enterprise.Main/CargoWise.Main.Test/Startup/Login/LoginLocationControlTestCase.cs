using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Startup.Login.Testing
{
	sealed class LoginLocationControlTestCase : TestCaseWithLoginDirectorAndMainForm
	{
		[RequiresSTA]
		public void TestValidationError()
		{
			LoginDirector.Instance.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser);
			var control = ShowLoginLocationControl();
			control.loginLocationBusinessObject.BranchCode = "XXX";
			GetLoginButton(control).PerformClick();
			AssertEquals(control, FindLoginLocationControl());
			AssertEquals("Error - Enter a valid Branch.", control.errorLabel.Text);
			control.loginLocationBusinessObject.BranchCode = Env.CurrentBranch.Code;
			AssertEquals("", control.errorLabel.Text);
			control.loginLocationBusinessObject.DepartmentCode = "XXX";
			GetLoginButton(control).PerformClick();
			AssertEquals(control, FindLoginLocationControl());
			AssertEquals("Error - Enter a valid Department.", control.errorLabel.Text);
			control.loginLocationBusinessObject.DepartmentCode = Env.CurrentDepartment.Code;
			AssertEquals("", control.errorLabel.Text);
		}

		[RequiresSTA]
		public void TestLoginError()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = Env.CurrentCompany.PK;
			GlbSecurity loginSecurity = Factory.New<GlbSecurity>();
			loginSecurity.GU_GB = branch.PK;
			loginSecurity.GU_GE = Env.CurrentDepartment.PK;
			loginSecurity.GU_GS = staff.PK;
			loginSecurity.GU_SecurityRight = Env.Security.Login.Code;
			loginSecurity.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			LoginDirector.Instance.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff);
			var control = ShowLoginLocationControl();
			control.loginLocationBusinessObject.BranchCode = branch.GB_Code;
			GetLoginButton(control).PerformClick();
			AssertEquals(control, FindLoginLocationControl());
			AssertStartsWith("error", "You do not have the security right to login to this branch or department.", control.errorLabel.Text);
			control.loginLocationBusinessObject.BranchCode = Env.CurrentBranch.Code;
			AssertEquals("", control.errorLabel.Text);
		}

		[RequiresSTA]
		public void TestLogin()
		{
			try
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = Env.CurrentCompany.PK;
				var department = Factory.NewWithValidTestData<GlbDepartment>();

				var security1 = Factory.NewWithValidTestData<GlbSecurity>();
				security1.GU_GS = staff.PK;
				security1.GU_SecurityItemIsAllowed = true;
				security1.GU_SecurityRight = "Login";

				Factory.Save();
				LoginDirector.Instance.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff);
				var control = ShowLoginLocationControl();
				control.loginLocationBusinessObject.BranchCode = branch.GB_Code;
				control.loginLocationBusinessObject.DepartmentCode = department.GE_Code;
				GetLoginButton(control).PerformClick();

				AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true).Length);
				AssertNotNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
				AssertEquals(staff.PK, Env.CurrentUser.PK);
				AssertEquals(branch.PK, Env.CurrentBranch.PK);
				AssertEquals(department.PK, Env.CurrentDepartment.PK);
			}
			finally
			{
				Env.LoginController.Logout();
			}
		}

		#region Implementation

		LoginLocationControl ShowLoginLocationControl()
		{
			StartupOpenMainFormTask.MainFormInstance.ShowLoginLocationControl();
			return FindLoginLocationControl();
		}

		LoginLocationControl FindLoginLocationControl()
		{
			var controls = StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true);
			AssertEquals(1, controls.Length);
			AssertType(typeof(LoginLocationControl), controls[0]);
			return (LoginLocationControl)controls[0];
		}

		ZToolStripButton GetLoginButton(Control control)
		{
			var toolStrip = (ZToolStrip)control.Controls.Find("toolStrip", true)[0];
			return (ZToolStripButton)toolStrip.Items.Find("LoginButton", true)[0];
		}

		#endregion
	}
}
