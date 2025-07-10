using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class ManufacturerProvider : IManufacturer
	{
		ManufacturerProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		public static ManufacturerProvider New(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new ManufacturerProvider(invoiceLine);

		readonly JobComInvoiceLine invoiceLine;

		public string Code => invoiceLine.JI_ManufacturerAuthorityIdentifier;

		public string Version => invoiceLine.JI_ManufacturerAuthorityVersion;

		public string CountryCode => invoiceLine.JI_CountryOfOrigin;

		public string RootCnpj
		{
			get
			{
				var result = string.Empty;

				if (invoiceLine.JI_ManufacturerIndicator != ManufacturerIndicatorList.Codes._3)
				{
					result = CountryCode == Core.Constants.CountryCodes.Brazil
						? invoiceLine.ManufacturerAddress?.Header?.GetRootCNPJFromCNPJ() ?? string.Empty
						: invoiceLine.GoodsCatalog?.Owner?.GetRootCNPJFromCNPJ() ?? string.Empty;
				}
				return result;
			}
		}
	}
}
