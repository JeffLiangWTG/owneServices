using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn;

public interface IPlugInLayoutProvider
{
	public IPanelLayoutProvider GetInvoiceHeaderPreviousDocumentsDetailsLayout(JobDeclaration declaration);

	public IPanelLayoutProvider GetInvoiceLinePreviousDocumentsDetailsLayout(JobDeclaration declaration);

	public IPanelLayoutProvider GetDeclarationPreviousDocumentsDetailsLayout(JobDeclaration declaration);

	public IPanelLayoutProvider GetEntryInstructionPreviousDocumentsDetailsLayout(JobDeclaration declaration);
}
