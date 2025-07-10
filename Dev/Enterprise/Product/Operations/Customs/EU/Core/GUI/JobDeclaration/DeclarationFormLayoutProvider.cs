using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class DeclarationFormLayoutProvider : Customs.GUI.IDeclarationFormLayoutProvider
	{
		public IPanelLayoutProvider GetDeclarationDetailsLayout() => null;

		public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayouts();

		public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayouts();

		public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => null;

		public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new OrganisationsLayout();

		public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => new MiscOptionsLayouts();

		public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => declaration != null && declaration.IsImport ? new ImportInvoiceDetailsLayout() : new ExportInvoiceDetailsLayout();

		public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => new EntryInstructionBasicDetailsLayoutProvider();

		public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
	}
}
