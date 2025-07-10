namespace Enterprise.Customs.AE.Manifest.Business;

sealed class LocationProvider : ILocationProvider
{
	public LocationProvider(string functionCode, string id)
	{
		LocationFunctionCode = functionCode;
		LocationIdentifier = id;
	}

	public string LocationFunctionCode { get; }

	public string LocationIdentifier { get; }
}
