using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosAccGLAccountDescriptorValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateConsolidationAccountNumGreaterThanAccountNum()
		{
			AccGLAccountDescriptor consolidationAccount = Factory.New<AccGLAccountDescriptor>();
			consolidationAccount.AJ_Language = Core.Constants.Languages.English;
			consolidationAccount.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			consolidationAccount.AJ_LocalAccountNumber = "2000";
			consolidationAccount.AJ_ReportCategory = Core.Constants.AccountType.Consolidation;
			AccGLAccountDescriptor bshAccount = Factory.New<AccGLAccountDescriptor>();
			bshAccount.AJ_Language = Core.Constants.Languages.English;
			bshAccount.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			bshAccount.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			bshAccount.AJ_LocalAccountNumber = "2000.00.10";
			bshAccount.AJ_AJ_ConsolidationNum = consolidationAccount.PK;
			Assert("Should ensure Consolidation Account number greater than account number", bshAccount.AJ_LocalAccountNumberInfo.HasError("Account number must be less than Consolidation number"));
			bshAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			Assert("Should disable Consolidation Num validation", !bshAccount.AJ_LocalAccountNumberInfo.HasError("Account number must be less than Consolidation number"));
			bshAccount.AJ_AJ_ConsolidationNum = consolidationAccount.PK;
			bshAccount.AJ_LocalAccountNumber = "2001";
			Assert("Local account number does not have dot. Should ensure Consolidation Account number greater than account number", bshAccount.AJ_LocalAccountNumberInfo.HasError("Account number must be less than Consolidation number"));
		}
	}
}
