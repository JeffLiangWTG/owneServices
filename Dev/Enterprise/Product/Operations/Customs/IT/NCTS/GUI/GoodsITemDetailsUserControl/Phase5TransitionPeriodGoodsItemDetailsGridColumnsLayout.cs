using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5TransitionPeriodGoodsItemDetailsGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= Phase5GoodsItemDetailsGridColumnsLayout.CreateLayout(true);
	IGridColumnLayout layout;
}
