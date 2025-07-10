using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryDetailsUserControl : ZUserControl
	{
		public EntryDetailsUserControl()
		{
			InitializeComponent();
		}

		public void SetEntryLineDetailsLayout(IPanelLayoutProvider layout)
		{
			DynamicEntryLineDetailsPanel.UpdateLayout(layout);
		}
	}
}
