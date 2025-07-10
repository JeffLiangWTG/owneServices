using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public abstract class JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorizationTest : JobPostingWorkflowProcessorTest
	{
		public void TestTransactionDatesForBackPosting()
		{
			CommonSetUp();
			jobPostingProcessor = CreateJobPostingProcessor(Shipment);
			AssertNoExceptionThrown(() => jobPostingProcessor.Process(Notifications));
			var invoice = GetInvoice();
			if (GUIProvider != null)
			{
				AssertEquals(PostDateOverride, invoice.AH_PostDate);
			}
		}

		[TestDate(2019, 10, 28)]
		public void TestTransactionNotPostedIfSubLedgerPeriodClosed()
		{
			CommonSetUp();
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			periodManager.CloseSubLedgerPeriod();
			AssertEquals("Precondition: SubLedger Period should be Closed to post", PostDateValidationResult.SubLedgerPeriodClosed, periodCalculator.IsPostDateValid(PostDateOverride));

			InvoiceDateOverride = ZDateTime.Empty;
			jobPostingProcessor = CreateJobPostingProcessor(Shipment);

			LogSubscriberToAbortLogGroupProcessingSilentlyException exceptionThrown = null;
			try
			{
				jobPostingProcessor.Process(Notifications);
			}
			catch (LogSubscriberToAbortLogGroupProcessingSilentlyException e)
			{
				exceptionThrown = e;
			}

			if (GUIProvider != null)
			{
				AssertNotNull(exceptionThrown);
				AssertNotNull("Email in exception", exceptionThrown.Emails);
				AssertEquals(1, Notifications.Events.Count());
				AssertEquals(CargoWise.ComponentModel.NotificationType.Error, Notifications.Events[0].Type);
				AssertEquals("Invoice Date: Please enter a value.\r\nPost Date: This date falls into a period where the sub-ledger is closed\r\n", Notifications.AsString);
			}
			else
			{
				AssertNull(exceptionThrown);
				AssertEquals(0, Notifications.Events.Count());
			}
		}

		protected override string ExpectedMessageForNothingWasPosted => @"You cannot post because job S00010001 has errors. Please fix errors before posting.
 - Invoice Type: There are deferred charges on this job that cannot post.  Please amend the Debtor's Periodic Invoicing setup and/or the Invoice Types used on charges on this job.  The ZZCC1 on this job has a Deferred Invoice Type however the Debtor's Periodic Invoicing configuration does not allow this charge code to be deferred.";

		void CommonSetUp()
		{
			Job.MarkAsNeedingValidation();
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();
			AssertNoErrors(Job);
		}

		protected override void SetUp()
		{
			InvoiceDateOverride = ZDateTime.Today;
			PostDateOverride = ZDateTime.Today.AddDays(-1);
			periodManager = TestObjectCreator.CreateTestPeriods(InvoiceDateOverride.AddMonths(-1));

			base.SetUp();
		}

		protected virtual IPostingJobTransactionsApprovalGUIProvider GUIProvider => new PostingGUIProviderForARCreditNoteApprovalSubscriber(null, null);

		protected ARInvoice GetInvoice()
		{
			return Factory.LoadTop1<ARInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, Shipment.JS_UniqueConsignRef)).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
		}

		protected ZDateTime PostDateOverride { get; set; }
		protected ZDateTime InvoiceDateOverride { get; set; }
		PeriodManager periodManager;
	}
}
