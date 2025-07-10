using System;
using CargoWise.BrandManager;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class StartupOpenMainFormTaskTest : AbstractApplicationStartupTaskTest<StartupOpenMainFormTask>
	{
		public void TestShouldExecute()
		{
			var arguments = new ApplicationArguments(Array.Empty<string>());
			var task = new StartupOpenMainFormTask();
			LoginDirector.Instance.HideUI = true;
			Assert("Should not run if UI is hidden", !task.ShouldExecute(arguments));

			LoginDirector.Instance.HideUI = false;
			Assert("Should show main form if UI is not hidden", task.ShouldExecute(arguments));
		}

		[RequiresSTA]
		public void TestNoAuthenticatedUser()
		{
			new StartupOpenMainFormTask().Execute(new ApplicationArguments(Array.Empty<string>()));
			AssertNotNull(StartupOpenMainFormTask.MainFormInstance);
			AssertEquals(1, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
			AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true).Length);
			AssertNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
		}

		[RequiresSTA]
		public void TestAuthenticatedNoLocation()
		{
			LoginDirector.Instance.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser);
			new StartupOpenMainFormTask().Execute(new ApplicationArguments(Array.Empty<string>()));
			AssertNotNull(StartupOpenMainFormTask.MainFormInstance);
			AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
			AssertEquals(1, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true).Length);
			AssertNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
		}

		[RequiresSTA]
		public void TestLoggedIn()
		{
			LoginDirector.Instance.LoginAutomatically();
			try
			{
				new StartupOpenMainFormTask().Execute(new ApplicationArguments(Array.Empty<string>()));
				AssertNotNull(StartupOpenMainFormTask.MainFormInstance);
				AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
				AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true).Length);
				AssertNotNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
			}
			finally
			{
				Env.LoginController.Logout();
			}
		}

		[RequiresSTA]
		public void TestMainFormHasCWNextLabelWhenCWNextEnalbedInFeatureControl()
		{
			LoginDirector.Instance.LoginAutomatically();
			try
			{
				using (CWNextFeatureTestHelper.EnableCWNext())
				{
					BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseNext);
					new StartupOpenMainFormTask().Execute(new ApplicationArguments(Array.Empty<string>()));
					AssertNotNull(StartupOpenMainFormTask.MainFormInstance);
					AssertEquals(true, StartupOpenMainFormTask.MainFormInstance.IsCWNext);
					AssertContains("CargoWise Next", StartupOpenMainFormTask.MainFormInstance.Text);
				}
			}
			finally
			{
				Env.LoginController.Logout();
			}
		}

		protected override void SetUp()
		{
			loginDirectorTestInstance = LoginDirector.UseTestInstance();
			originalMainForm = StartupOpenMainFormTask.MainFormInstance;
			base.SetUp();
		}

		protected override void TearDown()
		{
			loginDirectorTestInstance.Dispose();
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			if (mainForm != originalMainForm)
			{
				mainForm.Dispose();
				StartupOpenMainFormTask.MainFormInstance = originalMainForm;
			}

			ZFormActivityLogger.Instance.DisableActivityLogger();
			base.TearDown();
		}

		MainForm originalMainForm;
		IDisposable loginDirectorTestInstance;

		public override int DefaultErrorExitCode => ExitCodes.StartupOpenMainFormTaskError;
	}
}
