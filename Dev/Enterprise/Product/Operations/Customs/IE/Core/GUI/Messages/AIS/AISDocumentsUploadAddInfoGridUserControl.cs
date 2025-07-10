using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class AISDocumentsUploadAddInfoGridUserControl : ZUserControl
	{
		public AISDocumentsUploadAddInfoGridUserControl()
		{
			InitializeComponent();
			SetColumnVisibility();
		}

		void SetColumnVisibility()
		{
			if (!EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.Value)
			{
				var ccQualifier = AddInfosIM483Grid.GetColumnStyle(nameof(AdditionalInfoSendingObject.CCQualifier));
				AddInfosIM483Grid.ColumnStyles.Remove(ccQualifier);
			}
		}
	}
}
