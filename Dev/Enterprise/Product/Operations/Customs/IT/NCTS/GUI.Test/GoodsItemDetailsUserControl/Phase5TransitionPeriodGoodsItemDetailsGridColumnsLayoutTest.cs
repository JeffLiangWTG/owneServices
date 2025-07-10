using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class Phase5TransitionPeriodGoodsItemDetailsGridColumnsLayoutTest : GridColumnLayoutProviderAbstractTest<Phase5TransitionPeriodGoodsItemDetailsGridColumnsLayout>
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
		(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff, typeof(Universal.GUI.TariffColumnStyleInfo), 85),
		(NctsDepartureCargoDesc.Schema.BY_Type, typeof(ZDropEditColumnStyleInfo), 35),
		(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfDispatch, typeof(ZDropEditColumnStyleInfo), 90),
		(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfDestination, typeof(ZDropEditColumnStyleInfo), 100),
		(NctsDepartureCargoDesc.Schema.BY_RN_NKCountryOfOrigin, typeof(ZDropEditColumnStyleInfo), 75),
		(NctsDepartureCargoDesc.Schema.BY_CommercialReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 140),
		(NctsDepartureCargoDesc.Schema.BY_TransportChargesMethodOfPayment, typeof(ZDropEditColumnStyleInfo), 100),
		("UNDGs+UNDGSubstanceManagerGuid+Value", typeof(ZGuidFindBoxColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_CusC4Number, typeof(ZCodeFindBoxColumnStyleInfo), 65),
		("Consignee+OrganisationPK", typeof(ZOrganisationFindBoxColumnStyleInfo), 80),
		("Consignee+E2_OA_Address", typeof(ZAddressDropEditColumnStyleInfo), 160),
		(NctsDepartureCargoDesc.Schema.BY_CustomsSecondQuantity,  typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_CustomsSecondUnitQty,  typeof(ZDropEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_CustomsQuantity,  typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_CustomsThirdQuantity,  typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_CustomsThirdUnitQty,  typeof(ZDropEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_CustomsFourthQuantity,  typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_CustomsFourthUnitQty,  typeof(ZDropEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_MonetaryValue,  typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_ZZF_NKTaxType,  typeof(ZDropEditColumnStyleInfo), 75),
		(NctsDepartureCargoDesc.Schema.BY_Supplements,  typeof(ZTextBoxColumnStyleInfo), 80),
		(NctsDepartureCargoDesc.Schema.BY_LinePrice, typeof(ZCalcEditColumnStyleInfo), 110),
		(NctsDepartureCargoDesc.Schema.BY_RX_NKLinePriceCurrency, typeof(ZDropEditColumnStyleInfo), 50),
		(NctsDepartureCargoDesc.Schema.BY_Status, typeof(ZDropEditColumnStyleInfo), 50),
	};

	protected override Type GridBoundEntityType => typeof(NctsDepartureCargoDesc);
}
