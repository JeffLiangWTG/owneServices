using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public interface INctsPhase5TransitionPeriodLayoutProvider
	{
		IGridColumnLayoutProvider GetDepartureGoodsItemsGridColumnLayout();

		IGridColumnLayoutProvider GetHouseConsignmentDetailsGridColumnLayout();
	}
}
