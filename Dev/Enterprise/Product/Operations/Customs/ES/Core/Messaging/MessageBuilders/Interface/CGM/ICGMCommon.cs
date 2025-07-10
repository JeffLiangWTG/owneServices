using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICGMCommonDataProvider : IESEDIMessageCollectionProvider
{
	ZString SendEmailL { get; }
	ICGMPartyProviderWithAddressAndContactPerson PersonPresentingGoodsToCustomsForCGM { get; }
}

public interface ICGMPartyProviderWithAddressAndContactPerson : IPartyNameProvider
{
	IPartyAddressProvider Address { get; }
	IPartyContactProvider ContactPerson { get; }
}
