using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ConsignmentItemsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(CusExitConsignmentItem.Schema.CCI_LineNumber, typeof(ZTextBoxColumnStyleInfo), 80),
			(CusExitConsignmentItem.Schema.CCI_GrossMass, typeof(ZCalcEditColumnStyleInfo), 100),
			(CusExitConsignmentItem.Schema.CCI_NetMass, typeof(ZCalcEditColumnStyleInfo), 100),
			(CusExitConsignmentItem.Schema.CCI_UniqueConsignmentReference, typeof(ZTextBoxColumnStyleInfo), 140),
		};

		protected override Type GridBoundEntityType => typeof(CusExitConsignmentItem);
	}
}
