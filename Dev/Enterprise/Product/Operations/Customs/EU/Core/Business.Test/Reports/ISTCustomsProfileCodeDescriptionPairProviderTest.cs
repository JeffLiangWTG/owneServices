using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Reports.Testing
{
	class ISTCustomsProfileCodeDescriptionPairProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "ORG001";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "ORG002";
			CreateAuthorisation(orgHeader1.PK, "000001", "IST", "LAD", "FRC");
			CreateAuthorisation(orgHeader1.PK, "000002", "IST");
			CreateAuthorisation(orgHeader1.PK, "000003");
			CreateAuthorisation(orgHeader2.PK, "000004", "IST", "FRC");
			Factory.Save();
			CombineAssertions(() =>
			{
				var provider = new ISTCustomsProfileCodeDescriptionPairProvider();
				var list = provider.GetCodeDescriptionPairList();
				AssertEquals(3, list.Count);
				list = provider.GetDependenceCodeDescriptionPairList(orgHeader1.PK.ToString());
				AssertEquals(2, list.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "000001", "000002" }, list.GetAllCodes());
				list = provider.GetDependenceCodeDescriptionPairList(orgHeader2.PK.ToString());
				AssertEquals(1, list.Count);
			});
		}

		void CreateAuthorisation(ZGuid orgHeaderPK, ZString permitNumber, params ZString[] ruleValues)
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_OH_PermitHolder = orgHeaderPK;
			header.CPH_Number = permitNumber;
			header.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			foreach (var ruleValue in ruleValues)
			{
				var rule = header.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = "USE";
				rule.CPR_ValueFrom = ruleValue;
			}
		}
	}
}
