using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
{
	public IPanelLayoutProvider GetDeclarationDetailsLayout() => null;

	public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayouts();

	public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => new TransportDetailsLayouts();

	public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new CommonOrganisationsLayouts();

	public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayouts();

	public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => new MiscOptionsLayouts();

	public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => declaration.IsExport ? new ExportInvoiceDetailsLayoutProvider() : new ImportInvoiceDetailsLayoutProvider();

	public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => declaration.IsExport
			? new ExportEntryInstructionDetailsLayout()
			: new ImportEntryInstructionDetailsLayout();

	public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
}
