using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	internal class DocAPPaymentTestHelper
	{
		public DocAPPaymentTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void SetupInvoicesForPayment(APPayment payment, ZInt numberOfInvoices, ZDateTime invoiceDate, ZString groupName, string transactionNumPrefix = "inv")
		{
			for (int i = 0; i < numberOfInvoices; i++)
			{
				AddMatchedInvoiceToPayment(payment, transactionNumPrefix + i, i, invoiceDate, groupName);
			}

			CreateMatchLink(payment, payment.AH_InvoiceAmount, groupName);
			if (payment.AH_OutstandingAmount == 0m)
			{
				payment.AH_FullyPaidDate = ZDateTime.Now;
			}
		}

		public void AddMatchedInvoiceToPayment(APPayment payment, ZString transactionNumber, ZDecimal amount, ZDateTime invoiceDate, ZString groupName)
		{
			APInvoice invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_InvoiceDate = invoiceDate;
			invoice.AH_TransactionNum = transactionNumber;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			APInvoiceLine line = factory.NewWithValidTestData<APInvoiceLine>();
			line.AL_LocalExTaxAmount = amount;
			line.AL_AG = new TestObjectCreator(factory).GLHeader1.PK;
			invoice.Lines.Add(line);

			CreateMatchLink(((IMatching)payment).CurrentMatchGroup, invoice, -amount, groupName);
			invoice.AH_OutstandingAmount = 0m;

			payment.AH_InvoiceAmount += amount;
			payment.AH_OSTotalAmount += amount;
		}

		public void CreateMatchLink(TransactionHeader invoice, ZDecimal amount, ZString groupNum)
		{
			CreateMatchLink(((IMatching)invoice).CurrentMatchGroup, invoice, amount, groupNum);
		}

		public void CreateMatchLink(TransactionMatchLinkGroup group, TransactionHeader header, ZDecimal amount, ZString groupNum)
		{
			AccTransactionMatchLink matchLink = group.AddNew();
			matchLink.AP_AH = header.PK;
			matchLink.AP_Amount = amount;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = groupNum;
		}

		readonly BusinessObjectFactory factory;
	}
}
