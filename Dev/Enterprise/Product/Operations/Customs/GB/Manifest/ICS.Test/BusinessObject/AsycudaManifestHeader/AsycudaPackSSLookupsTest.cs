using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	public class AsycudaPackSSLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackUQList()
		{
			var mockTypeCodes = new List<string> { "1A", "VA", "ZZ" };

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes,
				"Package types for test");
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				li,
				$"Package type {li} for test",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime));

			Factory.Save();

			var pack = Factory.New<AsycudaPackSS>();
			var lookup = pack.Lookups.PackUQList;
			AssertEquals(string.Join(", ", mockTypeCodes), lookup.CodesAsString);
		}
	}
}
