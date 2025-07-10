using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class LayoutPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl
{
	public LayoutPreviousDocumentsUserControl()
	{
		InitializeComponent();
	}

	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new PreviousDocumentsGridColumnLayout();
}
