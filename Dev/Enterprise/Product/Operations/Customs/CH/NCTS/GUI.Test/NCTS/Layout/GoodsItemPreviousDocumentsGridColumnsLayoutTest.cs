using System;
using System.Collections.Generic;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(GoodsItemPreviousDocumentsGridColumnsLayout))]
sealed class GoodsItemPreviousDocumentsGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<GoodsItemPreviousDocumentsGridColumnsLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(NctsPreviousDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
		(NctsPreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo),200),
		(NctsPreviousDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 80),
		(NctsPreviousDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 200),
	};

	protected override Type GridBoundEntityType => typeof(NctsPreviousDocument);
}
