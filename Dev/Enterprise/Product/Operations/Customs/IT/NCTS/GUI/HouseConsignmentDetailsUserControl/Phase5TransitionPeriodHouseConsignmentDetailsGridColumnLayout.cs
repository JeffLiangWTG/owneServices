using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class Phase5TransitionPeriodHouseConsignmentDetailsGridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= HouseConsignmentDetailsGridColumnLayout.CreateLayout(true);
	IGridColumnLayout layout;
}
