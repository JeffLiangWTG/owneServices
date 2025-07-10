using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	sealed class TransactionLinesGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<TransactionLinesGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(AutoCusIntrastatLine.Schema.CIL_DescriptionOfGoods, typeof(ZTextBoxColumnStyleInfo), 385),
			(CusIntrastatLine.Schema.CIL_FormattedTariff, typeof(TariffColumnStyleInfo), 90),
			(AutoCusIntrastatLine.Schema.CIL_InvoiceValue, typeof(ZCalcEditColumnStyleInfo), 135),
			(AutoCusIntrastatLine.Schema.CIL_StatisticalValue, typeof(ZCalcEditColumnStyleInfo), 135),
			(AutoCusIntrastatLine.Schema.CIL_RX_NKCurrency, typeof(ZDropEditColumnStyleInfo), 70),
			(AutoCusIntrastatLine.Schema.CIL_MassInKilograms, typeof(ZCalcEditColumnStyleInfo), 95),
			(CusIntrastatLine.Schema.CIL_MassInKilogramsUnit, typeof(ZDropEditColumnStyleInfo), 70),
			(AutoCusIntrastatLine.Schema.CIL_SupplementaryQuantity, typeof(ZCalcEditColumnStyleInfo), 95),
			(AutoCusIntrastatLine.Schema.CIL_SupplementaryQuantityUnit, typeof(ZDropEditColumnStyleInfo), 70),
			(AutoCusIntrastatLine.Schema.CIL_RN_NKCountryOfOrigin, typeof(ZDropEditColumnStyleInfo), 70),
			(AutoCusIntrastatLine.Schema.CIL_Region, typeof(ZDropEditColumnStyleInfo), 70),
		};

		protected override Type GridBoundEntityType => typeof(CusIntrastatLine);
	}
}
