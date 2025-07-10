namespace Enterprise.Customs.AE.Manifest.Business;

sealed class ReferenceProvider : IReferenceProvider
{
	public ReferenceProvider(string code, string identifier)
	{
		ReferenceCode = code;
		ReferenceIdentifier = identifier;
	}

	public string ReferenceCode { get; }

	public string ReferenceIdentifier { get; }
}
