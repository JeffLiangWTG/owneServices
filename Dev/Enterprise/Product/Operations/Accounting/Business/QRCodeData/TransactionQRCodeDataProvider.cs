using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.QRCodeData
{
	public class TransactionQRCodeDataProvider : ITransactionQRCodeDataProvider
	{
		readonly InvoicingBase invoice;

		public TransactionQRCodeDataProvider(InvoicingBase invoice)
		{
			this.invoice = invoice;
		}

		ZDateTime ITransactionQRCodeDataProvider.InvoiceDate => invoice.AH_InvoiceDate;
		ZString ITransactionQRCodeDataProvider.ComplianceSubType => invoice.AH_ComplianceSubType;
		ZString ITransactionQRCodeDataProvider.TransactionType => invoice.AH_TransactionType;
		ZString ITransactionQRCodeDataProvider.TransactionReference => invoice.AH_TransactionReference;
		ZDecimal ITransactionQRCodeDataProvider.ExchangeRate => invoice.AH_ExchangeRate;
		ZString ITransactionQRCodeDataProvider.TransactionCurrency => invoice.AH_RX_NKTransactionCurrency;
		ZDecimal ITransactionQRCodeDataProvider.OSInvoiceTotal => invoice.AH_OSTotal;
		ZDecimal ITransactionQRCodeDataProvider.OSTaxTotal => invoice.AH_OSTaxAmount;
		ZDecimal ITransactionQRCodeDataProvider.LocalInvoiceTotal => invoice.AH_LocalTotal;
		ZDecimal ITransactionQRCodeDataProvider.LocalTaxTotal => invoice.AH_LocalTaxAmount;
		ZDecimal ITransactionQRCodeDataProvider.GSTTotal => invoice.AH_GSTAmount;
		ZString ITransactionQRCodeDataProvider.EInvoicingGovernmentAllocatedNumber => invoice.EInvoicingGovernmentAllocatedNumber;
		OrgHeader ITransactionQRCodeDataProvider.OrgHeader => invoice.Header;
		GlbCompany ITransactionQRCodeDataProvider.Company => invoice.Company;
		GlbBranch ITransactionQRCodeDataProvider.Branch => invoice.Branch;
		ITransactionHeaderWithLines ITransactionQRCodeDataProvider.GetTransactionDataForPortugalOnly_Obsolete_MustBeRefactoredToUseMembersOfThisInterface() => invoice;
	}
}
