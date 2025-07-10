using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class TransportMeanLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTruckKindList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType("C1307", "C1307 LIst", Core.Constants.CountryCodes.Israel);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "C1307", "2", "Test code2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "C1307", "1", "Test code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";
			var transportMean = (TransportMean)header.TransportMeans.AddNew();

			var codeList = transportMean.Lookups.TruckKindList as CodeDescriptionPairList;
			AssertEquals(2, codeList.Count);
			AssertEquals("list should be sorted", "1, 2", codeList.CodesAsString);
		}
	}
}
