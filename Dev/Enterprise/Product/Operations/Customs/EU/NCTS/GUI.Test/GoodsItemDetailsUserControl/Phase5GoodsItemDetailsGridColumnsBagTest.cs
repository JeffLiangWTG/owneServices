using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemDetailsGridColumnsBagTest : TestCase
	{
		public void TestLineNoTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.LineNoTextBoxColumn);
			var columnInfo = ColumnsBag.LineNoTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_LineNo", columnInfo.ColumnName);
		}

		public void TestDeclarationGoodsItemNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.DeclarationGoodsItemNumberTextBoxColumn);
			var columnInfo = ColumnsBag.DeclarationGoodsItemNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_DeclarationGoodsItemNumber", columnInfo.ColumnName);
		}

		public void TestDescriptionTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.DescriptionTextBoxColumn);
			var columnInfo = ColumnsBag.DescriptionTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_Description", columnInfo.ColumnName);
		}

		public void TestGrossWeightCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.GrossWeightCalcEditColumn);
			var columnInfo = ColumnsBag.GrossWeightCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertNull("BindToDecimalPlaces", columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", "BY_GrossWeight", columnInfo.ColumnName);
			});
		}

		public void TestGrossWeightUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.GrossWeightUnitDropEditColumn);
			var columnInfo = ColumnsBag.GrossWeightUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_GrossWeightUnit", columnInfo.ColumnName);
		}

		public void TestNetWeightCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.NetWeightCalcEditColumn);
			var columnInfo = ColumnsBag.NetWeightCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertNull("BindToDecimalPlaces", columnInfo.BindToDecimalPlaces);
				AssertEquals("ColumnName", "BY_NetWeight", columnInfo.ColumnName);
			});
		}

		public void TestNetWeightUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.NetWeightUnitDropEditColumn);
			var columnInfo = ColumnsBag.NetWeightUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_NetWeightUnit", columnInfo.ColumnName);
		}

		public void TestFormattedHarmonisedTariffColumn()
		{
			AssertNotNull(ColumnsBag.FormattedHarmonisedTariffColumn);
			var columnInfo = ColumnsBag.FormattedHarmonisedTariffColumn.CreateGridColumnInfo() as TariffColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertNull("CountryCode", columnInfo.GetCountryCode?.Invoke());
				AssertNull("SelectNomenclatureModes", columnInfo.SelectNomenclatureModes);
				AssertEquals("ShowDescriptionFilterOnNonNomenclatureTariffModule", false, columnInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule);
				AssertEquals("ColumnName", "BY_FormattedHarmonisedTariff", columnInfo.ColumnName);
				AssertEquals("TariffType", "IMP", columnInfo.TariffType);
			});
		}

		public void TestTypeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.TypeDropEditColumn);
			var columnInfo = ColumnsBag.TypeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("ColumnName", "BY_Type", columnInfo.ColumnName);
			});
		}

		public void TestCountryOfDispatchDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CountryOfDispatchDropEditColumn);
			var columnInfo = ColumnsBag.CountryOfDispatchDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_RN_NKCountryOfDispatch", columnInfo.ColumnName);
		}

		public void TestCountryOfDestinationDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CountryOfDestinationDropEditColumn);
			var columnInfo = ColumnsBag.CountryOfDestinationDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_RN_NKCountryOfDestination", columnInfo.ColumnName);
		}

		public void TestCountryOfOriginDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CountryOfOriginDropEditColumn);
			var columnInfo = ColumnsBag.CountryOfOriginDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_RN_NKCountryOfOrigin", columnInfo.ColumnName);
		}

		public void TestCommercialReferenceNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.CommercialReferenceNumberTextBoxColumn);
			var columnInfo = ColumnsBag.CommercialReferenceNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_CommercialReferenceNumber", columnInfo.ColumnName);
		}

		public void TestTransportChargesMethodOfPaymentDropEditColumn()
		{
			AssertNotNull(ColumnsBag.TransportChargesMethodOfPaymentDropEditColumn);
			var columnInfo = ColumnsBag.TransportChargesMethodOfPaymentDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("ColumnName", "BY_TransportChargesMethodOfPayment", columnInfo.ColumnName);
			});
		}

		public void TestUNDGsGuidFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.UNDGsGuidFindBoxColumn);
			var columnInfo = ColumnsBag.UNDGsGuidFindBoxColumn.CreateGridColumnInfo() as ZGuidFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("BindToList", "UNDGs+AllUNDGSubstances", columnInfo.BindToList);
				AssertEquals("CaptionResourceString", "DG Substance", columnInfo.CaptionResourceString?.Caption);
				AssertEquals("ColumnName", "UNDGs+UNDGSubstanceManagerGuid+Value", columnInfo.ColumnName);
			});
		}

		public void TestCusC4NumberCodeFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.CusC4NumberCodeFindBoxColumn);
			var columnInfo = ColumnsBag.CusC4NumberCodeFindBoxColumn.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_CusC4Number", columnInfo.ColumnName);
		}

		public void TestConsigneeOrganisationFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.ConsigneeOrganisationFindBoxColumn);
			var columnInfo = ColumnsBag.ConsigneeOrganisationFindBoxColumn.CreateGridColumnInfo() as ZOrganisationFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("BindToList", "Lookups.ConsigneeList", columnInfo.BindToList);
				AssertEquals("CaptionResourceString", "Consignee Org.", columnInfo.CaptionResourceString?.Caption);
				AssertEquals("ColumnName", "Consignee+OrganisationPK", columnInfo.ColumnName);
			});
		}

		public void TestConsigneeAddressDropEditColumn()
		{
			AssertNotNull(ColumnsBag.ConsigneeAddressDropEditColumn);
			var columnInfo = ColumnsBag.ConsigneeAddressDropEditColumn.CreateGridColumnInfo() as ZAddressDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CaptionResourceString", "Consignee Address", columnInfo.CaptionResourceString?.Caption);
				AssertEquals("ColumnName", "Consignee+E2_OA_Address", columnInfo.ColumnName);
			});
		}

		public void TestSupplementaryQtyDropEditColumn()
		{
			AssertNotNull(ColumnsBag.SupplementaryQuantityEditColumn);
			var columnInfo = ColumnsBag.SupplementaryQuantityEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_CustomsSecondQuantity, columnInfo.ColumnName);
			});
		}

		public void TestSupplementaryQtyUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.SupplementaryQuantityUnitDropEditColumn);
			var columnInfo = ColumnsBag.SupplementaryQuantityUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_CustomsSecondUnitQty, columnInfo.ColumnName);
			});
		}

		public void TestCustomsQuantityEditColumn()
		{
			AssertNotNull(ColumnsBag.CustomsQuantityDropEditColumn);
			var columnInfo = ColumnsBag.CustomsQuantityDropEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CaptionResourceString", "Customs Qty", columnInfo.CaptionResourceString?.Caption);
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_CustomsQuantity, columnInfo.ColumnName);
			});
		}

		public void TestThirdyQtyEditColumn()
		{
			AssertNotNull(ColumnsBag.ThirdQuantityEditColumn);
			var columnInfo = ColumnsBag.ThirdQuantityEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_CustomsThirdQuantity, columnInfo.ColumnName);
			});
		}

		public void TestThirdQtyUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.ThirdQuantityUnitDropEditColumn);
			var columnInfo = ColumnsBag.ThirdQuantityUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CaptionResourceString", "Third Qty Unit", columnInfo.CaptionResourceString?.Caption);
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_CustomsThirdUnitQty, columnInfo.ColumnName);
			});
		}

		public void TestFourthQtyEditColumn()
		{
			AssertNotNull(ColumnsBag.FourthQuantityEditColumn);
			var columnInfo = ColumnsBag.FourthQuantityEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_CustomsFourthQuantity, columnInfo.ColumnName);
			});
		}

		public void TestFourthQtyUnitDropEditColumn()
		{
			AssertNotNull(ColumnsBag.FourthQuantityUnitDropEditColumn);
			var columnInfo = ColumnsBag.FourthQuantityUnitDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("CaptionResourceString", "Fourth Qty Unit", columnInfo.CaptionResourceString?.Caption);
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_CustomsFourthUnitQty, columnInfo.ColumnName);
			});
		}

		public void TestCustomsValueCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.CustomsValueCalcDropEditColumn);
			var columnInfo = ColumnsBag.CustomsValueCalcDropEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_MonetaryValue, columnInfo.ColumnName);
		}

		public void TestTaxOrFeeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.TaxOrFeeDropEditColumn);
			var columnInfo = ColumnsBag.TaxOrFeeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_ZZF_NKTaxType, columnInfo.ColumnName);
			});
		}

		public void TestSupplementaryCodesDropEditColumn()
		{
			AssertNotNull(ColumnsBag.SupplementaryCodesFindBoxColumn);
			var columnInfo = ColumnsBag.SupplementaryCodesFindBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", NctsDepartureCargoDesc.Schema.BY_Supplements, columnInfo.ColumnName);
		}

		public void TestLinePriceCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.LinePriceCalcEditColumn);
			var columnInfo = ColumnsBag.LinePriceCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_LinePrice", columnInfo.ColumnName);
		}

		public void TestLinePriceCurrencyDropEditColumn()
		{
			AssertNotNull(ColumnsBag.LinePriceCurrencyDropEditColumn);
			var columnInfo = ColumnsBag.LinePriceCurrencyDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "BY_RX_NKLinePriceCurrency", columnInfo.ColumnName);
		}

		Phase5GoodsItemDetailsGridColumnsBag ColumnsBag => Phase5GoodsItemDetailsGridColumnsBag.Instance;
	}
}
