using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	internal class CognosAccGLAccountDescriptorValidationTest : BusinessObjectValidationTestCase
	{
		CognosAccGLAccountDescriptor CognosGLAccount
		{
			get
			{
				if (fCognosGLAccount == null)
				{
					fCognosGLAccount = Factory.New<CognosAccGLAccountDescriptor>();
				}

				return fCognosGLAccount;
			}
		}

		CognosAccGLAccountDescriptor fCognosGLAccount;
		public void TestValidateAJ_ConsolidationNum()
		{
			CognosGLAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			CognosGLAccount.AJ_LocalAccountNumber = "1001";
			CognosGLAccount.Validation.ValidateAJ_AJ_ConsolidationNum();
			Assert("Not a cognos sub-account", !CognosGLAccount.AJ_AJ_ConsolidationNumInfo.HasErrors());
			CognosGLAccount.AJ_LocalAccountNumber = "1001.1";
			CognosGLAccount.Validation.ValidateAJ_AJ_ConsolidationNum();
			Assert("Should have error", CognosGLAccount.AJ_AJ_ConsolidationNumInfo.HasError("Consolidation Account has to be specified for every Cognos sub-account (Local account number containing dot \".\")"));
			CognosGLAccount.AJ_AJ_ConsolidationNum = Factory.New<AccGLAccountDescriptor>().PK;
			Assert("Should have error", CognosGLAccount.AJ_AJ_ConsolidationNumInfo.HasError("Cannot attach to this Consolidation account. Consolidation Account has to have the Local account number '1001'"));
			CognosGLAccount.ConsolidationNum.AJ_ReportCategory = Core.Constants.AccountType.Consolidation;
			CognosGLAccount.ConsolidationNum.AJ_LocalAccountNumber = "1001";
			CognosGLAccount.Validation.ValidateAJ_AJ_ConsolidationNum();
			Assert("Should not have error", !CognosGLAccount.AJ_AJ_ConsolidationNumInfo.HasErrors());
		}

		public void TestValidationHelper()
		{
			AssertEquals(typeof(CognosAccGLAccountDescriptorValidationHelper), Factory.New<AccGLAccountDescriptor>().Validation.ValidationHelper.GetType());
		}
	}
}
