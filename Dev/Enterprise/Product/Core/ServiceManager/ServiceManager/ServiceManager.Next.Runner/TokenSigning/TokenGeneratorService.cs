using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using CargoWise.SystemToSystemTrust.Extensions;
using Enterprise.Registry.Business;
using WTG.OpenIDConnect.Token;

namespace CargoWise.ServiceManager.Next.Runner.TokenSigning;

public class TokenGeneratorService : ITokenGeneratorService
{
	readonly ILogger logger;
	readonly IHttpClientFactory httpClientFactory;

	public TokenGeneratorService(ILogger<TokenGeneratorService> logger, IHttpClientFactory httpClientFactory)
	{
		this.logger = logger;
		this.httpClientFactory = httpClientFactory;
	}

	public async Task<SignCwTokenResponse> SignCwTokenAsync(SignCwTokenRequest request, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var systemInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;

		var url = systemInfo.GetTokenEndpoint();
		using var privateKey = systemInfo.ReadPrivateKey();
		using var cert = systemInfo.ReadCertificate();
		var principal = systemInfo.ClientId;
		var audience = request.Audience;

		logger.LogInformation("Signing CW token for audience {Audience} and principal {Principal} using authority {AuthorityUrl}", audience, principal, url);
		var accessToken = await OAuthClientAssertion.GetClientAccessTokenAsync(url, privateKey, cert, principal, audience, () => httpClientFactory.CreateClient(nameof(TokenGeneratorService))).ConfigureAwait(false);

		return new SignCwTokenResponse(accessToken ?? throw new InvalidOperationException($"Unable to generate token for audience {audience}"));
	}
}
