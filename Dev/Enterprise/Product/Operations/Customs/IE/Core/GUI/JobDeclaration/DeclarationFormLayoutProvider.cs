using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class DeclarationFormLayoutProvider : IDeclarationFormLayoutProvider
	{
		public IPanelLayoutProvider GetDeclarationDetailsLayout() => null;

		public IPanelLayoutProvider GetDeclarationShipmentTypeLayout() => new ShipmentTypeLayout();

		public IPanelLayoutProvider GetDeclarationShipmentDetailsLayout() => null;

		public IPanelLayoutProvider GetDeclarationTransportDetailsLayout(BaseJobDeclaration declaration) => new TransportDetailsLayout();

		public IPanelLayoutProvider GetDeclarationOrganisationsLayout() => new OrganisationsLayout();

		public IPanelLayoutProvider GetMiscOptionsLayout(BaseJobDeclaration declaration) => new MiscOptionsLayouts();

		public IPanelLayoutProvider GetCommercialInvoiceDetailsLayout(BaseJobDeclaration declaration)
		{
			if (declaration.IsExport)
			{
				return new ExportInvoiceDetailsLayout();
			}
			else if (declaration.IsImport)
			{
				return new ImportInvoiceDetailsLayout();
			}
			else
			{
				return null;
			}
		}

		public IPanelLayoutProvider GetInstructionDetailsLayoutProvider(BaseJobDeclaration declaration) => new EntryInstructionDetailsBasicUserControlLayout();

		public IPanelLayoutProvider GetInvoiceLineCalculationsPanelLayout(BaseJobDeclaration declaration) => declaration.IsImport ? new ImportInvoiceLineCalculationsLayout() : null;

		public IGridColumnLayoutProvider GetInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetApportionedInvoiceChargesGridColumnLayout(BaseJobDeclaration declaration) => null;

		public IGridColumnLayoutProvider GetGroupChargesGridColumnLayout(BaseJobDeclaration declaration) => null;
	}
}
