using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class CustomJobConversationMessageUserControl : ZUserControl
	{
		public CustomJobConversationMessageUserControl(List<JobConversationMessage> jobConversationMessages)
			: base()
		{
			InitializeComponent();

			var collection = new CustomJobConversationMessageCollection(new BusinessObjectFactory());
			foreach (var item in jobConversationMessages)
			{
				collection.Add(new CustomJobConversationMessage(item));
			}
			SetDataBinding(collection, "");
		}

		public IEnumerable<CustomJobConversationMessage> SelectedItems
				=> ((CustomJobConversationMessageCollection)ConversationMessageGrid.ListManager.List).Cast<CustomJobConversationMessage>().Where(j => j.IsChecked);

		internal void SelectAllForTest()
		{
			((CustomJobConversationMessageCollection)ConversationMessageGrid.ListManager.List).Cast<CustomJobConversationMessage>().ForEach(c => c.IsChecked = true);
		}
	}
}
