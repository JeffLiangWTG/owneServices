using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class EDIWorkflowTabPageForTest : EDIWorkflowTabPage
	{
		public ZWorkflowUserControl TrackingUserControlForTest
		{
			get
			{
				return TrackingUserControl;
			}
		}
	}
}
