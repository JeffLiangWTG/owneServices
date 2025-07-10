using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using CusGoodsLocationQualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class LocationProvider : ILocationType
	{
		readonly EnRouteIncident incident;

		public LocationProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}

		public string QualifierOfIdentification => incident.GoodsLocation.CGL_Qualifier;

		public string UNLocode => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? (string)incident.GoodsLocation.Unlocode : null;

		public string GNSSLatitute => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? (incident.GoodsLocation.Address.E2_Latitude == 0 ? incident.GoodsLocation.Address.E2_Latitude.ToString(1) : incident.GoodsLocation.Address.E2_Latitude.ToString()) : null;
		
		public string GNSSLongitude => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? (incident.GoodsLocation.Address.E2_Longitude == 0 ? incident.GoodsLocation.Address.E2_Longitude.ToString(1) : incident.GoodsLocation.Address.E2_Longitude.ToString()) : null;

		public IAddress Address => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.Address ? address ?? (address = new AddressProvider(incident.GoodsLocation.Address)) : null;
		IAddress address;

		public string Country => incident.BN_EventCountryCode;
	}
}
