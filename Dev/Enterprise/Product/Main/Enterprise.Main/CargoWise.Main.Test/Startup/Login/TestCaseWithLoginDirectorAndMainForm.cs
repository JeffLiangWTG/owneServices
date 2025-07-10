using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Startup.Testing;
using Enterprise.ZArchitecture.ActivityLogging;

namespace Enterprise.Startup.Login.Testing
{
	internal abstract class TestCaseWithLoginDirectorAndMainForm : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			loginDirectorTestInstance = LoginDirector.UseTestInstance();
			actualMainForm = StartupOpenMainFormTask.MainFormInstance;
			StartupOpenMainFormTask.MainFormInstance = testMainForm = new MainFormTestCase.TestMainForm();
			testMainForm.Show();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testMainForm.Dispose();
			loginDirectorTestInstance.Dispose();
			StartupOpenMainFormTask.MainFormInstance = actualMainForm;
			ZFormActivityLogger.Instance.DisableActivityLogger();
			base.TearDown();
		}

		MainForm actualMainForm;
		MainFormTestCase.TestMainForm testMainForm;
		IDisposable loginDirectorTestInstance;
	}
}
