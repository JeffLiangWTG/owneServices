using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(DfiaImportItemDetailsGridColumnsLayout))]
sealed class DfiaImportItemDetailsGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<DfiaImportItemDetailsGridColumnsLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(DfiaImportItemDetail.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 50),
		(DfiaImportItemDetail.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 100),
		(DfiaImportItemDetail.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 130),
		(DfiaImportItemDetail.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 50),
		(DfiaImportItemDetail.Schema.CSI_IssuerType, typeof(ZDropEditColumnStyleInfo), 50)
	};

	protected override Type GridBoundEntityType => typeof(DfiaImportItemDetail);
}
