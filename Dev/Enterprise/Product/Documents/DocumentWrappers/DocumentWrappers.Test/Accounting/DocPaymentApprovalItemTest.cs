using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocPaymentApprovalItem))]
	sealed class DocPaymentApprovalItemTest : DocumentWrapperTestCase
	{
		public void TestProperties()
		{
			PaymentApprovalItem approvalItem = Factory.New<PaymentApprovalItem>();
			APInvoice invoice = Factory.New<APInvoice>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "STRAWBERRY";

			approvalItem.A2_PaymentThisRun = 100M;
			invoice.AH_TransactionNum = "00001001";
			invoice.AH_OSExTaxAmount = 100M;
			invoice.AH_OH = org.PK;
			invoice.AH_RequisitionStatus = "XYZ";
			invoice.AH_DueDate = new ZDateTime(2020, 3, 4);
			invoice.AH_RequisitionDate = new ZDateTime(2020, 2, 3);
			approvalItem.A2_AH = invoice.PK;
			DocPaymentApprovalItem approvalWrapper = DocPaymentApprovalItem.New(approvalItem, Factory);

			AssertEquals("TransactionType", invoice.AH_TransactionType, approvalWrapper.TransactionType);
			AssertEquals("Invoice Date", invoice.AH_InvoiceDate, approvalWrapper.InvoiceDate);
			AssertEquals("TransactionNumber", invoice.AH_TransactionNum, approvalWrapper.TransactionNumber);
			AssertEquals("CurrencyCode", invoice.AH_RX_NKTransactionCurrency, approvalWrapper.CurrencyCode);
			AssertEquals("ExchangeRate", invoice.AH_ExchangeRate, approvalWrapper.ExchangeRate);
			AssertEquals("OSAmount", approvalItem.OSAmountPaidThisRun, approvalWrapper.OSAmount);
			AssertEquals("ExchangeRate", invoice.AH_ExchangeRate, approvalWrapper.ExchangeRate);
			AssertEquals("Desc", invoice.AH_Desc, approvalWrapper.Desc);
			AssertEquals("Ledger", invoice.AH_Ledger, approvalWrapper.Ledger);
			AssertEquals("ExchangeRate", invoice.AH_ExchangeRate, approvalWrapper.ExchangeRate);
			AssertEquals("OSAmount", approvalItem.OSAmountPaidThisRun, approvalWrapper.OSAmount);
			AssertEquals("Amount", approvalItem.A2_PaymentThisRun, approvalWrapper.Amount);
			AssertEquals("Transaction Number Prefixed", approvalItem.TransactionHeader.TransactionNumberPrefixed, approvalWrapper.TransactionNumberPrefixed);
			AssertEquals("STRAWBERRY", approvalWrapper.OrganizationCode);
			AssertEquals(new ZDateTime(2020, 2, 3), approvalWrapper.PaymentRequestedDate);
			AssertEquals(new ZDateTime(2020, 3, 4), approvalWrapper.DueDate);
			AssertEquals("XYZ", approvalWrapper.Criticality);
			AssertEquals(100m, approvalWrapper.InvertedOriginalOSAmount);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			PaymentApprovalItem approvalItem = Factory.New<PaymentApprovalItem>();
			return new DocumentWrapper[] { DocPaymentApprovalItem.New(approvalItem, Factory) };
		}
	}
}
