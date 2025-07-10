using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStorageSupportingDocumentsDetailsLayoutControl : ZUserControl
	{
		public UCC6TemporaryStorageSupportingDocumentsDetailsLayoutControl()
		{
			InitializeComponent();
		}

		public void SetLayout(IPanelLayoutProvider layout)
		{
			DetailsPanel.UpdateLayout(layout);
		}
	}
}
