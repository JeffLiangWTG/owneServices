using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICommonRepresentative : IPartyIdProvider
{
	ZString Status { get; }
}

public interface ICommonRepresentativeWithContactPerson : ICommonRepresentative
{
	IPartyContactProvider ContactPerson { get; }
}
