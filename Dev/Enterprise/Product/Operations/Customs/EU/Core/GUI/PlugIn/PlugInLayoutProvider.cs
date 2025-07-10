using CargoWise.Application;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn;

public sealed class PlugInLayoutProvider : IPlugInLayoutProvider
{
	public static IPlugInLayoutProvider GetLayoutProvider(string countryOrGroupingCode)
	{
		var providers = ObjectFactory.Get<System.Collections.Hashtable>("PlugInLayoutProviders");
		return string.IsNullOrEmpty(countryOrGroupingCode)
			? GetProvider()
			: GetProvider(countryOrGroupingCode) ?? GetProvider();

		IPlugInLayoutProvider GetProvider(string key = "Default") => (IPlugInLayoutProvider)((ObjectHandle)providers[key])?.GetObject();
	}

	public IPanelLayoutProvider GetInvoiceHeaderPreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentFieldsLayout();

	public IPanelLayoutProvider GetInvoiceLinePreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentFieldsLayout();

	public IPanelLayoutProvider GetDeclarationPreviousDocumentsDetailsLayout(JobDeclaration declaration) => new PreviousDocumentFieldsLayout();

	public IPanelLayoutProvider GetEntryInstructionPreviousDocumentsDetailsLayout(JobDeclaration declaration)
		=> IsUCC6ImportOrExport(declaration) ? new UCC6PreviousDocumentFieldsLayout() : new PreviousDocumentFieldsLayout();

	bool IsUCC6ImportOrExport(JobDeclaration declaration) => declaration is not null && declaration.IsUCC6 && (declaration.IsImport || declaration.IsExport);
}
