using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentChargeDataListTest : TestCase
	{
		public void TestAddCharge()
		{
			List.AddCharge(ShipmentChargeTypeCode.Disbursement, 100m, Core.Constants.CurrencyCodes.Australia);
			AssertChargesArray("Should be added", 1, 100m, "405");
			List.AddCharge(ShipmentChargeTypeCode.Disbursement, 50m, Core.Constants.CurrencyCodes.Australia);
			AssertChargesArray("Should be accumulated", 1, 150m, "405");
			List.AddCharge(ShipmentChargeTypeCode.QuantumView, 200m, Core.Constants.CurrencyCodes.Australia);
			AssertChargesArray("Should be added", 2, 200m, "565");
		}

		#region TestAddChargeFromDescription
		public void TestAddChargeFromDescription()
		{
			List.AddCharge(CusEntryChargeTypeList.Descriptions.OtherCharges, 10m);
			AssertEquals("Should be ignored", 0, List.Count);
			List.AddCharge(CusEntryChargeTypeList.Descriptions.LCTAmount, 20m);
			AssertChargesArray("Should be classified as OtherCharges", 1, 20m, "216");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.ScreenFree, 30m);
			AssertChargesArray("Should be ignored", 1, 20m, "216");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.WetAmount, 40m);
			AssertChargesArray("Should be classified and accumulated into the OtherCharges", 1, 60m, "216");
			List.AddCharge("SFTASDF", 50m);
			AssertChargesArray("Should be ignored", 1, 60m, "216");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.DutyAmount, 100m);
			AssertChargesArray("Should be classified as Duty", 2, 100m, "201");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.GSTAmount, 300m);
			AssertChargesArray("Should be classified as GST", 3, 300m, "206");
		}

		public void TestAddChargeFromDescription_Duty()
		{
			List.AddCharge(CusEntryChargeTypeList.Descriptions.DutyAmount, 10m);
			AssertChargesArray("Should be classified as Duty", 1, 10m, "201");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.DutyAmount, 200m);
			AssertChargesArray("Should be classified and accumulated as Duty", 1, 210m, "201");
		}

		public void TestAddChargeFromDescription_GST()
		{
			List.AddCharge(CusEntryChargeTypeList.Descriptions.GSTAmount, 100m);
			AssertChargesArray("Should be classified as GST", 1, 100m, "206");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.GSTAmount, 400m);
			AssertChargesArray("Should be classified and accumulated as GST", 1, 500m, "206");
			List.AddCharge("VAT", 300m);
			AssertChargesArray("Should be classified as VAT", 2, 300m, "205");
		}

		public void TestAddChargeFromDescription_OtherGovtCharges()
		{
			List.AddCharge(CusEntryChargeTypeList.Descriptions.LCTAmount, 100m);
			AssertChargesArray("Should be classified and accumulated into the OtherCharges", 1, 100m, "216");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.WetAmount, 150m);
			AssertChargesArray("Should be classified and accumulated into the OtherCharges", 1, 250m, "216");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.Woodlevy, 50m);
			AssertChargesArray("Should be classified and accumulated into the OtherCharges", 1, 300m, "216");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.TotalPayableAdmin, 25m);
			AssertChargesArray("Should be classified and accumulated into the OtherCharges", 1, 325m, "216");
		}

		public void TestAddChargeFromDescription_Tradegate_CMR()
		{
			List.AddCharge(CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge, 12m);
			AssertChargesArray("Should be classified and accumulated into the Tradegate Fee", 1, 12m, "231");
			List.AddCharge(CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 100m);
			AssertChargesArray("Should be classified and accumulated into the Tradegate Fee", 1, 112m, "231");
		}

		public void TestAddChargeFromDescription_Unknown()
		{
			List.AddCharge(CusEntryChargeTypeList.Descriptions.ScreenFree, 30m);
			AssertEquals("Should be ignored", 0, List.Count);
			List.AddCharge("SFTASDF", 50m);
			AssertEquals("Should be ignored", 0, List.Count);
			List.AddCharge(CusEntryChargeTypeList.Descriptions.FlatDutyPortion, 400m);
			AssertEquals("Should be ignored", 0, List.Count);
		}

		#endregion
		public void TestAddChargeFromShipmentChargeData()
		{
			List.AddCharge(new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 10m, Core.Constants.CurrencyCodes.Australia));
			AssertChargesArray("Should be added", 1, 10m, "201");
			List.AddCharge(new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 50m, Core.Constants.CurrencyCodes.Australia));
			AssertChargesArray("Should be accumulated", 1, 60m, "201");
			List.AddCharge(new ShipmentChargeData(ShipmentChargeTypeCode.Disbursement, 100m, Core.Constants.CurrencyCodes.Australia));
			AssertChargesArray("Should be added", 2, 100m, "405");
		}

		public void TestAddCharges()
		{
			ShipmentChargeData chargeData1 = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 10m, Core.Constants.CurrencyCodes.Australia);
			ShipmentChargeData chargeData2 = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 50m, Core.Constants.CurrencyCodes.Australia);
			ShipmentChargeData chargeData3 = new ShipmentChargeData(ShipmentChargeTypeCode.Disbursement, 100m, Core.Constants.CurrencyCodes.Australia);
			ShipmentChargeData chargeData4 = new ShipmentChargeData(ShipmentChargeTypeCode.VAT, 200m, Core.Constants.CurrencyCodes.Australia);
			List.AddCharge(chargeData1, chargeData2, chargeData3, chargeData4);
			AssertChargesArray("Should be added", 3, 60m, "201");
			AssertChargesArray("Should be added", 3, 100m, "405");
			AssertChargesArray("Should be added", 3, 200m, "205");
		}

		public void TestCount()
		{
			ShipmentChargeData chargeData1 = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 10m, Core.Constants.CurrencyCodes.Australia);
			ShipmentChargeData chargeData2 = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 50m, Core.Constants.CurrencyCodes.Australia);
			ShipmentChargeData chargeData3 = new ShipmentChargeData(ShipmentChargeTypeCode.Disbursement, 100m, Core.Constants.CurrencyCodes.Australia);
			List.AddCharge(chargeData1, chargeData2, chargeData3);
			AssertEquals(2, List.Count);
		}

		#region Implementation
		void AssertChargesArray(string wrongCountMessage, int expectedCount, ZDecimal expectedGrossAmount, ZString expectedTypeCode)
		{
			ShipmentChargeData[] charges = List.ToArray();
			AssertEquals(wrongCountMessage, expectedCount, charges.Length);
			ShipmentChargeData expectedCharge = null;
			foreach (ShipmentChargeData charge in charges)
			{
				if (charge.TypeCode == expectedTypeCode)
				{
					expectedCharge = charge;
					break;
				}
			}

			AssertNotNull(expectedTypeCode + " is not found in the list", expectedCharge);
			AssertEquals("Incorrect gross amount", expectedGrossAmount, expectedCharge.GrossAmount);
		}

		ShipmentChargeDataList List
		{
			get
			{
				if (fList == null)
				{
					fList = new ShipmentChargeDataList();
				}

				return fList;
			}
		}

		ShipmentChargeDataList fList;
		#endregion
	}
}
