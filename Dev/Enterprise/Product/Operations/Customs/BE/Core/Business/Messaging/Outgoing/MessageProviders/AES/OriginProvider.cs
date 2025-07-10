using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class OriginProvider : IOrigin
{
	readonly JobComInvoiceLine invoiceLine;
	public OriginProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	public string CountryOfOrigin => invoiceLine.JI_CountryOfOrigin;

	public string RegionOfDispatch => invoiceLine.JI_StateOrRegionOfOrigin;

	public string CountryOfPreferentialOrigin => invoiceLine.ZG_CountryOfSupply;
}
