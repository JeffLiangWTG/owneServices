using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IPartyProvider : IPartyNameProvider
{
	ZString Address { get; }
	ZString City { get; }
	ZString PostCode { get; }
	ZString Country { get; }
}

public interface IPartyAddressProvider
{
	ZString Address { get; }
	ZString City { get; }
	ZString PostCode { get; }
	ZString Country { get; }
}

public interface IPartyEmailProvider : IPartyNameProvider
{
	ZString EmailAddress { get; }
}

public interface IPartyIdProviderWithContactPerson : IPartyIdProvider
{
	IPartyContactProvider ContactPerson { get; }
}
