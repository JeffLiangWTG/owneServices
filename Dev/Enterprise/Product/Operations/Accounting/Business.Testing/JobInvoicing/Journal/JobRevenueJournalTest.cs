using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobRevenueJournal))]
	public class JobRevenueJournalTest : TransactionHeaderWithLinesTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<JobRevenueJournal>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestUpdateTotalsCallAmount() => base.TestUpdateTotalsCallAmount();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestRETAmountUpdatedOnLoad() => base.TestRETAmountUpdatedOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestQSTAmountUpdatedOnLoad() => base.TestQSTAmountUpdatedOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestOTO6AmountUpdatedOnLoad() => base.TestOTO6AmountUpdatedOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestOSTaxAmountIsAlwaysZeroWhenLineTaxIsZero() => base.TestOSTaxAmountIsAlwaysZeroWhenLineTaxIsZero();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestGSTAndQSTBasedOnQCTAmountUpdatedOnLoad() => base.TestGSTAndQSTBasedOnQCTAmountUpdatedOnLoad();

		[SuspendSumOfLinesEqualZeroCriticalValidation]
		public override void TestEDUAmountUpdatedOnLoad() => base.TestEDUAmountUpdatedOnLoad();

		#region JobRevenueJournalGLAccountDefaultingRuleTypesTests

		public void TestSettingDefaultCostRevenueTypeToAndFromValueDoesNotShowSecurityError()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var journal = Factory.New<JobRevenueJournal>();
			journal.JournalCharges.AddNew();

			var invoicingLevelSecurity = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostRevenueType);
			var jobCostingLevelSecurity = Env.Security.JobRevenueJournalAllowOverrideCostRevenueType;
			invoicingLevelSecurity.IsAllowed = false;
			jobCostingLevelSecurity.IsAllowed = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			journal.ActivateSimpleEntry(job); //This will set default CostRevenueType
			AssertNull("Security rights should be ignored when setting default value", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints_InvoicingLevelAllowed_JobCostingLevelNotAllowed()
		{
			AssertSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints(true, false);
		}

		public void TestSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints_InvoicingLevelAllowed_JobCostingLevelAllowed()
		{
			AssertSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints(true, true);
		}

		public void TestSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints_InvoicingLevelNotAllowed_JobCostingLevelAllowed()
		{
			AssertSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints(false, true);
		}

		public void TestSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints_InvoicingLevelNotAllowed_JobCostingLevelNotAllowed()
		{
			AssertSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints(false, false);
		}

		public void TestCostRevenueTypeToAndFromDefaultValue_JobRevenueJournalGLAccountDefaultingRuleTypesRegistrySetToCost()
		{
			AssertCostRevenueTypeToAndFromDefaultValue(AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code);
		}

		public void TestCostRevenueTypeToAndFromDefaultValue_JobRevenueJournalGLAccountDefaultingRuleTypesRegistrySetToRevenue()
		{
			AssertCostRevenueTypeToAndFromDefaultValue(AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code);
		}

		public void TestCostRevenueTypeToAndFromDefaultValue_JobRevenueJournalGLAccountDefaultingRuleTypesRegistrySetToBoth()
		{
			AssertCostRevenueTypeToAndFromDefaultValue(AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code);
		}

		public void TestCostRevenueTypeFromReadOnly()
		{
			AssertCostRevenueTypeReadOnlyness((ZPropertyInfoString)TestJournal.CostRevenueTypeFromInfo);
		}

		public void TestCostRevenueTypeToReadOnly()
		{
			AssertCostRevenueTypeReadOnlyness((ZPropertyInfoString)TestJournal.CostRevenueTypeToInfo);
		}

		void AssertCostRevenueTypeReadOnlyness(ZPropertyInfoString property)
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				Assert(!property.ReadOnly);
			}
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				Assert(property.ReadOnly);
			}
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				Assert(property.ReadOnly);
			}
		}

		void AssertCostRevenueTypeToAndFromDefaultValue(string registryValue)
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var journal = Factory.New<JobRevenueJournal>();
				journal.ActivateSimpleEntry(job); //This will set default CostRevenueType
				if (registryValue == AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code)
				{
					AssertEquals(TransactionLineTypes.Cost, journal.CostRevenueTypeFrom);
					AssertEquals(TransactionLineTypes.Revenue, journal.CostRevenueTypeTo);
				}
				else
				{
					AssertEquals(registryValue, journal.CostRevenueTypeFrom);
					AssertEquals(registryValue, journal.CostRevenueTypeTo);
				}
			}
		}

		void AssertSettingCostRevenueTypeToAndFromValueWithSecurityCheckPoints(ZBool invoicingLevelSecurityAllowed, ZBool jobCostingLevelSecurityAllowed)
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var journal = Factory.New<JobRevenueJournal>();
			journal.JournalCharges.AddNew();

			var invoicingLevelSecurity = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostRevenueType);
			var jobCostingLevelSecurity = Env.Security.JobRevenueJournalAllowOverrideCostRevenueType;

			invoicingLevelSecurity.IsAllowed = invoicingLevelSecurityAllowed;
			jobCostingLevelSecurity.IsAllowed = jobCostingLevelSecurityAllowed;

			journal.ActivateSimpleEntry(job);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			journal.CostRevenueTypeFrom = TransactionLineTypes.Revenue;
			journal.CostRevenueTypeTo = TransactionLineTypes.Revenue;
			if (invoicingLevelSecurityAllowed)
			{
				AssertNull("Should only check Invoicing -> Create Job Revenue Journal -> Allow Override Cost/Revenue Type security right.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				var expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Create Job Revenue Journal -> Allow Override Cost/Revenue Type";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		public override void TestGenerateReverseTransaction()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);

			AssertNoErrors("Precondition", journal);
			journal.GenerateReverseTransaction(true);
			JobRevenueJournal reversedHeader = (JobRevenueJournal)journal.ReverseTransaction;
			AssertNotNull("Reversed transaction should not be null", reversedHeader);
			Assert(reversedHeader.IsReverseTransaction);
			AssertEquals(journal, reversedHeader.OriginalTransaction);
			AssertNoErrors(reversedHeader);

			// Check the DB values
			AssertEquals(journal.AH_AG, reversedHeader.AH_AG);
			AssertEquals(journal.AH_OH, reversedHeader.AH_OH);
			AssertEquals(journal.AH_AB, reversedHeader.AH_AB);
			AssertEquals(journal.AH_GB, reversedHeader.AH_GB);
			AssertEquals(journal.AH_GE, reversedHeader.AH_GE);
			AssertEquals(journal.AH_TransactionCategory, reversedHeader.AH_TransactionCategory);
			AssertEquals(journal.AH_JH, reversedHeader.AH_JH);
			AssertEquals(journal.AH_CashBasisGSTIndicator, reversedHeader.AH_CashBasisGSTIndicator);
			AssertEquals(journal.AH_ChequeDrawer, reversedHeader.AH_ChequeDrawer);
			AssertEquals(journal.AH_ChequeOrReference, reversedHeader.AH_ChequeOrReference);
			AssertEquals(journal.AH_DrawerBank, reversedHeader.AH_DrawerBank);
			AssertEquals(journal.AH_DrawerBranch, reversedHeader.AH_DrawerBranch);
			AssertEquals(journal.AH_InvoiceTerm, reversedHeader.AH_InvoiceTerm);
			AssertEquals(journal.AH_InvoiceTermDays, reversedHeader.AH_InvoiceTermDays);
			AssertEquals(journal.AH_ReceiptType, reversedHeader.AH_ReceiptType);
			AssertEquals(journal.AH_RX_NKTransactionCurrency, reversedHeader.AH_RX_NKTransactionCurrency);
			AssertEquals(ZDateTime.Empty, reversedHeader.AH_DueDate);
			AssertEquals("AH_OSExTaxAmount on the ReversedHeader should be 0", 0M, reversedHeader.AH_OSExTaxAmount);
			AssertEquals("AH_OSTaxAmount on the ReversedHeader should be 0", 0M, reversedHeader.AH_OSTaxAmount);
			AssertEquals("AH_LocalExTaxAmount on the ReversedHeader should be 0", 0M, reversedHeader.AH_LocalExTaxAmount);
			AssertEquals("AH_LocalTaxAmount on the ReversedHeader should be 0", 0M, reversedHeader.AH_LocalTaxAmount);
			AssertEquals("AH_OSTotalAmount on ReversedHeader should be 0", 0M, reversedHeader.AH_OSTotalAmount);
			AssertEquals("Exchange Rate should be the same", 1.0M, reversedHeader.AH_ExchangeRate);

			AssertEquals(journal.Lines.Count, reversedHeader.Lines.Count);
			for (int i = 0; i < journal.Lines.Count; i++)
			{
				AssertEquals(journal.Lines[i].AL_RX_NKTransactionCurrency, reversedHeader.Lines[i].AL_RX_NKTransactionCurrency);
				AssertEquals(journal.Lines[i].AL_ExchangeRate, reversedHeader.Lines[i].AL_ExchangeRate);
				AssertEquals(journal.Lines[i].AL_JH, reversedHeader.Lines[i].AL_JH);
				AssertEquals(journal.Lines[i].AL_AC, reversedHeader.Lines[i].AL_AC);
				AssertEquals(journal.Lines[i].AL_AG, reversedHeader.Lines[i].AL_AG);
				AssertEquals(journal.Lines[i].AL_Desc, reversedHeader.Lines[i].AL_Desc);
				AssertEquals(journal.Lines[i].AL_GB, reversedHeader.Lines[i].AL_GB);
				AssertEquals(journal.Lines[i].AL_GE, reversedHeader.Lines[i].AL_GE);
				AssertEquals(journal.Lines[i].AL_RevRecognitionType, reversedHeader.Lines[i].AL_RevRecognitionType);
				AssertEquals(journal.Lines[i].AL_OSAmount * -1, reversedHeader.Lines[i].AL_OSAmount);
				AssertEquals(journal.Lines[i].AL_LineAmount * -1, reversedHeader.Lines[i].AL_LineAmount);
				AssertEquals(0M, reversedHeader.Lines[i].AL_GSTVAT);
			}
		}

		public void TestPostManualJRJ_WithTaxRegistrationNumbers()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment();
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			var line1 = journal.Lines[0];
			var line2 = journal.Lines[1];
			line1.AL_GE = TestObjectCreator.FISDepartment.PK;
			line2.AL_GE = TestObjectCreator.FISDepartment.PK;
			AssertEquals("Precondition", GlbBranch.CurrentBranch.PK, line1.AL_GB);
			AssertEquals(GlbBranch.CurrentBranch.PK, line2.AL_GB);

			TestObjectCreator.AALSHI.PrimaryRegistrationNumber.Number = "73004700400";
			TestObjectCreator.ABIGAS.PrimaryRegistrationNumber.Number = "73004700400";

			journal.Validation.ValidateAll();
			AssertNoErrors(journal);

			TestObjectCreator.AALSHI.PrimaryRegistrationNumber.Number = "73004700411";
			TestObjectCreator.ABIGAS.PrimaryRegistrationNumber.Number = "73004700499";

			journal.Validation.ValidateAll();
			AssertNoErrors(journal);
		}

		protected override void SetupHeaderForReversing(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			SetupJob();
			Header.AH_PostDate = ZDateTime.Now.AddDays(-2);
			Header.AH_JH = Job.PK;
			Header.AH_GB = NonCurrentBranch.PK;
			Header.AH_GE = NonCurrentDepartment.PK;
			Header.AH_AG = GLAccount.PK;
			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Header.AH_ExchangeRate = 0.6m;
			Header.AH_OSExTaxAmount = 0M;
		}

		public void TestAH_TransactionNum_ReadOnly()
		{
			AssertEquals("AH_TransactionNum must be always readonly.", true, TestJournal.AH_TransactionNumInfo.ReadOnly);
		}

		public void TestAH_OSExTaxAmount_ReadOnly()
		{
			AssertEquals("AH_OSExTaxAmount must be always readonly.", true, TestJournal.AH_OSExTaxAmountInfo.ReadOnly);
		}

		public void TestRecalculateAH_JH()
		{
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job1, 100);
			journal.Lines[0].AL_JH = job1.PK;
			journal.Lines[1].AL_JH = job1.PK;
			Factory.Save();

			AssertEquals(job1.PK, journal.AH_JH);

			journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job1, 100);
			journal.Lines[0].AL_JH = job1.PK;
			journal.Lines[1].AL_JH = job2.PK;
			Factory.Save();

			AssertEquals(ZGuid.Empty, journal.AH_JH);
		}

		public void TestSetAH_PostDateOnSaving()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			journal.AH_PostDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			AssertEquals(ZDateTime.BrettsBirthday, journal.Lines[0].AL_PostDate);
			AssertEquals(ZDateTime.BrettsBirthday, journal.Lines[1].AL_PostDate);
		}

		public void TestSetBranchAndDepartmentOnSaving()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			Factory.Save();
			AssertEquals("Journal branch", TestObjectCreator.NonCurrentBranch.PK, journal.AH_GB);
			AssertEquals("Journal department", TestObjectCreator.NonCurrentDepartment.PK, journal.AH_GE);

			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job2.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			journal.Lines[0].AL_JH = job.PK;
			journal.Lines[1].AL_JH = job2.PK;
			Factory.Save();
			AssertEquals("Journal branch", GlbBranch.CurrentBranch.PK, journal.AH_GB);
			AssertEquals("Journal department", GlbDepartment.CurrentDepartment.PK, journal.AH_GE);

			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			Factory.Save();
			AssertEquals("Journal branch", GlbBranch.CurrentBranch.PK, journal.AH_GB);
			AssertEquals("Journal department", TestObjectCreator.NonCurrentDepartment.PK, journal.AH_GE);
		}

		public void TestChargesCreatingOnPosting()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			Factory.Save();

			AssertEquals("Charges.Count", 2, job.Charges.Count);
			AssertEquals("IsRevenuePosted", true, job.Charges[0].IsRevenuePosted);
			AssertEquals("IsRevenuePosted", true, job.Charges[0].IsRevenuePosted);
			if (job.Charges[0].JR_AL_ARLine == journal.Lines[0].PK)
			{
				AssertEquals("Charge should be linked to correct line.", journal.Lines[1].PK, job.Charges[1].JR_AL_ARLine);
			}
			else
			{
				AssertEquals("Charge should be linked to correct line.", journal.Lines[1].PK, job.Charges[0].JR_AL_ARLine);
				AssertEquals("Charge should be linked to correct line.", journal.Lines[0].PK, job.Charges[1].JR_AL_ARLine);
			}
		}

		public void TestGeneratingTransactionNumberOnPosting()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BusinessObjectFactory numberFountainFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreatorForTestFactory = new TestObjectCreator(testFactory);
			Job job = testFactory.NewJobWithValidTestDataForTesting<Job>();
			JobRevenueJournal journal = testObjectCreatorForTestFactory.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), testObjectCreatorForTestFactory.CC1, job, 100);

			string expectedNumber = AccountingNumberFountainWrapperFactory.Instance.JRJournal.PeekPreliminary(numberFountainFactory);
			testFactory.Save();

			AssertEquals(expectedNumber, journal.AH_TransactionNum);

			journal = testObjectCreatorForTestFactory.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), testObjectCreatorForTestFactory.CC1, job, 100);
			string prevExpectedNumber = expectedNumber;
			expectedNumber = AccountingNumberFountainWrapperFactory.Instance.JRJournal.PeekPreliminary(numberFountainFactory);
			AssertNotEquals("Precondition: number fontain should be generate new number", prevExpectedNumber, expectedNumber);
			testFactory.Save();

			AssertEquals(expectedNumber, journal.AH_TransactionNum);
		}

		[TestDate(2006, 12, 01)]
		public void TestSetJobChargesWIPAccrualCreationDate()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "ALL";
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;

			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, ZDateTime.Now);
			Factory.Save();

			var charge = Factory.Load<Charge>(journal.Lines[0].RelatedJobCharge.PK);
			AssertEquals("WIPAccrualCreationDate", ZDateTime.Now, charge.WIPAccrualCreationDate);

			journal.GenerateReverseTransaction(true);
			((JobRevenueJournal)journal.ReverseTransaction).AH_PostDate = ZDateTime.Now.AddDays(-2);
			journal.AH_IsCancelled = true;
			Factory.Save();
			AssertEquals("WIPAccrualCreationDate", ZDateTime.Now.AddDays(-2), charge.WIPAccrualCreationDate);
		}

		public void TestInvoiceJobRevenueRecognitionDate()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.Air;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;

			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ZDateTime expectedDate = ZDateTime.Today.AddDays(2);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = expectedDate;

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			Factory.Save();

			AssertEquals("Precondition", 0, job.RevenueRecognitionCollection.Count);

			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			Factory.Save();

			AssertEquals("Revenue Recognition Date should be filled.", 1, job.RevenueRecognitionCollection.Count);
			AssertEquals("Revenue Recognition Date should be filled.", expectedDate, job.RevenueRecognitionCollection[0].D3_RecognitionDate);
		}

		public void TestDependentLinesType()
		{
			AssertEquals("DependentTransactionLineType", typeof(JobRevenueJournalLine), TestJournal.DependentTransactionLineType);
			AssertType("GetDependentLinesCollection", typeof(JobRevenueJournalLineCollection), TestJournal.Lines);
		}

		public void TestLedgerTransactionTypeAndName()
		{
			AssertEquals("AH_Ledger", LedgerTypes.JobCosting, TestJournal.AH_Ledger);
			AssertEquals("AH_TransactionType", TransactionTypes.JobRevenueJournal, TestJournal.AH_TransactionType);
			AssertEquals("HumanReadableName", "Job Revenue Journal", TestJournal.HumanReadableName);
		}

		public void TestValidationType()
		{
			AssertType("Validation", typeof(JobRevenueJournalValidation), TestJournal.Validation);
			TestJournal.IsReverseTransaction = true;
			AssertType("Reversal Validation", typeof(TransactionReversalValidation), TestJournal.Validation);
		}

		public void TestActivateSimpleEntry()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			job.JH_Description = "Job Description";
			AssertEquals("JobDescription", "Job Description", job.JobDescription);

			AssertEquals("Precondition: IsInSimpleEntryMode", false, TestJournal.IsInSimpleEntryMode);
			TestJournal.ActivateSimpleEntry(job);
			AssertEquals("JournalCharges.Job", job.PK, TestJournal.JournalCharges.Job.PK);
			AssertEquals("BranchPKFrom", job.JH_GB, TestJournal.BranchPKFrom);
			AssertEquals("DepartmentPKFrom", job.JH_GE, TestJournal.DepartmentPKFrom);
			AssertEquals("BranchPKFrom", GlbBranch.CurrentBranch.PK, TestJournal.BranchPKTo);
			AssertEquals("DepartmentPKFrom", GlbDepartment.CurrentDepartment.PK, TestJournal.DepartmentPKTo);
			AssertEquals("IsInSimpleEntryMode", true, TestJournal.IsInSimpleEntryMode);
			AssertEquals("AH_Desc", "Job Description", TestJournal.AH_Desc);

			TestJournal.ActivateSimpleEntry(null);
			AssertNull("JournalCharges.Job", TestJournal.JournalCharges.Job);
			AssertEquals("BranchPKFrom", job.JH_GB, TestJournal.BranchPKFrom);
			AssertEquals("DepartmentPKFrom", job.JH_GE, TestJournal.DepartmentPKFrom);
			AssertEquals("BranchPKFrom", GlbBranch.CurrentBranch.PK, TestJournal.BranchPKTo);
			AssertEquals("DepartmentPKFrom", GlbDepartment.CurrentDepartment.PK, TestJournal.DepartmentPKTo);
			AssertEquals("IsInSimpleEntryMode", true, TestJournal.IsInSimpleEntryMode);
			AssertEquals("AH_Desc", "Job Description", TestJournal.AH_Desc);
		}

		public void TestDeactivateSimpleEntry()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			TestJournal.ActivateSimpleEntry(job);
			AssertEquals("Precondition: IsInSimpleEntryMode", true, TestJournal.IsInSimpleEntryMode);

			TestJournal.JournalCharges.AddNew();
			AssertEquals("Precondition: JournalCharges.Count", 1, TestJournal.JournalCharges.Count);
			TestJournal.DefaultSharing = -1;
			TestJournal.RunPreSaveValidation();
			AssertHasErrors("Precondition:", TestJournal.DefaultSharingInfo);

			TestJournal.DeactivateSimpleEntry();
			AssertEquals("IsInSimpleEntryMode", false, TestJournal.IsInSimpleEntryMode);
			AssertEquals("JournalCharges.Count", 0, TestJournal.JournalCharges.Count);
			AssertNoErrors(TestJournal.DefaultSharingInfo);
		}

		public void TestBranchPKFrom()
		{
			JobRevenueJournalCharge journalCharge = TestJournal.JournalCharges.AddNew();
			TestJournal.BranchPKFrom = TestObjectCreator.NonCurrentBranch.PK;
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, journalCharge.BranchPKFrom);
		}

		public void TestBranchPKTo()
		{
			JobRevenueJournalCharge journalCharge = TestJournal.JournalCharges.AddNew();
			TestJournal.BranchPKTo = TestObjectCreator.NonCurrentBranch.PK;
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, journalCharge.BranchPKTo);
		}

		public void TestDepartmentPKFrom()
		{
			JobRevenueJournalCharge journalCharge = TestJournal.JournalCharges.AddNew();
			TestJournal.DepartmentPKFrom = TestObjectCreator.NonCurrentDepartment.PK;
			AssertEquals(TestObjectCreator.NonCurrentDepartment.PK, journalCharge.DepartmentPKFrom);
		}

		public void TestDepartmentPKTo()
		{
			JobRevenueJournalCharge journalCharge = TestJournal.JournalCharges.AddNew();
			TestJournal.DepartmentPKTo = TestObjectCreator.NonCurrentDepartment.PK;
			AssertEquals(TestObjectCreator.NonCurrentDepartment.PK, journalCharge.DepartmentPKTo);
		}

		public void TestDefaultSharing()
		{
			JobRevenueJournalCharge journalCharge1 = TestJournal.JournalCharges.AddNew();
			JobRevenueJournalCharge journalCharge2 = TestJournal.JournalCharges.AddNew();
			journalCharge1.Share = 5M;
			TestJournal.DefaultSharing = 10M;
			AssertEquals(5M, journalCharge1.Share);
			AssertEquals(10M, journalCharge2.Share);
		}

		public void TestJournalChargesReadOnly()
		{
			TestJournal.FillWithValidTestData();
			Factory.Save();

			AssertEquals("JournalCharges.ReadOnly", true, TestJournal.JournalCharges.ReadOnly);
		}

		public void TestBranchDepartmentValidationJobRevenueJournal()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { department });
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			journal.RunPreSaveValidation();
			AssertEquals("should have errors", journal.HasErrors, true);
			AssertHasError(journal.AH_GEInfo, string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
				, TestObjectCreator.NonCurrentDepartment.GE_Code, Env.CurrentBranch.Code));
		}

		public void TestCanApplyTaxBranch() => AssertEquals(false, TestJournal.CanApplyTaxBranch);

		public void TestHeaderValidationIsSuspnededOnSaving()
		{
			var journal = SetupJRJForValidationSuspenderTest();
			using (journal.GetValidationSuspender())
			{
				journal.Lines[0].AL_JH = ZGuid.Invalid;
				journal.Lines[1].AL_JH = ZGuid.Invalid;
			}

			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				var msg = ex.Message;
				var expectedMsg = "Inner Message = The INSERT statement conflicted with the FOREIGN KEY constraint";
				AssertContains("Should have this exception message, but we don't care in this UT. ", expectedMsg, msg);
			}
			AssertNoErrors("We suspend validation to improve performance as the JobRevenueJournal on factory saving where we do not care about validation errors.", journal.AH_JHInfo);

			journal.Validation.ValidateAH_JH();
			AssertHasErrorContaining("This proofs that charge has validation error if validation is not suspended", journal.AH_JHInfo, "Enter a valid Job.");
		}

		public void TestLineValidationIsSuspnededOnSaving()
		{
			var journal = SetupJRJForValidationSuspenderTest();
			journal.AH_PostDate = ZDateTime.Invalid;

			Factory.Save();
			AssertNoErrors("We suspend validation to improve performance as the JobRevenueJournal on factory saving where we do not care about validation errors.", journal.Lines[0].AL_PostDateInfo);

			journal.Lines[0].Validation.ValidateAL_PostDate();
			AssertHasErrorContaining("This proofs that charge has validation error if validation is not suspended", journal.Lines[0].AL_PostDateInfo, "Enter a valid Post Date.");
		}

		JobRevenueJournal SetupJRJForValidationSuspenderTest()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var journal = TestObjectCreator.CreateJobRevenueJournal(GetExpectedBusinessObjectType(), TestObjectCreator.CC1, job, 100);
			return journal;
		}

		#region Implementation

		protected override Type TypeOfValidation
		{
			get { return typeof(JobRevenueJournalValidation); }
		}

		protected override Type TypeOfReversalValidation
		{
			get { return typeof(TransactionReversalValidation); }
		}

		JobRevenueJournal TestJournal
		{
			get { return TestJournal_cached ?? (TestJournal_cached = (JobRevenueJournal)GetNewBusinessObject()); }
		}
		JobRevenueJournal TestJournal_cached;

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
		}

		protected override void AssertReverseTransactionDueDateValue(TransactionHeader reverseHeader)
		{
			AssertEquals("Due Date should be empty", ZDateTime.Empty, reverseHeader.AH_DueDate.Date);
		}

		#endregion

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(JobRevenueJournalLine);
		}

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;
	}
}
