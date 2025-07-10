using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TestHelpers.MockServer;

namespace Enterprise.Messaging.GUI.Test
{
	public class OutboundOAuthUserControlTest : TestCaseWithFactory
	{
		MockServer CreateMockOAuth2Server(MockResponseJson mockResponse)
		{
			var server = new DynamicMockServer("Mock OAuth2 Server");
			server.AddRoute("POST", "/oauth/oauth2/token", (p) => {
				return mockResponse;
			});
			return server;
		}

		MockResponseJson CreateMockSuccessResponse()
		{
			var mockResponseToken = new AuthToken()
			{
				AccessToken = Guid.NewGuid().ToString(),
				ExpiresIn = 86400,
				TokenType = "Bearer ",
				RefreshToken = Guid.NewGuid().ToString()
			};
			var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
			return new MockResponseJson(mockResponse, 200);
		}

		MockResponseJson CreateMockErrorResponse(string type, string description, string link)
		{
			var mockResponseToken = new ErrorResponse()
			{
				Error = type,
				ErrorDescription = description,
				ErrorURI = link
			};
			var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
			return new MockResponseJson(mockResponse, 400);
		}

		ZLabel InvokeVerifyButton(Form form)
		{
			var control = form.Controls[0] as OutboundOAuthUserControl;
			var button = (ZButton)control.Controls.Find("VerifyButton", true)[0];
			control.VerifyButton_Click(button, null);
			return (ZLabel)control.Controls.Find("Tick", true)[0];
		}

		Form GetForm(EDICommunicationParty party)
		{
			var control = new OutboundOAuthUserControlForTest();
			control.SetDataBinding(party, "");

			var form = new Form();
			form.Controls.Add(control);

			return form;
		}

		EDICommunicationParty GetCommunicationParty()
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_Name = "party";
			var auth = party.OutboundConfig.Auth;

			auth.ECA_AuthorizationMode = EDICommunicationAuthModesList.Codes.OAuthAuthentication;
			auth.ECA_ClientSecret = "Secret";
			auth.ECA_ClientID = "ID";

