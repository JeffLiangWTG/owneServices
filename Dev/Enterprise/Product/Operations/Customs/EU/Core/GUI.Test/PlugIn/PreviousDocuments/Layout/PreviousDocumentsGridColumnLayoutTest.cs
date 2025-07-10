using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PreviousDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<PreviousDocumentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_CodeDescription, typeof(ZTextBoxColumnStyleInfo), 200),
			(PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_Procedure, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_CustomsOffice, typeof(ZCodeFindBoxColumnStyleInfo), 100),
			(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_Quantity3, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_UnitOfQuantity3, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_Quantity2, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_UnitOfQuantity2, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_RN_NKCountryCode, typeof(ZCodeFindBoxColumnStyleInfo), 120),
		};

		protected override Type GridBoundEntityType => typeof(PreviousDocument);
	}
}
