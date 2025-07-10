using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(PaymentLinkedeNettEDIMessage))]
	public class PaymentLinkedeNettEDIMessageTest : LinkedeNettEDIMessageTest
	{
		protected override ZString ExpectedEM_Ledger { get { return "AP"; } }
		protected override ZString ExpectedEM_TransactionType { get { return "PAY"; } }
		protected override ZString ExpectedEM_TransactionNumber { get { return ZString.Empty; } }
		protected override ZString ExpectedEM_JobInvoiceNo { get { return ZString.Empty; } }
		protected override ZString ExpectedEM_ChequeOrReference { get { return "12345678"; } }
		protected override ZString ExpectedEM_InvoiceDate { get { return "21-Mar-08 01:24:24"; } }
		protected override ZString ExpectedEM_PostDate { get { return "21-Mar-08 01:24:24"; } }
		protected override ZString ExpectedEM_LocalInvoiceAmtInclTax { get { return "99.49"; } }
		protected override ZString ExpectedEM_OSInvoiceAmtInclTax { get { return "232.00"; } }
		protected override ZString ExpectedEM_Currency { get { return "AUD"; } }
		protected override ZString ExpectedEM_OrganisationCode { get { return ZString.Empty; } }
		protected override ZString ExpectedEM_OrganisationName { get { return "PAYMENTS INCORPORATED"; } }

		protected override string GetMessageText()
		{
			return ENettWebServiceTestConstants.NewPayment;
		}
	}
}
