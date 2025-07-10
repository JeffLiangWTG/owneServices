using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class MeansOfTransportIdentityWrapper : IMeansOfTransportIdentity
	{
		public MeansOfTransportIdentityWrapper(ZString nationality, ZString identity, ZString identityLng)
		{
			this.nationality = nationality;
			this.identity = identity;
			this.identityLng = identityLng;
		}

		readonly ZString nationality;
		readonly ZString identity;
		readonly ZString identityLng;

		public string Nationality => nationality;

		public string Identity => identity;

		public string IdentityLNG => identityLng;
	}
}
