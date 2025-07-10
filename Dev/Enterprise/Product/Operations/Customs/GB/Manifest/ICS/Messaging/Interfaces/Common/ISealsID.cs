using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public interface ISealsID
	{
		ZString SealsIdentity { get; }
		ZString SealsIdentityLNG { get; }
	}
}
