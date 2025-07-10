using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class SupportingDocumentGridControl : ZUserControl
{
	public SupportingDocumentGridControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	void InitializeGridLayout()
	{
		var gridLayout = new SupportingDocumentGridColumnsLayout();
		SupportingDocumentGrid.ApplyGridColumnLayout(gridLayout);
	}
}
