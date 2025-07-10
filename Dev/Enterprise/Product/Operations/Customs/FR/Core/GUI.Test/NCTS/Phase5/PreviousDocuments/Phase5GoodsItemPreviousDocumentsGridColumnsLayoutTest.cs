using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.FR.GUI.Testing
{
	sealed class Phase5GoodsItemPreviousDocumentsGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<Phase5GoodsItemPreviousDocumentsGridColumnsLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(NctsPreviousDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
			(NctsPreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyleInfo), 200),
			(NctsPreviousDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsPreviousDocument.Schema.CSI_Quantity2, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsPreviousDocument.Schema.CSI_UnitOfQuantity2, typeof(ZDropEditColumnStyleInfo), 80),
			(NctsPreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsPreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 80),
			(NctsPreviousDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 200),
		};

		protected override Type GridBoundEntityType => typeof(NctsPreviousDocument);
	}
}
