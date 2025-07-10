using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class EDIWorkflowTabPageTest : TestCaseWithFactory
	{
		public void TestTrackingUserControl()
		{
			using (EDIWorkflowTabPageForTest tabPage = new EDIWorkflowTabPageForTest())
			{
				Assert("TrackingUserControl is of type EDIWorkflowUserControl", tabPage.TrackingUserControlForTest as EDIWorkflowUserControl != null);
			}
		}
	}
}
