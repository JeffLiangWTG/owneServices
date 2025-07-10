using System;
using System.Collections.Generic;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using CusExitConsignmentItem = Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ConsignmentItemsUcc6GridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ConsignmentItemsUcc6GridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(AutoCusExitConsignmentItem.Schema.CCI_LineNumber, typeof(ZTextBoxColumnStyleInfo), 80),
		(AutoCusExitConsignmentItem.Schema.CCI_GrossMass, typeof(ZCalcEditColumnStyleInfo), 100),
		(AutoCusExitConsignmentItem.Schema.CCI_NetMass, typeof(ZCalcEditColumnStyleInfo), 100),
		(AutoCusExitConsignmentItem.Schema.CCI_DiscrepancyStatus, typeof(ZDropEditColumnStyleInfo), 50),
		(AutoCusExitConsignmentItem.Schema.CCI_UniqueConsignmentReference, typeof(ZTextBoxColumnStyleInfo), 140),
		(AutoCusExitConsignmentItem.Schema.CCI_UniqueConsignmentReferenceStatus, typeof(ZDropEditColumnStyleInfo), 50),
	};

	protected override Type GridBoundEntityType => typeof(CusExitConsignmentItem);
}
