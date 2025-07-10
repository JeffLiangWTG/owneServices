using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
	{
		public IPanelLayoutProvider GetDeclarationDetailsLayout() => null;

		public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayout();

		public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => new ShipmentDetailsLayout();

		public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => null;

		public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => null;

		public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => null;

		public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => new CommercialInvoiceDetailsLayout();

		public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => new EntryInstructionDetailsLayout();

		public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
	}
}
