using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class OrgCusCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOK_CodeType_List()
		{
			var ediOrgCusCode = Factory.New<EDIOrgCusCode>();
			var ediOrgCusCodeLookups = new EDIOrgCusCodeLookups(ediOrgCusCode);
			AssertEquals(true, ediOrgCusCodeLookups.OK_CodeType_List.ContainsCode("ABM"));
			AssertEquals("ABM Customs Company Code", ediOrgCusCodeLookups.OK_CodeType_List.GetDescriptionFromCode("ABM"));
		}
	}
}