using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(NctsPhase5LayoutProvider))]
sealed class NctsPhase5TransitionPeriodLayoutProviderTest : EU.NCTS.GUI.Testing.NctsPhase5TransitionPeriodLayoutProviderAbstractTest
{
	protected override Type ExpectedGetDepartureGoodsItemsGridColumnLayout => typeof(Phase5TransitionPeriodGoodsItemDetailsGridColumnsLayout);

	protected override Type ExpectedGetHouseConsignmentDetailsGridColumnLayout => typeof(Phase5TransitionPeriodHouseConsignmentDetailsGridColumnLayout);

	protected override EU.NCTS.GUI.INctsPhase5TransitionPeriodLayoutProvider Provider => new NctsPhase5LayoutProvider();
}
