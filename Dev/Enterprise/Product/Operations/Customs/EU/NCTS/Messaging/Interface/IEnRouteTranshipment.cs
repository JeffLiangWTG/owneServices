using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IEnRouteTranshipment
	{
		ZString NewTransportID { get; }

		ZString NewTransportIDLanguage { get; }

		ZString NewTransportCountry { get; }

		ZString EndorsementDate { get; }

		ZString EndorsementAuthority { get; }

		ZString EndorsementAuthorityLanguage { get; }

		ZString EndorsementPlace { get; }

		ZString EndorsementPlaceLanguage { get; }

		ZString EndorsementCountry { get; }

		IReadOnlyCollection<ZString> ContainerNumbers { get; }
	}
}
