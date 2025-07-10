using System.IdentityModel.Tokens.Jwt;
using CargoWise.Types;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Octokit;
using WTG.IdentitySecurity;

namespace Enterprise.Client.EDI.Escrow
{
	class GitHubClientFactory : IGitHubClientFactory
	{
		public IGitHubClient Create(IGitAuthConfigurationRegistry gitAuthConfigurationRegistry)
		{
			var rsa = RSAKeyProvider.ImportPrivateKey(gitAuthConfigurationRegistry.GitHubAppPrivateKey);
			var time = ZDateTime.UtcNow.ToDateTime();
			var payload = new JwtPayload(null, null, null, time, time.AddMinutes(10), time)
			{
				{ "iss", gitAuthConfigurationRegistry.GitHubAppId },
			};
			var jwt = JwtSecurity.GenerateSignedJwt(rsa, payload);
			var appToken = new JwtSecurityTokenHandler().WriteToken(jwt);
			return new GitHubClient(new ProductHeaderValue("EDIEscrowSourceExporter"))
			{
				Credentials = new Credentials(appToken, AuthenticationType.Bearer)
			};
		}
	}
}
