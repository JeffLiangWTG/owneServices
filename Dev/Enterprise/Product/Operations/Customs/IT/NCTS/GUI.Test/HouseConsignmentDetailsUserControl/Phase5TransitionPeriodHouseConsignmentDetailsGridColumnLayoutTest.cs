using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class Phase5TransitionPeriodHouseConsignmentDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<Phase5TransitionPeriodHouseConsignmentDetailsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		("SequenceNumber", typeof(ZTextBoxColumnStyleInfo), 115),
		("B0_RN_NKCountryOfExport", typeof(ZDropEditColumnStyleInfo), 115),
		("B0_Weight", typeof(ZCalcEditColumnStyleInfo), 110),
		("B0_ReferenceID", typeof(ZTextBoxColumnStyleInfo), 330),
		("B0_BillStatus", typeof(ZDropEditColumnStyleInfo), 50)
	};

	protected override Type GridBoundEntityType => typeof(NctsBill);
}
