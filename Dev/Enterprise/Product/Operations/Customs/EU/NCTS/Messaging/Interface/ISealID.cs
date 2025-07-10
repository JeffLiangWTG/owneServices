using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ISealID
	{
		ZString SealIdentity { get; }
		ZString SealIdentityLanguage { get; }
	}
}
