using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth
{
	public interface ITaxInvoiceDocumentTypeAdditionalInfo
	{
		ZString IssueID { get; }

		ZString OriginalIssueID { get; }

		ZString AmendStatusCode { get; }

		ZString IssueDateTime { get; }

		ZString InvoiceeAlienRegistrationNo { get; set; }

		ZString InvoiceePassportNo { get; set; }

		ZString FullTypeCode { get; }

		ZString PaymentStatus { get; }
	}
}
