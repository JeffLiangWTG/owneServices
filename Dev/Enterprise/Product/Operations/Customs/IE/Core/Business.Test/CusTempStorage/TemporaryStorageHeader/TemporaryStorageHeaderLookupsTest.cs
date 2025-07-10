using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsStatusList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var customsStatusList = header.Lookups.CustomsStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "ACC, REJ, INV, REG, AMR", ((CodeDescriptionPairList)customsStatusList).CodesAsString);
			});
		}

		public void TestManifestTypeList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var manifestTypeList = header.Lookups.ManifestTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "V1, V2", manifestTypeList.CodesAsString);
				AssertSame("Cached", manifestTypeList, header.Lookups.ManifestTypeList);
			});
		}

		public void TestRepresentativeStatusCodeList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var representativeStatusCodeList = header.Lookups.RepresentativeStatusCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "2, 3", representativeStatusCodeList.CodesAsString);
				AssertSame("Cached", representativeStatusCodeList, header.Lookups.RepresentativeStatusCodeList);
			});
		}

		public void TestMeansIdentityTypeList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var meansIdentityTypeList = header.Lookups.MeansIdentityTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "10, 11, 20, 30, 40, 41, 80, 81", meansIdentityTypeList.CodesAsString);
				AssertSame("Cached", meansIdentityTypeList, header.Lookups.MeansIdentityTypeList);
			});
		}

		public void TestTransportTypeList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var lookups = header.Lookups;

			header.AMA_TransportMode = "SEA";
			AssertEquals("When Transport Mode is SEA", "10, 11", lookups.TransportTypeList.CodesAsString);

			header.AMA_TransportMode = "AIR";
			AssertEquals("When Transport Mode is AIR", "40, 41", lookups.TransportTypeList.CodesAsString);

			header.AMA_TransportMode = "IWT";
			AssertEquals("When Transport Mode is IWT", "80, 81", lookups.TransportTypeList.CodesAsString);

			header.AMA_TransportMode = "RAI";
			AssertEquals("When Transport Mode is RAI", "20", lookups.TransportTypeList.CodesAsString);

			header.AMA_TransportMode = "ROA";
			AssertEquals("When Transport Mode is ROA", "30", lookups.TransportTypeList.CodesAsString);
		}
	}
}
