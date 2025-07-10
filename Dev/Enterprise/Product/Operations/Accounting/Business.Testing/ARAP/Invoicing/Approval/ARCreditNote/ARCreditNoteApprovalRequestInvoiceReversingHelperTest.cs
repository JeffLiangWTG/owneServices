using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARCreditNoteApprovalRequestInvoiceReversingHelperTest : TestCaseWithFactory
	{
		public void TestReverseARInvoice()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK));
			arInvoice.AH_JH = job.PK;
			Factory.Save();

			var request = CreateApprovalRequest(arInvoice, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request.XP_GS_NKApprovingUser1 = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			request.XP_ApprovalDate = ZDateTime.Now;
			Factory.Save();

			Assert(!arInvoice.AH_IsCancelled);

			var logs = new List<string>();
			Action<LogType, string> appendToNotificationLog = (l, s) => { logs.Add(s); };
			bool canSaveFactory = ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, appendToNotificationLog);
			if (canSaveFactory)
			{ Factory.Save(); }

			var expectedMessage = FormattableString.Invariant($"Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {request.PK} can be reversed.");
			Assert(logs.Contains(expectedMessage));

			var newFactory = new BusinessObjectFactory();
			var reloadedInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
			Assert(reloadedInvoice.AH_IsCancelled);
			AssertEquals(0m, reloadedInvoice.AH_OutstandingAmount);

			var reloadedRequest = newFactory.Load<ARCreditNoteApprovalRequest>(request.PK);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, reloadedRequest.XP_ApprovalStatus);

			var arCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
			AssertEquals(1, arCreditNotes.Length);
			AssertEquals(reloadedInvoice.AH_GB, arCreditNotes[0].AH_GB);
			AssertEquals(reloadedInvoice.AH_GE, arCreditNotes[0].AH_GE);
			AssertEquals(reloadedInvoice.AH_LocalTotalAmount, arCreditNotes[0].AH_LocalTotalAmount);
			AssertEquals(Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating, arCreditNotes[0].AH_ReceiptType);

			var authLog = arCreditNotes[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode)).FirstOrDefault();
			AssertNotNull(authLog);
			AssertEquals(request.XP_GS_NKApprovingUser1, authLog.SL_GS_NKUser);
			AssertEquals(request.XP_ApprovalDate, authLog.SL_EventTime);
		}

		public void TestReversePositiveAdjustmentNote()
		{
			var positiveARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var request = CreateApprovalRequest(positiveARAdjustmentNote, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			Factory.Save();

			Assert(!positiveARAdjustmentNote.AH_IsCancelled);
			var logs = new List<string>();
			Action<LogType, string> appendToNotificationLog = (l, s) => { logs.Add(s); };
			ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, appendToNotificationLog);
			Factory.Save();

			var expectedMessage = FormattableString.Invariant($"Transaction {positiveARAdjustmentNote.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {request.PK} can be reversed.");
			Assert(logs.Contains(expectedMessage));

			var newFactory = new BusinessObjectFactory();
			var reloadedpositiveARAdjustmentNote = newFactory.Load<ARInvoice>(positiveARAdjustmentNote.PK);
			Assert(reloadedpositiveARAdjustmentNote.AH_IsCancelled);
			AssertEquals(0m, reloadedpositiveARAdjustmentNote.AH_OutstandingAmount);

			var reloadedRequest = newFactory.Load<ARCreditNoteApprovalRequest>(request.PK);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, reloadedRequest.XP_ApprovalStatus);

			var adjustmentNotes = newFactory.Load<ARAdjustmentNote>(new ZQuery());
			AssertEquals(2, adjustmentNotes.Length);
			var negativeAdjustmentNote = adjustmentNotes.First(x => x.PK != reloadedpositiveARAdjustmentNote.PK);
			AssertEquals(reloadedpositiveARAdjustmentNote.AH_GB, negativeAdjustmentNote.AH_GB);
			AssertEquals(reloadedpositiveARAdjustmentNote.AH_GE, negativeAdjustmentNote.AH_GE);
			AssertEquals(reloadedpositiveARAdjustmentNote.AH_LocalTotalAmount, -negativeAdjustmentNote.AH_LocalTotalAmount);
			AssertEquals(Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating, negativeAdjustmentNote.AH_ReceiptType);
		}

		public void TestValidationRunsWhenReversing()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK));
			arInvoice.AH_JH = job.PK;
			Factory.Save();

			var request = CreateApprovalRequest(arInvoice, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			Factory.Save();

			string cantReverseMessage;
			TestObjectCreator.ReverseTransaction(arInvoice, out cantReverseMessage);
			Factory.Save();

			var arCreditNotes = Factory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
			AssertEquals(1, arCreditNotes.Length);

			Assert(arInvoice.AH_IsCancelled);
			var logs = new List<string>();
			Action<LogType, string> appendToNotificationLog = (l, s) => { logs.Add(s); };
			AssertEquals(false, ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, appendToNotificationLog));
			Factory.Save();

			var expectedMessage = FormattableString.Invariant($"Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {request.PK} could not be reversed. This transaction cannot be reversed because it has already been reversed or is a reversal of another transaction.");
			Assert(logs.Contains(expectedMessage));

			var newFactory = new BusinessObjectFactory();
			var reloadedInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
			Assert(reloadedInvoice.AH_IsCancelled);

			var reloadedRequest = newFactory.Load<ARCreditNoteApprovalRequest>(request.PK);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, reloadedRequest.XP_ApprovalStatus);

			arCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
			AssertEquals(1, arCreditNotes.Length);
		}

		public void TestSecurityCheckPointsWhileReversing()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			TestObjectCreator.CreateRelatedARAndAPInvoices(job.PK, TestObjectCreator.CC1, out ARInvoice arInvoice, out APInvoice apInvoice);
			arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			arInvoice.AH_JH = job.PK;
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoice);
			var request = CreateApprovalRequest(arInvoice, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			Factory.Save();

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;

			Assert(!arInvoice.AH_IsCancelled);
			var logs = new List<string>();
			Action<LogType, string> appendToNotificationLog = (l, s) => { logs.Add(s); };
			AssertEquals(false, ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, appendToNotificationLog));
			Factory.Save();

			var expectedMessage = FormattableString.Invariant($"Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {request.PK} could not be reversed. {Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.ErrorMessageForNotAllowed}");
			Assert(logs.Contains(expectedMessage));

			var newFactory = new BusinessObjectFactory();
			var reloadedInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
			Assert(!reloadedInvoice.AH_IsCancelled);

			var reloadedRequest = newFactory.Load<ARCreditNoteApprovalRequest>(request.PK);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, reloadedRequest.XP_ApprovalStatus);

			var arCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
			AssertEquals(0, arCreditNotes.Length);
		}

		[TestDate(2015, 03, 08)]
		public void TestCorrectinvoiceAndPostDateForTheReversingTransaction_BackDateInvoicesConfigurationRegistry()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, arInvoice.PK));
			arInvoice.AH_JH = job.PK;
			Factory.Save();

			AssertEquals(new ZDateTime(2015, 03, 08), arInvoice.AH_InvoiceDate.Date);
			AssertEquals(new ZDateTime(2015, 03, 08), arInvoice.AH_PostDate.Date);

			var request = CreateApprovalRequest(arInvoice, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			Factory.Save();

			Assert(!arInvoice.AH_IsCancelled);

			var configuration = new BackDateInvoicesConfiguration();
			configuration.InvoiceDateConfigurationCollection.RemoveAll();
			TestObjectCreator.AddInvoiceDateConfiguration(configuration, "SHP", "ALL", "ALL", "ALL",
				InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth,
				InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate,
				true, true);
			configuration.OverridePostDate = true;
			configuration.DefaultPostDateFromInvoiceDate = true;
			using (AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configuration))
			{
				var logs = new List<string>();
				Action<LogType, string> appendToNotificationLog = (l, s) => { logs.Add(s); };
				ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, appendToNotificationLog);
				Factory.Save();

				var expectedMessage = FormattableString.Invariant($"Transaction {arInvoice.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {request.PK} can be reversed.");
				Assert(logs.Contains(expectedMessage));

				var newFactory = new BusinessObjectFactory();
				var reloadedInvoice = newFactory.Load<ARInvoice>(arInvoice.PK);
				Assert(arInvoice.AH_IsCancelled);

				var arCreditNotes = newFactory.Load<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "CRD"));
				AssertEquals(1, arCreditNotes.Length);
				AssertEquals("AH_InvoiceDate", new ZDateTime(2015, 02, 28), arCreditNotes[0].AH_InvoiceDate);
				AssertEquals("AH_PostDate", new ZDateTime(2015, 03, 08), arCreditNotes[0].AH_PostDate);
			}
		}

		public void TestHandleReversing_DefaultReturnValue()
		{
			AssertEquals(false, ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(null, null));
		}

		public void TestClosedJobsReopenWhenReversing()
		{
			var (job, request) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
			var reloadedJob = Factory.Load<Job>(job.PK);
			AssertEquals("Pre-requisite: Job should be closed", JobHeaderStatus.Closed.Code, reloadedJob.JH_Status);

			var canSaveFactory = ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, (x, y) => { /* Discard log */ });
			Assert(canSaveFactory);

			reloadedJob = Factory.Load<Job>(job.PK);
			AssertEquals("Job should have been reopened", JobHeaderStatus.Working.Code, reloadedJob.JH_Status);
		}

		public void TestClosedJobsReopenIndependentOfSecurityRights()
		{
			var (job, request) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
			var reloadedJob = Factory.Load<Job>(job.PK);
			AssertEquals("Pre-requisite: Job should be closed", JobHeaderStatus.Closed.Code, reloadedJob.JH_Status);

			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			var canSaveFactory = ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, (x,y) => { /* Discard log */ });
			Assert(canSaveFactory);

			reloadedJob = Factory.Load<Job>(job.PK);
			AssertEquals("Job should have been reopened", JobHeaderStatus.Working.Code, reloadedJob.JH_Status);
		}

		(Job, ARCreditNoteApprovalRequest) CreateARCreditNoteForInvoiceReversalApprovalRequest(bool closeJob)
		{
			var request = TestObjectCreator.CreateInvoiceReversalApprovalRequest(100m);
			var invoice = Factory.Load<ARInvoice>(request.XP_ParentID);
			var job = invoice.RelatedJobsForReversing.FirstOrDefault();
			if (closeJob)
			{
				job.Close(
					(job, errorMessage) => { Fail(errorMessage); },
					(sender, args) => { Fail(args.QueryMessage); }
				);
			}
			Factory.Save();

			return (job, request);
		}

		ARCreditNoteApprovalRequest CreateApprovalRequest(InvoicingBase parent, ZString approvalStatus)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.ChangeApprovalTypeForInvoiceReversal();
			approvalRequest.Initialize(new[] { parent }, parent.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = approvalStatus;
			approvalRequest.XP_ReasonCode = Enterprise.Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectRating;
			return approvalRequest;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
