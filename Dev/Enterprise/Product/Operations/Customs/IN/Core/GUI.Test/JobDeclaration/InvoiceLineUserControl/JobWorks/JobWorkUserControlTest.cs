using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(JobWorkUserControl))]
sealed class JobWorkUserControlTest : TestCase
{
	public void TestControlVisibility()
	{
		CombineAssertions(() =>
		{
			using (var control = new JobWorkUserControl())
			{
				AssertEquals("JobWorkGrid should be visible", true, control.JobWorkGrid.Visible);
				AssertEquals("JobWorkNotificationNoTextBox should be visible", true, control.JobWorkNotificationNoTextBox.Visible);
			}
		});
	}
}
