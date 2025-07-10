using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Enterprise.Client.EDI.OAuth2;
using Moq;
using NUnit.Framework;
using WTG.IdentitySecurity;

namespace ZClientEDI.Test.OAuth2.Testing
{
	class AccessTokenProviderTest : TestCase
	{
		public void TestGetAccessToken()
		{
			// Arrange
			var mockDiscoveryDocument = new Mock<IDiscoveryDocument>();
			mockDiscoveryDocument
				.Setup(x => x.GetAccessToken(It.IsAny<JwtPayload>()))
				.Returns(() =>
				{
					var payload = new JwtPayload
					{
						{ "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
						{ "exp", DateTimeOffset.UtcNow.AddMilliseconds(5000).ToUnixTimeSeconds() },
						{ "iss", "issuer" },
					};

					var privateKey = RSAKeyProvider.ImportPrivateKey(AccessTokenKeys.PrivateKey);
					var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(AccessTokenKeys.Certificate));
					var accessToken = JwtSecurity.GenerateSignedJwt(privateKey, certificate, payload);
					return accessToken;
				});

			var tokenProvider = new AccessTokenProvider(mockDiscoveryDocument.Object);

			// Act
			var token1 = tokenProvider.GetAccessToken();
			var token2 = tokenProvider.GetAccessToken();

			// Assert
			AssertEquals("Should use the cached token if it is not expired",token1, token2);

			Thread.Sleep(5000);
			var token3 = tokenProvider.GetAccessToken();
			AssertNotEquals("Should generate a new one",token1, token3);
			mockDiscoveryDocument.Verify(x => x.GetAccessToken(It.IsAny<JwtPayload>()), Times.Exactly(2));
		}
	}
}
