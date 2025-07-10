using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ImportOfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			CombineAssertions(() =>
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				jobDeclaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				var office1 = jobDeclaration.CustomsOffices.AddNew();
				var office2 = jobDeclaration.CustomsOffices.AddNew();
				var lookups1 = new ImportOfficeCodeLookups(office1);
				var lookups2 = new ImportOfficeCodeLookups(office2);
				var list1 = lookups1.CY_CodeList;
				var list2 = lookups2.CY_CodeList;
				AssertEquals("Valid Codes", "DSC, PRE, SVO", list1.CodesAsString);
				AssertSame("Cached", list1, list2);
			});
		}
	}
}
