using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using Enterprise.Registry.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Runner.Test.Fixture;

/// <summary>
/// Specific fixture class to mock call to the authority URL, using a mocked client handler 
/// </summary>
public class OAuthClientAssertionFixture : Assertion, IDisposable
{
	// interface of the protected mock object. It is not required but help to write Setup and Verify functions.
	interface IHttpSender
	{
		void Dispose(bool disposing);
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
	}

	// the request will be disposed by the time we can call Verify. We use this to capture the request content.
	record CaptureArgs
	{
		public string? Uri { get; init; }
		public string? Content { get; init; }
		public CancellationToken CancellationToken { get; init; }

		public CaptureArgs(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Uri = request.RequestUri?.AbsoluteUri;
			Content = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
			CancellationToken = cancellationToken;
		}
	}

	readonly string audience;
	readonly Mock<HttpClientHandler> httpClientHandlerMock = new (MockBehavior.Strict);
	readonly ICollection<CaptureArgs> captureArgsCollection = new List<CaptureArgs>();
	IProtectedAsMock<HttpClientHandler, IHttpSender> ProtectedMock => httpClientHandlerMock!.Protected().As<IHttpSender>();

	public OAuthClientAssertionFixture(string audience)
	{
		this.audience = audience;
	}

	public void Dispose()
	{
		httpClientHandlerMock!.VerifyNoOtherCalls();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "We cannot dispose the response as it needs to be returned by the mock HttpClient")]
	public HttpClient CreateFakeHttpClient()
	{
		ProtectedMock.Setup(m => m.Dispose(It.IsAny<bool>()));
		ProtectedMock
			.Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
			.Callback((HttpRequestMessage req, CancellationToken ct) => captureArgsCollection!.Add(new CaptureArgs(req, ct)))
			.ReturnsAsync(
				new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = JsonContent.Create(new
					{
						Access_token = GenerateMockCwToken(audience),
					}),
				});
		return new HttpClient(httpClientHandlerMock!.Object, disposeHandler: false);
	}

	public void VerifyCall(string? actualToken)
	{
		ProtectedMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once());
		var captureArgs = captureArgsCollection.FirstOrDefault();
		AssertNotNull(captureArgs);
		CombineAssertions(() =>
		{
			AssertEquals(GenerateMockCwToken(audience), actualToken);
			AssertEquals($"https://login.microsoftonline.com/{systemToSystemTrustInfo.TenantId}/oauth2/v2.0/token", captureArgs!.Uri);
			AssertNotNull(captureArgs.Content);
			var dict = HttpUtility.ParseQueryString(captureArgs.Content!);
			AssertEquals(systemToSystemTrustInfo.ClientId, dict["client_id"]);
			AssertEquals($"{audience}/.default", dict["scope"]);
		});
	}

	public void VerifyNoCall()
	{
		ProtectedMock.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never());
		AssertContainsExactElementsInExactOrder(Enumerable.Empty<CaptureArgs>(), captureArgsCollection);
	}

	public static string GenerateMockCwToken(string audience)
	{
		return $"Mock token for {audience}";
	}

	readonly SystemToSystemTrustInfo systemToSystemTrustInfo = new ()
	{
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
		PrivateKey = privateKeyPem,
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
		ClientId = Guid.NewGuid().ToString(),
		TenantId = Guid.NewGuid().ToString(),
		Certificate = new X509Certificate2(Encoding.UTF8.GetBytes(CertificateString)).RawData,
	};

	public SystemToSystemTrustInfo SystemToSystemTrustInfo => systemToSystemTrustInfo;

	const string privateKeyPem = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDnv8pQZ3Zgettj
