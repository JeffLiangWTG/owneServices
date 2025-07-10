using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class CusTempStorageRegLineItemDetailsLayoutControl : ZUserControl
	{
		public CusTempStorageRegLineItemDetailsLayoutControl()
		{
			InitializeComponent();
		}

		public void SetLayout(IPanelLayoutProvider layout)
		{
			ItemsPanel.UpdateLayout(layout);
		}
	}
}
