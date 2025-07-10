using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed partial class GoodsItemSupportingDocumentsGridUserControl : ZUserControl
{
	public GoodsItemSupportingDocumentsGridUserControl()
	{
		InitializeComponent();
		UpdateGridColumnLayout();
	}

	void UpdateGridColumnLayout()
	{
		SupportingDocumentsGrid.ApplyGridColumnLayout(new SupportingDocumentsGridColumnLayout());
	}
}
