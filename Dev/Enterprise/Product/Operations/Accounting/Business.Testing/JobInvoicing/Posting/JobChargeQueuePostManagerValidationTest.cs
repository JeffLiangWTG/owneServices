using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing.Posting
{
	class JobChargeQueuePostManagerValidationTest : PostManagerValidationTest
	{
		public void TestJobChargePostingHashMismatchValidationForCost()
		{
			PrepareHashMismatchValidationTestData(out Job job, out Charge charge1, out JobChargePostingQueue queue1);

			var charges = new List<(Job job, IEnumerable<Charge> charges)>();
			charges.Add((job, new[] { charge1 }));

			charge1.JR_OSCostAmt = 11M;
			Factory.Save();

			var validation = new JobChargeQueuePostManagerValidationForTest(new[] { job }, JobInvoicingPostingOption.Costs, charges, new[] { queue1 });

			AssertEquals("Count of posted charges", 1, validation.GetCharges_ForTestOnly(job).Count());

			var validationResult = validation.Validate();
			AssertNotNull("Validation result should be not null", validationResult);
			AssertEquals("Should be an Error", CargoWise.ComponentModel.NotificationType.Error, validationResult.Type);
			AssertEquals("Error Message", "The charges were changed after creating the posting queue.", validationResult.Message);
		}

		public void TestJobChargePostingHashMismatchValidationForRevenue()
		{
			PrepareHashMismatchValidationTestData(out Job job, out Charge charge1, out JobChargePostingQueue queue1);

			var charges = new List<(Job job, IEnumerable<Charge> charges)>();
			charges.Add((job, new[] { charge1 }));

			charge1.JR_OSSellAmt = 111M;
			Factory.Save();

			var validation = new JobChargeQueuePostManagerValidationForTest(new[] { job }, JobInvoicingPostingOption.Revenue, charges, new[] { queue1 });

			AssertEquals("Count of posted charges", 1, validation.GetCharges_ForTestOnly(job).Count());

			var validationResult = validation.Validate();
			AssertNotNull("Validation result should be not null", validationResult);
			AssertEquals("Should be an Error", CargoWise.ComponentModel.NotificationType.Error, validationResult.Type);
			AssertEquals("Error Message", "The charges were changed after creating the posting queue.", validationResult.Message);
		}

		public void TestJobChargePostingInvalidHashVersion()
		{
			PrepareHashMismatchValidationTestData(out Job job, out Charge charge1, out JobChargePostingQueue queue1);

			var charges = new List<(Job job, IEnumerable<Charge> charges)>();
			charges.Add((job, new[] { charge1 }));

			var validation = new JobChargeQueuePostManagerValidationForTest(new[] { job }, JobInvoicingPostingOption.Revenue, charges, new[] { queue1 });

			ZByte invalidVersion = AccountingConstants.ChargeHashCalculatorInfo.ChargeCostHashVersion + 1;
			queue1.JPQ_HashVersion = invalidVersion;
			Factory.Save();

			var ex = AssertExceptionThrown<ArgumentException>(() => validation.Validate());
			AssertEquals(FormattableString.Invariant($"Invalid hash version {invalidVersion} for Job Charge Sell part."), ex.Message);
		}

		public void TestJobChargePostingValidForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs);
			AssertHasNoValidationErrors(validator);
		}

		public void TestJobChargePostingValidForRevenue()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Revenue);
			AssertHasNoValidationErrors(validator);
		}

		public void TestJobChargePostingProfitLossReasonCodeValidation()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Revenue,
				() =>
				{
					//Charges 1 & 2 are to be posted - their combined profit margin = 33.33%
					charge1.JR_AC = CC1.PK;
					charge1.JR_OSCostAmt = 100;
					charge1.JR_OSSellAmt = 150;
					charge2.JR_AC = CC1.PK;
					charge2.JR_OSCostAmt = 100;
					charge2.JR_OSSellAmt = 150;
					//Charge 3 will not be posted but should still be included in the profit margin calculation
					charge3.JR_AC = CC1.PK;
					charge3.JR_OSCostAmt = 300;
					charge3.JR_OSSellAmt = 400;
				},
				isCreateChargeWithoutPostingInstruction: true);

			//All 3 charges above combined have profit margin = 28.57% which is within this accepted threshold 26-30%
			var plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.LossThreshold = 26M;
			plRequiringReasonParameters.ProfitThreshold = 30M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = testJob.JH_Status;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			base.AssertHasNoValidationErrors(validator);
		}

		public void TestJobChargePostingHasDifferentInvoiceDueDateForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs,
				() =>
				{
					charge1.JR_PaymentDate = charge1.JR_PaymentDate.AddDays(5);
				});
			AssertNotEquals("Pre-condition", charge1.JR_PaymentDate, charge2.JR_PaymentDate);
			AssertHasValidationError("Charges of the job cannot be posted because charges for the same creditor and invoice number should have same invoice due date.", validator);
		}

		public void TestJobChargePostingHasDifferentPaymentTypeForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs,
				() =>
				{
					charge1.JR_PaymentType = ReceiptTypes.Cash;
					charge1.JR_AB = TestObjectCreator.AUDBankAccount.PK;
					charge1.JR_AK = TestObjectCreator.AUDChequeBook.PK;
					charge1.JR_ChequeNo = "TSTCHK01";
					charge2.JR_PaymentType = ReceiptTypes.Cheque;
					charge2.JR_AB = TestObjectCreator.AUDBankAccount.PK;
					charge2.JR_AK = TestObjectCreator.AUDChequeBook.PK;
					charge2.JR_ChequeNo = "TSTCHK01";
				});
			AssertNotEquals("Pre-condition", charge1.JR_PaymentType, charge2.JR_PaymentType);
			AssertHasValidationError("Charges of the job cannot be posted because charges for the same creditor and invoice number should have same payment type.", validator);
		}

		public void TestJobChargePostingHasDifferentBankAccountForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs,
				() =>
				{
					charge1.JR_PaymentType = ReceiptTypes.Cheque;
					charge1.JR_AB = TestObjectCreator.AUDBankAccount.PK;
					charge1.JR_AK = TestObjectCreator.AUDChequeBook.PK;
					charge1.JR_ChequeNo = "TSTCHK01";
					charge2.JR_PaymentType = ReceiptTypes.Cheque;
					charge2.JR_AB = TestObjectCreator.AUDBankAccount2.PK;
					charge2.JR_AK = TestObjectCreator.AUDChequeBook.PK;
					charge2.JR_ChequeNo = "TSTCHK01";
				});
			AssertNotEquals("Pre-condition", charge1.JR_AB, charge2.JR_AB);
			AssertHasValidationError("Charges of the job cannot be posted because charges for the same creditor and invoice number should have same bank account.", validator);
		}

		public void TestJobChargePostingHasDifferentChequeBookForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs,
				() =>
				{
					charge1.JR_PaymentType = ReceiptTypes.Cheque;
					charge1.JR_AB = TestObjectCreator.AUDBankAccount.PK;
					charge1.JR_AK = TestObjectCreator.AUDChequeBook.PK;
					charge1.JR_ChequeNo = "TSTCHK01";
					charge2.JR_PaymentType = ReceiptTypes.Cheque;
					charge2.JR_AB = TestObjectCreator.AUDBankAccount.PK;
					charge2.JR_AK = TestObjectCreator.AUDChequeBook2.PK;
					charge2.JR_ChequeNo = "TSTCHK01";
				});
			AssertNotEquals("Pre-condition", charge1.JR_AK, charge2.JR_AK);
			AssertHasValidationError("Charges of the job cannot be posted because charges for the same creditor and invoice number should have same cheque book.", validator);
		}

		public void TestJobChargePostingHasDifferentChequeNumberForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs,
				() =>
				{
					charge1.JR_PaymentType = ReceiptTypes.Cheque;
					charge1.JR_AB = TestObjectCreator.AUDBankAccount.PK;
					charge1.JR_AK = TestObjectCreator.AUDChequeBook.PK;
					charge1.JR_ChequeNo = "TSTCHK01";
					charge2.JR_PaymentType = ReceiptTypes.Cheque;
					charge2.JR_AB = TestObjectCreator.AUDBankAccount.PK;
					charge2.JR_AK = TestObjectCreator.AUDChequeBook.PK;
					charge2.JR_ChequeNo = "TSTCHK02";
				});
			AssertNotEquals("Pre-condition", charge1.JR_ChequeNo, charge2.JR_ChequeNo);
			AssertHasValidationError("Charges of the job cannot be posted because charges for the same creditor and invoice number should have same cheque number.", validator);
		}

		public void TestJobChargePostingHasDifferentInvoiceDateForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs,
				() =>
				{
					charge1.JR_APInvoiceDate = charge1.JR_APInvoiceDate.AddDays(5);
				});
			AssertNotEquals("Pre-condition", charge1.JR_APInvoiceDate, charge2.JR_APInvoiceDate);
			AssertHasValidationError("Charges of the job cannot be posted because charges for the same creditor and invoice number should have same invoice date.", validator);
		}

		public void TestJobChargePostingHasRelativeChargesWithoutPostingInstructionForCost()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Costs, isCreateChargeWithoutPostingInstruction: true);
			AssertHasValidationError("Charges of the job cannot be posted because some charges with the same creditor and invoice number do not have posting instruction.", validator);
		}

		public void TestJobChargePostingHasRelativeChargesWithoutPostingInstructionForRevenue()
		{
			var validator = CreateValidatorWithTestData(JobInvoicingPostingOption.Revenue, isCreateChargeWithoutPostingInstruction: true);
			AssertHasNoValidationErrors(validator); // Allow to only post revenue charges with posting instruction.
		}

		public void TestJobChargePostingWithMultipleGroups()
		{
			PrepareTestEnvironment();

			var charge11 = CreateCharge(testJob, CC1, "Charge Code 1-1", AUD, 100M, Creditor1, AUD, 150M, LocalClient, "INV001", ZDateTime.Today, ZDateTime.Today);
			var charge12 = CreateCharge(testJob, CC2, "Charge Code 1-2", AUD, 200M, Creditor1, AUD, 300M, LocalClient, "INV001", ZDateTime.Today, ZDateTime.Today);
			var charge21 = CreateCharge(testJob, CC1, "Charge Code 2-1", AUD, 120M, Creditor2, AUD, 240M, LocalClient, "INV002", ZDateTime.Today, ZDateTime.Today);
			var charge22 = CreateCharge(testJob, CC2, "Charge Code 2-2", AUD, 220M, Creditor2, AUD, 440M, LocalClient, "INV002", ZDateTime.Today, ZDateTime.Today);
			Factory.Save();

			var jobReloaded = Factory.CreateNewFactory().Load<Job>(testJob.PK);

			var queue11 = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostCost, charge11, charge11.Job.JH_ParentID, charge11.Job.JH_ParentTableCode);
			var queue12 = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostCost, charge12, charge12.Job.JH_ParentID, charge12.Job.JH_ParentTableCode);
			var queue21 = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostCost, charge21, charge21.Job.JH_ParentID, charge21.Job.JH_ParentTableCode);
			var queue22 = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostCost, charge22, charge22.Job.JH_ParentID, charge22.Job.JH_ParentTableCode);

			var validator = new JobChargeQueuePostManagerValidationForTest(
				new[] { jobReloaded },
				JobInvoicingPostingOption.Costs,
				new List<(Job, IEnumerable<Charge>)> { (jobReloaded, new[] { charge11, charge12, charge21, charge22 }) },
				new JobChargePostingQueue[] { queue11, queue12, queue21, queue22 });

			AssertTwoChargesEqualForTest(charge11, charge12);
			AssertTwoChargesEqualForTest(charge21, charge22);

			AssertHasNoValidationErrors(validator);

			charge21.JR_PaymentDate = charge21.JR_PaymentDate.AddDays(5);
			Factory.Save();
			AssertNotEquals("Pre-condition", charge21.JR_PaymentDate, charge22.JR_PaymentDate);

			AssertHasValidationError("Charges of the job cannot be posted because charges for the same creditor and invoice number should have same invoice due date.", validator);
		}

		#region Override

		public new void TestBillOfLadingRevenueRecognitionValidationForVADAndVDD()
		{
			Assert("The sub class does not support PostingOption ALL", true);
		}

		public override void TestDifferencesInReloadedChargesValidation()
		{
			Assert("This test is not for JobChargeQueuePostManagerValidation because we don't cover charges difference", true);
		}

		protected override void AssertHasValidationError(string expectedError, PostManagerValidation validation)
		{
			ReloadPostManagerValidation(validation);
			base.AssertHasValidationError(expectedError, validation);
		}

		protected override void AssertHasNoValidationErrors(PostManagerValidation validation)
		{
			ReloadPostManagerValidation(validation);
			base.AssertHasNoValidationErrors(validation);
		}

		protected override void AssertHasNoValidationWarnings(PostManagerValidation validation)
		{
			ReloadPostManagerValidation(validation);
			base.AssertHasNoValidationWarnings(validation);
		}

		protected override void AssertNoChargesValidation(PostManagerValidation validation)
		{
			ReloadPostManagerValidation(validation);
			base.AssertNoChargesValidation(validation);
		}

		protected override PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			var charges = new List<(Job job, IEnumerable<Charge> charges)>();
			var queueManagerValidation = new JobChargeQueuePostManagerValidationForTest(jobs, postingOption, charges, Array.Empty<IJobChargePostingQueue>());
			queueManagerValidation.ReloadSelectedJobCharges_ForTestOnly();
			return queueManagerValidation;
		}

		#endregion

		#region Implementation

		void ReloadPostManagerValidation(PostManagerValidation validation)
		{
			var queueManagerValidation = validation as JobChargeQueuePostManagerValidationForTest;
			queueManagerValidation?.ReloadSelectedJobCharges_ForTestOnly();
		}

		void AssertTwoChargesEqualForTest(Charge charge1, Charge charge2)
		{
			AssertEquals("Date & Creditor", charge1.JR_APInvoiceDate, charge2.JR_APInvoiceDate);
			AssertEquals(charge1.JR_PaymentDate, charge2.JR_PaymentDate);
			AssertEquals(charge1.JR_APInvoiceNum, charge2.JR_APInvoiceNum);
			AssertEquals(charge1.JR_OH_CostAccount, charge2.JR_OH_CostAccount);
			AssertEquals("Payment Details", charge1.JR_PaymentType, charge2.JR_PaymentType);
			AssertEquals(charge1.JR_AB, charge2.JR_AB);
			AssertEquals(charge1.JR_AK, charge2.JR_AK);
			AssertEquals(charge1.JR_ChequeNo, charge2.JR_ChequeNo);
		}

		void PrepareTestEnvironment()
		{
			SetupInvoiceStyles(LocalClient, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			LocalClient.Factory.Save();
			Create2MonthPeriod();

			testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			Factory.Save();
		}

		Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, string desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor, ZString? apInvoiceNum, ZDateTime? invoiceDate, ZDateTime? paymentDate)
		{
			var charge = CreateCharge(parentJob, chargeCode, desc, costCurrency, oSCostAmt, creditor, sellCurrency, oSSellAmt, debtor);
			if (apInvoiceNum.HasValue)
			{
				charge.JR_APInvoiceNum = apInvoiceNum.Value;
			}
			if (invoiceDate.HasValue)
			{
				charge.JR_APInvoiceDate = invoiceDate.Value;
			}
			if (paymentDate.HasValue)
			{
				charge.JR_PaymentDate = paymentDate.Value;
			}

			return charge;
		}

		JobChargeQueuePostManagerValidation CreateValidatorWithTestData(JobInvoicingPostingOption postingInstruction, Action additionalSetting = null, bool isCreateChargeWithoutPostingInstruction = false)
		{
			PrepareTestEnvironment();

			ZString? invoiceNum = null;
			ZDateTime? invoiceDate = null;
			ZDateTime? paymentDate = null;
			if (postingInstruction == JobInvoicingPostingOption.Costs)
			{
				invoiceNum = "INV001";
				invoiceDate = ZDateTime.Today;
				paymentDate = ZDateTime.Today;
			}

			charge1 = CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient, invoiceNum, invoiceDate, paymentDate);
			charge2 = CreateCharge(testJob, CC2, "Charge Code 2", AUD, 200M, Creditor1, AUD, 300M, LocalClient, invoiceNum, invoiceDate, paymentDate);
			AssertTwoChargesEqualForTest(charge1, charge2);

			if (isCreateChargeWithoutPostingInstruction)
			{
				charge3 = CreateCharge(testJob, CC2, "Charge Code 3", AUD, 300M, Creditor1, AUD, 450M, LocalClient, invoiceNum, invoiceDate, paymentDate);
				AssertTwoChargesEqualForTest(charge2, charge3);
			}

			var nonRelativeJob = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			var chargeLinkingDifferentJob = CreateCharge(nonRelativeJob, CC1, "Charge Code 1", AUD, 400M, Creditor1, AUD, 600M, LocalClient, invoiceNum, invoiceDate, paymentDate);
			AssertNotEquals(charge1.JR_JH, chargeLinkingDifferentJob.JR_JH);

			if (additionalSetting != null)
			{
				additionalSetting();
			}

			Factory.Save();

			var jobReloaded = Factory.CreateNewFactory().Load<Job>(testJob.PK);
			var postOption = postingInstruction == JobInvoicingPostingOption.Costs ? JobChargePostingQueueLookups.PostCost : JobChargePostingQueueLookups.PostRevenue;

			queue1 = JobChargePostingQueue.CreateNew(Factory, postOption, charge1, charge1.Job.JH_ParentID, charge1.Job.JH_ParentTableCode);
			queue2 = JobChargePostingQueue.CreateNew(Factory, postOption, charge2, charge2.Job.JH_ParentID, charge2.Job.JH_ParentTableCode);

			var validator = new JobChargeQueuePostManagerValidationForTest(
				new[] { jobReloaded },
				postingInstruction,
				new List<(Job, IEnumerable<Charge>)> { (jobReloaded, new[] { charge1, charge2 }) },
				new JobChargePostingQueue[] { queue1, queue2 });

			return validator;
		}

		void PrepareHashMismatchValidationTestData(out Job job, out Charge charge1, out JobChargePostingQueue queue1)
		{
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
			charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 10M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc 2", TestObjectCreator.AUD, 20M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 200M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));

			queue1 = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostCost, charge1, shipment.PK, "JS");
			var queue2 = JobChargePostingQueue.CreateNew(Factory, JobChargePostingQueueLookups.PostRevenue, charge1, shipment.PK, "JS");
			Factory.Save();
		}

		class JobChargeQueuePostManagerValidationForTest : JobChargeQueuePostManagerValidation
		{
			public JobChargeQueuePostManagerValidationForTest(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<(Job job, IEnumerable<Charge> charges)> jobsWithSelectedCharges, IEnumerable<IJobChargePostingQueue> jobChargePostingQueues) : base(jobs, postingOption, jobsWithSelectedCharges, jobChargePostingQueues)
			{
			}

			public void ReloadSelectedJobCharges_ForTestOnly()
			{
				var charges = new List<(Job job, IEnumerable<Charge> charges)>();
				foreach (var job in Jobs)
				{
					charges.Add((job, job.Charges.ToArray<Charge>()));
				}

				var propertyInfo = typeof(JobChargeQueuePostManagerValidation).GetProperty("JobsWithSelectedCharges", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				propertyInfo.SetValue(this, charges);
			}

			public IEnumerable<Charge> GetCharges_ForTestOnly(Job job)
			{
				return GetCharges(job);
			}
		}

		Charge charge1, charge2, charge3;
		JobChargePostingQueue queue1, queue2;
		Job testJob;

		#endregion
	}
}
