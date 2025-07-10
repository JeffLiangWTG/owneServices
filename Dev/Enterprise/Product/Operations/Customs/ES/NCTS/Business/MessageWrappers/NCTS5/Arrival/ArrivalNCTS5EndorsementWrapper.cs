using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5EndorsementWrapper : IArrivalNCTSEndorsement
	{
		public ArrivalNCTS5EndorsementWrapper(EnRouteIncident enRouteIncident)
		{
			this.enRouteIncident = Argument.NotNull(enRouteIncident, nameof(enRouteIncident));
		}
		protected readonly EnRouteIncident enRouteIncident;

		public ZDateTime Date => enRouteIncident.BN_EndorsementDate;

		public ZString Authority => enRouteIncident.BN_EndorsementAuthority;

		public ZString Place => enRouteIncident.BN_EndorsementPlace;

		public ZString Country => enRouteIncident.BN_EndorsementCountryCode;
	}
}
