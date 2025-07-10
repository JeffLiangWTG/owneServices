using System;
using System.Collections.Generic;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class GoodsItemDifferencesDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<GoodsItemDifferencesDetailsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(NctsDepartureCargoDesc.Schema.BY_LineNo, typeof(ZTextBoxColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_DeclarationGoodsItemNumber, typeof(ZTextBoxColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_UnloadedState, typeof(ZDropEditColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_CusC4Number, typeof(ZCodeFindBoxColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_Description, typeof(ZTextBoxColumnStyleInfo), 240),
		(NctsDepartureCargoDesc.Schema.BY_GrossWeight, typeof(ZCalcEditColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_GrossWeightUnit, typeof(ZDropEditColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_NetWeight, typeof(ZCalcEditColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_NetWeightUnit, typeof(ZDropEditColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff, typeof(NctsTariffColumnStyleInfo), 100),
	};

	protected override Type GridBoundEntityType => typeof(NctsArrivalCargoDesc);
}
