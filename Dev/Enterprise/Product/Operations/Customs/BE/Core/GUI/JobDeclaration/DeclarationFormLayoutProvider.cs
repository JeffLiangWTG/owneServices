using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
{
	public IPanelLayoutProvider GetDeclarationDetailsLayout() => null;

	public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayout();

	public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayouts();

	public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => new TransportDetailsLayout();

	public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new OrganisationsLayout();

	public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => new MiscOptionsLayouts();

	public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => declaration.IsExport ? new ExportInvoiceDetailsLayout() : new ImportInvoiceDetailsLayout();

	public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => new EntryInstructionDetailsBasicUserControlLayout();

	public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

	public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
}
