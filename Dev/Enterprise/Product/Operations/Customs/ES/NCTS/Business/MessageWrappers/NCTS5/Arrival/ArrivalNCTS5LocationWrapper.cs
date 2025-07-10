using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5LocationWrapper : IArrivalNCTSLocation
	{
		public ArrivalNCTS5LocationWrapper(EnRouteIncident enRouteIncident)
		{
			this.enRouteIncident = Argument.NotNull(enRouteIncident, nameof(enRouteIncident));
		}
		readonly EnRouteIncident enRouteIncident;

		public ZString Qualifier => enRouteIncident.GoodsLocation?.CGL_Qualifier ?? ZString.Empty;

		public ZString UNLocode => (enRouteIncident.GoodsLocation?.CGL_Qualifier ?? ZString.Empty) == CusGoodsLocationQualifierList.Codes.UnLocode ? enRouteIncident.GoodsLocation.Unlocode : ZString.Empty;

		public ZString Country => enRouteIncident.BN_EventCountryCode;

		public IArrivalNCTSGNSS GNSS => gnss ?? (gnss = enRouteIncident.GoodsLocation != null && enRouteIncident.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates
														? new ArrivalNCTS5GNSSWrapper(enRouteIncident.GoodsLocation.Address.E2_Latitude, enRouteIncident.GoodsLocation.Address.E2_Longitude) : null);
		IArrivalNCTSGNSS gnss;

		public INCTSCommonAddress Address => address ?? (address = enRouteIncident.GoodsLocation != null && enRouteIncident.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address
														? NCTS5CommonAddressWrapper.New(enRouteIncident.GoodsLocation.Address.E2_Address1AndE2_Address2, enRouteIncident.GoodsLocation.Address.City, enRouteIncident.GoodsLocation.Address.Postcode, enRouteIncident.Header?.IsInPhase5TransitionPeriod ?? false) : null);
		INCTSCommonAddress address;
	}
}
