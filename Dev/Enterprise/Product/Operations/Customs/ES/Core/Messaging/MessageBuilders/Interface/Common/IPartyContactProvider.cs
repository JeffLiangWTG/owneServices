using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IPartyContactProvider
	{
		ZString Name { get; }
		ZString Email { get; }
		ZString PhoneNumber { get; }
	}
}
