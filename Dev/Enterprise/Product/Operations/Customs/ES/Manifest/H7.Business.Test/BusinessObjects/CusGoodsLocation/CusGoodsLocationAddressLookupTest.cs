using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class CusGoodsLocationAddressLookupTest : TestCaseWithFactory
	{
		public void TestAuthorisationNumberList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, "XXX", "111", "111", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, "LOC", "222", "222", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "LOC", "333", "333", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, "LOC", "444", "444", yesterday, tomorrow);

			Factory.Save();

			var address = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			var authorisationNumberList = (Universal.ZZRefCusCodeListCombinedCollection)address.Lookups.AuthorisationNumberList;
			authorisationNumberList.Load();
			var codes = authorisationNumberList.Select(x => x.ZZD_Code);
			Assert("Should include the correct CusCode", codes.Contains("444"));
			Assert("Should not include the incorrect CusCode", !codes.Contains("111"));
			Assert("Should not include the incorrect CusCode", !codes.Contains("222"));
			Assert("Should not include the incorrect CusCode", !codes.Contains("333"));
		}
	}
}
