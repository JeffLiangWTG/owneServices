using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ExportEntryInstructionLayoutSupportingDocumentsFieldsControl : EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl
{
	public ExportEntryInstructionLayoutSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
		InitializeComponent();
	}

	protected override IPanelLayoutProvider GetLayout() => new ExportEntryInstructionSupportingDocumentFieldsLayout();
}
