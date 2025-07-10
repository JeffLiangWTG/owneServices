using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IEDIClientAuth : IOAuth2Parameters
	{
		ZGuid PK { get; }
		string AuthorizationMode { get; }
	}
}
