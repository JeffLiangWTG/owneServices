using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADHeaderSecurityBlockWrapper : IETHeaderSecurityBlock
{
	public NctsSADHeaderSecurityBlockWrapper(NctsHeader nctsHeader)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		nctsMovementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
	}
	readonly NctsDepartureMovementHeader nctsMovementHeader;

	public ZString SpecificCircumstanceIndicator => ZString.Empty;

	public ZString PlaceOfLoadingCode => nctsMovementHeader.PlaceOfLoading;

	public ZString ConveyanceReferenceNumber => ZString.Empty;

	public ZString PlaceOfUnloadingCode => ZString.Empty;

	public IEnumerable<ZString> TransitCountries => System.Array.Empty<ZString>();

	public ITrader Carrier => new SADEmptyTraderWrapper();

	public ZInt SealsNumber => nctsMovementHeader.BM_SealQty;

	public ZString TransportChargesMethodOfPayment => ZString.Empty;

	public ZString CommercialReferenceNumber => ZString.Empty;

	public ITrader Consignor => new SADEmptyTraderWrapper();

	public ITrader Consignee => new SADEmptyTraderWrapper();
}
