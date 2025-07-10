using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class LocationWrapperTest : TestCaseWithFactory
	{
		public void TestLocationWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "ARBUE";
			header.AMA_RL_NKPortOfDischarge = "USMIA";
			_ = header.Bills.AddNew();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], MessageSubTypeCodes.Codes.Original);

			IMasterConsignment masterConsignment = wrapper.MasterConsignment;
			IHouseConsignment houseConsignment = masterConsignment.IncludedHouseConsignment;

			ILocation masterOriginLocation = masterConsignment.OriginLocation;
			ILocation masterFinalDestinationLocation = masterConsignment.FinalDestinationLocation;
			ILocation houseOriginLocation = houseConsignment.OriginLocation;
			ILocation houseFinalDestinationLocation = houseConsignment.FinalDestinationLocation;

			CombineAssertions(() =>
			{
				AssertEquals("EZE", masterOriginLocation.ID);
				AssertEquals("EZE", houseOriginLocation.ID);
				AssertEquals("Buenos Aires", masterOriginLocation.Name);
				AssertEquals("Buenos Aires", houseOriginLocation.Name);
				AssertEquals("MIA", masterFinalDestinationLocation.ID);
				AssertEquals("MIA", houseFinalDestinationLocation.ID);
				AssertEquals("Miami", masterFinalDestinationLocation.Name);
				AssertEquals("Miami", houseFinalDestinationLocation.Name);
			});
		}
	}
}
