using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed partial class DeclarationSupportingDocumentsGridUserControl : ZUserControl
{
	public DeclarationSupportingDocumentsGridUserControl()
	{
		InitializeComponent();
		UpdateGridColumnLayout();
	}

	void UpdateGridColumnLayout()
	{
		SupportingDocumentsGrid.ApplyGridColumnLayout(new SupportingDocumentsGridColumnLayout());
	}
}
