using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(InvoiceLinkedeNettEDIMessage))]
	public class InvoiceLinkedeNettEDIMessageTest : LinkedeNettEDIMessageTest
	{
		protected override ZString ExpectedEM_Ledger { get { return "AR"; } }
		protected override ZString ExpectedEM_TransactionType { get { return "INV"; } }
		protected override ZString ExpectedEM_TransactionNumber { get { return "165"; } }
		protected override ZString ExpectedEM_JobInvoiceNo { get { return "231"; } }
		protected override ZString ExpectedEM_ChequeOrReference { get { return ZString.Empty; } }
		protected override ZString ExpectedEM_InvoiceDate { get { return "17-Mar-08 00:00:00"; } }
		protected override ZString ExpectedEM_PostDate { get { return "17-Mar-08 00:00:00"; } }
		protected override ZString ExpectedEM_LocalInvoiceAmtInclTax { get { return "232.00"; } }
		protected override ZString ExpectedEM_OSInvoiceAmtInclTax { get { return "232.00"; } }
		protected override ZString ExpectedEM_Currency { get { return "AUD"; } }
		protected override ZString ExpectedEM_OrganisationCode { get { return "AALSHI"; } }
		protected override ZString ExpectedEM_OrganisationName { get { return "PAYMENTS INCORPORATED"; } }

		protected override string GetMessageText()
		{
			return ENettWebServiceTestConstants.GoodNewInvoice;
		}
	}
}
