using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutControl : ZUserControl
	{
		public UCC6TemporaryStoragePreviousDocumentsDetailsLayoutControl()
		{
			InitializeComponent();
		}

		public void SetLayout(IPanelLayoutProvider layout)
		{
			DetailsPanel.UpdateLayout(layout);
		}
	}
}
