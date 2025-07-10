using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public interface IIntrastatLayoutProvider
	{
		IPanelLayoutProvider OrganisationDetailsPanelLayout { get; }
		IPanelLayoutProvider TransactionDetailsPanelLayout { get; }
		IPanelLayoutWithGridProvider TransactionLineDetailsWithGridLayout { get; }
		IGridColumnLayoutProvider TransactionLinesGridColumnLayout { get; }
	}
}
