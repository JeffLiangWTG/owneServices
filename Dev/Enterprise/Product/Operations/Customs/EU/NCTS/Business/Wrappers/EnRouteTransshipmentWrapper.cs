using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteTransshipmentWrapper : IEnRouteTranshipment
	{
		public EnRouteTransshipmentWrapper(EnRouteTransshipment enRouteTransshipment)
		{
			this.enRouteTransshipment = Argument.NotNull(enRouteTransshipment, nameof(enRouteTransshipment));
		}
		readonly EnRouteTransshipment enRouteTransshipment;

		public ZString EndorsementDate => enRouteTransshipment.BN_EndorsementDate.GetLongDate();

		public ZString EndorsementAuthority => enRouteTransshipment.BN_EndorsementAuthority;

		public ZString EndorsementAuthorityLanguage => ZString.Empty;

		public ZString EndorsementPlace => enRouteTransshipment.BN_EndorsementPlace;

		public ZString EndorsementPlaceLanguage => ZString.Empty;

		public ZString EndorsementCountry => enRouteTransshipment.BN_EndorsementCountryCode;

		public ZString NewTransportID => enRouteTransshipment.BN_TransportID;

		public ZString NewTransportIDLanguage => ZString.Empty;

		public ZString NewTransportCountry => enRouteTransshipment.BN_TransportCountryCode;

		public IReadOnlyCollection<ZString> ContainerNumbers => containerNumbers ?? (containerNumbers = enRouteTransshipment.ContainersNumbers.Distinct().ToArray());
		IReadOnlyCollection<ZString> containerNumbers;
	}
}
