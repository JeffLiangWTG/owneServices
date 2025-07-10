using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class ISTCusTempStorageDecUserControl : ZUserControl
	{
		public ISTCusTempStorageDecUserControl()
		{
			InitializeComponent();
			LabelCaptionRenderProvider.SetLabelCaptionVisible(GuaranteeDescriptionTextBox, false);
			//SupportingDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitSupportingDocumentsUserControl());
		}
	}
}
