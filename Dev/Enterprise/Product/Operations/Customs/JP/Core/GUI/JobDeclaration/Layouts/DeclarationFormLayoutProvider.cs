using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
	{
		public IPanelLayoutProvider GetDeclarationDetailsLayout() => null;

		public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration) => new JobComInvoiceHeaderLayouts();

		public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new OrganizationsLayouts();

		public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => null;

		public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => null;

		public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => null;

		public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => new MiscOptionsLayouts();

		public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => null;

		public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
	}
}
