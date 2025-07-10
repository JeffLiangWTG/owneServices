using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class InvoicingPostManager_InnerTest : TestCaseWithFactory
	{
		public void TestProcessEligibleCharges()
		{
			Job testJob = Factory.NewJobForTesting<Job>();
			Charge charge1 = testJob.Charges.AddNew();
			InvoicingPostManager postManager = new InvoicingPostManager(testJob);

			IReceivablesPostingChargeCollection filteredCharges = new IReceivablesPostingChargeCollection();
			AssertEquals(false, filteredCharges.JobIsSet);

			postManager.ProcessEligibleCharges_ForTestOnly(filteredCharges);

			AssertEquals(true, filteredCharges.JobIsSet);
			AssertEquals(testJob.PK, filteredCharges.Job.PK);
		}

		public void TestProcessEligibleChargesCheckJH_ProfitLossReasonCode()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			AssertNotNull(creator.CC1);
			Factory.Save();

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_ProfitLossReasonCode = "";

			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			SetAndAssertRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), null);

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			Charge testCharge1 = testJob.Charges.AddNew();
			testCharge1.FillWithValidTestData();
			testCharge1.JR_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			testCharge1.JR_AC = creator.CC1.PK;
			testCharge1.JR_LocalCostAmt = 50m;
			testCharge1.JR_LocalSellAmt = 70m;
			IReceivablesPostingChargeCollection filteredCharges = new IReceivablesPostingChargeCollection();
			filteredCharges.Add(testCharge1);

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			postManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_CriticalCheckError);

			ProfitLossReasonRequiredError = false;
			postManager.FCancelPosting_ForTestOnly = false;
			postManager.ProcessEligibleCharges_ForTestOnly(filteredCharges);
			Assert("Posting should be cancelled", postManager.CancelPosting);
			Assert("Should be Job Profit Loss Reason required error", ProfitLossReasonRequiredError);

			SetAndAssertRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), new JobHeaderStatusList());
			ProfitLossReasonRequiredError = false;
			postManager.FCancelPosting_ForTestOnly = false;
			postManager.ProcessEligibleCharges_ForTestOnly(filteredCharges);
			Assert("Posting should not be cancelled", !postManager.CancelPosting);
			Assert("Should not be Job Profit Loss Reason required error", !ProfitLossReasonRequiredError);

			testCharge1.JR_LocalCostAmt = 200m;
			testCharge1.JR_LocalSellAmt = 210m;

			SetAndAssertRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), null);

			ProfitLossReasonRequiredError = false;
			postManager.FCancelPosting_ForTestOnly = false;
			postManager.ProcessEligibleCharges_ForTestOnly(filteredCharges);
			Assert("Posting should not be cancelled", !postManager.CancelPosting);
			Assert("Should not be Job Profit Loss Reason required error", !ProfitLossReasonRequiredError);

			postManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_CriticalCheckError);
		}

		public void TestProcessEligibleChargesCancelPostingOnChanrgeValidationError()
		{
			var testJob = Factory.NewJobForTesting<Job>();
			var charge1 = testJob.Charges.AddNew();
			charge1.JR_Desc = Enterprise.Rating.Business.RatingConstants.RateNotePrefix + "blah";
			AssertHasErrors(charge1.JR_DescInfo);
			var postManager = new InvoicingPostManager(testJob);
			ChargeDescriptionError = false;
			postManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_CriticalCheckError);

			var filteredCharges = new IReceivablesPostingChargeCollection();
			filteredCharges.Add(charge1);

			Factory.SuspendValidation();
			try
			{
				postManager.ProcessEligibleCharges_ForTestOnly(filteredCharges);
			}
			finally
			{
				Factory.ResumeValidation();
			}
			Assert("Posting should be cancelled", postManager.CancelPosting);
			Assert("Should be Charge Description error", ChargeDescriptionError);

			postManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_CriticalCheckError);
		}

		public void TestErrorOnInvalidOutstandingAmountAndFullyPaidDate()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge1 = testJob.Charges.AddNew();
			charge1.FillWithValidTestData();
			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			InvoiceCreatorForTest creator = new InvoiceCreatorForTest(testJob);
			postManager.FCostTransactionCreator_ForTestOnly = creator;
			postManager.FAPCreditNoteCreator_ForTestOnly = new APCreditNoteCreatorForTest(testJob);
			postManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_CriticalCheckError);

			creator.CauseOutstandingAmountFullyPaidDateError = true;
			postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Assert("Critical Validation was moved OnSaving, so should not be the error", !InvalidOutstandingAmountAndFullyPaidDateError);
		}

		public void TestErrorOnInvalidIsCancelled()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge1 = testJob.Charges.AddNew();
			charge1.FillWithValidTestData();
			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			InvoiceCreatorForTest creator = new InvoiceCreatorForTest(testJob);
			postManager.FCostTransactionCreator_ForTestOnly = creator;
			postManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_CriticalCheckError);

			creator.CauseIsCancelledError = true;
			postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			Assert("Critical Validation was moved OnSaving, so should not be the error", !InvalidIsCancelledError);

			try
			{
				Factory.Save();
				Fail("Critical Validation should prevent saving");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Critical Validation Error", IsCancelledError, ex.Message);
				ExceptionReporterTestListener.Instance.Clear();
			}

			creator.CauseIsCancelledError = false;
			InvalidIsCancelledError = false;

			postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			postManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_CriticalCheckError);
			Assert("Should be no error", !InvalidIsCancelledError);
		}

		void SetAndAssertRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Guid companyPK, CodeDescriptionPairList statusesWhenARInvoicePostingdoesnotChangeStatus)
		{
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(companyPK, statusesWhenARInvoicePostingdoesnotChangeStatus);

			var value = AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			foreach (CodeDescriptionBool item in value)
			{
				AssertEquals("Is status changed to Invoiced on posting first AR Invoice when Job Status: " + item.Code, (statusesWhenARInvoicePostingdoesnotChangeStatus != null && statusesWhenARInvoicePostingdoesnotChangeStatus.ContainsCode(item.Code)), item.Bool);
			}
		}

		protected void TestPostManager_CriticalCheckError(object sender, CriticalPostingErrorEventArgs e)
		{
			if (e is CriticalTransactionPostingErrorEventArgs)
			{
				TransactionHeader header = ((CriticalTransactionPostingErrorEventArgs)e).Header;
				TransactionHeaderValidation transactionValidation = header.Validation as TransactionHeaderValidation;
				InvoiceBaseValidation invoiceValidation = header.Validation as InvoiceBaseValidation;
				foreach (INotification notification in header.RowErrors)
				{
					if (transactionValidation != null)
					{
						if (notification.Message.Contains(IsCancelledError))
						{
							InvalidIsCancelledError = true;
						}
					}
					if (invoiceValidation != null)
					{
						if (notification.Message.Contains(OutstandingAmountAndFullyPaidDateError))
						{
							InvalidOutstandingAmountAndFullyPaidDateError = true;
						}
						else if (notification.Message.Contains(" is already used "))
						{
							DuplicateAPInvoiceNumberError = true;
						}
					}
				}
			}
			else if (e is CriticalChargePostingErrorEventArgs)
			{
				ChargeWithCost charge = ((CriticalChargePostingErrorEventArgs)e).Charges[0] as ChargeWithCost;
				AssertNotNull("Should be ChargeWithCost", charge);

				ChargeDescriptionError = charge.JR_DescInfo.HasErrors();
			}
			else if (e is CriticalJobPostingErrorEventArgs)
			{
				Job job = ((CriticalJobPostingErrorEventArgs)e).Jobs[0];
				AssertNotNull("Job should be not null", job);

				ProfitLossReasonRequiredError = job.JH_ProfitLossReasonCode.IsEmpty;
			}
			else
			{
				Fail(string.Format("Unknown Critical Posting error event argument type {0}", e.GetType()));
			}
		}

		string IsCancelledError
		{
			get { return "Missing Reversing Transaction for this canceled transaction"; }
		}

		string OutstandingAmountAndFullyPaidDateError
		{
			get { return "invalid Fully Paid Date with respect to the outstanding amount"; }
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;

		protected bool InvalidOutstandingAmountAndFullyPaidDateError;
		protected bool DuplicateAPInvoiceNumberError;
		protected bool InvalidIsCancelledError;
		protected bool ChargeDescriptionError;
		protected bool ProfitLossReasonRequiredError;

		class InvoiceCreatorForTest : APInvoiceCreator
		{
			public InvoiceCreatorForTest(Job job)
				: base(job)
			{
			}

			protected override bool CreateTransactionsCore(TransactionCreatorHashtable transactions, bool isMultiJobOperationInProgress)
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

				OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
				transactions.AddAPInvoice(invoice, header.OH_Code, invoice.InvoiceNumber);
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				if (CauseOutstandingAmountFullyPaidDateError)
				{
					line.AL_LineAmount = 1m;
					invoice.AH_InvoiceAmount = 1m;
					TransactionMatchLink match = ((IMatching)invoice).CurrentMatchGroup.AddNew();
					match.AP_AH = invoice.PK;
					match.AP_Amount = 1;
				}
				else if (CauseIsCancelledError)
				{
					invoice.AH_IsCancelled = ZBool.True;
				}

				invoice.AH_OH = header.PK;
				invoice.AH_OutstandingAmount = ZDecimal.Zero;
				invoice.AH_FullyPaidDate = ZDateTime.Empty;

				return true;
			}

			public bool CauseOutstandingAmountFullyPaidDateError;
			public bool CauseIsCancelledError;
		}

		class APCreditNoteCreatorForTest : APCreditNoteCreator
		{
			public APCreditNoteCreatorForTest(Job job)
				: base(job)
			{
			}

			public override bool CreateTransactions(TransactionCreatorHashtable transactions)
			{
				return false;
			}
		}
	}
}
