using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Ucc6ExportPreviousDocumentGridColumnLayout))]
sealed class Ucc6ExportPreviousDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<Ucc6ExportPreviousDocumentGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),

			(PreviousDocument.Schema.CSI_PackQty, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_PackType, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 80),
	};

	protected override Type GridBoundEntityType => typeof(PreviousDocument);
}
