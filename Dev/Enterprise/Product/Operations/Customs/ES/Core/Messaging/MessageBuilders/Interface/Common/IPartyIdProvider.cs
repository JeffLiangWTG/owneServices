using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IPartyIdProvider
	{
		ZString Id { get; }
	}
}

