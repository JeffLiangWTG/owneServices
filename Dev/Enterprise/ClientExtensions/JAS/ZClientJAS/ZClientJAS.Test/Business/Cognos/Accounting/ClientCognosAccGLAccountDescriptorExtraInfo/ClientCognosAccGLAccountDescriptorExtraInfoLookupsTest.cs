using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class ClientCognosAccGLAccountDescriptorExtraInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCognosAccounts()
		{
			CognosAccGLAccountDescriptor account0 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount);
			account0.AJ_ReportType = "xyz";
			CognosAccGLAccountDescriptor account1 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount);
			CognosAccGLAccountDescriptor account2 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.Alternate);
			CognosAccGLAccountDescriptor account3 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, CognosAccGLAccountDescriptor.CognosSubClassificationAccountType);
			CognosAccGLAccountDescriptor account4 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.Consolidation);
			CognosAccGLAccountDescriptor account5 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.Header);
			CognosAccGLAccountDescriptor account6 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount);
			CognosAccGLAccountDescriptor account7 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.Total);
			CognosAccGLAccountDescriptor account8 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, "CFW");
			CognosAccGLAccountDescriptor account9 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount);
			CognosAccGLAccountDescriptor account10 = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.Ukrainian, Core.Constants.AccountType.BalanceSheetAccount);
			ExtraInfo.Lookups.CognosAccounts.Load();
			AssertEquals("Should not include Cognos HDR, Accounts with other languages and the Current Account", 7, ExtraInfo.Lookups.CognosAccounts.Count);
			Assert("Should be included", ExtraInfo.Lookups.CognosAccounts.Contains(account1.PK));
			Assert("Should be included", ExtraInfo.Lookups.CognosAccounts.Contains(account2.PK));
			Assert("Should be included", ExtraInfo.Lookups.CognosAccounts.Contains(account3.PK));
			Assert("Should be included", ExtraInfo.Lookups.CognosAccounts.Contains(account4.PK));
			Assert("Should be included", ExtraInfo.Lookups.CognosAccounts.Contains(account6.PK));
			Assert("Should be included", ExtraInfo.Lookups.CognosAccounts.Contains(account7.PK));
			Assert("Should be included", ExtraInfo.Lookups.CognosAccounts.Contains(account8.PK));
		}

		public void TestDebtorGroups()
		{
			AssertEquals(typeof(OrgDebtorGroupCollection), ExtraInfo.Lookups.DebtorGroups.GetType());
		}

		public void TestCreditorGroups()
		{
			AssertEquals(typeof(OrgCreditorGroupCollection), ExtraInfo.Lookups.CreditorGroups.GetType());
		}

		public void TestAccountAgeList()
		{
			AssertEquals("There should be 3 elements in the list", 4, ExtraInfo.Lookups.AccountAgeList.Count);
			AssertEquals("Account aged between 0 - 30 days", ExtraInfo.Lookups.AccountAgeList.GetDescriptionFromCode("30"));
			AssertEquals("Account aged between 31 - 60 days", ExtraInfo.Lookups.AccountAgeList.GetDescriptionFromCode("60"));
			AssertEquals("Account aged between 61 - 90 days", ExtraInfo.Lookups.AccountAgeList.GetDescriptionFromCode("90"));
			AssertEquals("Account aged 90+ days", ExtraInfo.Lookups.AccountAgeList.GetDescriptionFromCode("++"));
		}

		#region Implementation
		CognosAccGLAccountDescriptorExtraInfo ExtraInfo
		{
			get
			{
				if (fExtraInfo == null)
				{
					CognosAccGLAccountDescriptor cognosGLAccount = CreateNewCognosAccGLAccountDescriptor(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, CognosAccGLAccountDescriptor.CognosSubClassificationAccountType);
					fExtraInfo = cognosGLAccount.ExtraInfo;
				}

				return fExtraInfo;
			}
		}

		CognosAccGLAccountDescriptor CreateNewCognosAccGLAccountDescriptor(ZString language, ZString reportType)
		{
			CognosAccGLAccountDescriptor result = Factory.New<CognosAccGLAccountDescriptor>();
			result.AJ_Language = language;
			result.AJ_ReportCategory = reportType;
			result.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			return result;
		}

		CognosAccGLAccountDescriptorExtraInfo fExtraInfo;
		#endregion
	}
}
