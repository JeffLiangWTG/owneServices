using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class GlbExternalPasswordLookups_GBTests : TestCaseWithFactory
	{
		public void TestBadges()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "GB";
			company.GC_Code = "CUK";

			var branch = company.Branches.AddNew();
			branch.GB_Code = "CKB";

			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting1 = badgeCodeSettings.AddNew();
			badgeCodeSetting1.BadgeCode = "ABC";
			badgeCodeSetting1.RL_PortCode = "GBLBA";
			badgeCodeSetting1.Direction = "EXP";
			badgeCodeSetting1.CSPCode = "CCSUK";
			badgeCodeSetting1.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Air;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var badgeCodeSettings2 = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting2 = badgeCodeSettings2.AddNew();
			badgeCodeSetting2.BadgeCode = "DEF";
			badgeCodeSetting2.RL_PortCode = "GBLBA";
			badgeCodeSetting2.Direction = "EXP";
			badgeCodeSetting2.CSPCode = "CCSUK";
			badgeCodeSetting2.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Air;

			var badgeCodeSettings3 = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting3 = badgeCodeSettings2.AddNew();
			badgeCodeSetting3.BadgeCode = "ABC";
			badgeCodeSetting3.RL_PortCode = "GBLBA";
			badgeCodeSetting3.Direction = "EXP";
			badgeCodeSetting3.CSPCode = "CCSUK";
			badgeCodeSetting3.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Air;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, badgeCodeSettings2);

			Factory.Save();

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = company.PK;
			AssertEquals(2, ((GlbExternalPasswordLookups_GB)password.Lookups).BadgeCodes.Count);
			Assert("Should be the cached", object.ReferenceEquals(company.Factory.GetCachedValue("GBExternalPasswordBadgeCodes_" + company.GC_Code, () => new CodeDescriptionPairList()), ((GlbExternalPasswordLookups_GB)password.Lookups).BadgeCodes));

			password = Factory.New<GlbExternalPassword_GB>();
			AssertEquals(0, ((GlbExternalPasswordLookups_GB)password.Lookups).BadgeCodes.Count);
		}

		public void TestEORIs()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "GB";
			company.GC_Code = "CUK";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Code = "COP";
			orgProxy.OH_FullName = "Corporation";
			var orgCusCode = orgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000");
			orgCusCode.OK_RN_NKCodeCountry = "GB";
			company.GC_OH_OrgProxy = orgProxy.PK;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			branch.GB_RL_NKHomePort = "GBLHR";
			branch.GB_BranchName = "Chris";
			var orgProxy1 = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy1.OH_Code = "COP1";
			orgProxy1.OH_FullName = "Corporation1";
			var orgCusCode1 = orgProxy1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000");
			orgCusCode1.OK_RN_NKCodeCountry = "GB";
			branch.GB_OH_OrgProxy = orgProxy1.PK;

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "DEF";
			branch2.GB_RL_NKHomePort = "GBLHR";
			branch2.GB_BranchName = "Chris2";
			var orgProxy2 = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy2.OH_Code = "COP2";
			orgProxy2.OH_FullName = "Corporation2";
			var orgCusCode2 = orgProxy2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000");
			orgCusCode2.OK_RN_NKCodeCountry = "GB";
			branch2.GB_OH_OrgProxy = orgProxy2.PK;

			Factory.Save();

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = company.PK;
			AssertEquals(3, ((GlbExternalPasswordLookups_GB)password.Lookups).EORIs.Count);
			Assert("Should be the cached", object.ReferenceEquals(company.Factory.GetCachedValue("GBExternalPasswordEORIs_" + company.GC_Code, () => new CodeDescriptionPairList()), ((GlbExternalPasswordLookups_GB)password.Lookups).EORIs));

			AssertEquals("GB123456789000 (COP1 / Corporation1)", ((GlbExternalPasswordLookups_GB)password.Lookups).EORIs[0].Description);

			AssertEquals("GB987654321000 (COP2 / Corporation2)", ((GlbExternalPasswordLookups_GB)password.Lookups).EORIs[1].Description);

			AssertEquals("GB123456789000 (COP / Corporation)", ((GlbExternalPasswordLookups_GB)password.Lookups).EORIs[2].Description);

			password = Factory.New<GlbExternalPassword_GB>();
			AssertEquals(0, ((GlbExternalPasswordLookups_GB)password.Lookups).EORIs.Count);
		}
	}
}
