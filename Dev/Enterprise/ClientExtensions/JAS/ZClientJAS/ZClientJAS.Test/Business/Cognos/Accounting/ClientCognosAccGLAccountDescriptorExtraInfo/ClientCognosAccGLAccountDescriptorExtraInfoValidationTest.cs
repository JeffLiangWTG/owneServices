using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class ClientCognosAccGLAccountDescriptorExtraInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateIsSubClassifiedByDebtor()
		{
			AssertNoErrors("Pre-condition", ExtraInfo.IsSubClassifiedByDebtorInfo);
			AccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			AccountDescriptor.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			AccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.Validation.ValidateIsSubClassifiedByDebtor();
			AssertNoErrors("Should not validate if not a sub-classification account", ExtraInfo.IsSubClassifiedByDebtorInfo);
			AccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			ExtraInfo.Validation.ValidateIsSubClassifiedByDebtor();
			AssertNoErrors("Should not validate if not a sub-classification account", ExtraInfo.IsSubClassifiedByDebtorInfo);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor;
			ExtraInfo.Validation.ValidateIsSubClassifiedByDebtor();
			AssertHasError(ExtraInfo.IsSubClassifiedByDebtorInfo, "Should map at least one Debtor Group when the Account is Sub Classified by Debtor");
			ExtraInfo.MappedDebtorGroups.AddNew();
			ExtraInfo.Validation.ValidateIsSubClassifiedByDebtor();
			AssertNoErrors("There is one mapping, should not have error", ExtraInfo.IsSubClassifiedByDebtorInfo);
		}

		public void TestValidateIsSubClassifiedByCreditor()
		{
			AssertNoErrors("Pre-condition", ExtraInfo.IsSubClassifiedByCreditorInfo);
			AccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			AccountDescriptor.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			AccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.Validation.ValidateIsSubClassifiedByCreditor();
			AssertNoErrors("Should not validate if not a sub-classification account", ExtraInfo.IsSubClassifiedByCreditorInfo);
			AccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			ExtraInfo.Validation.ValidateIsSubClassifiedByCreditor();
			AssertNoErrors("Should not validate if not a sub-classification account", ExtraInfo.IsSubClassifiedByCreditorInfo);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor;
			ExtraInfo.Validation.ValidateIsSubClassifiedByCreditor();
			AssertHasError(ExtraInfo.IsSubClassifiedByCreditorInfo, "Should map at least one Creditor Group when the Account is Sub Classified by Creditor");
			ExtraInfo.MappedCreditorGroups.AddNew();
			ExtraInfo.Validation.ValidateIsSubClassifiedByCreditor();
			AssertNoErrors("There is one mapping, should not have error", ExtraInfo.IsSubClassifiedByCreditorInfo);
		}

		public void TestValidateT9_ReconciliationTotalAccount()
		{
			AccountDescriptor.AJ_LocalAccountNumber = "101";
			ExtraInfo.Validation.ValidateT9_ReconciliationTotalAccount();
			Assert("Sanity check", !ExtraInfo.ShouldReconciliateTotal);
			AssertNoErrors("Should not validate if ShouldReconciliateTotal is false", ExtraInfo.T9_ReconciliationTotalAccountInfo);
			ExtraInfo.ShouldReconciliateTotal = true;
			ExtraInfo.Validation.ValidateT9_ReconciliationTotalAccount();
			AssertMandatoryValidationError(ExtraInfo.T9_ReconciliationTotalAccountInfo, true);
			ExtraInfo.T9_ReconciliationTotalAccount = "MEH";
			AssertNoErrors(ExtraInfo.T9_ReconciliationTotalAccountInfo);
			CognosAccGLAccountDescriptor otherAccount = Factory.New<CognosAccGLAccountDescriptor>();
			otherAccount.ExtraInfo.ShouldReconciliateTotal = true;
			otherAccount.ExtraInfo.T9_ReconciliationTotalAccount = "MEH";
			AssertHasError(otherAccount.ExtraInfo.T9_ReconciliationTotalAccountInfo, "Account Number 'MEH' has already been used");
			CognosAccGLAccountDescriptor yetAnotherAccount = Factory.New<CognosAccGLAccountDescriptor>();
			yetAnotherAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			yetAnotherAccount.AJ_LocalAccountNumber = "OTHERACC";
			otherAccount.ExtraInfo.T9_ReconciliationTotalAccount = "OTHERACC";
			AssertHasError(otherAccount.ExtraInfo.T9_ReconciliationTotalAccountInfo, "Account Number 'OTHERACC' has already been used");
			otherAccount.ExtraInfo.T9_ReconciliationTotalAccount = "UNIQUE";
			AssertNoErrors(otherAccount.ExtraInfo.T9_ReconciliationTotalAccountInfo);
		}

		public void TestValidateT9_AccountAge()
		{
			AccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			AccountDescriptor.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			AccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.Validation.ValidateT9_AccountAge();
			AssertNoErrors("Should not validate if not a sub-classification account", ExtraInfo.T9_AccountAgeInfo);
			AccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			ExtraInfo.Validation.ValidateT9_AccountAge();
			AssertNoErrors("Should not validate if not sub classified by age", ExtraInfo.T9_AccountAgeInfo);
			ExtraInfo.T9_SubClassificationCode = CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge;
			ExtraInfo.Validation.ValidateT9_AccountAge();
			AssertMandatoryValidationError(ExtraInfo.T9_AccountAgeInfo, true);
			ExtraInfo.T9_AccountAge = "__";
			AssertListValidationInvalidCodeError(ExtraInfo.T9_AccountAgeInfo, true);
			ExtraInfo.T9_AccountAge = "30";
			AssertNoErrors(ExtraInfo.T9_AccountAgeInfo);
		}

		public void TestValidateT9_AJ_AccountToBeSubClassified()
		{
			AccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			AccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			AccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.Validation.ValidateT9_AJ_AccountToBeSubClassified();
			AssertMandatoryValidationError(ExtraInfo.T9_AJ_AccountToBeSubClassifiedInfo, true);
			ExtraInfo.T9_AJ_AccountToBeSubClassified = Factory.New<CognosAccGLAccountDescriptor>().PK;
			ExtraInfo.AccountToBeSubClassified.AJ_Language = Core.Constants.GLLanguages.English;
			AssertMandatoryValidationError(ExtraInfo.T9_AJ_AccountToBeSubClassifiedInfo, false);
			AssertListValidationInvalidCodeError(ExtraInfo.T9_AJ_AccountToBeSubClassifiedInfo, true);
			ExtraInfo.AccountToBeSubClassified.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			ExtraInfo.AccountToBeSubClassified.AJ_ReportCategory = Core.Constants.AccountType.Consolidation;
			ExtraInfo.AccountToBeSubClassified.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.Validation.ValidateT9_AJ_AccountToBeSubClassified();
			AssertListValidationInvalidCodeError(ExtraInfo.T9_AJ_AccountToBeSubClassifiedInfo, false);
		}

		public void TestValidateT9_AJ_AccountToBeSubClassified_ShouldOnlyAllowSubClassificationsToTwoLevelDown()
		{
			CognosAccGLAccountDescriptor level1SubclassificationAccount = Factory.New<CognosAccGLAccountDescriptor>();
			level1SubclassificationAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			level1SubclassificationAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			level1SubclassificationAccount.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			CognosAccGLAccountDescriptor level2SubclassificationAccount = Factory.New<CognosAccGLAccountDescriptor>();
			level2SubclassificationAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			level2SubclassificationAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			level2SubclassificationAccount.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			level2SubclassificationAccount.ExtraInfo.T9_AJ_AccountToBeSubClassified = level1SubclassificationAccount.PK;
			AccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			AccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			AccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.T9_AJ_AccountToBeSubClassified = level2SubclassificationAccount.PK;
			AssertHasError(ExtraInfo.T9_AJ_AccountToBeSubClassifiedInfo, "Cannot select an Account that is sub-classifying another Sub-Classification Account. The Sub-Classification hierarchy only goes to two levels down");
			ExtraInfo.T9_AJ_AccountToBeSubClassified = level1SubclassificationAccount.PK;
			AssertNoErrors("Should be able to sub-classify another sub-classification account (1 level down)", ExtraInfo.T9_AJ_AccountToBeSubClassifiedInfo);
		}

		public void TestValidateT9_AJ_AccountToBeSubClassified_ShouldNotValidateWhenNotASubclassificationAccount()
		{
			AccountDescriptor.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			AccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.Validation.ValidateT9_AJ_AccountToBeSubClassified();
			AssertNoErrors("Should not be validated when AccountType is not Sub-Classification", ExtraInfo.T9_AJ_AccountToBeSubClassifiedInfo);
		}

		public void TestValidateAll()
		{
			AccountDescriptor.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			AccountDescriptor.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			AccountDescriptor.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			ExtraInfo.IsSubClassifiedByCreditor = true;
			ExtraInfo.Validation.ValidateAll();
			AssertHasErrors("ValidateIsSubClassifiedByCreditor should be called by ValidateAll", ExtraInfo.IsSubClassifiedByCreditorInfo);
			ExtraInfo.IsSubClassifiedByDebtor = true;
			ExtraInfo.Validation.ValidateAll();
			AssertHasErrors("ValidateIsSubClassifiedByDebtor should be called by ValidateAll", ExtraInfo.IsSubClassifiedByDebtorInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CognosAccGLAccountDescriptor nonCOALocalAccount = Factory.NewWithValidTestData<CognosAccGLAccountDescriptor>();
			// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
			nonCOALocalAccount.AJ_DebitCredit = Core.Constants.DebitCredit.Debit;
			AccountDescriptor.AJ_ReportType = "AAA";
			Factory.Save();
		}

		CognosAccGLAccountDescriptor AccountDescriptor
		{
			get
			{
				if (fAccountDescriptor == null)
				{
					fAccountDescriptor = Factory.New<CognosAccGLAccountDescriptor>();
					// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
					fAccountDescriptor.AJ_DebitCredit = Core.Constants.DebitCredit.Debit;
				}

				return fAccountDescriptor;
			}
		}

		CognosAccGLAccountDescriptorExtraInfo ExtraInfo
		{
			get
			{
				return AccountDescriptor.ExtraInfo;
			}
		}

		CognosAccGLAccountDescriptor fAccountDescriptor;
	}
}
