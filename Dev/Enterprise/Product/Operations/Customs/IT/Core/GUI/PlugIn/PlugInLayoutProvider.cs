using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class PlugInLayoutProvider : IPlugInLayoutProvider
{
	public IPanelLayoutProvider GetInvoiceLinePreviousDocumentsDetailsLayout(JobDeclaration declaration) => declaration switch
	{
		not null when declaration.IsImport => new InvoiceLineImportPreviousDocumentFieldsLayout(),
		not null when declaration.IsUCC6AndIsExport => new Ucc6ExportPreviousDocumentFieldsLayout(),
		_ => new PreviousDocumentFieldsLayout()
	};

	public IPanelLayoutProvider GetInvoiceHeaderPreviousDocumentsDetailsLayout(JobDeclaration declaration) => declaration switch
	{
		not null when declaration.IsImport => new ImportPreviousDocumentFieldsLayout(),
		_ => new PreviousDocumentFieldsLayout()
	};

	public IPanelLayoutProvider GetEntryInstructionPreviousDocumentsDetailsLayout(JobDeclaration declaration) => declaration switch
	{
		not null when declaration.IsUCC6AndIsExport => new Ucc6ExportEntryInstructionPreviousDocumentFieldsLayout(),
		_ => new EntryInstructionPreviousDocumentFieldsLayout()
	};

	public IPanelLayoutProvider GetDeclarationPreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentFieldsLayout();
}