			return party;
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestVerifyConfigNoError()
		{
			using (var server = CreateMockOAuth2Server(CreateMockSuccessResponse()))
			{
				var party = GetCommunicationParty();

				party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials;
				party.OutboundConfig.Auth.ECA_AuthorizationEndpoint = server.GetFullUrl("oauth/oauth2/token");

				using (var form = GetForm(party))
				{
					var tick = InvokeVerifyButton(form);

					AssertEquals("Tick updated", "✓", tick.Text);
					AssertNoErrors("No validation error", party.OutboundConfig.Auth.ECA_FlowCodeInfo);
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestVerifyConfigErrorResponse()
		{
			var errorType = OAuth2Errors.EnumToString(OAuth2ErrorTypes.InvalidRequest);
			var errorDescription = "error";
			var errorLink = "link";

			using (var server = CreateMockOAuth2Server(CreateMockErrorResponse(errorType, errorDescription, errorLink)))
			{
				var party = GetCommunicationParty();

				party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials;
				party.OutboundConfig.Auth.ECA_AuthorizationEndpoint = server.GetFullUrl("oauth/oauth2/token");

				using (var form = GetForm(party))
				{
					var tick = InvokeVerifyButton(form);

					AssertEquals("Tick updated", "X", tick.Text);
					AssertHasErrorContaining(party.OutboundConfig.Auth.ECA_FlowCodeInfo, errorDescription);
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestVerifyConfigInvalidFlowCode()
		{
			using (var server = CreateMockOAuth2Server(CreateMockSuccessResponse()))
			{
				var party = GetCommunicationParty();

				party.OutboundConfig.Auth.ECA_FlowCode = "tst";
				party.OutboundConfig.Auth.ECA_AuthorizationEndpoint = server.GetFullUrl("oauth/oauth2/token");

				using (var form = GetForm(party))
				{
					var tick = InvokeVerifyButton(form);

					AssertEquals("Tick updated", "X", tick.Text);
					AssertHasErrorContaining(party.OutboundConfig.Auth.ECA_FlowCodeInfo, "An authorization flow code must be specified");
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestVerifyConfigInvalidAuthUrl()
		{
			using (var server = CreateMockOAuth2Server(CreateMockSuccessResponse()))
			{
				var party = GetCommunicationParty();

				party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials;
				party.OutboundConfig.Auth.ECA_AuthorizationEndpoint = "abc";

				using (var form = GetForm(party))
				{
					var tick = InvokeVerifyButton(form);

					AssertEquals("Tick updated", "X", tick.Text);
					AssertHasErrorContaining(party.OutboundConfig.Auth.ECA_FlowCodeInfo, "Malformed Authorization URL");
				}
			}
		}

		public void TestHandleUnexpectedException()
		{
			var party = GetCommunicationParty();

			party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials;
			party.OutboundConfig.Auth.ECA_AuthorizationEndpoint = "abc";

			using (var form = GetForm(party))
			{
				var control = new OutboundOAuthUserControlForTestUnexpectedException();
				control.SetDataBinding(party, "");
				form.Controls.Add(control);

				var button = (ZButton)control.Controls.Find("VerifyButton", true)[0];
				control.VerifyButton_Click(button, null);
				var tick = (ZLabel)control.Controls.Find("Tick", true)[0];

				AssertEquals("Tick updated", "X", tick.Text);
				AssertHasErrorContaining(party.OutboundConfig.Auth.ECA_FlowCodeInfo, "Mocked exception");

				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertContains("Error on config verification: Mocked exception.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestFieldsVisibilityByFlowCode()
		{
			var party = GetCommunicationParty();
			using (var form = GetForm(party))
			{
				form.Show();
				var control = form.Controls[0] as OutboundOAuthUserControl;
				var username = (ZTextBox)control.Controls.Find("Username", true)[0];
				var password = (ZTextBox)control.Controls.Find("Password", true)[0];
				var clientSecret = (ZTextBox)control.Controls.Find("ClientSecret", true)[0];
				var certificateGroupBox = (ZGroupBox)control.Controls.Find("CertificateGroupBox", true)[0];

				party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate;
				Assert(!username.Visible);
				Assert(!password.Visible);
				Assert(!clientSecret.Visible);
				Assert(certificateGroupBox.Visible);

				party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials;
				Assert(!username.Visible);
				Assert(!password.Visible);
				Assert(clientSecret.Visible);
				Assert(!certificateGroupBox.Visible);

				party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.Password;
				Assert(username.Visible);
				Assert(password.Visible);
				Assert(clientSecret.Visible);
				Assert(!certificateGroupBox.Visible);
			}
		}

		public void TestClientIdAuthorityUrlReadOnlyStatus()
		{
			var party = GetCommunicationParty();

			void AssertClientIdAuthorityUrlReadOnlyStatus()
			{
				using (var form = GetForm(party))
				{
					var control = form.Controls[0] as OutboundOAuthUserControl;
					var authorizationURL = (ZTextBox)control.Controls.Find("AuthorizationURL", true)[0];
					var clientId = (ZTextBox)control.Controls.Find("ClientID", true)[0];

					Assert("AuthorizationURL text box is enabled", authorizationURL.Enabled);
					Assert("ClientID text box is enabled", clientId.Enabled);
					Assert("AuthorizationURL text box is not readonly", !authorizationURL.ReadOnly);
					Assert("ClientID text box is not readonly", !clientId.ReadOnly);
				}
			}

			AssertClientIdAuthorityUrlReadOnlyStatus();

			GlbStaff.CurrentUser.GS_LoginName = "Dummy";

			AssertClientIdAuthorityUrlReadOnlyStatus();
		}

		public TwoWayEncoder TwoWayEncoder => TwoWayEncoder.NewWithStandardInitialisationVector();

		public void TestShowCertificate()
		{
			var party = GetCommunicationParty();

			party.ECP_Name = "EDIClientName";
			party.OutboundConfig.Auth.ECA_FlowCode = EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate;
			party.OutboundConfig.Auth.ECA_AuthorizationEndpoint = "abc";

			AssertExceptionThrown<Exception>("Certificate is not valid", "Certificate is not valid.", () => party.OutboundConfig.Auth.SetCertificate("123", party.ECP_Name));

			var certificatePem = @"-----BEGIN CERTIFICATE-----
MIIEOjCCAyKgAwIBAgIIX4MTnZS2hhcwDQYJKoZIhvcNAQELBQAwgboxKTAnBgkq
hkiG9w0BCQEWGnN1cHBvcnRAd2lzZXRlY2hnbG9iYWwuY29tMQswCQYDVQQGEwJB
VTEPMA0GA1UEBwwGU3lkbmV5MRgwFgYDVQQIDA9OZXcgU291dGggV2FsZXMxGDAW
BgNVBAoMD1dpc2VUZWNoIEdsb2JhbDEeMBwGA1UECwwVRURJL0RBVC9FRElDbGll
bnROYW1lMRswGQYDVQQDDBJ3aXNldGVjaGdsb2JhbC5jb20wHhcNMjQwNzA1MDcw
MzM3WhcNMjUwNzA1MDcwMzM3WjCBujEpMCcGCSqGSIb3DQEJARYac3VwcG9ydEB3
aXNldGVjaGdsb2JhbC5jb20xCzAJBgNVBAYTAkFVMQ8wDQYDVQQHDAZTeWRuZXkx
GDAWBgNVBAgMD05ldyBTb3V0aCBXYWxlczEYMBYGA1UECgwPV2lzZVRlY2ggR2xv
YmFsMR4wHAYDVQQLDBVFREkvREFUL0VESUNsaWVudE5hbWUxGzAZBgNVBAMMEndp
c2V0ZWNoZ2xvYmFsLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEB
AJh2uI1Z4DUCLis/ESVZ7qwAagTfoyWEF2O/aljFAmrjDj6DeETAq2IkIPnqnI7Q
EpfbE6uEquMlFAckoQ3z2x9kYIRqyGbKSHhFKJaa1R02VPtmDndGMQJDBstJYFO4
MSVrFMy0hsjymm0LT/3hsKDoCu+/jIZcI6n/of5eGXKbpiyDuV9bNnAym0Aek8CM
T8Zx54JIRfWPepCdz+ydy18vY+adnVtvR/HI8IUNQQGgSPcOrYalpUYEFmsnTtSY
3s53mI49uwtxkOtHRQGMiWuLp9ZPAPnDhoYS3+WZV3WACDcfeF87+NvKzcVYrzNp
uAD1LdFEtW62zkXIYrTnTRcCAwEAAaNCMEAwHwYDVR0jBBgwFoAUXcqGD5uGCnG5
v5A2eg/dn6gqhNEwHQYDVR0OBBYEFF3Khg+bhgpxub+QNnoP3Z+oKoTRMA0GCSqG
SIb3DQEBCwUAA4IBAQB5OTgdKXza9vi7dqwUF+pEzWspyskI4/WJYJV2Euqav8aU
Fw52dmFHntNndYMHtaVAuqrFNOr345E5rg5moggOjvs77ZV+Y/WpnRT7K8NzV/A/
fMVW0Om+fF7DVwJ3u1OyKZNa6Zg4lXch2D3w147A6SHvqNfqFw4XFCBnSBO8vmb9
APVcwWvHyMLFp+29sAvUKQSSyp8gJJz7oyCywHT7llGrgG229GzfB6LoK98385V8
6UscEGYFZHa5U0pJcgwlRRTiM1mYH9KXRccoKtwvGV5pkzr059Tju6nZ9diBy9Lm
Hs2Qrf72nFOdINRlsSdbyoUvvWJvm7k+ANMQXL0v
-----END CERTIFICATE-----
";
			var notMatchedPrivateKeyPem = @"-----BEGIN RSA PRIVATE KEY-----
MIIEoAIBAAKCAQEAniWs0yt/2uLhxia447Jquy/WdWlAl5W88zHycp2QsYPrwqfS
1Hz3ESZIdii6kD5jp/zftIvL33tvPdgaQ/njG8BnMlg0II6OQ9FV/R/3roeZ/ttx
0ba3gyFaISzp4fQGbw/jSDFz20VIp9DfJBqdaSYOUO6iqNfeN08YVh41EKU5V6WE
98xARZKn1/OJV6FBK0MlA4YREZS5tOYiSIg/10y9Xpf3Hfyho+7ZhtZcQKTgvD8R
NwtGFv/fsKeDzA8HSy609Fy52j8I+AqieWxojcv+LQCeXyhDXZtWQmBpl/4abzxl
a5enlbsaG7zshgYTaVingAOcGMSf8FrAeAFEBQIDAQABAoH/SKXSvmIJ80/fmlov
YZz6nc9b6C0+S5dKaG1WJbcIwOlylywaek7tlozMVkf5BX09FyF5O91iluCDbLI/
XpQ48u0JR7DWp7ehorjtlCa0095DrGkiRmdCspPS02qbOTDgKiS73TraZEY8TUE/
polgLIbocxH5QGODSrHXr1FkQkCD+V0hKksoiDzQbBTL1pEWNJlBrL3Xfd55p7Jo
PWYOHjdPoO4t8fWB8NvZrUvCKOW4JGATT/0dv2d0iqZZoJenftXmGMypuOjej7xH
6oZPNVPDZ+HfQ2Vnhuug6gqjymgx83JFA4xh29kEuq7jiHprkGl5KHruBLkDnz0n
XNvRAoGBAN1+02UPVX8RJFZ/AeX9QXm4kO44LkxgserTZzGmFzFWvUICC6dSjlGf
h8dUisw6vUcicJk+1h9+gBr+bgLIdLmMlIIYn1J0CxZfvwoejVfceTD4wBuWmm1V
9W2+oNLXwt4C0BkFFix0Dj3HaJCRByOe+RTC+I/TQuumCU9Up/LXAoGBALbIig/0
KyqCXzJOSYslxkFpzYH0RaoPjmro9TnqRAGfUSj6f6sPDD0UJjg2xBWPy//T8NM1
BTJ9PnS0m7LqCnPdwPWeI15tOn8Q4iRu4u2SxH77jdZ+ZFQFMxDt07dVdnpWkGPH
jh2CssmokiShClur07yZcZrcCCn2LOxYcACDAoGAVNxjd6OxWPajWnRcsNYgANei
x3JE4sRlRhfqwI3m8uNNrX2FI30ww7sAVXFhoC+dFyTtyPjXJsfbovv6ABz3A6QF
vfZbDEcjfFYAporgElqJICcdyzQfRGabmolfqog5w7+GJR/ax/tK/YZp3IGEH7Sn
r7mna8tAJ3K8vGUt148CgYA8M2aNhJzm/lN+TRyZ0NIwXzmJcLu1HDSK77k83vSR
GDjx5zc/TtB4GXfREfCZLWhCFTr/RTo1+9wjMg0J612P9dyEyRXbFtyqVxQ0bbUe
DDXdoNqBZ9WBesjr7XjKrEI+Xt5ljqc2/wIFuxiPKd8wfDJbrqolGA00TNlU6nfR
IwKBgE5+q0i90qlahqEqKil//FPfVTSVr3qaG7C5MOfKd97K2K+JvqDdBZZKBkPZ
qe/IPFsG7VxIf6VOQyEyuoqyiqhk3HKge3glANrPcDFVCFTs/SVV2IaUqYsymN34
zZL9tvhA1eM1ZAkAUmT9Nyq37EmdqbXarEy4/J+qpV0v2YJn
-----END RSA PRIVATE KEY-----";
			party.OutboundConfig.Auth.SetPrivateKey(notMatchedPrivateKeyPem);

			AssertExceptionThrown<Exception>("Certificate does not match the Private Key.", "Certificate does not match the Private Key.", () => party.OutboundConfig.Auth.SetCertificate(certificatePem, party.ECP_Name));

			var matchedPrivateKeyPem = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAmHa4jVngNQIuKz8RJVnurABqBN+jJYQXY79qWMUCauMOPoN4
RMCrYiQg+eqcjtASl9sTq4Sq4yUUByShDfPbH2RghGrIZspIeEUolprVHTZU+2YO
d0YxAkMGy0lgU7gxJWsUzLSGyPKabQtP/eGwoOgK77+Mhlwjqf+h/l4ZcpumLIO5
X1s2cDKbQB6TwIxPxnHngkhF9Y96kJ3P7J3LXy9j5p2dW29H8cjwhQ1BAaBI9w6t
hqWlRgQWaydO1JjezneYjj27C3GQ60dFAYyJa4un1k8A+cOGhhLf5ZlXdYAINx94
Xzv428rNxVivM2m4APUt0US1brbORchitOdNFwIDAQABAoIBAAKe48w2M8blezKu
GlbYhWQ6e5gK2gyOiTJjO2o8NK7uqTOE4f/YifmdYl25XSiNRgyLLPrhRGi0HfSD
eis5ulX/TTNpfHlb18QNeEWicrBWWz6ZAf2l3LjLuyWqZLf7rgiVHx3nqntwxBvE
uoLEKtuRMYLueXVjxw8ogDnVlz1jjZQDQ4ajQNbvW30W3rufm+IWqZ7W/EDokWcc
9ed/eu8EptO5/mH5877gGwGEMeCOOzfLTo8wDebw5kBRXWuVqY8d8gL5+SSOBq5o
VYyv1Y+Vk9l1I3EMqqYlIvaREL+Hmdh9KmlLuGPJ+4xpbC43hTn7VF+09lzXpqaC
7D9/Ew0CgYEA9Qe3Ohe0OslAX9WiDAmL5S7G6Hi6dendPy8yWLAMq85fhcbsoF1Z
D7IyrzNTy/CSonLmiu3bTpW09lPt2s7vsInfT+99gR9rlM1u42vk2ifxcf3FQSJm
CHLnXxZT62qhk4a3ohMz6iIvSFSFOicwvgxsMNn+ieCEjWclvfK5MX0CgYEAn0oa
qcxq/lYc+RIA8fT2wbLtiKNhP/3f1se26N9OU5tcHh+BPYn/jsHWI9zB48uaXwod
1BUSW3ZKiV923vS2NxE8GoFwUeQH+fkG3TSH7TV7NJaGIuF0DxrU5TWHQPTcGRX8
JF30W6Xdkzan6ti0RhWWbcncXkbzFC15rd3x/SMCgYEAuzLC3CIB8quQf+cB33pn
o5diJXce1Tjva/dN2o3dkGChf93jJ/1JLoGw0UNAcN2B2ZQ458kitF4Rm+OxI2rX
miMrNbG9S6nKkiuE3UCv3a+IedMsIT/7fdbzRyUSxhd4C/JvVuae0fB9+R+BjVUl
mvx4p7XUDlg2TKWSIxVOQS0CgYBiIo16vu3L89G1wVnDt1+uxkWBQObRPd+Bu1j8
71aaO8Ts6gv9ld9UXCdJwN/TL8TTeLAX0UOWBbK2H5Jkme8Izh1xVv2T9iDT6JBK
B+sWQTS+mV3ab3vJMoanD+tcIX7YFatZ3GiHbhCseafKD+hApVwgF5UkoCFx9PJa
I7rKcQKBgC49p7C/nftbsPY7AKluCAcSUmPz0Y5WN07RoKv9+N46bmEyzgryzMy+
f/mQa2693fNU0+hzsyy3FJkPcXAyXskZF+QR47JdxIrb9w2f3gl5mwaRD65FahTR
NrBJ/uXxhgOnFsrZNsFqqOgodP0oeJoziC0yi4um/9e4evQ2S4Aq
-----END RSA PRIVATE KEY-----";

			var notVerifiedSubject = "E=support@wisetechglobal.com,C=AU,L=Sydney,ST=New South Wales,O=WiseTech Global,OU=EDI/DAT/EDIClient,CN=wisetechglobal.com";

			party.OutboundConfig.Auth.SetPrivateKey(matchedPrivateKeyPem);
			AssertExceptionThrown<Exception>("Certificate subject information does not match the CSR.", "Certificate subject information does not match the CSR.", () => party.OutboundConfig.Auth.SetCertificate(certificatePem, party.ECP_Name, notVerifiedSubject));

			var verifiedSubject = string.Format("E=support@wisetechglobal.com,C=AU,L=Sydney,ST=New South Wales,O=WiseTech Global,OU=EDI/DAT/{0},CN=wisetechglobal.com", party.ECP_Name);

			party.OutboundConfig.Auth.SetCertificate(certificatePem, party.ECP_Name, verifiedSubject);

			using (var form = GetForm(party))
			{
				var control = new OutboundOAuthUserControlForTest();
				control.SetDataBinding(party, "");
				form.Controls.Add(control);
				control.OpenCertificateForm();

				using (var shownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(shownForm);
					AssertType(typeof(TextDialogForm), shownForm);

					AssertEquals("Certificate should be equal", certificatePem, ((TextDialogForm)shownForm).SecretString);
				}
			}
		}

		public void TestNonDuplicateClientNameBeforeCSR()
		{
			var party1 = Factory.NewWithValidTestData<EDICommunicationParty>();
			party1.ECP_Name = "party1";
			Factory.Save();
			var party2 = GetCommunicationParty();
			using (var form = GetForm(party2))
			{
				form.Show();
				party2.ECP_Name = "party1";
				var control = form.Controls[0] as OutboundOAuthUserControl;
				var certificateGroupBox = (ZGroupBox)control.Controls.Find("CertificateGroupBox", true)[0];
				var button = (ZButton)certificateGroupBox.Controls.Find("GenerateCSRButton", true)[0];
				button.PerformClick();
				AssertEquals("Error when saving duplicate name", "The name 'party1' is already in use by another EDI Client. Please select a different name.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}

	class OutboundOAuthUserControlForTest : OutboundOAuthUserControl
	{
		public OutboundOAuthUserControlForTest() : base() { }

		protected override Task<AuthToken> GetAuthToken(EDICommunicationPartyConfig config)
		{
			var token = OAuth2Connect.RequestAuthToken(Config.Auth, new CancellationToken()).ConfigureAwait(false).GetAwaiter().GetResult();
			return Task.FromResult(token);
		}
	}

	class OutboundOAuthUserControlForTestUnexpectedException : OutboundOAuthUserControl
	{
		public OutboundOAuthUserControlForTestUnexpectedException() : base() { }

		protected override Task<AuthToken> GetAuthToken(EDICommunicationPartyConfig config)
		{
			throw new InvalidOperationException("Mocked exception");
		}
	}
}
