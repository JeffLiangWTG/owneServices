using System;
using Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class DigitalSignatureSigningFailureEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get { return typeof(DigitalSignatureSigningFailureEmail); }
		}

		public void TestEmailWithInvoice()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var invoice = creator.CreateARInvoice<ARInvoice>("1", creator.AUD, 1m, creator.ABIGAS);
			invoice.AH_TransactionNum = "00001001";
			invoice.AH_ComplianceSubType = "TXI";

			DigitalSignatureSigningFailureEmail mail = new DigitalSignatureSigningFailureEmail(invoice, InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice);

			string expectedSubject = string.Format("Failed to sign transaction AR INV 00001001 with a valid digital Signature");
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));

			string expectedBody = string.Format(@"When post transaction, CW1 failed to sign transaction AR INV 00001001 with a valid digital Signature due to the following reason:
{0}",
			InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice);

			AssertMultilineASCIIEquals("Email Body", expectedBody, GetBody(mail));
		}

		public void TestEmailWithoutInvoice()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			DigitalSignatureSigningFailureEmail mail = new DigitalSignatureSigningFailureEmail(InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice);

			string expectedSubject = string.Format("Failed to sign transactions with valid digital Signatures");
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));

			string expectedBody = string.Format(@"When post transactions, CW1 failed to sign transactions with valid digital Signatures due to the following reason:
{0}",
			InvoiceBatchComplianceSequenceNumberAllocator.WarningSignatureFaildDueToEmptySignatureInPreviousInvoice);

			AssertMultilineASCIIEquals("Email Body", expectedBody, GetBody(mail));
		}
	}
}
