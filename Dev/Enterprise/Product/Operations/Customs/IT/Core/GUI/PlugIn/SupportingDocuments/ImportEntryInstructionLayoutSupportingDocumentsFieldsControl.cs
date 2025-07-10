using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ImportEntryInstructionLayoutSupportingDocumentsFieldsControl : EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl
{
	public ImportEntryInstructionLayoutSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
		InitializeComponent();
	}

	protected override IPanelLayoutProvider GetLayout() => new ImportEntryInstructionSupportingDocumentFieldsLayout();
}
