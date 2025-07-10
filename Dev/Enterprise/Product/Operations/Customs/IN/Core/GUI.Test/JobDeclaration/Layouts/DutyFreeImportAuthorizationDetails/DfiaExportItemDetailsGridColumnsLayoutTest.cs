using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DfiaExportItemDetailsGridColumnsLayout))]
sealed class DfiaExportItemDetailsGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<DfiaExportItemDetailsGridColumnsLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(DfiaExportItemDetail.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 50),
		(DfiaExportItemDetail.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 175),
		(DfiaExportItemDetail.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 80),
		(DfiaExportItemDetail.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 80),
		(DfiaExportItemDetail.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 100),
		(DfiaExportItemDetail.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 40)
	};

	protected override Type GridBoundEntityType => typeof(DfiaExportItemDetail);
}
