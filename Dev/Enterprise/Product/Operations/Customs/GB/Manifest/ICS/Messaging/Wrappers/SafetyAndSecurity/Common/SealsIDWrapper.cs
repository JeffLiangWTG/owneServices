using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class SealsIDWrapper : ISealsID
	{
		public SealsIDWrapper(ZString seal)
		{
			this.seal = seal;
		}
		readonly ZString seal;

		public string SealsIdentity => seal;

		public string SealsIdentityLNG => ZString.Empty;
	}
}
