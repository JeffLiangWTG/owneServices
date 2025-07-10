using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Reports.Testing;

sealed class DefermentAccountNumberListProviderTest : TestCaseWithFactory
{
	public void TestGetCodeDescriptionPairList()
	{
		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.OH_Code = "ORG001";
		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.OH_Code = "ORG002";
		CreateAuthorisation(orgHeader1.PK, "000001", "DPO", "ABC");
		CreateAuthorisation(orgHeader1.PK, "000002", "DPO", "DEF");
		CreateAuthorisation(orgHeader1.PK, "000003", "DPA", "GHI");
		CreateAuthorisation(orgHeader2.PK, "000004", "DPO", "JKL");

		Factory.Save();
		var list = new DefermentAccountNumberListProvider().GetCodeDescriptionPairList();
		AssertEquals(@"000001 - ORG001 - ABC
000002 - ORG001 - DEF
000004 - ORG002 - JKL", list.ElementsAsString);
		AssertEquals(3, list.Count);
	}

	void CreateAuthorisation(ZGuid orgHeaderPK, ZString permitNumber, ZString authType, ZString description)
	{
		var header = Factory.New<CusAuthorisationHeader>();
		header.CPH_OH_PermitHolder = orgHeaderPK;
		header.CPH_Number = permitNumber;
		header.CPH_PermitDescription = description;
		header.CPH_Type = authType;
	}
}
