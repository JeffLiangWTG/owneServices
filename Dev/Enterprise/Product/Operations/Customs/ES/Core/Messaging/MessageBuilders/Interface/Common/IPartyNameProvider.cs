using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IPartyNameProvider : IPartyIdProvider
	{
		ZString Name { get; }
	}
}
