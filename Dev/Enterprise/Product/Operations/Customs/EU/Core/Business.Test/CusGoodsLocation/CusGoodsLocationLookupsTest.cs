using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestUnlocodeList()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			NUnit.Framework.Assert.That(cusGoodsLocation.Lookups.UnlocodeList, NUnit.Framework.Is.TypeOf<RefUNLOCOCollection>(), "Type");
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = new CustomsOfficeCodeTestHelper(helper);
			Factory.Save();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.Parent = Factory.NewWithValidTestData<JobDeclaration>();
			cusGoodsLocation.CGL_LocationUse = "DEP";
			var collection = cusGoodsLocation.Lookups.CustomsOfficeList;
			collection.Load();
			var expectedOfficeCodes = new ZString[] { "DE004323", "DE003478", "IT008734", "IT009278", "XI005342" };
			NUnit.Framework.Assert.That(collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code), NUnit.Framework.Is.EquivalentTo(expectedOfficeCodes));
		}
	}
}
