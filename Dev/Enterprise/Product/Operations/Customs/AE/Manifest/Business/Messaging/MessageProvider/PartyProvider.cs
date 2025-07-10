using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class PartyProvider : IPartyProvider
{
	public PartyProvider(string functionCode, string identifier)
	{
		PartyFunctionCode = functionCode;
		PartyIdentifier = identifier;
	}

	public string PartyFunctionCode { get; }

	public string PartyIdentifier { get; }
}
