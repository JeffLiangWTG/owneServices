using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class PlugInLayoutProvider : EU.GUI.PlugIn.IPlugInLayoutProvider
{
	public IPanelLayoutProvider GetInvoiceLinePreviousDocumentsDetailsLayout(EU.Business.Declaration.JobDeclaration declaration) => new PreviousDocumentFieldsLayout();

	public IPanelLayoutProvider GetInvoiceHeaderPreviousDocumentsDetailsLayout(EU.Business.Declaration.JobDeclaration declaration) => new PreviousDocumentFieldsLayout();

	public IPanelLayoutProvider GetDeclarationPreviousDocumentsDetailsLayout(EU.Business.Declaration.JobDeclaration declaration) => new PreviousDocumentFieldsLayout();

	public IPanelLayoutProvider GetEntryInstructionPreviousDocumentsDetailsLayout(EU.Business.Declaration.JobDeclaration declaration) => new PreviousDocumentFieldsLayout();
}
