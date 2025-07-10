using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	public class CustomJobConversationMessageUserControlTest : TestCaseWithFactory
	{
		public void TestSelectedItems()
		{
			var workitem1 = Factory.NewWithValidTestData<NewWorkItem>();
			var workitem2 = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();

			workitem1.Conversation.Messages.AddNew(null, "broadcast 1", false, false, true);
			workitem1.Conversation.Messages.AddNew(null, "no broadcast", false, false, false);

			workitem2.Conversation.Messages.AddNew(null, "broadcast 2", false, false, true);

			var jobConversationMessages = new List<JobConversationMessage>();
			jobConversationMessages.AddRange(workitem1.GetAllEConversationBroadcastMessages());
			jobConversationMessages.AddRange(workitem2.GetAllEConversationBroadcastMessages());

			using (var form = new ZForm())
			{
				form.Controls.Add(new CustomJobConversationMessageUserControl(jobConversationMessages));
				form.Show();

				var control = (CustomJobConversationMessageUserControl)form.Controls.Find("CustomJobConversationMessageUserControl", true)[0];
				AssertEquals(0, control.SelectedItems.Count());

				control.SelectAllForTest();
				AssertEquals(2, control.SelectedItems.Count());
			}
		}
	}
}
