using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class LogisticsTransportMovementWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestLogisticsTransportMovementWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], MessageSubTypeCodes.Codes.Original);
			ILogisticsTransportMovement ltm = wrapper.MasterConsignment.IncludedHouseConsignment.SpecifiedLogisticsTransportMovement;
			IEvent arrivalEvent = ltm.ArrivalEvent;
			IEvent departureEvent = ltm.DepartureEvent;

			CombineAssertions(() =>
			{
				AssertEquals("On-Carriage", ltm.StageCode);
				AssertEquals("Voyage", ltm.ID);
				AssertEquals("Carrier", ltm.NameOfTransport);

				AssertEquals("2023-01-11T00:00:00", departureEvent.Date.ToString("yyyy-MM-ddTHH:mm:ss"));
				AssertEquals("EZE", departureEvent.LocationCode);
				AssertEquals("Buenos Aires", departureEvent.LocationName);

				AssertEquals("2023-01-12T00:00:00", arrivalEvent.Date.ToString("yyyy-MM-ddTHH:mm:ss"));
				AssertEquals("MIA", arrivalEvent.LocationCode);
				AssertEquals("Miami", arrivalEvent.LocationName);
			});
		}

		void PopulateManifestHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Carrier";

			header.AMA_E_ARV = new ZDate(2023, 01, 12);
			header.AMA_E_DEP = new ZDate(2023, 01, 11);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_RL_NKPortOfLoading = "ARBUE";
			header.AMA_RL_NKPortOfDischarge = "USMIA";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Argentina;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "Voyage";

			_ = header.Bills.AddNew();
		}
	}
}
