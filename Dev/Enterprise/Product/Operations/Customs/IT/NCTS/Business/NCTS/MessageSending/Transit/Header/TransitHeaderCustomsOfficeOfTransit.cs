using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitHeaderCustomsOfficeOfTransit : IETHeaderTransitCustomsOffice
{
	public TransitHeaderCustomsOfficeOfTransit(EU.NCTS.Business.NctsEuOfficeCode customsOffice)
	{
		this.customsOffice = Argument.NotNull(customsOffice, nameof(customsOffice));
	}
	readonly EU.NCTS.Business.NctsEuOfficeCode customsOffice;

	public ZString ReferenceNumber => customsOffice.CY_Data;

	public ZDateTime EstimatedArrivalTime => customsOffice.CY_Date;
}
