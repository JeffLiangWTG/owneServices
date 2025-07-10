using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemValidation))]
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_GoodsValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "ABC";

			var bill = header.Bills.AddNew();
			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.UnitedStates;
			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C07;

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_GoodsValue = 50;
			packedItem1.API_RX_NKGoodsValueCurrency = CurrencyCodes.EuropeanUnion;

			AssertNoMessageErrors(packedItem1.API_GoodsValueInfo);

			packedItem1.API_GoodsValue = 160;
			AssertHasMessageError(packedItem1.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07.");

			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C08;
			packedItem1.Validation.ValidateAll();
			AssertHasMessageError(packedItem1.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed EUR 45 when Add. Procedure(s) is C08 and Customs Office (Lodgement) is not in the Canary Islands.");

			header.AMA_CustomsOffice = "ES0035001";
			packedItem1.Validation.ValidateAll();
			AssertHasMessageError(packedItem1.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed EUR 45 when Add. Procedure(s) is C08 and Customs Office (Lodgement) is in the Canary Islands and Exporter Country/Region is not in the EU, Ceuta or Melilla.");

			packedItem1.API_GoodsValue = 50;
			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Spain;
			packedItem1.Validation.ValidateAll();
			AssertNoMessageErrors(packedItem1.API_GoodsValueInfo);

			packedItem1.API_GoodsValue = 120;
			packedItem1.Validation.ValidateAll();
			AssertHasMessageError(packedItem1.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed EUR 110 when Add. Procedure(s) is C08 and Customs Office (Lodgement) is in the Canary Islands and Exporter Country/Region is in the EU, Ceuta or Melilla.");

			bill.ABL_RN_NKShipperCountry = Core.Constants.NonStandardCountryCodes.Codes.XC;
			packedItem1.Validation.ValidateAll();
			AssertHasMessageError(packedItem1.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed EUR 110 when Add. Procedure(s) is C08 and Customs Office (Lodgement) is in the Canary Islands and Exporter Country/Region is in the EU, Ceuta or Melilla.");
		}

		public void TestCheckAPI_GoodsValue_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_GoodsValue = 0.0m;
			AssertHasMessageError(packedItem.API_GoodsValueInfo, "You have not entered an Intrinsic Value.");
		}

		public void TestCheckAPI_CustomsQty2_MandatoryCheck()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			bill.ABL_Procedure = ESH7AdditionalProcedureCodeList.Codes.C08;

			foreach (var tariffPrefix in tariffPrefixes)
			{
				packedItem.API_Tariff = tariffPrefix + DateTime.Now.Millisecond;
				packedItem.API_CustomsQty2 = ZDecimal.Zero;
				AssertHasMessageError(packedItem.API_CustomsQty2Info, "You have not entered a Supplementary Units.");

				packedItem.API_CustomsQty2 = DateTime.Now.Millisecond;
				AssertNoMessageError(packedItem.API_CustomsQty2Info, "You have not entered a Supplementary Units.");
			}
		}

		public void TestAPI_GrossWeight_MandatoryCheckAndNotExceeding100KG()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_GrossWeight = ZDecimal.Zero;
			AssertHasMessageError(packedItem.API_GrossWeightInfo, "You have not entered a Gross Weight.");

			foreach (var procedure07 in procedures07)
			{
				bill.ABL_Procedure = procedure07;
				packedItem.API_GrossWeightUQ = Constants.Weight.Grams;
				packedItem.API_GrossWeight = 101000;
				AssertHasMessageError(packedItem.API_GrossWeightInfo, grossWeightNotExceeding100KgMessage);

				packedItem.API_GrossWeightUQ = Constants.Weight.Ounces;
				packedItem.API_GrossWeight = 203;
				AssertNoMessageError(packedItem.API_GrossWeightInfo, grossWeightNotExceeding100KgMessage);
			}
		}

		readonly string grossWeightNotExceeding100KgMessage = "Gross Weight must not exceed 100 KG.";

		readonly List<string> tariffPrefixes = ["2204", "2205", "2206", "2207", "2208", "240210", "240220", "2401", "2403", "3303"];

		readonly List<string> procedures07 = [ESH7AdditionalProcedureCodeList.Codes.C07, ESH7AdditionalProcedureCodeList.Codes.C07F48, ESH7AdditionalProcedureCodeList.Codes.C07F49];
	}
}
