using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ScreeningMethodLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestScreeningMethodCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2SM", "IC2SM");
			helper.CreateCusCodeList("EUN", "IC2SM", "0586", "TestDescription1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2SM", "0587", "TestDescription2", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2SM", "0588", "An invalid code list outside of the date range", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(-1));

			Factory.Save();

			var screeningMethod = Factory.New<ScreeningMethod>();
			var list = screeningMethod.Lookups.CodeList;
			((BusinessObjectCollection)list).Load();

			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertSame(list, screeningMethod.Lookups.CodeList);
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "0586"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "0587"));
			});
		}
	}
}
