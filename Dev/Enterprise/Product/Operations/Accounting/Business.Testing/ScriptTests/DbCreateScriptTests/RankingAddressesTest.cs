using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests;

public class RankingAddressesTest : ScriptTest
{
	public void TestRankingAddressWithGlobalOrgIsTrue()
	{
		SetUpForRankingAddressWithGlobalOrgIsTrue();
		var result = RunScript(true, "AU", "EN", true, false, false, false);
		AssertEquals($"The ARM address PK should be '{addressARM.PK}'", addressARM.PK, new ZGuid(result));

		result = RunScript(true, "AU", "EN", false, true, false, false);
		AssertEquals($"The ARM address PK should be '{addressAPM.PK}'", addressAPM.PK, new ZGuid(result));

		result = RunScript(true, "AU", "EN", false, false, true, false);
		AssertEquals($"The ARM address PK should be '{addressPST.PK}'", addressPST.PK, new ZGuid(result));

		result = RunScript(true, "AU", "EN", false, false, false, true);
		AssertEquals($"The ARM address PK should be '{addressOFC.PK}'", addressOFC.PK, new ZGuid(result));
	}

	public void TestRankingAddressWithGlobalOrgIsFalse()
	{
		SetUpForRankingAddressWithGlobalOrgIsFalse();
		var result = RunScript(false, "AU", "EN", true, false, false, false);
		AssertEquals($"The ARM address PK should be '{addressARM1.PK}'", addressARM1.PK, new ZGuid(result));

		result = RunScript(false, "AU", "EN", false, true, false, false);
		AssertEquals($"The ARM address PK should be '{addressAPM.PK}'", addressAPM.PK, new ZGuid(result));

		result = RunScript(false, "AU", "EN", false, false, true, false);
		AssertEquals($"The ARM address PK should be '{addressPST.PK}'", addressPST.PK, new ZGuid(result));

		result = RunScript(false, "AU", "EN", false, false, false, true);
		AssertEquals($"The ARM address PK should be '{addressOFC1.PK}'", addressOFC1.PK, new ZGuid(result));
	}

	public void TestRankingAddressWithNotSetMainAddress()
	{
		SetUpRankingAddressWithNotSetMainAddress();
		var result = RunScript(true, "AU", "EN", true, false, false, false);
		AssertEquals($"The ARM address PK should be '{addressARM1.PK}'", addressARM1.PK, new ZGuid(result));

		result = RunScript(true, "AU", "EN", false, true, false, false);
		AssertEquals($"The ARM address PK should be '{addressAPM.PK}'", addressAPM.PK, new ZGuid(result));

		result = RunScript(true, "AU", "EN", false, false, true, false);
		AssertEquals($"The ARM address PK should be NULL", DBNull.Value, result);

		result = RunScript(true, "AU", "EN", false, false, false, true);
		AssertEquals($"The ARM address PK should be '{addressOFC1.PK}'", addressOFC1.PK, new ZGuid(result));
	}

	void SetUpRankingAddressWithNotSetMainAddress()
	{
		addressARM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, false);
		addressARM.OA_RL_NKRelatedPortCode = "CNSYD";
		addressARM.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressARM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, true);
		addressARM1.OA_RL_NKRelatedPortCode = "AUSYD";
		addressARM1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		addressAPM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, true);
		addressAPM.OA_RL_NKRelatedPortCode = "CNAAA";
		addressAPM.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressAPM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, false);
		addressAPM1.OA_RL_NKRelatedPortCode = "AUAAA";
		addressAPM1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		addressPST = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, false);
		addressPST.OA_RL_NKRelatedPortCode = "AUBBB";
		addressPST.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressPST1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, false);
		addressPST1.OA_RL_NKRelatedPortCode = "AUBBB";
		addressPST1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		addressOFC = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, false);
		addressOFC.OA_RL_NKRelatedPortCode = "CNSYD";
		addressOFC.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressOFC1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, true);
		addressOFC1.OA_RL_NKRelatedPortCode = "AUSYD";
		addressOFC1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		Factory.Save();
	}

	void SetUpForRankingAddressWithGlobalOrgIsTrue()
	{
		addressARM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, true);
		addressAPM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, true);
		addressPST = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, true);
		addressOFC = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, true);
		addressARM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, false);
		addressAPM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, false);
		addressPST1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, false);
		addressOFC1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, false);
		addressARM.OA_RL_NKRelatedPortCode = "AUSYD";
		addressARM1.OA_RL_NKRelatedPortCode = "AUSYD";
		addressAPM.OA_RL_NKRelatedPortCode = "AUAAA";
		addressAPM1.OA_RL_NKRelatedPortCode = "AUAAA";
		addressPST.OA_RL_NKRelatedPortCode = "AUBBB";
		addressPST1.OA_RL_NKRelatedPortCode = "AUBBB";
		addressOFC.OA_RL_NKRelatedPortCode = "AUSYD";
		addressOFC1.OA_RL_NKRelatedPortCode = "AUSYD";

		Factory.Save();
	}

	void SetUpForRankingAddressWithGlobalOrgIsFalse()
	{
		addressARM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, false);
		addressARM.OA_RL_NKRelatedPortCode = "CNSYD";
		addressARM.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressARM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, true);
		addressARM1.OA_RL_NKRelatedPortCode = "AUSYD";
		addressARM1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		addressAPM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, true);
		addressAPM.OA_RL_NKRelatedPortCode = "AUAAA";
		addressAPM.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressAPM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, false);
		addressAPM1.OA_RL_NKRelatedPortCode = "AUAAA";
		addressAPM1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		addressPST = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, true);
		addressPST.OA_RL_NKRelatedPortCode = "AUBBB";
		addressPST.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressPST1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, false);
		addressPST1.OA_RL_NKRelatedPortCode = "AUBBB";
		addressPST1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		addressOFC = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, false);
		addressOFC.OA_RL_NKRelatedPortCode = "CNSYD";
		addressOFC.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;
		addressOFC1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, true);
		addressOFC1.OA_RL_NKRelatedPortCode = "AUSYD";
		addressOFC1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

		Factory.Save();
	}

	object RunScript(bool isGlobalOrg, string currentCountryCode, string orgLanguage, bool isARM, bool isAPM, bool isPST, bool isOFC)
	{
		var sql = $@"SELECT * FROM {ScriptDbName}.dbo.RankingAddresses('{company.PK}', '{branch.PK}', '{orgHeader.PK}', {(isGlobalOrg ? "1" : "0")}, '{currentCountryCode}', '{orgLanguage}', {(isARM ? 1 : 0)}, {(isAPM ? 1 : 0)}, {(isPST ? 1 : 0)}, {(isOFC ? 1 : 0)})";
		var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
		AssertEquals("The result should contain 1 rows", 1, result.Rows.Count);
		return result.Rows[0]["Result"];
	}

	protected string ScriptDbName
	{
		get { return Db.DatabaseName; }
	}

	protected override void SetUp()
	{
		base.SetUp();

		company = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
		branch = company.Branches[0];
		orgHeader = company.OrgProxy;
	}

	GlbCompany company;
	GlbBranch branch;
	OrgHeader orgHeader;
	OrgAddress addressARM;
	OrgAddress addressAPM;
	OrgAddress addressPST;
	OrgAddress addressOFC;
	OrgAddress addressARM1;
	OrgAddress addressAPM1;
	OrgAddress addressPST1;
	OrgAddress addressOFC1;
}
