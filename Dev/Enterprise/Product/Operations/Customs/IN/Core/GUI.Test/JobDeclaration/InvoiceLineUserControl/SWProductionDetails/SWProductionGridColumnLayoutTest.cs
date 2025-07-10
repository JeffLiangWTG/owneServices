using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;
[TestedType(typeof(SWProductionGridColumnLayout))]
sealed class SWProductionGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<SWProductionGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(SWProduction.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 50),
		(SWProduction.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 100),
		(SWProduction.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 100),
		(SWProduction.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyleInfo), 100),
		(SWProduction.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 100),
		(SWProduction.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyleInfo), 100),
		(SWProduction.Schema.CSI_EffectiveDate, typeof(ZDateTimeOffsetEditColumnStyleInfo), 100),
	};

	protected override Type GridBoundEntityType => typeof(SWProduction);
}
