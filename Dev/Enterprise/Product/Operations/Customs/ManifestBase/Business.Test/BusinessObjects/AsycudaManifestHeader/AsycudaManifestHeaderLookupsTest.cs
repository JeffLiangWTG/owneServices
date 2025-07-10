using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ManifestBase.Testing
{
	internal class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVessels()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_VesselName = "VESSEL";
			header.AMA_LloydsNumber = "9832343";
			header.AMA_RadioCallSign = "CALLME";
			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Australia;
			var collection = new AsycudaManifestHeaderLookups(header).Vessels;
			CombineAssertions(() =>
			{
				AssertType<RefVesselCollection>("Type", collection);
				AssertEquals("Vessel Name:Property", "VESSEL", collection.FilterBusinessObjectDefaults["Vessel Name:Property"].Value);
				AssertEquals("Lloyds Number:Property", "9832343", collection.FilterBusinessObjectDefaults["Lloyds Number:Property"].Value);
				AssertEquals("Radio Call Sign:Property", "CALLME", collection.FilterBusinessObjectDefaults["Radio Call Sign:Property"].Value);
				AssertEquals("Country of Registration:Property", "AU", collection.FilterBusinessObjectDefaults["Country of Registration:Property"].Value);
			});
		}

		public void TestConveyanceNationalities()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertType<RefCountryCollection>(header.Lookups.ConveyanceNationalities);
		}
	}
}
