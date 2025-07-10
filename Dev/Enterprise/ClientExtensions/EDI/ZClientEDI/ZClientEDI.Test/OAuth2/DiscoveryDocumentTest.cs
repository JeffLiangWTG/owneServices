using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.OAuth2;
using Enterprise.Registry.Business;
using Moq;
using Moq.Protected;

namespace ZClientEDI.Test.OAuth2.Testing
{
	class DiscoveryDocumentTest : TestCaseWithFactory
	{
		public void TestGetAccessToken()
		{
			// Arrange
			var clientId = Guid.NewGuid().ToString();
			var discoveryContent = $"{{\"issuer\":\"https://identity.wisetechglobal.com/{clientId}\",\"jwks_uri\":\"https://identity.wisetechglobal.com/{clientId}/openid/v1/jwks\"}}";

			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(discoveryContent)
				}));

			var discoveryEndpoint = "https://identity.wisetechglobal.com/.well-known/openid-configuration";
			var httpClient = new HttpClient(mockHandler.Object);
			var discoveryDocument = new DiscoveryDocument(httpClient, discoveryEndpoint);

#pragma warning disable CS0618
			var trustInfo = new SystemToSystemTrustInfo
			{
				ClientId = clientId,
				Certificate = Encoding.UTF8.GetBytes(AccessTokenKeys.Certificate),
				PrivateKey = AccessTokenKeys.PrivateKey
			};
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, trustInfo))
			{
				var payload = new JwtPayload
				{
					{ "sub", "1234567890" },
					{ "name", "John Doe" },
					{ "admin", true },
					{ "aud", clientId }
				};

				// Act
				var accessToken = discoveryDocument.GetAccessToken(payload);

				// Assert
				AssertNotNull(accessToken);
				var tokenHandler = new JwtSecurityTokenHandler();
				Assert(tokenHandler.CanReadToken(accessToken));

				var token = tokenHandler.ReadJwtToken(accessToken);
				CombineAssertions(() =>
				{
					AssertEquals("0153A7C4B112DE00D8E50756C5249D5789C64B03", token.Header.Kid);
					AssertEquals($"https://identity.wisetechglobal.com/{clientId}", token.Issuer);
					AssertDateTimeWithinOneSecond("The expiry should be 30 minutes", token.IssuedAt.AddMinutes(30), token.ValidTo);
					AssertDateTimeWithinOneSecond("The expiry should be 30 minutes", token.ValidFrom.AddMinutes(30), token.ValidTo);
					AssertEquals(clientId, token.Payload.Aud.FirstOrDefault());
					AssertEquals("John Doe", token.Payload["name"]);
					AssertEquals(true, token.Payload["admin"]);
					AssertEquals(clientId, token.Payload["sub"]);
					AssertNotNullOrEmpty(token.Payload["jti"].ToString());
				});
			}
		}
	}
}
