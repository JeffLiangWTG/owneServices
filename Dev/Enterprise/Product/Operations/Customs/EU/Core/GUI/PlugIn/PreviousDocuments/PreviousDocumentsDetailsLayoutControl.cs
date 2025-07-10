using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class PreviousDocumentsDetailsLayoutControl : ZUserControl
	{
		public PreviousDocumentsDetailsLayoutControl()
		{
			InitializeComponent();
		}

		public void SetLayout(IPanelLayoutProvider layout)
		{
			DetailsPanel.UpdateLayout(layout);
		}
	}
}
