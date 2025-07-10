using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IN.GUI.Testing;

sealed class NonStandardExchangeRatesGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<NonStandardExchangeRatesGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(NonStandardExchangeRate.Schema.CSI_RX_NKCurrency, typeof(ZTextBoxColumnStyleInfo), 40),
		(NonStandardExchangeRate.Schema.CSI_Description, typeof(ZTextBoxColumnStyleInfo), 100),
		(NonStandardExchangeRate.Schema.CSI_EffectiveDate, typeof(ZDateTimeOffsetEditColumnStyleInfo), 100),
		(NonStandardExchangeRate.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 100),
		(NonStandardExchangeRate.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 100),
		(NonStandardExchangeRate.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyleInfo), 100),
		(NonStandardExchangeRate.Schema.CSI_Value, typeof(ZCalcEditColumnStyleInfo), 100),
		(NonStandardExchangeRate.Schema.CSI_AdditionalDescription, typeof(ZTextBoxColumnStyleInfo), 100),
	};

	protected override Type GridBoundEntityType => typeof(NonStandardExchangeRate);
}
