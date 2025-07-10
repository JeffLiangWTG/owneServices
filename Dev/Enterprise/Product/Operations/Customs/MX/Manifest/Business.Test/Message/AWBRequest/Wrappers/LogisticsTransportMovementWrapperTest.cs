using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class LogisticsTransportMovementWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestLogisticsTransportMovementWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], "ORG");
			ILogisticsTransportMovement ltm = wrapper.MasterConsignment.IncludedHouseConsignment.SpecifiedLogisticsTransportMovement;
			IEvent arrivalEvent = ltm.ArrivalEvent;
			IEvent departureEvent = ltm.DepartureEvent;

			CombineAssertions(() =>
			{
				AssertEquals("On-Carriage", ltm.StageCode);
				AssertEquals("Voyage", ltm.ID);
				AssertEquals("Carrier", ltm.NameOfTransport);

				AssertEquals("2021-05-11T00:00:00", departureEvent.Date.ToString("yyyy-MM-ddTHH:mm:ss"));
				AssertEquals("ACA", departureEvent.LocationCode);
				AssertEquals("Acapulco", departureEvent.LocationName);

				AssertEquals("2021-05-31T00:00:00", arrivalEvent.Date.ToString("yyyy-MM-ddTHH:mm:ss"));
				AssertEquals("MIA", arrivalEvent.LocationCode);
				AssertEquals("Miami", arrivalEvent.LocationName);
			});
		}

		void PopulateManifestHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Carrier";

			header.AMA_E_ARV = new ZDate(2021, 05, 31);
			header.AMA_E_DEP = new ZDate(2021, 05, 11);
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_RL_NKPortOfLoading = "MXACA";
			header.AMA_RL_NKPortOfDischarge = "USMIA";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "Voyage";

			_ = header.Bills.AddNew();
		}
	}
}
