using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	internal class SealsIDWrapper : ISealsID
	{
		readonly ZString seal;

		public SealsIDWrapper(ZString seal)
		{
			this.seal = seal;
		}

		ZString ISealsID.SealsIdentity => seal;

		ZString ISealsID.SealsIdentityLNG => ZString.Empty;
	}
}
