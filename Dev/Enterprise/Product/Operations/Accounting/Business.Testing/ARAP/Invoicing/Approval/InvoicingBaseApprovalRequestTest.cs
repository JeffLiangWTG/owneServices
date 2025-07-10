using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Business.TransactionApproval.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoicingBaseApprovalRequestTest<RequestType, DetailsType> : TransactionApprovalRequestTest<RequestType, DetailsType>
			where RequestType : InvoicingBaseApprovalRequest<DetailsType>
			where DetailsType : ApprovalRequestDetails
	{
		public abstract void TestPostingOptionForDisplay();

		public virtual void TestJobNumber()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestApprovalRequest.XP_ParentID = job.PK;
			TestApprovalRequest.XP_ParentTableCode = job.TablePrefix;
			AssertEquals("S001", TestApprovalRequest.JobNumber);

			TestApprovalRequest.XP_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals("", TestApprovalRequest.JobNumber);

			TestApprovalRequest.XP_ParentID = consol.PK;
			AssertEquals("C001", TestApprovalRequest.JobNumber);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1");
			TestApprovalRequest.XP_ParentID = invoice.PK;
			TestApprovalRequest.XP_ParentTableCode = invoice.TablePrefix;
			AssertEquals("INV1", TestApprovalRequest.JobNumber);
		}

		public void TestLogAddedInInvoiceWhenStatusOfApprovalRequestChanges()
		{
			SetupAndAssertLogAddedInTransactionWhenStatusOfApprovalRequestChanges(InvoiceBasedTransactionType);
		}

		public void TestLogAddedInCreditNoteWhenStatusOfApprovalRequestChanges()
		{
			SetupAndAssertLogAddedInTransactionWhenStatusOfApprovalRequestChanges(CreditNoteBasedTransactionType);
		}

		void SetupAndAssertLogAddedInTransactionWhenStatusOfApprovalRequestChanges(ZString transactionType)
		{
			if (ShouldAddLogsOnTransactionWithApprovalRequestStatusChange)
			{
				var expectedReference = $"{Ledger}|{transactionType}|Cancelled By User";
				InvoicingBase transaction1;
				GenApprovalRequest approvalRequest1;
				(transaction1, approvalRequest1) = CreateTransactionAndApprovalRequest(transactionType, "1");
				approvalRequest1.SetContext(BusinessContext.CancelApprovalRequestByUser);
				approvalRequest1.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Cancelled;
				Factory.Save();
				var query1 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code);
				query1.AddToFilter(StmALogSchema.SL_Reference, expectedReference);
				Assert("IAP log added with reference: Cancelled By User", transaction1.Logs.HasLogWith(query1));

				expectedReference = $"{Ledger}|{transactionType}|Rejected By User";
				InvoicingBase transaction2;
				GenApprovalRequest approvalRequest2;
				(transaction2, approvalRequest2) = CreateTransactionAndApprovalRequest(transactionType, "2");
				approvalRequest2.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Rejected;
				Factory.Save();
				var query2 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code);
				query2.AddToFilter(StmALogSchema.SL_Reference, expectedReference);
				Assert("IAP log added with reference: Rejected By User", transaction2.Logs.HasLogWith(query2));

				expectedReference = $"{Ledger}|{transactionType}|{RequestApprovedReference}";
				InvoicingBase transaction3;
				GenApprovalRequest approvalRequest3;
				(transaction3, approvalRequest3) = CreateTransactionAndApprovalRequest(transactionType, "3");
				approvalRequest3.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Approved;
				Factory.Save();
				var query3 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code);
				query3.AddToFilter(StmALogSchema.SL_Reference, expectedReference);
				Assert($"IAP log added with reference: {RequestApprovedReference}", transaction3.Logs.HasLogWith(query3));

				expectedReference = $"{Ledger}|{transactionType}|Cancelled Due To Edit/Update";
				InvoicingBase transaction4;
				GenApprovalRequest approvalRequest4;
				(transaction4, approvalRequest4) = CreateTransactionAndApprovalRequest(transactionType, "4");
				approvalRequest4.SetContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction);
				approvalRequest4.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Cancelled;
				Factory.Save();
				var query4 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code);
				query4.AddToFilter(StmALogSchema.SL_Reference, expectedReference);
				Assert("IAP log added with reference: Cancelled Due To Edit/Update", transaction4.Logs.HasLogWith(query4));

				expectedReference = $"{Ledger}|{transactionType}|Cancelled As Transaction Already Cancelled/Posted";
				InvoicingBase transaction5;
				GenApprovalRequest approvalRequest5;
				(transaction5, approvalRequest5) = CreateTransactionAndApprovalRequest(transactionType, "5");
				approvalRequest5.SetContext(BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted);
				approvalRequest5.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Cancelled;
				Factory.Save();
				var query5 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code);
				query5.AddToFilter(StmALogSchema.SL_Reference, expectedReference);
				Assert("IAP log added with reference: Cancelled As Transaction Already Cancelled/Posted", transaction5.Logs.HasLogWith(query5));
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestCancellingApprovalRequestThorwsDeveloperExceptionWhenAllThreeContextsArePresent()
		{
			if (ShouldAddLogsOnTransactionWithApprovalRequestStatusChange)
			{
				var expectedMessage = "Approval request cannot be cancelled with more than one context responsible for cancelling. Please investigate why we have added theses contexts at the same time.";
				InvoicingBase transaction1;
				GenApprovalRequest approvalRequest1;
				(transaction1, approvalRequest1) = CreateTransactionAndApprovalRequest(InvoiceBasedTransactionType, "1");
				Factory.Save();

				approvalRequest1.SetContext(BusinessContext.CancelApprovalRequestByUser);
				approvalRequest1.SetContext(BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted);
				approvalRequest1.SetContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction);

				approvalRequest1.SetContext(BusinessContext.APInvoiceForm);

				approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;

				Factory.Save();

				var instance = ExceptionReporterTestListener.Instance;
				AssertEquals("Should throw exception as more than one cancelling context present", expectedMessage, instance[0].InnerException.Message);

				instance.Clear();
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestCancellingApprovalRequestThorwsDeveloperExceptionWhenTwoContextsPresent()
		{
			if (ShouldAddLogsOnTransactionWithApprovalRequestStatusChange)
			{
				var expectedMessage = "Approval request cannot be cancelled with more than one context responsible for cancelling. Please investigate why we have added theses contexts at the same time.";
				InvoicingBase transaction1;
				GenApprovalRequest approvalRequest1;
				(transaction1, approvalRequest1) = CreateTransactionAndApprovalRequest(InvoiceBasedTransactionType, "1");
				Factory.Save();

				approvalRequest1.SetContext(BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted);
				approvalRequest1.SetContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction);

				approvalRequest1.SetContext(BusinessContext.APInvoiceForm);

				approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;

				Factory.Save();

				var instance = ExceptionReporterTestListener.Instance;
				AssertEquals("Should throw exception as more than one cancelling context present", expectedMessage, instance[0].InnerException.Message);

				instance.Clear();
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestCancellingApprovalRequestThrowsDeveloperExceptionWhenNoRelevantContextPresent()
		{
			if (ShouldAddLogsOnTransactionWithApprovalRequestStatusChange)
			{
				GenApprovalRequest approvalRequest1;
				(_, approvalRequest1) = CreateTransactionAndApprovalRequest(InvoiceBasedTransactionType, "1");
				Factory.Save();

				approvalRequest1.SetContext(BusinessContext.APInvoiceForm);

				approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;

				Factory.Save();

				var noBusinessContextError = @"We set at least one Business Context when cancelling Approval request. For some reason no context was set while cancelling this approval request.
Please check if we missed to add one of the Business context when its valid to cancel an approval request.
Or if in the unit test we are setting the Approval Status to CAN directly then we might need to set one of the contexts to the Approval Request.";

				var expectedJobNumberInfo = @"XP_ParentTableCode: AH
JobNumber/TransactionNum: 1";

				var expectedStackTraceMethodLine = "TestCancellingApprovalRequestThrowsDeveloperExceptionWhenNoRelevantContextPresent()";

				AssertEquals("Should contain the No Business Context error message", true, ErrorReporter.LastMessageReported.Contains(noBusinessContextError));

				if (this is APInvoiceChargesApprovalRequestTest)
				{
					var noStackTraceInfo = "ApprovalRequestCancelledWithoutAnyBusinessContext: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1";
					AssertEquals("Request Info should contain the no data collected message", true, ErrorReporter.LastMessageReported.Contains(noStackTraceInfo));
				}
				else
				{
					AssertEquals("Request Info should contain the expected transaction job number", true, ErrorReporter.LastMessageReported.Contains(expectedJobNumberInfo));
					AssertEquals("Request Info should contain the expected line in the Stack Trace", true, ErrorReporter.LastMessageReported.Contains(expectedStackTraceMethodLine));
				}

				ErrorReporter.Clear();
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestCancellingApprovalRequestNoDeveloperExceptionWhenTwoContextPresent()
		{
			if (ShouldAddLogsOnTransactionWithApprovalRequestStatusChange)
			{
				GenApprovalRequest approvalRequest1;
				(_, approvalRequest1) = CreateTransactionAndApprovalRequest(InvoiceBasedTransactionType, "1");
				Factory.Save();

				approvalRequest1.SetContext(BusinessContext.APInvoiceForm);
				approvalRequest1.SetContext(BusinessContext.CancelApprovalRequestByUser);

				approvalRequest1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;

				Factory.Save();

				var noBusinessContextError = @"We set at least one Business Context when cancelling Approval request. For some reason no context was set while cancelling this approval request.
Please check if we missed to add one of the Business context when its valid to cancel an approval request.
Or if in the unit test we are setting the Approval Status to CAN directly then we might need to set one of the contexts to the Approval Request.";

				AssertEquals("Should not contain the No Business Context error message", false, ErrorReporter.LastMessageReported.Contains(noBusinessContextError));

				ErrorReporter.Clear();
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		protected virtual ZBool ShouldAddLogsOnTransactionWithApprovalRequestStatusChange => false;
		protected virtual (InvoicingBase, GenApprovalRequest) CreateTransactionAndApprovalRequest(ZString transactionType, ZString transactionNumber) { return (null, null); }
		protected virtual ZString Ledger => ZString.Empty;
		protected virtual ZString InvoiceBasedTransactionType => ZString.Empty;
		protected virtual ZString CreditNoteBasedTransactionType => ZString.Empty;
		protected virtual ZString RequestApprovedReference => ZString.Empty;
	}
}
