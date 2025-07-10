using System;
using System.Collections.Generic;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(GoodsItemDetailsGridColumnLayout))]
sealed class GoodsItemDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<GoodsItemDetailsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(NctsDepartureCargoDesc.Schema.BY_LineNo, typeof(ZTextBoxColumnStyleInfo), 60),
		(NctsDepartureCargoDesc.Schema.BY_DeclarationGoodsItemNumber, typeof(ZTextBoxColumnStyleInfo), 60),
		(NctsDepartureCargoDesc.Schema.BY_Description, typeof(ZTextBoxColumnStyleInfo), 310),
		(NctsDepartureCargoDesc.Schema.BY_GrossWeight, typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_GrossWeightUnit, typeof(ZDropEditColumnStyleInfo), 50),
		(NctsDepartureCargoDesc.Schema.BY_NetWeight, typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_NetWeightUnit, typeof(ZDropEditColumnStyleInfo), 50),
		(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff, typeof(NctsTariffColumnStyleInfo), 100),
		(NctsDepartureCargoDesc.Schema.BY_Type, typeof(ZDropEditColumnStyleInfo), 35),
		(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfDispatch, typeof(ZDropEditColumnStyleInfo), 90),
		(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfDestination, typeof(ZDropEditColumnStyleInfo), 100),
		(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfOrigin, typeof(ZDropEditColumnStyleInfo), 75),
		(NctsDepartureCargoDesc.Schema.BY_CommercialReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 140),
		(NctsDepartureCargoDesc.Schema.BY_TransportChargesMethodOfPayment, typeof(ZDropEditColumnStyleInfo), 100),
		("UNDGs+UNDGSubstanceManagerGuid+Value", typeof(ZGuidFindBoxColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_CusC4Number, typeof(ZCodeFindBoxColumnStyleInfo), 65),
	};

	protected override Type GridBoundEntityType => typeof(NctsDepartureCargoDesc);
}
