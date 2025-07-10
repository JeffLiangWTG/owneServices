using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class CusTempStorageDecUserControl : ZUserControl
	{
		public CusTempStorageDecUserControl()
		{
			InitializeComponent();
			LabelCaptionRenderProvider.SetLabelCaptionVisible(GuaranteeDescriptionTextBox, false);
		}
	}
}
