using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class FRNctsGuaranteeRefresherTest : TestCaseWithFactory
	{
		public void TestOtherGuaranteeReferencePopulatedWhenAble()
		{
			var principal = CreateOrgHeader("PRINCIPAL");

			var guaranteeHeader = CreateGuaranteeHeader("19860101", EUGuaranteeTypeList.Codes.COD, EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee);
			guaranteeHeader.CPH_OH_PermitHolder = principal.PK;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			var refresher = nctsHeader.GuaranteeRefresher;
			refresher.PopulateGuaranteeWithFallbacks();

			var guarantee = nctsHeader.MovementHeader.Guarantees.Single();
			AssertEquals("Guarantee number should populate PW_BondNumber.", "19860101", guarantee.PW_BondNumber);
			AssertEquals("Guarantee number should populate PW_BondNumber.", ZString.Empty, guarantee.PW_BondNumber2);

			guaranteeHeader.CPH_SubType = EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			Factory.Save();

			nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
			refresher.PopulateGuaranteeWithFallbacks();

			guarantee = nctsHeader.MovementHeader.Guarantees.Single();
			AssertEquals("Cash Deposit guarantee number should specifically populate PW_BondNumber2 instead of PW_BondNumber.", "19860101", guarantee.PW_BondNumber2);
			AssertEquals("Cash Deposit guarantee number should specifically populate PW_BondNumber2 instead of PW_BondNumber.", ZString.Empty, guarantee.PW_BondNumber);
		}

		public void TestGuaranteeRefresherGuaranteeTypeFilter()
		{
			var principal = CreateOrgHeader("PRINCIPAL");

			var guaranteeHeader1 = CreateGuaranteeHeader("19860101", EUGuaranteeTypeList.Codes.TST);
			var guaranteeHeader2 = CreateGuaranteeHeader("19860102", EUGuaranteeTypeList.Codes.TRA);
			var guaranteeHeader3 = CreateGuaranteeHeader("19860103", EUGuaranteeTypeList.Codes.COD);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;

			var refresher = nctsHeader.GuaranteeRefresher;
			nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
			guaranteeHeader1.CPH_OH_PermitHolder = principal.PK;
			guaranteeHeader2.CPH_OH_PermitHolder = principal.PK;
			guaranteeHeader3.CPH_OH_PermitHolder = principal.PK;
			refresher.PopulateGuaranteeWithFallbacks();

			AssertEquals("Guarantee with CPH_Type COD is retrieved for France due to the GuaranteeTypeFilter.", "19860103", nctsHeader.GetEffectiveGuarantees().Single().PW_BondNumber);
		}

		public void TestGuaranteeRefresherGuaranteeSubTypeFilter()
		{
			var org1 = CreateOrgHeader("ORG1");
			var guaranteeHeader1 = CreateGuaranteeHeader("19860101", EUGuaranteeTypeList.Codes.COD, ZString.Empty);
			var guaranteeHeader2 = CreateGuaranteeHeader("19860102", EUGuaranteeTypeList.Codes.COD, "1");

			Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;

			var refresher = nctsHeader.GuaranteeRefresher;
			guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
			refresher.PopulateGuaranteeWithFallbacks();

			AssertEquals("Guarantee with non empty CPH_SubType is retrieved", "19860102", nctsHeader.GetEffectiveGuarantees().Single().PW_BondNumber);
		}

		OrgHeader CreateOrgHeader(ZString code)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = code;
			const string regNo1 = "12345";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, regNo1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return orgHeader;
		}

		CusGuaranteeHeader CreateGuaranteeHeader(ZString guaranteeNumber, ZString guaranteeType, string guaranteeSubType = "1")
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = guaranteeNumber;
			guaranteeHeader.CPH_Type = guaranteeType;
			guaranteeHeader.CPH_SubType = guaranteeSubType;
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule.CPR_ValueFrom = "#1";
			return guaranteeHeader;
		}
	}
}
