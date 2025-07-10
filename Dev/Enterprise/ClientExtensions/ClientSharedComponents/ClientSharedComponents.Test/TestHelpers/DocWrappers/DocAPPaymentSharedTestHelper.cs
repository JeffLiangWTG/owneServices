using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ClientSharedComponents.DocWrappers.Testing
{
	public class DocAPPaymentSharedTestHelper
	{
		public DocAPPaymentSharedTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public void SetupInvoicesForPayment(APPayment payment, ZInt numberOfInvoices, ZDateTime invoiceDate, ZString groupName, string transactionNumPrefix = "inv")
		{
			for (int i = 0; i < numberOfInvoices; i++)
			{
				AddMatchedInvoiceToPayment(payment, transactionNumPrefix + i, i, invoiceDate, groupName);
			}
		}

		public void AddMatchedInvoiceToPayment(APPayment payment, ZString transactionNumber, ZDecimal amount, ZDateTime invoiceDate, ZString groupName)
		{
			APInvoice invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_InvoiceDate = invoiceDate;
			invoice.AH_TransactionNum = transactionNumber;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			APInvoiceLine line = factory.NewWithValidTestData<APInvoiceLine>();
			line.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;
			line.AL_LocalExTaxAmount = amount;
			invoice.Lines.Add(line);

			((IMatching)payment).CurrentMatchGroup.Add(CreateMatchLink(invoice, -amount, groupName));
			invoice.AH_OutstandingAmount = 0;
			invoice.AH_FullyPaidDate = ZDateTime.Now;

			payment.AH_InvoiceAmount += amount;
			payment.AH_OSTotalAmount += amount;
			((IMatching)payment).CurrentMatchGroup.Add(CreateMatchLink(payment, amount, groupName));
			payment.AH_FullyPaidDate = ZDateTime.Now;
		}

		public TransactionMatchLink CreateMatchLink(TransactionHeader invoice, ZDecimal amount, ZString groupNum)
		{
			TransactionMatchLink matchLink = factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = amount;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = groupNum;

			return matchLink;
		}

		readonly BusinessObjectFactory factory;
	}
}
