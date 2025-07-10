using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class OrgCusCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestOK_CodeType_List()
		{
			UPEOrgCusCode uPEOrgCusCode = Factory.New<UPEOrgCusCode>();
			UPEOrgCusCodeLookups uPEOrgCusCodeLookups = new UPEOrgCusCodeLookups(uPEOrgCusCode);
			AssertEquals("UPE Specific is first in the list", UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, uPEOrgCusCodeLookups.OK_CodeType_List[0].Code);
			AssertEquals("Unique Account Number", uPEOrgCusCodeLookups.OK_CodeType_List[0].Description);
			Assert("At least one generic code", uPEOrgCusCodeLookups.OK_CodeType_List.ContainsCode(OrgCusCode.CodeTypes.LegacySystemCode));
		}
	}
}
