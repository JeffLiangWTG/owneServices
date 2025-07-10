using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.EConversation.GUI
{
	public partial class AddInternalLogForm : ZChildForm
	{
		readonly List<SubscriberWrapper> participantsReferenced = new List<SubscriberWrapper>();

		public AddInternalLogForm(IAutoCompleteField autocompleteManager)
		{
			InitializeComponent();

			messageTextbox.ItemSelected += (o, e) => participantsReferenced.Add((SubscriberWrapper)e.SelectedItem);
			messageTextbox.AutocompleteManager = autocompleteManager;
			messageTextbox.TextChanged += (o, e) => okButton.Enabled = !string.IsNullOrWhiteSpace(messageTextbox.Text);
			messageTextbox.Hotkeys.RegisterHotKey(Keys.Control | Keys.Enter, () =>
			{
				if (okButton.Enabled)
				{
					okButton.PerformClick();
				}
			}, Res.GetString("c6e02b78-77ed-40ba-9ae2-71cb9986b60d", "Send message"));
		}

		public IEnumerable<SubscriberWrapper> ParticipantsReferenced => participantsReferenced;
		public string MessageText => messageTextbox.Text;
	}
}
