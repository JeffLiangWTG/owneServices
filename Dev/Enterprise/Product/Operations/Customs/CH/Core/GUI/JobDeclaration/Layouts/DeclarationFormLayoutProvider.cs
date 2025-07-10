using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
{
	public IPanelLayoutProvider GetDeclarationDetailsLayout() => null;

	public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayout();

	public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayout();

	public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => new TransportDetailsLayout();

	public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new OrganisationsLayout();

	public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => null;

	public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => null;

	public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => ((JobDeclaration)declaration).IsExportOrExportDeclarationActivation ? new ExportEntryInstructionDetailsLayout() : new ImportEntryInstructionDetailsLayout();

	public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
}
