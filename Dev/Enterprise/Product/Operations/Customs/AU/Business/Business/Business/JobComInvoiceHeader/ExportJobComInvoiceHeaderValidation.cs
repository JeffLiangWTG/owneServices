using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ExportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		bool IsAQISCertificateRequest => Parent.JobDeclaration.IsAQISCertificateRequest && Parent.JobDeclaration.IsQuarantine;
		bool IsNexdocsWithoutEDN => Parent.JobDeclaration.IsQuarantine && !Parent.QuarantineExDocHeader.QH_ObtainExportCustomsPermit && Parent.QuarantineExDocHeader.IsNEXDOCSActive;

		protected override bool MiscSupplierIsAllowed => true;

		#region CheckJZ_InvoiceAmount

		protected override void CheckJZ_InvoiceAmount()
		{
			if (!IsAQISCertificateRequest && !IsNexdocsWithoutEDN)
			{
				base.CheckJZ_InvoiceAmount();
			}
		}

		#endregion

		#region CheckJZ_RX_NKInvoice_Currency

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			if (!IsNexdocsWithoutEDN)
			{
				ZPropertyInfo currencyInfo = Parent.JZ_RX_NKInvoice_CurrencyInfo;
				RefCurrency theCurrency = Parent.Invoice_Currency;
				if (!currencyInfo.HasNotifications())
				{
					if (theCurrency == null || !EXDCurrencyChecker.IsValidCurrency(theCurrency))
					{
						currencyInfo.AddMessageError("The entered currency is invalid for EXD messages");
					}
				}

				if (Parent.JobDeclaration != null)
				{
					ZString currentCurrency = Parent.JZ_RX_NKInvoice_Currency;
					foreach (JobComInvoiceHeader invoice in Parent.JobDeclaration.Invoices)
					{
						if (!invoice.JZ_RX_NKInvoice_Currency.IsEmpty && invoice.JZ_RX_NKInvoice_Currency != currentCurrency)
						{
							Parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(MultipleCurrencyError);
							break;
						}
					}
				}
			}
		}

		public const string MultipleCurrencyError = "Multiple currencies are not allowed on an Export Declaration.";

		#endregion
	}
}
