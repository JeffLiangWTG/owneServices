using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	internal class MasterConsignmentWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestMasterConsigmentWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			CreateAndPopulateHouseBill();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], "ORG");

			IMasterConsignment masterConsignment = wrapper.MasterConsignment;
			IMeasure measure = masterConsignment.IncludedTareGrossWeightMeasure;

			ILocation originLocation = masterConsignment.OriginLocation;
			ILocation finalDestinationLocation = masterConsignment.FinalDestinationLocation;

			CombineAssertions(() =>
			{
				AssertEquals((ZDecimal)20.00, measure.Value);
				AssertEquals("Kgm", measure.UnitCode);

				AssertEquals((ZDecimal)400.00, masterConsignment.TotalPieceQuantity);
				AssertEquals("MANIFESTNUMBER", masterConsignment.TransportContractDocument);
			});
		}

		void PopulateManifestHeader()
		{
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Argentina;
			header.AMA_MasterBill = "MANIFESTNUMBER";
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "HOUSELIGADAMASTER0001";
			bill.ABL_ManifestQty = 400;
			bill.ABL_GrossWeight = 20;
			bill.ABL_GrossWeightUQ = "KG";
		}
	}
}
