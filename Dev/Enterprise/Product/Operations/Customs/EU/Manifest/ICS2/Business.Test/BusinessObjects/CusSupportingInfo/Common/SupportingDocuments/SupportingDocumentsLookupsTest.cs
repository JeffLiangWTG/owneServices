using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class SupportingDocumentsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEUICS2SupportingDocumentsCodes()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2DT", "IC2DT");

			helper.CreateCusCodeList("EUN", "IC2DT", "Y001", "Wholly obtained in Lebanon and transported directly from that country to the Community.", yesterday, tomorrow);
			helper.CreateCusCodeList("EUN", "IC2DT", "C647", "Confirmation of receipt", yesterday, tomorrow);

			Factory.Save();
			var header = Factory.NewWithValidTestData<SupportingDocument>();
			var list = header.Lookups.CodeList;
			((BusinessObjectCollection)list).Load();
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "Y001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "C647"));
		}
	}
}
