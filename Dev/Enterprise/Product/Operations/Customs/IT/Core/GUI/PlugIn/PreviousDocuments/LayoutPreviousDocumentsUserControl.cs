using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class LayoutPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl
{
	public LayoutPreviousDocumentsUserControl()
	{
		InitializeComponent();
	}

	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new PreviousDocumentGridColumnLayout();
}
