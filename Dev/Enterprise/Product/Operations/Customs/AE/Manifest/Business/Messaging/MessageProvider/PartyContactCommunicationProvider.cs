namespace Enterprise.Customs.AE.Manifest.Business;

sealed class PartyContactCommunicationProvider : IPartyContactCommunicationProvider
{
	public PartyContactCommunicationProvider(string code, string identifier)
	{
		CommunicationCode = code;
		CommunicationIdentifier = identifier;
	}

	public string CommunicationCode { get; }

	public string CommunicationIdentifier { get; }
}
