using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public interface IEMCSLayoutProvider
	{
		IPanelLayoutWithGridProvider GetInvoiceLineDetailsPanelLayoutWithGrid(ZString dataGroupingCode);

		IPanelLayoutProvider DeclarationOrganizationsPanelLayout { get; }
	}
}
