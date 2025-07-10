using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADHeaderSafetyAndSecurityBlockWrapper : IETHeaderSecurityBlock
{
	public NctsSADHeaderSafetyAndSecurityBlockWrapper(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		nctsMovementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
	}
	readonly NctsHeader nctsHeader;
	readonly NctsDepartureMovementHeader nctsMovementHeader;

	public ZString SpecificCircumstanceIndicator => nctsMovementHeader.BM_BTAIndicator;

	public ZString PlaceOfLoadingCode => nctsMovementHeader.PlaceOfLoading;

	public ZString ConveyanceReferenceNumber => nctsMovementHeader.BM_ConveyanceNumber;

	public ZString PlaceOfUnloadingCode => nctsMovementHeader.BM_PlaceOfUnloading;

	public IEnumerable<ZString> TransitCountries => nctsHeader.Itinerary.Cast<EU.NCTS.Business.NonPersistentItineraryCountry>().Select(x => x.CountryCode);

	public ITrader Carrier => GetTraderOrEmptyIfNull(nctsHeader.CarrierOrgAddress);

	public ZInt SealsNumber => nctsMovementHeader.BM_SealQty;

	public ZString TransportChargesMethodOfPayment => nctsMovementHeader.BM_MethodOfPayment;

	public ZString CommercialReferenceNumber => nctsMovementHeader.BM_AdditionalText;

	public ITrader Consignor => GetTraderOrEmptyIfNull(nctsHeader.SecurityConsignor?.Address);

	public ITrader Consignee => GetTraderOrEmptyIfNull(nctsHeader.SecurityConsignee?.Address);

	#region Implementation

	ITrader GetTraderOrEmptyIfNull(OrgAddress orgAddress) => orgAddress != null ? new SADTraderWrapper(orgAddress) : new SADEmptyTraderWrapper();

	#endregion
}
