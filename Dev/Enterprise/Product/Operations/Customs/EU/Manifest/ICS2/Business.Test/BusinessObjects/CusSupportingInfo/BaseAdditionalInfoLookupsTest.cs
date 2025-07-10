using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public abstract class BaseAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2AI", "IC2AI");
			helper.CreateCusCodeList("EUN", "IC2AI", "0586", "TestDescription1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "0587", "TestDescription2", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "0588", "An invalid code list outside of the date range", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(-1));

			Factory.Save();

			var lookups = GetLookups();
			var list = lookups.CodeList;

			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertSame(list, lookups.CodeList);
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "0586"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "0587"));
			});
		}

		public void TestCodeListIsSorted()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("IC2AI", "IC2AI");
			helper.CreateCusCodeList("EUN", "IC2AI", "C10", "TestDescription1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "D00", "TestDescription2", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "B20", "TestDescription3", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "A30", "TestDescription4", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "D02", "TestDescription5", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "B00", "TestDescription6", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "S00", "TestDescription7", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "B01", "TestDescription8", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "A70", "TestDescription9", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", "IC2AI", "A00", "TestDescription10", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			Factory.Save();

			var lookups = GetLookups();
			var list = lookups.CodeList;
			var codes = list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code);

			AssertContainsExactElementsInExactOrder("Should be sorted by code in alphabetical order", new[] { "A00", "A30", "A70", "B00", "B01", "B20", "C10", "D00", "D02", "S00" }, codes);
		}

		public void TestSubTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			Factory.Save();

			var lookups = GetLookups();
			var list = lookups.SubTypeList;

			CombineAssertions(() =>
			{
				AssertEquals(5, list.Count);

				Assert(list.Cast<ICodeDescription>().Any(x => x.Code == "R1"));
				Assert(list.Cast<ICodeDescription>().Any(x => x.Code == "R2"));
				Assert(list.Cast<ICodeDescription>().Any(x => x.Code == "R4"));
				Assert(list.Cast<ICodeDescription>().Any(x => x.Code == "C1"));
				Assert(list.Cast<ICodeDescription>().Any(x => x.Code == "C2"));
			});
		}

		protected abstract BaseAdditionalInfoLookups GetLookups();
	}
}
