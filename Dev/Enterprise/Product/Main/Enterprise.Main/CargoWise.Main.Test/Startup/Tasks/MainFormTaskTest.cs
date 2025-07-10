using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class MainFormTaskTest : TestCase
	{
		public void TestShouldExecuteInnerTaskWhenMainFormIsVisibleOnly()
		{
			var mainFormTask = new MainFormTask<ConfigurationItemChecker>();
			var actualMainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = null;
				AssertEquals("Should execute as Hide UI argument specified and main form is visible", false, mainFormTask.ShouldExecute());

				using (var mainForm = new MainForm())
				{
					StartupOpenMainFormTask.MainFormInstance = mainForm;
					AssertEquals("Should wrap inner task description", new ConfigurationItemChecker().TaskDescription, mainFormTask.TaskDescription);
					AssertEquals(true, mainFormTask.ShouldExecute());
				}
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance = actualMainForm;
			}
		}
	}
}
