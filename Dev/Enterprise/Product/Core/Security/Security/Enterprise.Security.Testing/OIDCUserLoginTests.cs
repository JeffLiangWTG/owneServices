using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using WTG.OpenIDConnect.Login;
using WTG.OpenIDConnect.Token;

namespace Enterprise.Security.Testing
{
	[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
	public class OIDCUserLoginTests : TestCaseWithFactory
	{
		public void TestShouldReturnFailedIfThereAreMultiStaffsMatchedMappingClaims()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "testemail@123.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "testemail@123.com";
			Factory.Save();

			var mockOidcConfig = new Mock<IOIDCConfig>();
			mockOidcConfig.Setup(config => config.ClaimsMappings).Returns(new OIDCClaimsMappingCollection()
			{
				new OIDCClaimsMapping
					{
						ClaimName = "user_email",
						Identifier = "GlbStaff.GS_EmailAddress"
					}
			});
			var claims = new List<Claim>();
			claims.Add(new Claim("user_email", "testemail@123.com"));
			claims.Add(new Claim("company_code", "WTG"));

			LoginAuthenticationInfo loginAuthenticationInfo = null;
			AssertNoExceptionThrown(() => loginAuthenticationInfo = OIDCUserLogin.VerifyUserByOidcClaims(mockOidcConfig.Object, claims));

			AssertNotNull(loginAuthenticationInfo);
			AssertEquals(false, loginAuthenticationInfo.IsOK);
			AssertEquals(false, loginAuthenticationInfo.LoginValidated);
			AssertEquals("Multiple users found based on the matched key 'GlbStaff.GS_EmailAddress' and value 'testemail@123.com'. Please contact your system administrator.", loginAuthenticationInfo.FailureMessage);
		}

		public void TestShouldCreateOneFactoryForMapping()
		{
			var staff = CreateStaff(new BusinessObjectFactory(), "Bob", "testpassword", "tst", true, false);
			LoginAuthenticationInfo loginAuthenticationInfo = null;

			using (var identityServer = DefaultMockOpenIdIdentityServer)
			{
				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(
					oidcConfig,
					TestOidcWebLauncher,
					CancellationToken.None);
			}

			AssertNotNull(loginAuthenticationInfo);
			AssertEquals(LoginAuthenticationInfo.Status.OK, loginAuthenticationInfo.State);
			AssertEquals(loginAuthenticationInfo.User.LoginName, staff.GS_LoginName);

			var factories = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories()
				.Where(factory => factory.NameForDebugging == "TryGetUserFromIdentityToken");
			AssertEquals("There should be only one factory created for TryGetUserFromIdentityToken", 1,
				factories.Count());
		}

		#region AuthorityUrl Validations

		public void TestEmptyAuthorityUrlShouldNotThrowException_PromptUserLoginAndClearRefreshToken()
		{
			var loginAuthenticationInfo = TestAuthorityUrlShouldNotThrowException(
				authorityUrl: null,
				(oidcConfig) =>
					OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, TestOidcWebLauncher,
						CancellationToken.None));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("Authority URL is empty.", loginAuthenticationInfo.FailureMessage);
		}

		public void TestEmptyAuthorityUrlShouldNotThrowException_SingleSignOnLoginWithRefreshToken()
		{
			var loginAuthenticationInfo = TestAuthorityUrlShouldNotThrowException(
				authorityUrl: null,
				(oidcConfig) =>
					OIDCUserLogin.SingleSignOnLoginWithRefreshToken(oidcConfig, TestOidcWebLauncher,
						CancellationToken.None));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("Authority URL is empty.", loginAuthenticationInfo.FailureMessage);
		}

		public void TestEmptyAuthorityUrlShouldNotThrowException_VerifyOidcConfig()
		{
			var loginAuthenticationInfo = TestAuthorityUrlShouldNotThrowException(
				authorityUrl: null,
				(oidcConfig) =>
					OIDCUserLogin.VerifyOidcConfig(oidcConfig, TestOidcWebLauncher, "domain", CancellationToken.None));
			AssertContains("Authority URL is empty.", loginAuthenticationInfo);
		}

