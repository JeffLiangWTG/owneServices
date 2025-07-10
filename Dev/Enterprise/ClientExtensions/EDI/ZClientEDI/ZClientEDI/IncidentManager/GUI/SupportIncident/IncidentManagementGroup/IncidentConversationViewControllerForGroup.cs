using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	internal class IncidentConversationViewControllerForGroup : IncidentConversationViewController
	{
		public IncidentConversationViewControllerForGroup(ZUserControl conversationContainer, bool warnUponSend = false) : base(warnUponSend)
		{
			ConversationContainer = conversationContainer;
		}

		public ZUserControl ConversationContainer { get; set; }

		protected override SupportIncident Incident => (SupportIncident)ConversationContainer.CurrentDataItem;
	}
}
