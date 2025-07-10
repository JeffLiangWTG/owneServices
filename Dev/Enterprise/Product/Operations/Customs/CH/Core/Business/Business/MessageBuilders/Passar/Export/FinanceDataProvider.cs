using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class FinanceDataProvider : IFinanceData
{
	public static FinanceDataProvider New(JobComInvoiceHeader invoiceHeader) => invoiceHeader == null ? null : new FinanceDataProvider(invoiceHeader);

	FinanceDataProvider(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
	}
	readonly JobComInvoiceHeader invoiceHeader;

	public string Incoterms => GetIncoterms();

	public string VatNumber => (vatNumber ?? (vatNumber = GetVATNumber())).ReturnDefaultValueIfNullOrEmpty();
	ZString? vatNumber;

	public string CustomsInvoiceRecipient => (customsInvoiceRecipient ?? (customsInvoiceRecipient = GetBIDNumber())).ReturnDefaultValueIfNullOrEmpty();

	public string CustomsInvoiceReferenceNumber => null;

	public bool ImmediatePaymentRequest => false;

	public string VatInvoiceRecipient => null;

	public string VatInvoiceReferenceNumber => null;

	ZString? customsInvoiceRecipient;

	string GetIncoterms()
	{
		switch (invoiceHeader.JZ_IncoTerm)
		{
			case Core.Constants.IncoTerms.FreeCarrierSeller:
			case Core.Constants.IncoTerms.FreeCarrierBuyer:
				return Core.Constants.IncoTerms.FreeCarrier;
			default:
				return invoiceHeader.JZ_IncoTerm;
		}
	}

	ZString GetVATNumber() => invoiceHeader.JobDeclaration.Supplier?.GetCHCustomsRegNo(OrgCusCode.CodeTypes.VATCode) ?? ZString.Empty;

	ZString GetBIDNumber() => invoiceHeader.JobDeclaration.Supplier?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID) ?? ZString.Empty;
}
