using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.EMCS.GUI
{
	public class EMCSLayoutProvider : IEMCSLayoutProvider
	{
		public IPanelLayoutWithGridProvider GetInvoiceLineDetailsPanelLayoutWithGrid(ZString dataGroupingCode) => new InvoiceLineDetailsLayoutWithGrid(dataGroupingCode);

		public IPanelLayoutProvider DeclarationOrganizationsPanelLayout => new DeclarationOrganizationsLayout();
	}
}
