using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[UseSnapshotProtection]
	class NonTransactionedARCreditNoteApprovalSubscriberTest : NUnit.Framework.TestCase
	{
		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_ZCannotSaveExceptionThatCannotRecover()
		{
			var invoice = SubscriberTest.GetInvoiceWithLine();
			var creditNote = SubscriberTest.GetAmendingCreditNote(invoice);
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = creditNote.AH_ReceiptType;
			SubscriberTest.CreateAndSaveRequest(creditNote, invoice);

			using (CriticalValidationServiceTestOnlyExtensions.TemporaryForceCriticalValidationErrorInAnyFactory_ForTestOnly(CriticalValidationErrorType.DummyErrorKeyForTest))
			{
				SubscriberTest.ProcessLogs();
				ErrorReporter.Clear();
			}

			var newFactory = new BusinessObjectFactory();
			var transaction = SubscriberTest.GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			SubscriberTest.Request = newFactory.Load<ARCreditNoteApprovalRequest>(SubscriberTest.Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, SubscriberTest.Request.XP_ApprovalStatus);
			Assert("Logs match", SubscriberTest.NotifiedEventListForTest.Contains(@"[ARCreditNoteApprovalSubscriber] failed to process logs. Affected records will be processed again one-by-one.
Forced Critical Validation Error - For Test Only."));
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_AnyExceptionOtherThanHandled()
		{
			var invoice = SubscriberTest.GetInvoiceWithLine();
			var creditNote = SubscriberTest.GetAmendingCreditNote(invoice);
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = creditNote.AH_ReceiptType;
			SubscriberTest.CreateAndSaveRequest(creditNote, invoice);

			var exception = new InvalidOperationException("InvalidOperationException message.");

			using (CriticalValidationServiceTestOnlyExtensions.TemporaryForceExceptionInAnyFactory_ForTestOnly(exception))
			{
				SubscriberTest.ProcessLogs();
				ErrorReporter.Clear();
			}

			var newFactory = new BusinessObjectFactory();
			var transaction = SubscriberTest.GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			SubscriberTest.Request = newFactory.Load<ARCreditNoteApprovalRequest>(SubscriberTest.Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, SubscriberTest.Request.XP_ApprovalStatus);
			Assert("Logs match", SubscriberTest.NotifiedEventListForTest.Contains(string.Format("[ARCreditNoteApprovalSubscriber] failed to process logs. Affected records will be processed again one-by-one.\r\n{0}", exception.Message)));
		}

		[SuspendCriticalValidation]
		public void TestAmendingWithCreditNote_ZSaveException()
		{
			var invoice = SubscriberTest.GetInvoiceWithLine();
			var creditNote = SubscriberTest.GetAmendingCreditNote(invoice);
			var invoiceDate = creditNote.AH_InvoiceDate;
			var postDate = creditNote.AH_PostDate;
			var amendingReasonCode = creditNote.AH_ReceiptType;
			SubscriberTest.CreateAndSaveRequest(creditNote, invoice);

			var factory = new BusinessObjectFactory();
			var dummyBizObj = factory.New<DummyBusinessObject>();
			var row = ((INeedRow)dummyBizObj).Row;
			var connection = ((IDbConnected)factory).Connection;
			var sqlException = SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired.");
			var zDataException = new ZDataException(sqlException, row, connection);
			var exception = new ZSaveException(zDataException, factory);

			using (CriticalValidationServiceTestOnlyExtensions.TemporaryForceExceptionInAnyFactory_ForTestOnly(exception))
			{
				SubscriberTest.ProcessLogs();
			}

			var newFactory = new BusinessObjectFactory();
			var transaction = SubscriberTest.GetTransactionNotMatchingInvoicePK(newFactory, invoice);

			AssertNull(transaction);
			SubscriberTest.Request = newFactory.Load<ARCreditNoteApprovalRequest>(SubscriberTest.Request.PK);
			AssertEquals("Approved", Constants.GenApprovalRequestApprovalStatus.Approved, SubscriberTest.Request.XP_ApprovalStatus);
			var exceptionMessage = $@"[ARCreditNoteApprovalSubscriber] failed to process logs. Affected records will be processed again one-by-one.

** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: DummyBizo
PK: {dummyBizObj.PK}
RowState: Added
Factory validation suspended: False
Factory name for debugging: 
Business object around row = CargoWise.EntityFramework.Testing.DummyBusinessObject
Business object validation suspended: 0
Business object is marking as needing validation suspended: False
Business object light validation is enabled: True
Business object additional info: 

Inner Message = Execution timeout expired.

";
			Assert("Logs match", SubscriberTest.NotifiedEventListForTest.Contains(string.Format(exceptionMessage, SubscriberTest.Request.PK)));
		}

		ARCreditNoteApprovalSubscriberTest SubscriberTest
		{
			get
			{
				if (subscriberTest == null)
				{
					subscriberTest = new ARCreditNoteApprovalSubscriberTest();
				}
				return subscriberTest;
			}
		}
		ARCreditNoteApprovalSubscriberTest subscriberTest;

		protected override void SetUp()
		{
			SubscriberTest.SetupForTest();
		}
	}
}
