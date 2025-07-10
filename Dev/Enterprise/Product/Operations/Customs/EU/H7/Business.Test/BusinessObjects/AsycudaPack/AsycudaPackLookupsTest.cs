using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaPackLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestPackTypeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "pack type");

			var mockTypeCodes = new List<string> { "ABC", "ABD", "ABE" };
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, li,
				"desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime));

			Factory.Save();

			var pack = Factory.New<AsycudaPack>();
			var packUQList = pack.Lookups.PackUQList;

			CombineAssertions(() =>
			{
				AssertSame("Cached", packUQList, pack.Lookups.PackUQList);
				AssertContainsExactElementsInAnyOrder("items exist in packtype list", new[] { "ABC", "ABD", "ABE" }, packUQList.GetAllCodesZString());
			});
		}
	}
}
