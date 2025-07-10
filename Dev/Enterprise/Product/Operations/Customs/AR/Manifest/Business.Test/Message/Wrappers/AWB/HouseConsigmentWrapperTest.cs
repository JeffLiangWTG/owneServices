using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class HouseConsignmentWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestHouseConsigmentWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();
			CreateAndPopulateHouseBill();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], "ORG");

			IMasterConsignment masterConsignment = wrapper.MasterConsignment;

			IHouseConsignment houseConsignment = masterConsignment.IncludedHouseConsignment;

			CombineAssertions(() =>
			{
				AssertEquals(true, houseConsignment.NilCarriageValueIndicator);
				AssertEquals((ZDecimal)300.00, houseConsignment.DeclaredValueForCarriageAmount);
				AssertEquals("MXN", houseConsignment.DeclaredValueForCarriageAmountCurrencyID);

				AssertEquals(true, houseConsignment.NilCustomsValueIndicator);
				AssertEquals((ZDecimal)500.00, houseConsignment.DeclaredValueForCustomsAmount);
				AssertEquals("EUR", houseConsignment.DeclaredValueForCustomsAmountCurrencyID);

				AssertEquals(true, houseConsignment.NilInsuranceValueIndicator);
				AssertEquals((ZDecimal)400.00, houseConsignment.InsuranceValueAmount);
				AssertEquals("USD", houseConsignment.InsuranceValueAmountCurrencyID);

				AssertEquals((ZDecimal)20.00, houseConsignment.IncludedTareGrossWeightMeasure);
				AssertEquals("Kgm", houseConsignment.IncludedTareGrossWeightMeasureUnitCode);
				AssertEquals((ZDecimal)25.00, houseConsignment.GrossVolumeMeasure);
				AssertEquals("Cmq", houseConsignment.GrossVolumeMeasureUnitCode);
				AssertEquals((ZDecimal)1, houseConsignment.TotalPieceQuantity);

				AssertEquals((ZDecimal)100.00, houseConsignment.ValuationTotalChargeAmount);
				AssertEquals("USD", houseConsignment.ValuationTotalChargeAmountCurrencyID);
				AssertEquals("Goods", houseConsignment.SummaryDescription);
				AssertEquals((ZDecimal)400.00, houseConsignment.ConsignmentItemQuantity);
			});
		}

		void PopulateManifestHeader()
		{
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Argentina;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "HOUSELIGADAMASTER0001";
			bill.ABL_ManifestQty = 400;
			bill.ABL_ManifestUQ = "PCS";
			bill.ABL_GrossWeight = 20;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 25;
			bill.ABL_VolumeUQ = "CC";

			bill.ABL_FreightValue = 100;
			bill.ABL_RX_NKFreightValueCurrency = "USD";

			bill.ABL_TransportValue = 300;
			bill.ABL_RX_NKTransportValueCurrency = "MXN";

			bill.ABL_InsuranceValue = 400;
			bill.ABL_RX_NKInsuranceValueCurrency = "USD";

			bill.ABL_CustomsValue = 500;
			bill.ABL_RX_NKCustomsValueCurrency = "EUR";

			bill.ABL_GoodsDescription = "Goods";

			bill.Packs.AddNew();
		}
	}
}
