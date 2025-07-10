using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournalLine))]
	public class JobRevenueJournalLineTest : DependentTransactionLineTest
	{
		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestCalculateOSAmountsWhenZeroGST() => base.TestCalculateOSAmountsWhenZeroGST();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestGSTandQSTSplittingOnLoad() => base.TestGSTandQSTSplittingOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestGSTandRETSplittingOnLoad() => base.TestGSTandRETSplittingOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestGSTandEDUSplittingOnLoad() => base.TestGSTandEDUSplittingOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestVATandSPVSplittingOnLoad() => base.TestVATandSPVSplittingOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestVATandSPVSplittingOnLoad_AL_GSTVATExtraIsPersistent() => base.TestVATandSPVSplittingOnLoad_AL_GSTVATExtraIsPersistent();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestQCTSplittingOnLoad() => base.TestQCTSplittingOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestOTOSplittingOnLoad() => base.TestOTOSplittingOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestLineWithSTATypeTax() => base.TestLineWithSTATypeTax();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestOverseasAmountsAreCalculatedCorrectlyWhenReloaded_ForeignCurrency_NoTax() => base.TestOverseasAmountsAreCalculatedCorrectlyWhenReloaded_ForeignCurrency_NoTax();

		public override void TestLocalExtraAmountIsUpdatedOnInvalidTaxID()
		{
			Assert("Not applicable", true);
		}

		public override void TestOSAndLocalAmountsFromOSExTaxAmountForMexico_TestingCases()
		{
			Assert("Not applicable", true);
		}

		public override void TestVATandExtraTaxOnLoad_AL_GSTVATExtraIsPersistent()
		{
			Assert("Not applicable", true);
		}

		public override void TestAL_OSExtraTaxAmount_CalculateAL_LocalExtraTaxAmountAfterTaxIDUpdate()
		{
			Assert("Not applicable", true);
		}

		public void TestReversalValidation()
		{
			TestObjectCreator.ReverseTransaction(JournalLine.MasterTransactionHeader, out string cantReverseMessage);
			AssertNullOrEmpty("Precondition: Reversing is successful", cantReverseMessage);
			AssertType(typeof(TransactionLineEmptyValidation), JournalLine.Validation);
		}

		#region JobRevenueJournalGLAccountDefaultingRuleTypesTests

		public override void TestSettingAL_ACSetsAL_AG()
		{
			//This is tested in TestSettingAL_AC_SetsAL_AGAndPostToGLAccount unit test methods with different registry settings
			Assert(true);
		}

		public void TestSettingAL_AC_SetsAL_AGAndPostToGLAccount_JobRevenueJournalGLAccountDefaultingRulesSetToRevenue()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedRevenueAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedRevenueAccount.AG_AccountNum, JournalLine.PostToGLAccount);
				AssertNoErrors(JournalLine.AL_ACInfo);
			}
		}

		public void TestSettingAL_AC_SetsAL_AGAndPostToGLAccount_JobRevenueJournalGLAccountDefaultingRulesSetToCost()
		{
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedCostAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedCostAccount.AG_AccountNum, JournalLine.PostToGLAccount);
				AssertNoErrors(JournalLine.AL_ACInfo);
			}
		}

		public void TestSettingAL_AC_SetsAL_AGAndPostToGLAccount_JobRevenueJournalGLAccountDefaultingRulesSetToBoth()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedCostAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedCostAccount.AG_AccountNum, JournalLine.PostToGLAccount);
				AssertNoErrors(JournalLine.AL_ACInfo);

				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedRevenueAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedRevenueAccount.AG_AccountNum, JournalLine.PostToGLAccount);
				AssertNoErrors(JournalLine.AL_ACInfo);
			}
		}

		public void TestSettingCostRevenueTypeSetsAL_AGAndPostToGLAccount_JobRevenueJournalGLAccountDefaultingRulesSetToRevenue()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedRevenueAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedRevenueAccount.AG_AccountNum, JournalLine.PostToGLAccount);

				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				AssertHasError(JournalLine.CostRevenueTypeInfo, @"When 'Job Revenue Journal GL Account Defaulting Rules' Registry is set to REV, Line Cost/Revenue Type must be REV.");
				AssertEquals(expectedRevenueAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedRevenueAccount.AG_AccountNum, JournalLine.PostToGLAccount);
			}
		}

		public void TestSettingCostRevenueTypeSetsAL_AGAndPostToGLAccount_JobRevenueJournalGLAccountDefaultingRulesSetToCost()
		{
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedCostAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedCostAccount.AG_AccountNum, JournalLine.PostToGLAccount);

				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				AssertHasError(JournalLine.CostRevenueTypeInfo, @"When 'Job Revenue Journal GL Account Defaulting Rules' Registry is set to CST, Line Cost/Revenue Type must be CST.");
				AssertEquals(expectedCostAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedCostAccount.AG_AccountNum, JournalLine.PostToGLAccount);
			}
		}

		public void TestSettingCostRevenueTypeSetsAL_AGAndPostToGLAccount_JobRevenueJournalGLAccountDefaultingRulesSetToBoth()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedRevenueAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedRevenueAccount.AG_AccountNum, JournalLine.PostToGLAccount);

				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				AssertNoErrors(JournalLine.CostRevenueTypeInfo);
				AssertEquals(expectedCostAccount.PK, JournalLine.AL_AG);
				AssertEquals(expectedCostAccount.AG_AccountNum, JournalLine.PostToGLAccount);
			}
		}

		public void TestSettingGLAccountsDoesNotChangeCostRevenueType_JobRevenueJournalGLAccountDefaultingRulesSetToRevenue()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				var newChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				newChargeCode.AC_AG_CostAccount = expectedCostAccount.PK;
				newChargeCode.AC_AG_RevenueAccount = expectedRevenueAccount.PK;

				var newJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
				var newLineCR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineCR.AL_AC = newChargeCode.PK;
				newLineCR.OSUnsignedLineAmount = 500;
				newLineCR.DebitCreditSign = DebitCreditDataEntry.CR;
				newLineCR.AL_AG = TestObjectCreator.GLHeader1.PK;
				var newLineDR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineDR.AL_AC = newChargeCode.PK;
				newLineDR.OSUnsignedLineAmount = 500;
				newLineDR.DebitCreditSign = DebitCreditDataEntry.DR;
				newLineDR.AL_AG = TestObjectCreator.GLHeader1.PK;

				Factory.Save();

				AssertEquals(TransactionLineTypes.Revenue, newLineCR.CostRevenueType);
				AssertEquals(TransactionLineTypes.Revenue, newLineDR.CostRevenueType);

				newChargeCode.AC_AG_CostAccount = expectedRevenueAccount.PK;
				newChargeCode.AC_AG_RevenueAccount = expectedCostAccount.PK;

				Factory.Save();

				AssertEquals(TransactionLineTypes.Revenue, newLineCR.CostRevenueType);
				AssertEquals(TransactionLineTypes.Revenue, newLineDR.CostRevenueType);
			}
		}

		public void TestSettingGLAccountsDoesNotChangeCostRevenueType_JobRevenueJournalGLAccountDefaultingRulesSetToCost()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				var newChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				newChargeCode.AC_AG_CostAccount = expectedCostAccount.PK;
				newChargeCode.AC_AG_RevenueAccount = expectedRevenueAccount.PK;

				var newJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
				var newLineCR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineCR.AL_AC = newChargeCode.PK;
				newLineCR.OSUnsignedLineAmount = 500;
				newLineCR.DebitCreditSign = DebitCreditDataEntry.CR;
				newLineCR.AL_AG = TestObjectCreator.GLHeader1.PK;
				var newLineDR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineDR.AL_AC = newChargeCode.PK;
				newLineDR.OSUnsignedLineAmount = 500;
				newLineDR.DebitCreditSign = DebitCreditDataEntry.DR;
				newLineDR.AL_AG = TestObjectCreator.GLHeader1.PK;

				Factory.Save();

				AssertEquals(TransactionLineTypes.Cost, newLineCR.CostRevenueType);
				AssertEquals(TransactionLineTypes.Cost, newLineDR.CostRevenueType);

				newChargeCode.AC_AG_CostAccount = expectedRevenueAccount.PK;
				newChargeCode.AC_AG_RevenueAccount = expectedCostAccount.PK;

				Factory.Save();

				AssertEquals(TransactionLineTypes.Cost, newLineCR.CostRevenueType);
				AssertEquals(TransactionLineTypes.Cost, newLineDR.CostRevenueType);
			}
		}

		public void TestSettingGLAccountsDoesNotChangeCostRevenueType_JobRevenueJournalGLAccountDefaultingRulesSetToBoth()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				var newChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				newChargeCode.AC_AG_CostAccount = expectedCostAccount.PK;
				newChargeCode.AC_AG_RevenueAccount = expectedRevenueAccount.PK;

				var newJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
				var newLineCR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineCR.AL_AC = newChargeCode.PK;
				newLineCR.CostRevenueType = TransactionLineTypes.Revenue;
				newLineCR.OSUnsignedLineAmount = 500;
				newLineCR.DebitCreditSign = DebitCreditDataEntry.CR;
				newLineCR.AL_AG = TestObjectCreator.GLHeader1.PK;
				var newLineDR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineDR.AL_AC = newChargeCode.PK;
				newLineDR.CostRevenueType = TransactionLineTypes.Cost;
				newLineDR.OSUnsignedLineAmount = 500;
				newLineDR.DebitCreditSign = DebitCreditDataEntry.DR;
				newLineDR.AL_AG = TestObjectCreator.GLHeader1.PK;

				Factory.Save();

				AssertEquals(TransactionLineTypes.Revenue, newLineCR.CostRevenueType);
				AssertEquals(TransactionLineTypes.Cost, newLineDR.CostRevenueType);

				newChargeCode.AC_AG_CostAccount = expectedRevenueAccount.PK;
				newChargeCode.AC_AG_RevenueAccount = expectedCostAccount.PK;

				Factory.Save();

				AssertEquals(TransactionLineTypes.Revenue, newLineCR.CostRevenueType);
				AssertEquals(TransactionLineTypes.Cost, newLineDR.CostRevenueType);
			}
		}

		public void TestSettingDefaultCostRevenueTypeValueDoesNotShowSecurityError()
		{
			var jobCostingLevelSecurity = Env.Security.JobRevenueJournalAllowOverrideCostRevenueType;
			jobCostingLevelSecurity.IsAllowed = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var line = Factory.New<JobRevenueJournalLine>();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSecurityToOverrideCostRevenueType_UserHasSecurityRights()
		{
			AssertSettingCostRevenueTypeValueWithSecurityCheckPoints(true);
		}

		public void TestSecurityToOverrideCostRevenueType_UserHasNoSecurityRights()
		{
			AssertSettingCostRevenueTypeValueWithSecurityCheckPoints(false);
		}

		public void TestCostRevenueTypeDefaultValue_JobRevenueJournalGLAccountDefaultingRuleTypesRegistrySetToCost()
		{
			AssertDefaultCostRevenueTypeValue(AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code);
		}

		public void TestCostRevenueTypeDefaultValue_JobRevenueJournalGLAccountDefaultingRuleTypesRegistrySetToRevenue()
		{
			AssertDefaultCostRevenueTypeValue(AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code);
		}

		public void TestCostRevenueTypeDefaultValue_JobRevenueJournalGLAccountDefaultingRuleTypesRegistrySetToBoth()
		{
			AssertDefaultCostRevenueTypeValue(AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code);
		}

		public void TestCostRevenueTypeAndPostToGlAccountForPostedLine_Revenue()
		{
			AssertCostRevenueTypeAndPostToGlAccountForPostedLine(true);
		}

		public void TestCostRevenueTypeAndPostToGlAccountForPostedLine_Cost()
		{
			AssertCostRevenueTypeAndPostToGlAccountForPostedLine(false);
		}

		public void TestCostRevenueTypeReadOnly()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				Assert(!JournalLine.CostRevenueTypeInfo.ReadOnly);
			}
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				Assert(JournalLine.CostRevenueTypeInfo.ReadOnly);
			}
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				Assert(JournalLine.CostRevenueTypeInfo.ReadOnly);
			}
		}

		public void TestPostToGLAccountReadOnly()
		{
			Assert("Post To GL Account field must always be read only", JournalLine.PostToGLAccountInfo.ReadOnly);
		}

		public void TestCopyValuesFrom_CopiesValuesInCorrectOrder()
		{
			var expectedRevenueAccount = TestObjectCreator.GLHeader1;
			var expectedCostAccount = TestObjectCreator.GLHeader2;
			TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;
			TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;
			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				JournalLine.CostRevenueType = TransactionLineTypes.Cost;
				JournalLine.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals(expectedCostAccount.AG_AccountNum, JournalLine.PostToGLAccount);

				var newJournal = Factory.New<JobRevenueJournal>();
				var newJournalLine = newJournal.JournalLines.AddNew();
				newJournalLine.CopyValuesFrom(JournalLine);
				AssertEquals(TransactionLineTypes.Cost, newJournalLine.CostRevenueType);
				AssertEquals(TestObjectCreator.CC1.PK, newJournalLine.AL_AC);
				AssertEquals(expectedCostAccount.AG_AccountNum, newJournalLine.PostToGLAccount);
			}
		}

		public void TestSettingDebitCreditSignSetsDefaultCostRevenueType()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
				AssertEquals(TransactionLineTypes.Revenue, JournalLine.CostRevenueType);
				JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;
				AssertEquals(TransactionLineTypes.Cost, JournalLine.CostRevenueType);
			}
		}

		public void TestSettingDebitCreditSignDoesNotSetDefaultCostRevenueTypeForReverseingJournal()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
				AssertEquals(TransactionLineTypes.Revenue, JournalLine.CostRevenueType);
				JournalLine.ParentJournal.IsReverseTransaction = true;
				JournalLine.ReverseDebitCreditSign();
				AssertEquals(DebitCreditDataEntry.DR, JournalLine.DebitCreditSign);
				AssertEquals(TransactionLineTypes.Revenue, JournalLine.CostRevenueType);
			}
		}

		void AssertCostRevenueTypeAndPostToGlAccountForPostedLine(ZBool isTestingRevenue)
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				var journal = Factory.NewWithValidTestData<JobRevenueJournal>();
				var line1 = journal.Lines.AddNew() as JobRevenueJournalLine;
				var line2 = journal.Lines.AddNew() as JobRevenueJournalLine;

				line1.AL_JH = job.PK;
				line2.AL_JH = job.PK;

				var expectedRevenueAccount = TestObjectCreator.GLHeader1;
				var expectedCostAccount = TestObjectCreator.GLHeader2;
				TestObjectCreator.CC1.AC_AG_RevenueAccount = expectedRevenueAccount.PK;
				TestObjectCreator.CC1.AC_AG_CostAccount = expectedCostAccount.PK;

				var expectedCostRevenueType = isTestingRevenue ? TransactionLineTypes.Revenue : TransactionLineTypes.Cost;
				var expectedPostToGlAccount = isTestingRevenue ? expectedRevenueAccount.AG_AccountNum : expectedCostAccount.AG_AccountNum;

				line1.AL_AC = TestObjectCreator.CC1.PK;
				line1.OSUnsignedLineAmount = 200.00m;
				line1.DebitCreditSign = DebitCreditDataEntry.DR;
				line1.CostRevenueType = expectedCostRevenueType;

				line2.AL_AC = TestObjectCreator.CC1.PK;
				line2.OSUnsignedLineAmount = 200.00m;
				line2.DebitCreditSign = DebitCreditDataEntry.CR;
				line2.CostRevenueType = expectedCostRevenueType;
				Factory.Save();

				AssertEquals(expectedCostRevenueType, line1.CostRevenueType);
				AssertEquals(expectedPostToGlAccount, line1.PostToGLAccount);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var loadedLine = newFactory.Load<JobRevenueJournalLine>(line1.PK);
				AssertEquals(expectedCostRevenueType, loadedLine.CostRevenueType);
				AssertEquals(expectedPostToGlAccount, loadedLine.PostToGLAccount);
			}
		}

		void AssertDefaultCostRevenueTypeValue(string registryValue)
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			{
				var line = Factory.New<JobRevenueJournalLine>();
				var expectedDefaultValue = ZString.Empty;
				if (registryValue == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code)
				{
					expectedDefaultValue = line.DebitCreditSign == DebitCreditDataEntry.DR.ToString() ? AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code : AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code;
				}
				else
				{
					expectedDefaultValue = registryValue;
				}
				AssertEquals(expectedDefaultValue, line.CostRevenueType);
			}
		}

		void AssertSettingCostRevenueTypeValueWithSecurityCheckPoints(ZBool jobCostingLevelSecurityAllowed)
		{
			var jobCostingLevelSecurity = Env.Security.JobRevenueJournalAllowOverrideCostRevenueType;
			jobCostingLevelSecurity.IsAllowed = jobCostingLevelSecurityAllowed;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			JournalLine.CostRevenueType = TransactionLineTypes.Revenue;
			if (jobCostingLevelSecurityAllowed)
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				var expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Job Costing -> Job Revenue Journals -> New -> Allow Override Cost/Revenue Type";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		public void TestZDecimalsHaveCorrectDecimalPlacesJobRevenueJournalLine()
		{
			var localList = new List<string>
			{
				nameof(JournalLine.LocalUnsignedLineAmount)
			};

			var osList = new List<string>
			{
				nameof(JournalLine.OSUnsignedLineAmount)
			};

			var tester = new DecimalPlacesAttributeTester(JournalLine, JournalLine.Company);
			tester.CheckLocalCurrency(localList, nameof(JournalLine.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(JournalLine.CurrencyDecimals), nameof(JournalLine.AL_RX_NKTransactionCurrency), JournalLine);
		}

		[SuspendCriticalValidation]
		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			JournalLine.OSUnsignedLineAmount = 200.00m;
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;
			SetupAndSaveLine();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionLine loadedLine = (TransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Amount should equal LineAmount", Line.AL_OSExTaxAmount, Line.AL_OverseasTotal);
		}

		public void TestLineType()
		{
			AssertEquals("Line type", TransactionLineTypes.Revenue, JournalLine.AL_LineType);
		}

		public void TestLineType_SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused()
		{
			var line = Factory.New<JobRevenueJournalLineForTestOnly>();

			AssertEquals("Precondition", TransactionLineTypes.Revenue, line.CostRevenueType);
			AssertEquals("Line type", TransactionLineTypes.Revenue, line.AL_LineType);
			AssertEquals("Line type", TransactionLineTypes.Revenue, line.LineTypeForTestOnly);

			line.CostRevenueType = TransactionLineTypes.Cost;

			AssertEquals("Precondition", TransactionLineTypes.Cost, line.CostRevenueType);
			AssertEquals("Line type", TransactionLineTypes.Revenue, line.AL_LineType);
			AssertEquals("Line type", TransactionLineTypes.Revenue, line.LineTypeForTestOnly);

			using (AccountingConfigurationRegistry.Instance.SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				line = Factory.New<JobRevenueJournalLineForTestOnly>();

				AssertEquals("Precondition", TransactionLineTypes.Revenue, line.CostRevenueType);
				AssertEquals("Line type", TransactionLineTypes.Revenue, line.AL_LineType);
				AssertEquals("Line type", TransactionLineTypes.Revenue, line.LineTypeForTestOnly);

				line.CostRevenueType = TransactionLineTypes.Cost;

				AssertEquals("Precondition", TransactionLineTypes.Cost, line.CostRevenueType);
				AssertEquals("Line type", TransactionLineTypes.Cost, line.AL_LineType);
				AssertEquals("Line type", TransactionLineTypes.Cost, line.LineTypeForTestOnly);
			}
		}

		public void TestCopyValuesFrom()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			JournalLine.AL_JH = job.PK;
			JournalLine.AL_RX_NKTransactionCurrency = "USD";
			JournalLine.AL_ExchangeRate = 2M;
			JournalLine.AL_AC = TestObjectCreator.CC1.PK;
			JournalLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			JournalLine.AL_Desc = "Test Description";
			JournalLine.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			JournalLine.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;
			JournalLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			JournalLine.CostRevenueType = "REV";
			JournalLine.OSUnsignedLineAmount = 100M;
			JournalLine.AL_LineType = "XXX";
			AssertEquals("Precondition for LocalUnsignedLineAmount", 50M, JournalLine.LocalUnsignedLineAmount);

			JobRevenueJournalLine newLine = Factory.New<JobRevenueJournalLine>();
			newLine.CopyValuesFrom(JournalLine);

			AssertEquals("AL_RX_NKTransactionCurrency", "USD", newLine.AL_RX_NKTransactionCurrency);
			AssertEquals("AL_ExchangeRate", 2M, newLine.AL_ExchangeRate);
			AssertEquals("AL_JH", job.PK, newLine.AL_JH);
			AssertEquals("AL_AC", TestObjectCreator.CC1.PK, newLine.AL_AC);
			AssertEquals("AL_AG", TestObjectCreator.GLHeader1.PK, newLine.AL_AG);
			AssertEquals("AL_Desc", "Test Description", newLine.AL_Desc);
			AssertEquals("AL_GB", TestObjectCreator.NonCurrentBranch.PK, newLine.AL_GB);
			AssertEquals("AL_GE", TestObjectCreator.NonCurrentDepartment.PK, newLine.AL_GE);
			AssertEquals("AL_RevRecognitionType", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, newLine.AL_RevRecognitionType);
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.CR, newLine.DebitCreditSign);
			AssertEquals("CostRevenueType", "REV", newLine.CostRevenueType);
			AssertEquals("OSUnsignedLineAmount", 100M, newLine.OSUnsignedLineAmount);
			AssertEquals("LocalUnsignedLineAmount", 50M, newLine.LocalUnsignedLineAmount);
			AssertEquals("AL_LineType", "XXX", newLine.AL_LineType);
		}

		public void TestValidationType()
		{
			AssertType(typeof(JobRevenueJournalLineValidation), JournalLine.Validation);
		}

		public void TestSetDefaultValue()
		{
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.DR, JournalLine.DebitCreditSign);
		}

		[TestDate(2011, 01, 10)]
		[DisableZeroExchangeRateOverriding]
		public void TestSetExchangeRate()
		{
			ZDateTime arrivalDate = new ZDateTime(2010, 10, 10);
			ZDateTime fallbackDate = new ZDateTime(2010, 11, 10);
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_E_ARV = arrivalDate;
			Job job = TestObjectCreator.CreateJob(shipment);
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "ALL", "ALL", preference: "HAR");
			GlbCompany.CurrentCompany.Factory.Save();

			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 10M);

			RefExchangeRate rate = TestObjectCreator.GBP.ExchangeRates.AddNew();
			rate.RE_StartDate = arrivalDate;
			rate.RE_ExpiryDate = arrivalDate;
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 5M;
			rate = TestObjectCreator.GBP.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 15M;

			RefCurrency uAH = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Ukraine);
			rate = uAH.ExchangeRates.AddNew();
			rate.RE_StartDate = fallbackDate;
			rate.RE_ExpiryDate = fallbackDate;
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 3M;

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			RefCurrency nZD = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.NewZealand);
			rate = nZD.ExchangeRates.AddNew();
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			rate.RE_SellRate = 20M;
			Factory.Save();

			Enterprise.ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			AssertEquals("Precondition for AL_JH", ZGuid.Empty, JournalLine.AL_JH);

			JournalLine.AL_ExchangeRate = 0M;
			JournalLine.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.Australia;
			AssertEquals("AL_ExchangeRate", 1M, JournalLine.AL_ExchangeRate);

			JournalLine.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.UnitedStates;
			AssertEquals("AL_ExchangeRate", 0M, JournalLine.AL_ExchangeRate);

			JournalLine.SuspendAL_JHSettingDefaults();
			JournalLine.AL_JH = job.PK;
			JournalLine.ResumeAL_JHSettingDefaults();
			AssertEquals("AL_ExchangeRate", 0M, JournalLine.AL_ExchangeRate);

			JournalLine.AL_JH = job.PK;
			AssertEquals("AL_ExchangeRate", 10M, JournalLine.AL_ExchangeRate);

			JournalLine.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.UnitedKingdom;
			AssertEquals("AL_ExchangeRate", 5M, JournalLine.AL_ExchangeRate);

			JournalLine.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.Ukraine;
			AssertEquals("AL_ExchangeRate", 0M, JournalLine.AL_ExchangeRate);

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			JournalLine.AL_JH = ZGuid.Empty;
			AssertEquals("AL_ExchangeRate", 0M, JournalLine.AL_ExchangeRate);
			JournalLine.AL_JH = job.PK;
			AssertEquals("AL_ExchangeRate", 3M, JournalLine.AL_ExchangeRate);

			JournalLine.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.NewZealand;
			AssertEquals("AL_ExchangeRate", 20M, JournalLine.AL_ExchangeRate);
		}

		public void TestAL_DescDefaultOnChargeCodeSet()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Desc = "Charge Code Desc";
			JournalLine.AL_AC = chargeCode.PK;
			AssertEquals("Desc should default from Charge Code", "Charge Code Desc", JournalLine.AL_Desc);

			chargeCode.AC_Desc = "Charge Code Desc 2";
			JournalLine.AL_Desc = "Desc";
			JournalLine.AL_AC = ZGuid.Empty;
			JournalLine.AL_AC = chargeCode.PK;
			AssertEquals("Desc should not default from Charge Code because it was changed by user to different value.", "Desc", JournalLine.AL_Desc);

			JournalLine.AL_Desc = "";
			JournalLine.AL_AC = ZGuid.Empty;
			JournalLine.AL_AC = chargeCode.PK;
			AssertEquals("Desc should default from Charge Code because it was empty.", "Charge Code Desc 2", JournalLine.AL_Desc);
		}

		public void TestCommentChargeCodeResetAmountAndMakeThemReadonly()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeCode commentChargeCode = Factory.New<AccChargeCode>();
			commentChargeCode.AC_ChargeType = Constants.ChargeType.Comment;

			AssertEquals("Precondition: charge code must not be comment code.", false, chargeCode.IsComment);
			AssertEquals("Precondition: charge code must be comment code.", true, commentChargeCode.IsComment);

			JournalLine.OSUnsignedLineAmount = 100M;
			JournalLine.AL_AC = chargeCode.PK;
			AssertEquals("OSUnsignedLineAmount after not comment charge code set", 100M, JournalLine.OSUnsignedLineAmount);
			AssertEquals("OSUnsignedLineAmount should not be readonly after not comment charge code set", false, JournalLine.OSUnsignedLineAmountInfo.ReadOnly);
			AssertEquals("LocalUnsignedLineAmount should not be readonly after not comment charge code set", false, JournalLine.LocalUnsignedLineAmountInfo.ReadOnly);

			JournalLine.AL_AC = commentChargeCode.PK;
			AssertEquals("OSUnsignedLineAmount after comment charge code set", 0M, JournalLine.OSUnsignedLineAmount);
			AssertEquals("OSUnsignedLineAmount should be readonly after not comment charge code set", true, JournalLine.OSUnsignedLineAmountInfo.ReadOnly);
			AssertEquals("LocalUnsignedLineAmount should be readonly after not comment charge code set", true, JournalLine.LocalUnsignedLineAmountInfo.ReadOnly);
		}

		public void TestCalculateBranchAndDepartment()
		{
			Job job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			JournalLine.SuspendAL_JHSettingDefaults();
			JournalLine.AL_JH = job.PK;
			JournalLine.ResumeAL_JHSettingDefaults();
			AssertEquals(GlbBranch.CurrentBranch.PK, JournalLine.AL_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, JournalLine.AL_GE);

			JournalLine.AL_JH = job.PK;
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, JournalLine.AL_GB);
			AssertEquals(TestObjectCreator.NonCurrentDepartment.PK, JournalLine.AL_GE);
		}

		public void TestAL_ExchangeRate_ReadOnly()
		{
			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			AssertEquals("AL_ExchangeRateInfo.ReadOnly", true, JournalLine.AL_ExchangeRateInfo.ReadOnly);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("AL_ExchangeRateInfo.ReadOnly", false, JournalLine.AL_ExchangeRateInfo.ReadOnly);
		}

		public void TestDebitCreditSignList()
		{
			AssertEquals("DebitCreditSignList.Count", 2, JournalLine.DebitCreditSignList.Count);
			Assert("DebitCreditSignList.ContainsCode(DebitCreditDataEntry.DR)", JournalLine.DebitCreditSignList.ContainsCode(DebitCreditDataEntry.DR));
			Assert("DebitCreditSignList.ContainsCode(DebitCreditDataEntry.CR)", JournalLine.DebitCreditSignList.ContainsCode(DebitCreditDataEntry.CR));
		}

		public void TestDebitCreditSignAndUnsignedLineAmounts()
		{
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;
			JournalLine.OSUnsignedLineAmount = 100M;
			JournalLine.AL_ExchangeRate = 2M;
			AssertEquals("AL_OSExTaxAmount", -100M, JournalLine.AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", -50M, JournalLine.AL_LocalExTaxAmount);

			JournalLine.LocalUnsignedLineAmount = 100M;
			AssertEquals("AL_OSExTaxAmount", -200M, JournalLine.AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", -100M, JournalLine.AL_LocalExTaxAmount);

			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals("AL_OSExTaxAmount", 200M, JournalLine.AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", 100M, JournalLine.AL_LocalExTaxAmount);

			JournalLine.OSUnsignedLineAmount = 100M;
			AssertEquals("AL_OSExTaxAmount", 100M, JournalLine.AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", 50M, JournalLine.AL_LocalExTaxAmount);

			JournalLine.LocalUnsignedLineAmount = 100M;
			AssertEquals("AL_OSExTaxAmount", 200M, JournalLine.AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", 100M, JournalLine.AL_LocalExTaxAmount);
		}

		public override void TestOSTotalSplitParts_ZeroVAT()
		{
			AccTaxRate fgstFree = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			if (fgstFree == null)
			{
				fgstFree = Factory.New<AccTaxRate>();
				fgstFree.AT_Code = "FREEGST";
				fgstFree.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			fgstFree.AT_Type = AccTaxRate.Types.Rated;
			fgstFree.SetRateNumerator_ForTestOnly(0);

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var newJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
			var newLineCR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
			newLineCR.DebitCreditSign = DebitCreditDataEntry.CR;
			newLineCR.CostRevenueType = TransactionLineTypes.Revenue;
			newLineCR.AL_AC = TestObjectCreator.FRT.PK;
			newLineCR.AL_AG = TestObjectCreator.FRT.RevenueAccount.PK;
			newLineCR.AL_ExchangeRate = 177.4937;
			newLineCR.AL_AT = fgstFree.PK;
			newLineCR.AL_OSExTaxAmount = 30380329.38;
			newLineCR.AL_JH = job.PK;

			var newLineDR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
			newLineDR.DebitCreditSign = DebitCreditDataEntry.DR;
			newLineDR.CostRevenueType = TransactionLineTypes.Cost;
			newLineDR.AL_AC = TestObjectCreator.FRT.PK;
			newLineDR.AL_AG = TestObjectCreator.FRT.CostAccount.PK;
			newLineDR.OSUnsignedLineAmount = newLineCR.OSUnsignedLineAmount;
			newLineDR.AL_JH = job.PK;

			Factory.Save();

			var factoryForNoCache = new BusinessObjectFactory();
			var newLineCRForNewFactory = factoryForNoCache.Load<JobRevenueJournalLine>(newLineCR.PK);
			AssertOSValues(newLineCRForNewFactory, 30380329.3800, 0, 30380329.3800);
		}

		public override void TestOSTotalSplitParts_NonZeroVAT()
		{
			AccTaxRate gST = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			if (gST == null)
			{
				gST = Factory.New<AccTaxRate>();
				gST.AT_Code = "GST";
				gST.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			gST.AT_Type = AccTaxRate.Types.Rated;
			gST.SetRate_ForTestOnly(175, 10);

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var newJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
			var newLineCR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
			newLineCR.DebitCreditSign = DebitCreditDataEntry.CR;
			newLineCR.CostRevenueType = TransactionLineTypes.Revenue;
			newLineCR.AL_AC = TestObjectCreator.FRT.PK;
			newLineCR.AL_AG = TestObjectCreator.FRT.RevenueAccount.PK;
			newLineCR.AL_ExchangeRate = 177.4937;
			newLineCR.AL_AT = gST.PK;
			newLineCR.AL_OSExTaxAmount = 30380329.38;
			newLineCR.AL_JH = job.PK;

			var newLineDR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
			newLineDR.DebitCreditSign = DebitCreditDataEntry.DR;
			newLineDR.CostRevenueType = TransactionLineTypes.Cost;
			newLineDR.AL_AC = TestObjectCreator.FRT.PK;
			newLineDR.AL_AG = TestObjectCreator.FRT.CostAccount.PK;
			newLineDR.OSUnsignedLineAmount = newLineCR.OSUnsignedLineAmount;
			newLineDR.AL_JH = job.PK;

			Factory.Save();

			var factoryForNoCache = new BusinessObjectFactory();
			var newLineCRForNewFactory = factoryForNoCache.Load<JobRevenueJournalLine>(newLineCR.PK);
			AssertOSValues(newLineCRForNewFactory, 30380329.38, 5316557.6400, 35696887.0200);
		}

		public override void TestTaxAmountsForIndiaSTA()
		{
			Assert("Not applicable", true);
		}

		public void TestReverseDebitCreditSign()
		{
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;

			JournalLine.ReverseDebitCreditSign();
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.CR, JournalLine.DebitCreditSign);

			JournalLine.ReverseDebitCreditSign();
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.DR, JournalLine.DebitCreditSign);
		}

		public void TestPostJobRevenueJournal_SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused()
		{
			using (AccountingConfigurationRegistry.Instance.SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();

				var newJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
				var newLineCR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineCR.AL_AC = TestObjectCreator.FRT.PK;
				newLineCR.AL_AG = TestObjectCreator.FRT.RevenueAccount.PK;
				newLineCR.OSUnsignedLineAmount = 500;
				newLineCR.DebitCreditSign = DebitCreditDataEntry.CR;
				newLineCR.CostRevenueType = TransactionLineTypes.Revenue;
				newLineCR.AL_JH = job.PK;

				var newLineDR = newJournal.Lines.AddNew() as JobRevenueJournalLine;
				newLineDR.AL_AC = TestObjectCreator.FRT.PK;
				newLineDR.AL_AG = TestObjectCreator.FRT.CostAccount.PK;
				newLineDR.OSUnsignedLineAmount = 500;
				newLineDR.DebitCreditSign = DebitCreditDataEntry.DR;
				newLineDR.CostRevenueType = TransactionLineTypes.Cost;
				newLineDR.AL_JH = job.PK;

				Factory.Save();

				var newFactory = job.CreateNewFactory();
				var newLineCRForNewFactory = newFactory.Load<JobRevenueJournalLine>(newLineCR.PK);
				var newLineDRForNewFactory = newFactory.Load<JobRevenueJournalLine>(newLineDR.PK);

				AssertEquals(TransactionLineTypes.Revenue, newLineCRForNewFactory.AL_LineType);
				AssertEquals(TransactionLineTypes.Cost, newLineDRForNewFactory.AL_LineType);
			}
		}

		public void TestSetOSAmountDebitCredit()
		{
			JournalLine.SetOSAmountDebitCredit(false);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;
			JournalLine.OSUnsignedLineAmount = 100M;
			AssertEquals("AL_OSExTaxAmount", -100M, JournalLine.AL_OSExTaxAmount);

			JournalLine.SetOSAmountDebitCredit(true);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;
			JournalLine.OSUnsignedLineAmount = 100M;
			AssertEquals("AL_OSExTaxAmount", 100M, JournalLine.AL_OSExTaxAmount);

			JournalLine.SetOSAmountDebitCredit(false);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			JournalLine.OSUnsignedLineAmount = 100M;
			AssertEquals("AL_OSExTaxAmount", 100M, JournalLine.AL_OSExTaxAmount);

			JournalLine.SetOSAmountDebitCredit(true);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			JournalLine.OSUnsignedLineAmount = 100M;
			AssertEquals("AL_OSExTaxAmount", -100M, JournalLine.AL_OSExTaxAmount);
		}

		public void TestSetLocalAmountDebitCredit()
		{
			JournalLine.SetLocalAmountDebitCredit(false);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;
			JournalLine.LocalUnsignedLineAmount = 100M;
			AssertEquals("AL_LocalExTaxAmount", -100M, JournalLine.AL_LocalExTaxAmount);

			JournalLine.SetLocalAmountDebitCredit(true);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;
			JournalLine.LocalUnsignedLineAmount = 100M;
			AssertEquals("AL_LocalExTaxAmount", 100M, JournalLine.AL_LocalExTaxAmount);

			JournalLine.SetLocalAmountDebitCredit(false);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			JournalLine.LocalUnsignedLineAmount = 100M;
			AssertEquals("AL_LocalExTaxAmount", 100M, JournalLine.AL_LocalExTaxAmount);

			JournalLine.SetLocalAmountDebitCredit(true);
			JournalLine.DebitCreditSign = DebitCreditDataEntry.CR;
			JournalLine.LocalUnsignedLineAmount = 100M;
			AssertEquals("AL_LocalExTaxAmount", -100M, JournalLine.AL_LocalExTaxAmount);
		}

		public void TestDebitCreditSign_OSAmountIsZero()
		{
			JournalLine.DebitCreditSign = DebitCreditDataEntry.DR;

			JournalLine.AL_OSExTaxAmount = 0m;

			JournalLine.AL_LineAmount = 1m;
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.CR, JournalLine.DebitCreditSign);

			JournalLine.AL_LineAmount = -1m;
			AssertEquals("DebitCreditSign", DebitCreditDataEntry.DR, JournalLine.DebitCreditSign);
		}

		public override void TestRelatedJobChargeWithoutDbHit()
		{
			AssertDbHitCount(0);

			Factory.Save();

			AssertDbHitCount(2);
		}

		#region Implementation

		protected override Type MasterHeaderType
		{
			get { return typeof(JobRevenueJournal); }
		}

		protected override bool LineCanHaveTaxComponent
		{
			get { return false; }
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get { return true; }
		}

		protected JobRevenueJournalLine JournalLine
		{
			get { return (JobRevenueJournalLine)Line; }
		}

		protected override bool AcceptAL_AC
		{
			get { return false; }
		}

		protected override bool AcceptAL_AG
		{
			get { return false; }
		}

		protected override bool LinkChargeToLine(Charge charge, TransactionLine line)
		{
			return true;
		}

		protected override void SetupRelatedObjectsForFetchHintTest(TransactionLinesCollection collection)
		{
		}

		class JobRevenueJournalLineForTestOnly : JobRevenueJournalLine
		{
			public JobRevenueJournalLineForTestOnly(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZString LineTypeForTestOnly => LineType;
		}

		#endregion
	}
}
