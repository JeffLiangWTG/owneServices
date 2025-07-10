using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.GUI;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class IncidentConversationViewControllerForManagementGroupTest : IncidentConversationViewControllerTest
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

		public void TestRefresh()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.EConversation.Conversation.AddMessageFromCurrentUser("aaa", false);

			using (var rootForm = new ZForm(incident))
			{
				var conversationArea = new IncidentManagementGroupControlCenterCommunicationAreaUserControl();
				rootForm.Controls.Add(conversationArea);
				conversationArea.SetDataBinding(incident, "");

				rootForm.Show();
				var messageContainer = conversationArea.Controls.Find("eConversationMessageListUserControl1", true)[0] as EConversationMessageListUserControl;
				AssertNotNull(messageContainer);

				var messageBubbles = messageContainer.messagesLayoutPanel.Controls.ToList<Control>();
				conversationArea.Refresh();

				Assert($"The old bubbles should be disposed: {string.Join(";",messageBubbles.Where(x => !x.IsDisposed).Select(y => y.Name))}", messageBubbles.All(x => x.IsDisposed));
			}
		}
	}
}
