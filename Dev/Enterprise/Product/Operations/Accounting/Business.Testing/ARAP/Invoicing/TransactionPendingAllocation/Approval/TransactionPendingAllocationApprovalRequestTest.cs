using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalRequest))]
	class TransactionPendingAllocationApprovalRequestTest : InvoicingBaseApprovalRequestTest<TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		public override void TestIsPostingActionTheSame()
		{
			var a = GetNewApprovalRequest();
			var b = GetNewApprovalRequest();
			a.PostingDetails.MaxAmountToApprove = 1;
			b.PostingDetails.MaxAmountToApprove = 0;
			Assert("Posting action is the same for any details", a.IsPostingActionTheSame(b));
			Assert("Posting action is the same for any details", b.IsPostingActionTheSame(a));
		}

		public void TestLookupsType()
		{
			var approvalForTest = GetNewApprovalRequest();
			AssertType<TransactionPendingAllocationApprovalRequestLookups>(approvalForTest.Lookups);
		}

		public void TestInitialize()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100, 21);
			var request = GetNewApprovalRequest();
			request.Initialize(transaction);

			Assert(!request.XP_ReasonDescription.IsEmpty);
			AssertEquals(transaction.AH_Desc, request.XP_ReasonDescription);
			AssertEquals(transaction.PK, request.XP_ParentID);
			AssertEquals(transaction.TablePrefix, request.XP_ParentTableCode);
			AssertEquals(transaction.TablePrefix, request.XP_ParentTableCode);
			AssertEquals(121M, request.PostingDetails.MaxAmountToApprove);
			AssertEquals(TestObjectCreator.Creditor1.PK, request.PostingDetails.CreditorPK);
			AssertEquals("INV1112", request.PostingDetails.TransactionNumber);
			AssertEquals("", request.PostingDetails.SourceXML);
			AssertEquals(expected: false, request.PostingDetails.IsCrossLedgerImportFromXML);
		}

		public void TestInitializeWithPreviousRequestXML()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100, 21);
			var request = GetNewApprovalRequest();
			var expectedXml = "<xml>some xml</xml>";
			request.Initialize(transaction, expectedXml, isCrossLedgerImportFromXML: true);

			request = GetNewApprovalRequest();
			request.Initialize(transaction);

			this.AssertXMLEqualsByDiff(expectedXml, request.PostingDetails.SourceXML);
			AssertEquals(expected: true, request.PostingDetails.IsCrossLedgerImportFromXML);
		}

		public void TestInitializeWithXML()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100, 21);
			var request = GetNewApprovalRequest();
			request.Initialize(transaction, "some xml", isCrossLedgerImportFromXML: true);

			AssertEquals(transaction.PK, request.XP_ParentID);
			AssertEquals(transaction.TablePrefix, request.XP_ParentTableCode);
			Assert(!request.XP_ReasonDescription.IsEmpty);
			AssertEquals(transaction.AH_Desc, request.XP_ReasonDescription);
			AssertEquals(121M, request.PostingDetails.MaxAmountToApprove);
			AssertEquals(TestObjectCreator.Creditor1.PK, request.PostingDetails.CreditorPK);
			AssertEquals("INV1112", request.PostingDetails.TransactionNumber);
			AssertEquals("some xml", request.PostingDetails.SourceXML);
			AssertEquals(expected: true, request.PostingDetails.IsCrossLedgerImportFromXML);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			var approvalForTest = GetNewApprovalRequest();
			AssertEquals(GenApprovalRequestSchema.Constants.XP_ApprovalType, Constants.GenApprovalRequestApprovalType.TransactionPendingAllocation, approvalForTest.XP_ApprovalType);
		}

		public override void TestJobNumber()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100);
			var request = GetNewApprovalRequest();
			request.Initialize(transaction);
			AssertEquals("INV1112", request.JobNumber);

			transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV777", TestObjectCreator.Creditor1, 100);
			request = GetNewApprovalRequest();
			request.XP_ParentID = transaction.PK;
			request.XP_ParentTableCode = transaction.TablePrefix;
			AssertEquals("Imitation of request loaded from db when Initialize is not called", "INV777", request.JobNumber);
		}

		public void TestFinalizeCancelling()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100);
			TestApprovalRequest.Initialize(transaction);
			Factory.Save();

			Assert("Precondition: " + AccTransactionHeaderSchema.Constants.AH_IsCancelled, !transaction.AH_IsCancelled);
			var prevTransactionCountValue = transaction.AH_TransactionCount;
			TestApprovalRequest.FinalizeCancelling();
			Factory.Save();
			Assert(AccTransactionHeaderSchema.Constants.AH_IsCancelled, transaction.AH_IsCancelled);
			AssertEquals("AH_TransactionCount", prevTransactionCountValue + 1, transaction.AH_TransactionCount);

			var transaction2 = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 10);
			AssertEquals(transaction.AH_TransactionNum, transaction2.AH_TransactionNum);

			byte transaction2Count = 123;
			transaction2.AH_TransactionCount = transaction2Count;
			Factory.Save();
			TestApprovalRequest.FinalizeCancelling();
			Factory.Save();
			AssertEquals(transaction2Count + 1, transaction.AH_TransactionCount);

			transaction2.AH_TransactionCount = byte.MaxValue;
			Factory.Save();
			TestApprovalRequest.FinalizeCancelling();
			Factory.Save();
			AssertEquals((byte)1, transaction.AH_TransactionCount);
			AssertEquals(@"Invoice AH_TransactionCount has reached a maximum value. As such invoice parameters won't be unique, no more invoices can be created with: 
Transaction Ledger: 'PA', Type: 'IPA', Number: 'INV1112', Organisation: 'ZCreditor1', Company: 'EDI'.

Approach with incrementing AH_TransactionCount on approval requests cancelling may require reconsideration.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestIsTransactionEligibleToCreateRejectionRequest()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100);
			TestApprovalRequest.Initialize(transaction);
			Factory.Save();

			Assert("Approval Request is eligible to reject if rejection privider is null", TestApprovalRequest.IsTransactionEligibleToCreateRejectionRequest(null));

			AccountingMasterFilesRegistry.Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var (rejectionProviderMock, _) = TestObjectCreator.MockCountryFactoryForTPAAeInvoicing(transactionEligibilityDefaultReturnValue: false);
			var countryCompliance = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory("") as IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>)?.Get();
			Assert("Approval Request is eligible to reject when rejection provider is implemented, registry is not enabled", TestApprovalRequest.IsTransactionEligibleToCreateRejectionRequest(countryCompliance));

			AccountingMasterFilesRegistry.Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			rejectionProviderMock.Setup(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>())).Returns(true);
			Assert("Approval Request is eligible to create rejection request if rejection provider result is true", TestApprovalRequest.IsTransactionEligibleToCreateRejectionRequest(countryCompliance));

			rejectionProviderMock.Setup(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>())).Returns(false);
			Assert("Transaction is not eligible to create rejection request", !TestApprovalRequest.IsTransactionEligibleToCreateRejectionRequest(countryCompliance));

			rejectionProviderMock.Verify(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>()), Times.Exactly(2));
		}

		public void TestEligibilityAndQueueIsTriggered_OnSaving()
		{
			var mockProxy = new Mock<IEInvoicingTransaction>();
			mockProxy.Setup(x => x.EvaluateEligibilityAndQueue()).Verifiable();
			var mockProxyFactory = new Mock<IEInvoicingTransactionProxyFactory>();
			mockProxyFactory.Setup(x => x.GetProxy(It.IsAny<AccTransactionHeader>())).Returns(mockProxy.Object);
			ObjectFactory.Substitute(mockProxyFactory.Object);

			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100);
			TestApprovalRequest.Initialize(transaction);
			Factory.Save();

			AssertNoExceptionThrown(() => mockProxy.Verify(x => x.EvaluateEligibilityAndQueue(), Times.Once()));

			TestApprovalRequest.XP_ApprovalStatus = "REJ";
			Factory.Save();

			AssertNoExceptionThrown(() => mockProxy.Verify(x => x.EvaluateEligibilityAndQueue(), Times.Exactly(2)));
		}

		public void TestFetchStrategy()
		{
			AssertNotNull("FetchStrategy must be an instance of TransactionPendingAllocationApprovalRequestFetchStrategy", GetNewApprovalRequest().FetchStrategy as TransactionPendingAllocationApprovalRequestFetchStrategy);
		}

		public void TestXP_ApprovalStatus_CriticalValidationInfoWhenNoCancellingBusinessContext()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV1112", TestObjectCreator.Creditor1, 100);
			TestApprovalRequest.Initialize(transaction);
			Factory.Save();

			TestApprovalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(TestApprovalRequest.PK, CriticalValidationInfoCollectorServiceKeyType.ApprovalRequestCancelledWithoutAnyBusinessContext);
			AssertEquals("Should contain the collected info", true, !collectedInfo.IsNullOrEmpty());

			ErrorReporter.Clear();
		}

		protected override ZBool ShouldAddLogsOnTransactionWithApprovalRequestStatusChange => true;
		protected override ZString Ledger => LedgerTypes.TransactionsPendingAllocation;
		protected override ZString InvoiceBasedTransactionType => TransactionTypes.InvoicePendingAllocation;
		protected override ZString CreditNoteBasedTransactionType => TransactionTypes.CreditNotePendingAllocation;
		protected override ZString RequestApprovedReference => "Approved For Allocation";

		TransactionPendingAllocationApprovalRequest GetNewApprovalRequest() => (TransactionPendingAllocationApprovalRequest)GetNewBusinessObject();

		protected override (InvoicingBase, GenApprovalRequest) CreateTransactionAndApprovalRequest(ZString transactionType, ZString transactionNumber)
		{
			var amount = ZDecimal.Zero;
			if (InvoiceBasedTransactionType == transactionType)
			{
				amount = 100M;
			}
			else if (CreditNoteBasedTransactionType == transactionType)
			{
				amount = -100M;
			}
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation(transactionNumber, TestObjectCreator.Creditor1, amount);
			var approvalRequest = GetNewApprovalRequest();
			approvalRequest.Initialize(transaction);
			Factory.Save();

			return (transaction, approvalRequest);
		}

		public override void TestPostingOptionForDisplay()
		{
			Assert("Posting option is not used here", condition: true);
		}

		protected override string GetExpectedEmailSubjectForTestSendEmail(TransactionPendingAllocationApprovalRequest request) => "";

		protected override bool ShouldSendEmailOnApproving => false;

		protected override bool ShouldSendEmailOnRejecting => false;
	}
}
