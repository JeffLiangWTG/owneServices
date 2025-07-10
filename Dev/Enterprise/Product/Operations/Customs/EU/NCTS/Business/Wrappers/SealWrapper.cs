using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SealWrapper : ISealID
	{
		public SealWrapper(ZString seal)
		{
			SealIdentity = seal;
		}

		public ZString SealIdentity { get; }

		public ZString SealIdentityLanguage => ZString.Empty;
	}
}
