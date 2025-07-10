using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed partial class HouseConsignmentSupportingDocumentsGridUserControl : ZUserControl
{
	public HouseConsignmentSupportingDocumentsGridUserControl()
	{
		InitializeComponent();
		UpdateGridColumnLayout();
	}

	void UpdateGridColumnLayout()
	{
		SupportingDocumentsGrid.ApplyGridColumnLayout(new SupportingDocumentsGridColumnLayout());
	}
}
