using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class LocationWrapperTest : TestCaseWithFactory
	{
		public void TestLocationWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "MXACA";
			header.AMA_RL_NKPortOfDischarge = "USMIA";
			_ = header.Bills.AddNew();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], "ORG");

			IMasterConsignment masterConsignment = wrapper.MasterConsignment;
			IHouseConsignment houseConsignment = masterConsignment.IncludedHouseConsignment;

			ILocation masterOriginLocation = masterConsignment.OriginLocation;
			ILocation masterFinalDestinationLocation = masterConsignment.FinalDestinationLocation;
			ILocation houseOriginLocation = houseConsignment.OriginLocation;
			ILocation houseFinalDestinationLocation = houseConsignment.FinalDestinationLocation;

			CombineAssertions(() =>
			{
				AssertEquals("ACA", masterOriginLocation.ID);
				AssertEquals("ACA", houseOriginLocation.ID);
				AssertEquals("Acapulco", masterOriginLocation.Name);
				AssertEquals("Acapulco", houseOriginLocation.Name);
				AssertEquals("MIA", masterFinalDestinationLocation.ID);
				AssertEquals("MIA", houseFinalDestinationLocation.ID);
				AssertEquals("Miami", masterFinalDestinationLocation.Name);
				AssertEquals("Miami", houseFinalDestinationLocation.Name);
			});
		}
	}
}
