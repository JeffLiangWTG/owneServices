using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class ExporterProvider : IExporter
	{
		ExporterProvider(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		}
		public static ExporterProvider New(JobComInvoiceHeader invoiceHeader) => invoiceHeader == null ? null : new ExporterProvider(invoiceHeader);

		readonly JobComInvoiceHeader invoiceHeader;

		public string Code => invoiceHeader.JZ_SupplierAuthorityIdentifier;

		public string Version => invoiceHeader.JZ_SupplierAuthorityVersion;

		public string CountryCode => invoiceHeader.Supplier?.CountryCode ?? string.Empty;

		public string RootCnpj => CountryCode == Core.Constants.CountryCodes.Brazil ? invoiceHeader.Supplier?.GetRootCNPJFromCNPJ() ?? string.Empty : invoiceHeader.JobDeclaration?.Importer?.GetRootCNPJFromCNPJ() ?? string.Empty;
	}
}
