using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalFormDataTransshipmentWrapper : IArrivalFormData
	{
		public ArrivalFormDataTransshipmentWrapper(EnRouteTransshipment transshipment)
		{
			this.transshipment = Argument.NotNull(transshipment, nameof(transshipment));
		}
		readonly EnRouteTransshipment transshipment;

		public ZDateTime FormDate => transshipment.BN_EndorsementDate;

		public ZString FormAuthority => transshipment.BN_EndorsementAuthority;

		public ZString FormAuthorityLanguage => ZString.Empty;

		public ZString FormLocation => transshipment.BN_EndorsementPlace;

		public ZString FormLocationLanguage => ZString.Empty;

		public ZString FormCountry => transshipment.BN_EndorsementCountryCode;

		public ZString FormText => transshipment.BN_TransportID;

		public ZString FormTextLanguage => ZString.Empty;
	}
}
