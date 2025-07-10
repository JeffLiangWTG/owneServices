using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ImportUCC5OfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var lookups1 = new ImportUCC5OfficeCodeLookups(Factory.New<OfficeCode>());
			var list1 = lookups1.CY_CodeList;

			CombineAssertions(() =>
			{
				AssertEquals("Valid Codes", "DSC, PRE, SVO", list1.CodesAsString);
				AssertEquals("DSC Description", "Office of Discharge", list1.GetDescriptionFromCode("DSC"));
				AssertEquals("PRE Description", "[5/26] Customs office of presentation", list1.GetDescriptionFromCode("PRE"));
				AssertEquals("SVO Description", "[5/27] Supervising customs office", list1.GetDescriptionFromCode("SVO"));
			});

			var lookups2 = new ImportUCC5OfficeCodeLookups(Factory.New<OfficeCode>());
			var list2 = lookups2.CY_CodeList;
			AssertSame("Cached", list1, list2);
		}
	}
}
