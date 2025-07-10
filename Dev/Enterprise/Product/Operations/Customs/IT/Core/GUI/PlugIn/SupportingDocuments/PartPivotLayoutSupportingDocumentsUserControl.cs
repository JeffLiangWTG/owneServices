using System.Collections.Generic;

namespace Enterprise.Customs.IT.GUI;

public partial class PartPivotLayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.LayoutSupportingDocumentsUserControl
{
	public PartPivotLayoutSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration)
		=> new PartPivotLayoutSupportingDocumentsFieldsControl();

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		new SupportingDocumentsGridInitializer(SupportingDocumentsGrid).Initialize();
	}

	protected override IReadOnlyList<string> AvailableColumnNames => new SupportingDocumentsGridColumnStylesHelper().GetColumnStylesForPartPivot();

	protected override string GetSupportingDocumentsFieldsControlBindingString() => "PivotsForBinding.SupportingDocuments";
}
