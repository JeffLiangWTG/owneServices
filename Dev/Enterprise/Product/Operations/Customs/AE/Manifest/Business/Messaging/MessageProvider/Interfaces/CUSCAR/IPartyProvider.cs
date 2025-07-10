using Enterprise.Customs.AE.Manifest;

namespace Enterprise.Customs.AE.Business;

public interface IPartyProvider
{
	string PartyFunctionCode { get; }

	string PartyIdentifier { get; }
}

public interface IPartyFromOrgAddressProvider : IPartyProvider
{
	string PartyName { get; }

	string StreetAddress { get; }

	string City { get; }

	string Country { get; }

	IPartyContactCommunicationProvider ContactCommunication {  get; }

	string CodeListIdentificationCode { get; }
}
