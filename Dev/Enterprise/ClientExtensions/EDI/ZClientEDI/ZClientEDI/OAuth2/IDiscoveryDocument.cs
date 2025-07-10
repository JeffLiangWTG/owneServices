using System.IdentityModel.Tokens.Jwt;

namespace Enterprise.Client.EDI.OAuth2
{
	public interface IDiscoveryDocument
	{
		string GetAccessToken(JwtPayload customizedPayload);
	}
}
