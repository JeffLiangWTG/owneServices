using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class PartPivotLayoutSupportingDocumentsFieldsControl : EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl
{
	public PartPivotLayoutSupportingDocumentsFieldsControl()
	{
		InitializeComponent();
	}

	protected override IPanelLayoutProvider GetLayout() => new PartPivotSupportingDocumentFieldsLayout();
}
