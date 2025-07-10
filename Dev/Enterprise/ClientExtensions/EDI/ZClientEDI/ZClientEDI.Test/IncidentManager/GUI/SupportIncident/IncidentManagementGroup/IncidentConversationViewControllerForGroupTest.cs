using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class IncidentConversationViewControllerForGroupTest : IncidentConversationViewControllerTest
	{
		public void TestDataSource()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			using (var rootForm = new ZForm(incident))
			{
				var conversationArea = new IncidentManagementGroupControlCenterCommunicationAreaUserControl();
				rootForm.Controls.Add(conversationArea);
				conversationArea.SetDataBinding(incident, "");
				AssertEquals(incident, conversationArea.CurrentDataItem);
			}
		}
	}
}
