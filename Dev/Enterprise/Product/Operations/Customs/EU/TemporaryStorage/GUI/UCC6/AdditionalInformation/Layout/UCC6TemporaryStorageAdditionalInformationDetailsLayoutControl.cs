using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStorageAdditionalInformationDetailsLayoutControl : ZUserControl
	{
		public UCC6TemporaryStorageAdditionalInformationDetailsLayoutControl()
		{
			InitializeComponent();
		}

		public void SetLayout(IPanelLayoutProvider layout)
		{
			DetailsPanel.UpdateLayout(layout);
		}
	}
}
