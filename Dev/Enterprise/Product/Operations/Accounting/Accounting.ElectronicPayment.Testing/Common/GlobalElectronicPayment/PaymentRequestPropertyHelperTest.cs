using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ARAP.EPayment
{
	public class PaymentRequestPropertyHelperTest : TestCaseWithFactory
	{
		public void TestCreatePaymentReference_FreeText()
		{
			AssertCreatePaymentReferenceCore(EPaymentReferenceTypes.FreeText, "Payment Reference For Test", "Payment Reference For Test");
		}

		public void TestCreatePaymentReference_InvoiceNumbers()
		{
			AssertCreatePaymentReferenceCore(EPaymentReferenceTypes.InvoiceNumbers, null, "I001 I002");
		}

		public void TestCreatePaymentReference_InvoiceNumbers_MaxLength()
		{
			var chequeBook = TestObjectCreator.GetAutoPrintChequeBook(Factory, 1, 200, 1);
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook.BankAccount, chequeBook);
			paymentApproval.AV_ChequeOrReference = "Payment Reference For Test";
			paymentApproval.AV_Amount = 500M;

			var invoices = new InvoicingBaseCollection(Factory);
			for (int i = 1; i <= 5; i++)
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "I000000000000000000000000000" + i, TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				Factory.Save();
				invoices.Add(invoice);
			}

			paymentApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());

			var accountDetailCollection = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetailCollection.RemoveAndDeleteAll();

			var accountDetails = accountDetailCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_RX_NKAccountCurrency = paymentApproval.CurrencyCode;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_EPaymentReferenceType = EPaymentReferenceTypes.InvoiceNumbers;
			Factory.Save();

			AssertEquals("I0000000000000000000000000001 I0000000000000000000000000002 I0000000000000000000000000003 I0000000000000000000000000004 I0000000000000000...", PaymentRequestPropertyHelper.CreatePaymentReference(paymentApproval, accountDetails));
		}

		public void TestCreatePaymentReference_PaymentReferenceNum()
		{
			AssertCreatePaymentReferenceCore(EPaymentReferenceTypes.PaymentReferenceNum, null, "Payment Reference For Test");
		}

		public void TestCreatePaymentReference_NullPaymentApproval()
		{
			var chequeBook = TestObjectCreator.GetAutoPrintChequeBook(Factory, 1, 200, 1);
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook.BankAccount, chequeBook);
			Factory.Save();

			AssertEquals(null, PaymentRequestPropertyHelper.CreatePaymentReference(paymentApproval, null));
		}

		public void TestCreatePaymentReference_NullAPAccountDetails()
		{
			var chequeBook = TestObjectCreator.GetAutoPrintChequeBook(Factory, 1, 200, 1);
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook.BankAccount, chequeBook);
			Factory.Save();

			var accountDetailCollection = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetailCollection.RemoveAndDeleteAll();

			var accountDetails = accountDetailCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_RX_NKAccountCurrency = paymentApproval.CurrencyCode;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_EPaymentReferenceType = EPaymentReferenceTypes.FreeText;
			accountDetails.A1_EPaymentReference = "Test";
			Factory.Save();

			AssertEquals(null, PaymentRequestPropertyHelper.CreatePaymentReference(null, accountDetails));
		}

		public void TestCreatePaymentReference_InvalidPaymentReferenceType()
		{
			var chequeBook = TestObjectCreator.GetAutoPrintChequeBook(Factory, 1, 200, 1);
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook.BankAccount, chequeBook);
			Factory.Save();

			var accountDetailCollection = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetailCollection.RemoveAndDeleteAll();

			var accountDetails = accountDetailCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_RX_NKAccountCurrency = paymentApproval.CurrencyCode;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_EPaymentReferenceType = "XXX";

			AssertExceptionThrown<InvalidOperationException>("Invalid", "Invalid E-Payment Reference Type", () => PaymentRequestPropertyHelper.CreatePaymentReference(paymentApproval, accountDetails));
		}

		void AssertCreatePaymentReferenceCore(string paymentReferenceType, string paymentReference, string expectedResult)
		{
			var apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "I001", TestObjectCreator.USD, 1m, 200m, 0m, 200m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "I002", TestObjectCreator.USD, 1m, 100m, 0m, 100m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			var chequeBook = TestObjectCreator.GetAutoPrintChequeBook(Factory, 1, 200, 1);
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook.BankAccount, chequeBook);
			paymentApproval.AV_ChequeOrReference = "Payment Reference For Test";
			paymentApproval.AV_Amount = 300M;
			var invoices = new InvoicingBaseCollection(Factory);
			invoices.AddRange(new[] { apInvoice1, apInvoice2 });
			paymentApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());
			Factory.Save();

			var accountDetailCollection = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetailCollection.RemoveAndDeleteAll();

			var accountDetails = accountDetailCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_RX_NKAccountCurrency = paymentApproval.CurrencyCode;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_EPaymentReferenceType = paymentReferenceType;
			accountDetails.A1_EPaymentReference = paymentReference;
			Factory.Save();

			AssertEquals(expectedResult, PaymentRequestPropertyHelper.CreatePaymentReference(paymentApproval, accountDetails));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
