using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class OriginProvider : IOrigin
	{
		public OriginProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		public string OriginCountry => invoiceLine.JI_CountryOfOrigin;
	}
}
