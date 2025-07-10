using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(InvoiceLineImportPreviousDocumentGridColumnLayout))]
sealed class InvoiceLineImportPreviousDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<InvoiceLineImportPreviousDocumentGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
			(PreviousDocument.Schema.CSI_Procedure, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_CustomsOffice, typeof(ZCodeFindBoxColumnStyleInfo), 100),
			(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyleInfo), 80),
	};

	protected override Type GridBoundEntityType => typeof(PreviousDocument);
}