		public void TestInvalidAuthorityUrlShouldNotThrowException_PromptUserLoginAndClearRefreshToken()
		{
			var loginAuthenticationInfo = TestAuthorityUrlShouldNotThrowException(
				authorityUrl: "InvalidUrl",
				(oidcConfig) =>
					OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, TestOidcWebLauncher,
						CancellationToken.None));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("Authority URL is invalid.", loginAuthenticationInfo.FailureMessage);
		}

		public void TestInvalidAuthorityUrlShouldNotThrowException_SingleSignOnLoginWithRefreshToken()
		{
			var loginAuthenticationInfo = TestAuthorityUrlShouldNotThrowException(
				authorityUrl: "InvalidUrl",
				(oidcConfig) =>
					OIDCUserLogin.SingleSignOnLoginWithRefreshToken(oidcConfig, TestOidcWebLauncher,
						CancellationToken.None));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("Authority URL is invalid.", loginAuthenticationInfo.FailureMessage);
		}

		public void TestInvalidAuthorityUrlShouldNotThrowException_VerifyOidcConfig()
		{
			var loginAuthenticationInfo = TestAuthorityUrlShouldNotThrowException(
				authorityUrl: "InvalidUrl",
				(oidcConfig) =>
					OIDCUserLogin.VerifyOidcConfig(oidcConfig, TestOidcWebLauncher, "domain", CancellationToken.None));
			AssertContains("Authority URL is invalid.", loginAuthenticationInfo);
		}

		T TestAuthorityUrlShouldNotThrowException<T>(string authorityUrl, Func<OIDCConfig, T> loginTask)
		{
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				ClientIdentifier = "interactive.public",
			};
			if (authorityUrl != null)
			{
				oidcConfig.AuthorityURL = authorityUrl;
			}

			var result = loginTask(oidcConfig);

			return result;
		}

		#endregion

		public void TestEDIProdLoginCheckCompanyCode()
		{
			var staff = CreateStaff(new BusinessObjectFactory(), "Bob", "testpassword", "tst", true, false);
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				identityServer.Claims = new List<Claim> { new Claim("username", "Bob") };
				identityServer.ClientIdentifier = "interactive.public";
				var oidcConfig = DefaultOidcConfig(identityServer.Port);
				var loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, TestOidcWebLauncher, CancellationToken.None);
				AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
				AssertEquals("Please log in using a user account within the WiseTech Global tenancy.", loginAuthenticationInfo.FailureMessage);

				identityServer.Claims.Add(new Claim("company_code", "WTG"));
				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, TestOidcWebLauncher, CancellationToken.None);
				AssertEquals(LoginAuthenticationInfo.Status.OK, loginAuthenticationInfo.State);
				AssertEquals(loginAuthenticationInfo.User.LoginName, staff.GS_LoginName);
			}
		}

		public void TestPromptUserLoginSuccess()
		{
			var exceptions = new List<Exception>();
			LoginAuthenticationInfo loginAuthenticationInfo;
			var staff = CreateStaff(new BusinessObjectFactory(), "Bob", "testpassword", "tst", true, false);

			using (var identityServer = DefaultMockOpenIdIdentityServer)
			{
				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(
					oidcConfig,
					TestOidcWebLauncher,
					CancellationToken.None);

				exceptions.AddRange(identityServer.Exceptions);
				identityServer.Dispose();
				exceptions.AddRange(identityServer.Exceptions);
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.OK, loginAuthenticationInfo.State);
			AssertEquals(staff.GS_LoginName, loginAuthenticationInfo.User.LoginName);
		}

		public void TestPromptUserEmailMappingLoginSuccess()
		{
			var exceptions = new List<Exception>();
			LoginAuthenticationInfo loginAuthenticationInfo;

			var staff = CreateStaff(new BusinessObjectFactory(), "Bob", "testpassword", "tst", true, false);

			using (var identityServer = DefaultMockWtgIdentityServer)
			{
				var oidcConfig = DefaultWtgIdPConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(
					oidcConfig,
					TestOidcWebLauncher,
					CancellationToken.None);

				exceptions.AddRange(identityServer.Exceptions);
				identityServer.Dispose();
				exceptions.AddRange(identityServer.Exceptions);
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.OK, loginAuthenticationInfo.State);
			AssertEquals(staff.GS_LoginName, loginAuthenticationInfo.User.LoginName);
		}

		public void TestPromptUserEmailMappingLoginFailure()
		{
			var exceptions = new List<Exception>();
			LoginAuthenticationInfo loginAuthenticationInfo;

			var factory = new BusinessObjectFactory();
			var staff = CreateStaff(factory, "Bob", "testpassword", "tst", true, false);
			staff.GS_EmailAddress = "";
			factory.Save();

			using (var identityServer = DefaultMockWtgIdentityServer)
			{
				var oidcConfig = DefaultWtgIdPConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(
					oidcConfig,
					TestOidcWebLauncher,
					CancellationToken.None);
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("No user record could be found that matched the retrieved identity information.", loginAuthenticationInfo.FailureMessage);
		}

		public void TestVerifyUserByOidcClaimsSuccess()
		{
			Exception[] exceptions;
			LoginAuthenticationInfo loginAuthenticationInfo;
			var staff = CreateStaff(new BusinessObjectFactory(), "Bob", "testpassword", "tst", true, false);

			using (var identityServer = DefaultMockOpenIdIdentityServer)
			{
				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.VerifyUserByOidcClaims(oidcConfig, identityServer.Claims);
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.OK, loginAuthenticationInfo.State);
			AssertEquals(staff.GS_LoginName, loginAuthenticationInfo.User.LoginName);
			AssertLoginTokenIsSame(loginAuthenticationInfo.User.LoginToken, new LoginToken());
		}

		public void TestVerifyUserByOidcClaimsSuccessWithDummyToken()
		{
			Exception[] exceptions;
			LoginAuthenticationInfo loginAuthenticationInfo;
			var staff = CreateStaff(new BusinessObjectFactory(), "Bob", "testpassword", "tst", true, false);

			using (var identityServer = DefaultMockLoginSimulatorServer)
			{
				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.VerifyUserByOidcClaims(oidcConfig, identityServer.Claims);
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.OK, loginAuthenticationInfo.State);
			AssertEquals(staff.GS_LoginName, loginAuthenticationInfo.User.LoginName);
			AssertLoginTokenIsSame(loginAuthenticationInfo.User.LoginToken, new LoginToken()
			{
				DummyTokenOriginalUserName = "Hunter",
				DummyTokenOriginalUserEmail = "Hunter@dummy.com"
			});
		}

		void AssertLoginTokenIsSame(ILoginToken loginToken, ILoginToken expected)
		{
			AssertNotNull(loginToken);
			AssertEquals(expected.IsDeveloper, loginToken.IsDeveloper);
			AssertEquals(expected.LoggedInWithSupportToken, loginToken.LoggedInWithSupportToken);
			AssertEquals(expected.ForcedRemoteLogoff, loginToken.ForcedRemoteLogoff);
			AssertEquals(expected.SupportTokenUserCode, loginToken.SupportTokenUserCode);
			AssertEquals(expected.SupportTokenUserName, loginToken.SupportTokenUserName);
			AssertEquals(expected.DummyTokenOriginalUserName, loginToken.DummyTokenOriginalUserName);
			AssertEquals(expected.DummyTokenOriginalUserEmail, loginToken.DummyTokenOriginalUserEmail);
		}

		public void TestVerifyUserByOidcClaimsFailure()
		{
			Exception[] exceptions;
			LoginAuthenticationInfo loginAuthenticationInfo;
			var staff = CreateStaff(new BusinessObjectFactory(), "James", "testpassword", "tst", true, false);

			using (var identityServer = DefaultMockOpenIdIdentityServer)
			{
				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.VerifyUserByOidcClaims(oidcConfig, identityServer.Claims);
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("No user record could be found that matched the retrieved identity information.", loginAuthenticationInfo.FailureMessage);
		}

		[ExpectNoExceptions]
		public void TestSingleSignOnLoginWithRefreshToken_OneLogin()
		{
			var mockOIDCLoginServer = new Mock<IOIDCLoginServer>();
			var oidcConfig = new OIDCConfig
			{
				OIDCServerType = OIDCServerTypes.OneLogin,
				AuthorityURL = $"https://{MockIdentityServerBase.HostName}",
			};

			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				OIDCUserLogin.SingleSignOnLoginWithRefreshToken(
					oidcConfig,
					TestOidcWebLauncher,
					CancellationToken.None);

				mockOIDCLoginServer.Verify(expression: s => s.LoginLocal(
					It.Is<OIDCLoginRequestMessage>(m => m.Scopes.SequenceEqual(new[] { "openid", "offline_access" }) && m.ServerType == OIDCLoginRequestMessage.OIDCServer.OneLogin),
					It.IsAny<OIDCWebLauncher>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<OIDCLoginFactory>()), Times.Once);
			}
		}

		public void TestPromptUserTokenExpired()
		{
			Exception[] exceptions;
			LoginAuthenticationInfo loginAuthenticationInfo;
			var originalIOIDCLoginServer = ObjectFactory.Get<IOIDCLoginServer>();
			var mockIOIDCLoginServer = new Mock<IOIDCLoginServer>();
			mockIOIDCLoginServer.Setup(server => server.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), It.IsAny<OIDCWebLauncher>(), CancellationToken.None, It.IsAny<OIDCLoginFactory>())).Returns<OIDCLoginRequestMessage, OIDCWebLauncher, CancellationToken, OIDCLoginFactory>((a, b, c, d) => originalIOIDCLoginServer.LoginLocal(a, b, c, d));
			mockIOIDCLoginServer.Setup(server => server.ValidateIdentityToken(It.IsAny<OIDCLoginRequestMessage>(), It.IsAny<OIDCLoginResponseMessage>(), CancellationToken.None)).Returns<OIDCLoginRequestMessage, OIDCLoginResponseMessage, CancellationToken>((a, b, c) => originalIOIDCLoginServer.ValidateIdentityToken(a, b, c));

			using (ObjectFactory.Substitute(mockIOIDCLoginServer.Object))
			using (var identityServer = DefaultMockOpenIdIdentityServer)
			{
				identityServer.TokensAreExpired = true;

				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(
					oidcConfig,
					TestOidcWebLauncher,
					CancellationToken.None);

				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("Lifetime validation failed", loginAuthenticationInfo.FailureMessage);
			mockIOIDCLoginServer.Verify(server => server.ValidateIdentityToken(It.IsAny<OIDCLoginRequestMessage>(), It.IsAny<OIDCLoginResponseMessage>(), CancellationToken.None), Times.Once(), "This validation method should be called so the expired error is returned by this method.");
		}

		public void TestPromptUserNotFound()
		{
			Exception[] exceptions;
			LoginAuthenticationInfo loginAuthenticationInfo;

			using (var identityServer = DefaultMockOpenIdIdentityServer)
			{
				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(
					oidcConfig,
					TestOidcWebLauncher,
					CancellationToken.None);

				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("No user record could be found", loginAuthenticationInfo.FailureMessage);
		}

		public void TestPromptUserCancel()
		{
			Exception[] exceptions;
			LoginAuthenticationInfo loginAuthenticationInfo;

			using (var cts = new CancellationTokenSource())
			using (var identityServer = DefaultMockOpenIdIdentityServer)
			{
				var oidcConfig = DefaultOidcConfig(identityServer.Port);

				loginAuthenticationInfo = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(
					oidcConfig,
					(startUrl) =>
					{
						cts.Cancel();
					},
					cts.Token);

				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.Message));
			AssertEquals(LoginAuthenticationInfo.Status.Failure, loginAuthenticationInfo.State);
			AssertContains("operation was canceled", loginAuthenticationInfo.FailureMessage);
		}

		MockOpenIDIdentityServer DefaultMockOpenIdIdentityServer
			=> new MockOpenIDIdentityServer()
			{
				Claims =
				{
					new Claim("username", "Bob"),
					new Claim("company_code", "WTG")
				},
				ClientIdentifier = "interactive.public",
			};

		MockOpenIDIdentityServer DefaultMockLoginSimulatorServer
			=> new MockOpenIDIdentityServer()
			{
				Claims =
				{
					new Claim("username", "Bob"),
					new Claim("company_code", "WTG"),
					new Claim("original_user_name", "Hunter"),
					new Claim("original_user_email", "Hunter@dummy.com")
				},
				ClientIdentifier = "interactive.public",
			};

		OIDCConfig DefaultOidcConfig(int listeningPort) =>
			new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{listeningPort}",
				ClientIdentifier = "interactive.public",
				ClaimsMappings =
				{
					new OIDCClaimsMapping
					{
						ClaimName = "usercode",
						Identifier = "GlbStaff.GS_EmailAddress"
					},
					new OIDCClaimsMapping
					{
						ClaimName = "username",
						Identifier = "GlbStaff.GS_LoginName"
					},
				},
			};

		OIDCConfig DefaultWtgIdPConfig(int listeningPort) =>
			new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.WiseTechIdP,
				AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{listeningPort}",
				ClientIdentifier = "interactive.public",
				ClaimsMappings =
				{
							new OIDCClaimsMapping
							{
								ClaimName = "email",
								Identifier = "GlbStaff.GS_EmailAddress"
							},
				},
			};

		MockOpenIDIdentityServer DefaultMockWtgIdentityServer
			=> new MockOpenIDIdentityServer()
			{
				Claims =
				{
					new Claim("company_code", "WTG"),
					new Claim("email", "e@mail.com")
				},
				ClientIdentifier = "interactive.public",
			};

		OIDCWebLauncher TestOidcWebLauncher => (startUrl) =>
		{
			var statePos = startUrl.IndexOf("state=");
			var state = startUrl.Substring(statePos, startUrl.IndexOf('&', statePos) - statePos);
			var pretendServer = new HttpClient();
			pretendServer.GetAsync(
				@"http://127.0.0.1:80/CargowiseOne/Authorize/?code=F99D7E65C1029E67B2B00565FB7D32C5B4FC4DAB53B6FF5B003E338C15BCE327&scope=openid%20profile%20api%20offline_access&" +
				state +
				"&session_state=PZV2X0rn1FueCpnIYz3CUzWhM1EYnq4D2N3aB6ajtsk.D490E01EF7BE079E072FBA3A8450574A");
		};

		GlbStaff CreateStaff(BusinessObjectFactory factory, string loginName, string password, string code, bool active, bool resource, bool isOperational = true, bool isController = false, bool canLogin = true, bool twoFactorEnabled = false)
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_IsResource = resource;
			staff.GS_CanLogin = canLogin;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			staff.StaffPlainTextPassword = password;
			staff.GS_IsActive = active;
			staff.GS_IsOperational = isOperational;
			staff.GS_IsController = isController;
			staff.GS_IsTwoFactorAuthenticationEnabled = twoFactorEnabled;
			staff.GS_EmailAddress = "e@mail.com";

			var security1 = factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			factory.Save();
			return staff;
		}

		public void TestVerifyTokenClaimReturnErrorIfLoginFailed()
		{
			var mockOIDCWebLauncher = new Mock<IOIDCWebLauncher>(MockBehavior.Strict);
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var errorDescription = "error description - test";
			var mockOIDCLoginServer = SetupMockOidcLoginServerWithFailure(loginRequestMessages, errorDescription);
			var cancellationToken = new CancellationToken();
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://something/",
				ClientIdentifier = "interactive.public",
			};
			string verifyResult;
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				verifyResult = OIDCUserLogin.VerifyOidcConfig(oidcConfig, mockOIDCWebLauncher.Object.Launch, "myDomain", cancellationToken);
			}
			AssertEquals(errorDescription, verifyResult);
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), mockOIDCWebLauncher.Object.Launch, cancellationToken, null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			AssertEquals(oidcConfig.AuthorityURL, loginRequestMessage.Authority);
			AssertEquals(oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
			AssertEquals("myDomain", loginRequestMessage.DomainHint);
			AssertEquals(OIDCLoginRequestMessage.OIDCServer.Generic, loginRequestMessage.ServerType);
			mockOIDCLoginServer.VerifyNoOtherCalls();
			mockOIDCWebLauncher.VerifyNoOtherCalls();
		}

		public void TestReturnErrorIfLoginFailed_PromptUserLoginAndClearRefreshToken()
		{
			string LoginTask(OIDCConfig oidcConfig, OIDCWebLauncher oidcWebLauncher, CancellationToken cancellationToken)
			{
				var response = OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, oidcWebLauncher, cancellationToken);
				AssertEquals(LoginAuthenticationInfo.Status.Failure, response.State);
				return response.ExtendedErrorInformation;
			}

			TestReturnErrorIfLoginFailed(LoginTask,
				"Azure",
				OIDCLoginRequestMessage.LoginPrompt.Login,
				"openid");
		}

		public void TestReturnErrorIfLoginFailed_SingleSignOnLoginWithRefreshToken()
		{
			string Login(OIDCConfig oidcConfig, OIDCWebLauncher oidcWebLauncher, CancellationToken cancellationToken)
			{
				var response = OIDCUserLogin.SingleSignOnLoginWithRefreshToken(oidcConfig, oidcWebLauncher, cancellationToken);
				AssertEquals(LoginAuthenticationInfo.Status.Failure, response.State);
				return response.ExtendedErrorInformation;
			}

			TestReturnErrorIfLoginFailed(Login,
				"Azure",
				OIDCLoginRequestMessage.LoginPrompt.Default,
				"openid", "offline_access");
		}

		public void TestReturnErrorIfLoginFailed_VerifyOidcConfig()
		{
			var domain = "myDomain";
			TestReturnErrorIfLoginFailed((oidcConfig, oidcWebLauncher, cancellationToken) => OIDCUserLogin.VerifyOidcConfig(oidcConfig, oidcWebLauncher, domain, cancellationToken),
				domain,
				OIDCLoginRequestMessage.LoginPrompt.Login,
				"openid");
		}

		void TestReturnErrorIfLoginFailed(Func<OIDCConfig, OIDCWebLauncher, CancellationToken, string> loginFunc,
			string expectedDomainHint,
			OIDCLoginRequestMessage.LoginPrompt expectedPrompt,
			params string[] expectedScopes)
		{
			var mockOIDCWebLauncher = new Mock<IOIDCWebLauncher>(MockBehavior.Strict);
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var errorDescription = "error description - test";
			var mockOIDCLoginServer = SetupMockOidcLoginServerWithFailure(loginRequestMessages, errorDescription);
			var cancellationToken = new CancellationToken();
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://something/",
				ClientIdentifier = "interactive.public",
			};
			string loginResult;
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				loginResult = loginFunc(oidcConfig, mockOIDCWebLauncher.Object.Launch, cancellationToken);
			}
			AssertEquals(errorDescription, loginResult);
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), mockOIDCWebLauncher.Object.Launch, cancellationToken, null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			AssertEquals(oidcConfig.AuthorityURL, loginRequestMessage.Authority);
			AssertEquals(oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
			AssertEquals(expectedDomainHint, loginRequestMessage.DomainHint);
			AssertEquals(OIDCLoginRequestMessage.OIDCServer.Generic, loginRequestMessage.ServerType);

			mockOIDCLoginServer.VerifyNoOtherCalls();
			mockOIDCWebLauncher.VerifyNoOtherCalls();
		}

		public void TestVerifyTokenClaimReturnEmptyIfLoginSucceedAndClaimIsPresent()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var oidcConfig = DefaultOidcConfig(1234);
			var claims = oidcConfig.ClaimsMappings.OfType<OIDCClaimsMapping>().Select(cm => new Claim(cm.ClaimName, cm.Identifier)).ToList();
			claims.Add(new Claim("company_code", "WTG"));
			var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages, claims);
			var mockOIDCWebLauncher = new Mock<IOIDCWebLauncher>(MockBehavior.Strict);
			var cancellationToken = new CancellationToken();
			string verifyResult = null;
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				verifyResult = OIDCUserLogin.VerifyOidcConfig(oidcConfig, mockOIDCWebLauncher.Object.Launch, "myDomain", cancellationToken);
			}
			AssertEquals(string.Empty, verifyResult);
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), mockOIDCWebLauncher.Object.Launch, cancellationToken, null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			AssertEquals($"{oidcConfig.AuthorityURL}/", loginRequestMessage.Authority);
			AssertEquals(oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
			AssertEquals("myDomain", loginRequestMessage.DomainHint);
			AssertEquals(OIDCLoginRequestMessage.OIDCServer.Generic, loginRequestMessage.ServerType);
			mockOIDCLoginServer.Verify(m => m.ValidateIdentityToken(loginRequestMessage, It.IsAny<OIDCLoginResponseMessage>(), cancellationToken));
			mockOIDCLoginServer.VerifyNoOtherCalls();
			mockOIDCWebLauncher.VerifyNoOtherCalls();
		}

		public void TestVerifyTokenClaimReturnErrorIfLoginSucceedAndClaimIsNotPresent()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var oidcConfig = DefaultOidcConfig(1234);
			var claims = new List<Claim>
			{
				new Claim("anotherClaim", "claim"),
				new Claim("company_code", "WTG")
			};
			var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages, claims);
			var mockOIDCWebLauncher = new Mock<IOIDCWebLauncher>(MockBehavior.Strict);
			var cancellationToken = new CancellationToken();
			string verifyResult = null;
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				verifyResult = OIDCUserLogin.VerifyOidcConfig(oidcConfig, mockOIDCWebLauncher.Object.Launch, "myDomain", cancellationToken);
			}
			AssertContains("Requested claim not found", verifyResult);
			AssertContains("anotherClaim", verifyResult);
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), mockOIDCWebLauncher.Object.Launch, cancellationToken, null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			AssertEquals($"{oidcConfig.AuthorityURL}/", loginRequestMessage.Authority);
			AssertEquals(oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
			AssertEquals("myDomain", loginRequestMessage.DomainHint);
			AssertEquals(OIDCLoginRequestMessage.OIDCServer.Generic, loginRequestMessage.ServerType);
			AssertEquals(90, loginRequestMessage.TimeoutSeconds);
			mockOIDCLoginServer.Verify(m => m.ValidateIdentityToken(loginRequestMessage, It.IsAny<OIDCLoginResponseMessage>(), cancellationToken));
			mockOIDCLoginServer.VerifyNoOtherCalls();
			mockOIDCWebLauncher.VerifyNoOtherCalls();
		}

		public void TestVerifyTokenClaimReturnErrorIfOperationCancelled()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var oidcConfig = DefaultOidcConfig(1234);
			var claims = oidcConfig.ClaimsMappings.OfType<OIDCClaimsMapping>().Select(cm => new Claim(cm.ClaimName, cm.Identifier));
			var mockOIDCLoginServer = SetupMockOidcLoginServer(loginRequestMessages, claims);
			var mockOIDCWebLauncher = new Mock<IOIDCWebLauncher>(MockBehavior.Strict);
			var cancellationToken = new CancellationToken(canceled: true);
			string verifyResult = null;
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			{
				verifyResult = OIDCUserLogin.VerifyOidcConfig(oidcConfig, mockOIDCWebLauncher.Object.Launch, "myDomain", cancellationToken);
			}
			AssertEquals("Authentication operation was canceled, please try again.", verifyResult);
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), mockOIDCWebLauncher.Object.Launch, cancellationToken, null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			AssertEquals($"{oidcConfig.AuthorityURL}/", loginRequestMessage.Authority);
			AssertEquals(oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
			AssertEquals("myDomain", loginRequestMessage.DomainHint);
			AssertEquals(OIDCLoginRequestMessage.OIDCServer.Generic, loginRequestMessage.ServerType);
			mockOIDCLoginServer.VerifyNoOtherCalls();
			mockOIDCWebLauncher.VerifyNoOtherCalls();
		}

		Mock<IOIDCLoginServer> SetupMockOidcLoginServer(List<OIDCLoginRequestMessage> loginRequestMessages, IEnumerable<Claim> claims)
		{
			var mockOIDCLoginServer = new Mock<IOIDCLoginServer>(MockBehavior.Strict);
			mockOIDCLoginServer.SetupGet(m => m.IsSupported).Returns(true);
			mockOIDCLoginServer
				.Setup(m => m.LoginLocal(Capture.In(loginRequestMessages), It.IsAny<OIDCWebLauncher>(),
					It.IsAny<CancellationToken>(), It.IsAny<OIDCLoginFactory>()))
				.Returns((OIDCLoginRequestMessage loginRequest, OIDCWebLauncher webLauncher, CancellationToken cancellationToken,
						OIDCLoginFactory oidcLoginFactory) =>
					BuilDummyResponseMessage(claims, loginRequest, webLauncher, oidcLoginFactory, cancellationToken));
			mockOIDCLoginServer
				.Setup(m => m.ValidateIdentityToken(It.IsAny<OIDCLoginRequestMessage>(),
					It.IsAny<OIDCLoginResponseMessage>(), It.IsAny<CancellationToken>()))
				.Returns((OIDCLoginRequestMessage request, OIDCLoginResponseMessage response, CancellationToken cancellationToken) =>
					BuildDummyJwtToken(request, response, cancellationToken));
			return mockOIDCLoginServer;
		}

		Mock<IOIDCLoginServer> SetupMockOidcLoginServerWithFailure(List<OIDCLoginRequestMessage> loginRequestMessages, string errorDescription)
		{
			var mockOIDCLoginServer = new Mock<IOIDCLoginServer>(MockBehavior.Strict);

			mockOIDCLoginServer.SetupGet(m => m.IsSupported).Returns(true);
			mockOIDCLoginServer
				.Setup(m => m.LoginLocal(Capture.In(loginRequestMessages), It.IsAny<OIDCWebLauncher>(),
					It.IsAny<CancellationToken>(), It.IsAny<OIDCLoginFactory>()))
				.Returns(OIDCLoginResponseMessage.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.LoginFailed, "error - test", errorDescription));
			return mockOIDCLoginServer;
		}

		OIDCLoginResponseMessage BuilDummyResponseMessage(IEnumerable<Claim> claims, OIDCLoginRequestMessage request, OIDCWebLauncher webLauncher, OIDCLoginFactory oidcLoginFactory, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return OIDCLoginResponseMessage.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.OperationCanceled, "Operation cancelled", "Operation cancelled for test");
			}

			var accessToken = Guid.NewGuid().ToString();
			var sub = Guid.NewGuid().ToString();
			var accessTokenExpiration = DateTimeOffset.UtcNow.AddMinutes(1);
			var idToken = MockJwtTokenProvider.GenerateValidJwtIDToken(claims, request.Authority, request.ClientID, sub, DateTime.UtcNow, accessTokenExpiration.UtcDateTime);
			return OIDCLoginResponseMessage.CreateSuccessResponse(idToken, accessToken, accessTokenExpiration);
		}

		JwtSecurityToken BuildDummyJwtToken(OIDCLoginRequestMessage request, OIDCLoginResponseMessage response, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return TokenValidator.ReadJwtToken(response.IdentityToken, null);
		}

		public interface IOIDCWebLauncher
		{
			void Launch(string url);
		}
	}
}
