using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OrgSupBuyLinkTrnModeAddInfoBizObjLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2019, 12, 26)]
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CNJ", "Nanjing Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CSH", "Shanghai Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 25));
			helper.CreateNewOrGetExistingCusCodeList("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CNY", "New York Office", new ZDateTime(2019, 5, 25), new ZDateTime(2019, 12, 30));
			Factory.Save();
			var bizObj = new OrgSupBuyLinkTrnModeAddInfoBizObj(Factory);
			var offices = bizObj.Lookups.CustomsOfficeList;
			offices.Load();
			AssertEquals(1, offices.Count);
			AssertEquals("Customs Office must be 'CNJ'", "CNJ", offices[0].ZZD_Code);
		}
	}
}
