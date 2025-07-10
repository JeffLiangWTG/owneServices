using System;
using System.Collections.Generic;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLinePreviousDocumentGridColumnLayout))]
	sealed class ImportInvoiceLinePreviousDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ImportInvoiceLinePreviousDocumentGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 80),
		};

		protected override Type GridBoundEntityType => typeof(PreviousDocument);
	}
}