WJWJJfAYJZ7xSy/HE8+w0DkOXOMfWkDLo4lvl1ILxI8ievnFAK/B/frSJ/deuRII
mS8mXH1GFweCmdBB8uK69Ugd9tlAcxEvv46rgA/N01eSdcExJ1SONz15IGOnFGEM
2p8vEdwmVLMZlk0kKMueOcveq0rlYE2CO7uD5k+58woFF8ByI2UAlBYwr8IsUK22
WUf3C4UFHwhhjqivN98nWehx3TCV7mVw2DyKGCwZKiqAxxHfZlTeX4w0TyoNwuKV
XY5pbRqWi7Yret3emrxY61lOAYYw4dpMQ4EBeqBdlBvOC6JcR7qKOgsGDCbOSCse
rDomAZ59AgMBAAECggEAC5sz1H4HVC1NonaTxUgZ86OislPrzc0PXZFit3ZDG6qx
9wuMJ64M5QZQOCUE2u8TXk72yk1HxWiAX4ozGHk7qZB1XFQ8Cql8MxUd/6jWrYnL
EQ3yD7hnUjgPjogI3Qoqtmqhe1j6FKqKcmcv317GHJdTrD2LmdAp47/iQWxpC6lZ
d0g5F6lmdztkZdQhm7njhxExJ+CdU68rnJeks3GPYQzCZA4V7tkxP98Y5XaBWcdT
/dK1vcXS1SrInEDZvli/cXIc8sAf6LqDB9yqdcPw++oI+aUFLGmOnImCouPx4jA5
MXLdf0UXsLJbPgteBCtTxBeWene76Ym3CkyS3n0TMwKBgQDvzsABwAx7rXpIVwbL
ff3a11T2JQVHG8Ts1TvWtxFYBcLt0QThTMve4VENDQUk/il3Ltn+lLgjx+5Vyyaa
3UDYuZmDnbrlq4DfePZa7VNWj6TBFNRChMzxZG3MWUHP7qda8NcFM6pEZJpyFSeQ
8ij5Wu7PuywW5D3rkoc0sDOyKwKBgQD3Zb6KQr1wvCHy0GH86FSNYB0P3mS45KWM
JU3AdlwndtCVlKyDuhtlcaxK2OlZZiLU0aKnvLR+y77OPu5YawcEdGrZmwGerVYd
fzKDzSJuEn8MM13+GcAJlFlsoklUaeu3bHmAGKUw3ph1B6Ix0c2/HHFkjHelbT9b
t2Kc8GGl9wKBgEEzqrsPF5XNDjF7EArmH86Pu7cNS8kQwNNQCuwPbHTNZDm7GiOT
+N6JzrrIrnxnaqjQIU956jM4WhIToVR8EfSbSiUiDr4BipG4VutUGdOwTLB+1FOd
vgdoMf5cymsZzYEJeL0eVg4weFnKbK6ZWRCra8EpeAxlVHyno4Fs4zFvAoGBAOrQ
y3V3u09RgfdyCk9+RSKa43q4X2mOvAK1NYND1FwwzfHr14KAFpjGt/2ivHl6E/1j
rLsAxWDECirAWIHbtCFqTjCUi4kMhPwiStQG1HMdYzE1YDVaQ4fUIryVnHxevLiw
YPJQchpcbOBHio82z85hNM928+k0NDrdaOAE2OopAoGAaMsqGcNzwXZmYI+GNH6Z
zZPH0t4Cg+m3L1HliI+8FeMxwL6sWRxxL1qGG73xj7JGUjxCIPX52oGCoUob/x1Y
VE5Exfnv+O555Sy5hksOvTepn1fiZWVoDQc3KntZegRFKGEuozqQ1eKCkLieQ0/m
k8VXc3uhpFenM2ZGK0TedMY=
-----END PRIVATE KEY-----";

	const string CertificateString = @"-----BEGIN CERTIFICATE-----
MIID2jCCAsKgAwIBAgIQR0RemLM7Lo27g7CJGUVSRzANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwNTA4MDYxNDAzWhcNMjQwNTA3MDcxNDAz
WjBjMQswCQYDVQQGEwJBVTEMMAoGA1UECAwDU1lEMQwwCgYDVQQHDANTWUQxDDAK
BgNVBAoMA1dURzEMMAoGA1UECwwDV1RHMRwwGgYDVQQDDBN3aXNldGVjaC5nbG9i
YWwuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA57/KUGd2YHrb
Y1iViSXwGCWe8UsvxxPPsNA5DlzjH1pAy6OJb5dSC8SPInr5xQCvwf360if3XrkS
CJkvJlx9RhcHgpnQQfLiuvVIHfbZQHMRL7+Oq4APzdNXknXBMSdUjjc9eSBjpxRh
DNqfLxHcJlSzGZZNJCjLnjnL3qtK5WBNgju7g+ZPufMKBRfAciNlAJQWMK/CLFCt
tllH9wuFBR8IYY6orzffJ1nocd0wle5lcNg8ihgsGSoqgMcR32ZU3l+MNE8qDcLi
lV2OaW0alou2K3rd3pq8WOtZTgGGMOHaTEOBAXqgXZQbzguiXEe6ijoLBgwmzkgr
Hqw6JgGefQIDAQABo3wwejAJBgNVHRMEAjAAMB8GA1UdIwQYMBaAFMUZFOQcnWCL
2XW0xLCOqDhZ7wLiMB0GA1UdDgQWBBRgVXKxahXMIirYYm0846gHnJeY5jAOBgNV
HQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMA0GCSqG
SIb3DQEBCwUAA4IBAQBcRs+X0iH/K5dOTQkZ5/v13huLOXb3QFExTJW2+tKmRYe1
e3CoQVE6LQJ+cCQ+Tl6UmbyeyTIqEuFpTmm6Yhl9agu41tlgiobP1+YQ/VMjasgh
Vhgr34KA09iVzpLsIlROdNW5Q5rfjRh1WuBAEPcABKKNFgaqVvS2BKzd/a6aXaId
Zu+YuRV232OXOUZP07DkaLhax6wfSf+tfkNLQLvoVNcJmcF6mNgzg2HULuvia77u
HPpykW02IbnSAr3jDVGHezVNLctFrpHpCWRlTUMulW56xc74ZiONSf+N/2WZJo0x
wNakFhJRZGZJpKCx8xrIO4D9y7jt0WGP7ZCvcXeQ
-----END CERTIFICATE-----";
}
