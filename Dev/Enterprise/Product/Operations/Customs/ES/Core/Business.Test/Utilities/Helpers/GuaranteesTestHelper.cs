using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class GuaranteesTestHelper
	{
		public static void CreateGuaranteesHeader(BusinessObjectFactory factory, ZGuid permitHolderId)
		{
			var grnGuaranteesCodes = new ZString[]
			{
				"22ESAGL9990000096", "22ESAGP9990000096", "22ESAGQ9990000096", "22ESAGR9990000096", "22ESAGS9990000096", "22ESAGT9990000096", "22ESAGD9990000096", "22ESAGW9990000096",
				"22ESCGL9990000096", "22ESCGP9990000096", "22ESCGQ9990000096", "22ESCGR9990000096", "22ESCGS9990000096", "22ESCGT9990000096", "22ESCGD9990000096", "22ESCGW9990000096"
			};

			foreach (var guaranteeCode in grnGuaranteesCodes)
			{
				CreateGuaranteesHeaderDetail(factory, permitHolderId, guaranteeCode, false);
			}
			CreateGuaranteesHeaderDetail(factory, permitHolderId, "22ESAGL9990000050", true);
			CreateGuaranteesHeaderDetail(factory, permitHolderId, "22ESCGL9990000050", true);

			factory.Save();
		}

		public static void CreateGuaranteesHeaderDetail(BusinessObjectFactory factory, ZGuid permitHolderId, ZString guaranteeCode, bool withEndDate = false)
		{
			var guaranteeHeader1 = factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Number = guaranteeCode;
			guaranteeHeader1.CPH_OH_PermitHolder = permitHolderId;
			guaranteeHeader1.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader1.CPH_Type = EUGuaranteeTypeList.Codes.IMP;
			guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
			if (withEndDate)
			{
				guaranteeHeader1.CPH_EndDate = new ZDate(2021, 7, 15);
			}
		}

		public static void CreateGuaranteesHeaderWithRuleCUS(BusinessObjectFactory factory, ZGuid permitHolderId)
		{
			CreateGuaranteesHeaderDetailWithRuleCUS(factory, permitHolderId, "22ESAGP9990000096", "Office");
			factory.Save();
		}

		public static CusGuaranteeHeader CreateGuaranteesHeaderDetailWithRuleCUS(BusinessObjectFactory factory, ZGuid permitHolderId, ZString guaranteeCode, ZString office)
		{
			var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = guaranteeCode;
			guaranteeHeader.CPH_OH_PermitHolder = permitHolderId;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.IMP;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
			guaranteeHeader = CreateGuaranteesRuleDetailWithRuleCUS(guaranteeHeader, office);

			return guaranteeHeader;
		}

		public static CusGuaranteeHeader CreateGuaranteesRuleDetailWithRuleCUS(CusGuaranteeHeader guaranteeHeader, ZString office)
		{
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.CUS;
			rule.CPR_ValueFrom = office;

			return guaranteeHeader;
		}

		public static void CreateGuaranteeForEntryInstruction(JobDeclaration declaration, params (ZGuid entryInstructionPK, string bondNumber)[] guaranteesInfo)
		{
			foreach (var (entryInstructionPK, bondNumber) in guaranteesInfo)
			{
				var guarantee = declaration.Guarantees.AddNew();
				guarantee.PW_BondNumber = bondNumber;
				guarantee.EntryInstructionID = entryInstructionPK;
			}
		}

		public static void CreateMultipleGuaranteesForEntryInstruction(JobDeclaration declaration, ZGuid entryInstructionPK, ZString[] bondNumbers)
		{
			foreach (var bondNumber in bondNumbers)
			{
				var guarantee = declaration.Guarantees.AddNew();
				guarantee.PW_BondNumber = bondNumber;
				guarantee.EntryInstructionID = entryInstructionPK;
			}
		}
	}
}
