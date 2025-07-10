namespace Enterprise.Customs.AE.Manifest.Business;

public interface ILocationProvider
{
	string LocationFunctionCode { get; }

	string LocationIdentifier { get; }
}
