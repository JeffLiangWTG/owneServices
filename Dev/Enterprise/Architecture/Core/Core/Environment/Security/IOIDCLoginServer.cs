using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using WTG.OpenIDConnect.Login;

namespace Enterprise.ZArchitecture.Core
{
	public delegate OIDCLogin OIDCLoginFactory(OIDCWebLauncher webLauncher);

	public interface IOIDCLoginServer
	{
		bool IsSupported { get; }
		OIDCLoginResponseMessage LoginRemote(OIDCLoginRequestMessage loginRequest, CancellationToken cancellationToken);
		OIDCLoginResponseMessage LoginLocal(OIDCLoginRequestMessage loginRequest, OIDCWebLauncher webLauncher, CancellationToken cancellationToken, OIDCLoginFactory oidcLoginFactory = null);
		JwtSecurityToken ValidateIdentityToken(OIDCLoginRequestMessage request, OIDCLoginResponseMessage response, CancellationToken cancellationToken);
	}
}
