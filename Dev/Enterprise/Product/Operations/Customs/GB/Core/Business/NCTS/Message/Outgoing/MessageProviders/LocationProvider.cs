using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class LocationProvider : ILocationType
	{
		public LocationProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}

		public string QualifierOfIdentification => incident.GoodsLocation.CGL_Qualifier;

		public string UNLocode => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? incident.GoodsLocation.Unlocode.ToString() : null;

		public string GNSSLatitude => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? incident.GoodsLocation.Address.E2_Latitude.ToString() : null;

		public string GNSSLongitude => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? incident.GoodsLocation.Address.E2_Longitude.ToString() : null;

		public CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.IAddress Address => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.Address ? address ?? (address = new AddressProvider(incident.GoodsLocation.Address, incident.Header?.IsInPhase5TransitionPeriod ?? false)) : null;
		CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.IAddress address;

		public string Country => incident.BN_EventCountryCode;

		readonly EnRouteIncident incident;
	}
}
