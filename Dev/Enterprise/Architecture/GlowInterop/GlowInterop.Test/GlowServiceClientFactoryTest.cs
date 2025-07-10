using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Authentication.Glow.Client;
using CargoWise.Authentication.Primitives;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class GlowServiceClientFactoryTest : TestCase
	{
		public void TestGlowServiceClientFactory_CanCreateMultipleClients()
		{
			Uri uri = new Uri("http://test.tst");
			factory.Create(uri).Dispose(); // dispose first client
			using (var client = factory.Create(uri))
			{
				AssertNoExceptionThrown(() => client.GetAsync("test").Wait());
			}
		}

		public void TestClearUserData_ExpiresCookies()
		{
			Uri uri1 = new Uri("http://test.tst");
			using (var client = factory.Create(uri1))
			{
				client.GetAsync("test").Wait();
				messageHandler.CookieContainer.Add(uri1, new Cookie("TestCookie1", "Value1"));
				messageHandler.CookieContainer.Add(uri1, new Cookie("TestCookie2", "Value2"));
				messageHandler.CookieContainer.Add(uri1, new Cookie("TestCookie3", "Value3"));
			}

			Uri uri2 = new Uri("http://google.com");
			using (var client = factory.Create(uri2))
			{
				client.GetAsync("test").Wait();
				messageHandler.CookieContainer.Add(uri2, new Cookie("GoogleCookie", "Value"));
			}

			Uri uri3 = new Uri("http://microsoft.com");
			using (var client = factory.Create(uri3))
			{
				client.GetAsync("test").Wait();
				messageHandler.CookieContainer.Add(uri3, new Cookie("MicrosoftCookie1", "Value1"));
				messageHandler.CookieContainer.Add(uri3, new Cookie("MicrosoftCookie2", "Value2"));
			}

			factory.ClearUserData();

			AssertEquals(0, messageHandler.CookieContainer.GetCookies(uri1).Count);
			AssertEquals(0, messageHandler.CookieContainer.GetCookies(uri2).Count);
			AssertEquals(0, messageHandler.CookieContainer.GetCookies(uri3).Count);
		}

		public void TestSessionType()
		{
			var userSessionAndRelatedControllers = new ConcurrentDictionary<string, (IUserSession UserSession, IAuthenticationController)>();
			var mockController = ReplaceOrCreateMockControllers(userSessionAndRelatedControllers, ActualUserKey);
			mockController.Setup(x => x.LoadAuthenticationStateAsync()).ReturnsAsync(default(SessionAuthenticationDetails));
			var factory = GetMockGlowServiceClientFactory(userSessionAndRelatedControllers);
			using (var client = factory.Create(BaseUri))
			{
				client.GetAsync("foo").Wait();
			}

			var beginSessionRequestJsonObject = JsonConvert.DeserializeObject<JObject>(actualBeginSessionRequestBody);
			AssertEquals(beginSessionRequestJsonObject["sessionType"].ToString(), "general");
		}

		public void TestClientHasCorrectSessionWhenInTempUserScope()
		{
			var userPk1 = new Guid("00000000-0000-0000-0000-000000000001");
			var branchPk1 = new Guid("00000000-0000-0000-0000-000000000002");
			var departmentPk1 = new Guid("00000000-0000-0000-0000-000000000003");
			var mockUserContext1 = CreateMockUserContext(userPk1, branchPk1, departmentPk1);

			var userPk2 = new Guid("00000000-0000-0000-0000-000000000004");
			var branchPk2 = new Guid("00000000-0000-0000-0000-000000000005");
			var departmentPk2 = new Guid("00000000-0000-0000-0000-000000000006");
			var mockUserContext2 = CreateMockUserContext(userPk2, branchPk2, departmentPk2);

			var actualUserPk = new Guid("00000000-0000-0000-0000-000000000007");
			var actualBranchPk = new Guid("00000000-0000-0000-0000-000000000008");
			var actualDepartmentPk = new Guid("00000000-0000-0000-0000-000000000009");

			var actualUserContext = CreateMockUserContext(actualUserPk, actualBranchPk, actualDepartmentPk);

			var userSessionAndRelatedControllers = new ConcurrentDictionary<string, (IUserSession UserSession, IAuthenticationController)>();
			var key1 = $"{userPk1}_{branchPk1}_{departmentPk1}";
			ReplaceOrCreateMockControllers(userSessionAndRelatedControllers, key1);
			var key2 = $"{userPk2}_{branchPk2}_{departmentPk2}";
			ReplaceOrCreateMockControllers(userSessionAndRelatedControllers, key2);
			ReplaceOrCreateMockControllers(userSessionAndRelatedControllers, ActualUserKey);

			Env.SetUserContext(actualUserContext);

			var factory = GetMockGlowServiceClientFactory(userSessionAndRelatedControllers);

			using (Env.SetTemporaryUserContext(mockUserContext1))
			{
				using (Env.SetTemporaryUserContext(mockUserContext2))
				{
					using (var client = factory.Create(BaseUri))
					{
						client.GetAsync("foo").Wait();
					}
					AssertEquals(userSessionAndRelatedControllers[key2].UserSession.AuthenticationToken, $"{userPk2}+{branchPk2}+{departmentPk2}");
				}

				using (var client = factory.Create(BaseUri))
				{
					client.GetAsync("foo").Wait();
				}
				AssertEquals(userSessionAndRelatedControllers[key1].UserSession.AuthenticationToken, $"{userPk1}+{branchPk1}+{departmentPk1}");
			}

			using (var client = factory.Create(BaseUri))
			{
				client.GetAsync("foo").Wait();
			}
			AssertEquals(userSessionAndRelatedControllers[ActualUserKey].UserSession.AuthenticationToken, PersistentToken);
		}

		public void TestClientHasCorrectSessionWhenInMasterTempUserScope()
		{
			var userPk = new Guid("00000000-0000-0000-0000-000000000001");
			var branchPk = new Guid("00000000-0000-0000-0000-000000000002");
			var departmentPk = new Guid("00000000-0000-0000-0000-000000000003");

			var mockUserContext = CreateMockUserContext(userPk, branchPk, departmentPk);

			var userSessionAndRelatedControllers = new ConcurrentDictionary<string, (IUserSession UserSession, IAuthenticationController)>();
			ReplaceOrCreateMockControllers(userSessionAndRelatedControllers, ActualUserKey);
			var factory = GetMockGlowServiceClientFactory(userSessionAndRelatedControllers);

			using (Env.Instance.SetTemporaryMasterUserContext(mockUserContext))
			{
				ReplaceOrCreateMockControllers(userSessionAndRelatedControllers, ActualUserKey);
				using (var client = factory.Create(BaseUri))
				{
					client.GetAsync("foo").Wait();
				}
				Assert(!userSessionAndRelatedControllers.ContainsKey($"{userPk}_{branchPk}_{departmentPk}"));
				AssertEquals(userSessionAndRelatedControllers[ActualUserKey].UserSession.AuthenticationToken, PersistentToken);
				AssertEquals(userSessionAndRelatedControllers[ActualUserKey].UserSession.SessionId, PersistentSessionId);
			}

			ReplaceOrCreateMockControllers(userSessionAndRelatedControllers, ActualUserKey);
			using (var client = factory.Create(BaseUri))
			{
				client.GetAsync("foo").Wait();
			}
			AssertEquals(userSessionAndRelatedControllers[ActualUserKey].UserSession.AuthenticationToken, PersistentToken);
		}

		IGlowServiceClientFactory factory;
		StubMessageHandler messageHandler;

		protected override void SetUp()
		{
			base.SetUp();

			messageHandler = new StubMessageHandler();
			var httpClient = new HttpClient(messageHandler);

			var details = default(SessionAuthenticationDetails);

			var mockAuthenticationController = new Mock<IAuthenticationController>();
			mockAuthenticationController.Setup(x => x.LoadAuthenticationStateAsync()).ReturnsAsync(details);

			var userSessionAndRelatedControllers = new ConcurrentDictionary<string, (IUserSession, IAuthenticationController)>();
			userSessionAndRelatedControllers[ActualUserKey] = (new UserSession(SessionType.General), mockAuthenticationController.Object);
			factory = new GlowServiceClientFactory(messageHandler, userSessionAndRelatedControllers);
			actualBeginSessionRequestBody = string.Empty;
		}

		public static GlowServiceClientFactory GetMockGlowServiceClientFactory(ConcurrentDictionary<string, (IUserSession, IAuthenticationController)> userSessionAndRelatedControllers = null)
		{
			var mockHttpClientHandler = new Mock<HttpClientHandler>(MockBehavior.Strict);
			mockHttpClientHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(requestMessage => requestMessage.RequestUri.AbsolutePath == "/auth/v2/credential/claim"), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("{\"result\":0,\"token\":\"QAQ\"}", Encoding.UTF8, "application/json"),
				});

			mockHttpClientHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(requestMessage => requestMessage.RequestUri.AbsolutePath == "/auth/v2/session/begin"), ItExpr.IsAny<CancellationToken>())
				.Callback((HttpRequestMessage requestMessage, CancellationToken _) =>
				{
					actualBeginSessionRequestBody = requestMessage.Content.ReadAsStringAsync().Result;
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("{\"result\":0}", Encoding.UTF8, "application/json"),
				}
				);

			mockHttpClientHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(requestMessage => requestMessage.RequestUri.AbsolutePath == "/foo"), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
				{
					return new HttpResponseMessage(HttpStatusCode.OK);
				});

			var factory = new GlowServiceClientFactory(mockHttpClientHandler.Object, userSessionAndRelatedControllers);

			var mockGlowSingleSignOnTokenProvider = new Mock<IGlowSingleSignOnTokenProvider>();
			mockGlowSingleSignOnTokenProvider
				.Setup(x => x.CreateLimitedToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<GlowSingleSignOnTokenOptions>()))
				.Returns((Guid userPK, string _, GlowSingleSignOnTokenOptions options) =>
				{
					return $"{userPK}+{options.BranchPK}+{options.DepartmentPK}";
				});
			ObjectFactory.Substitute("IGlowSingleSignOnTokenProvider", mockGlowSingleSignOnTokenProvider.Object);
			ObjectFactory.Substitute("IGlowServiceClientFactory", factory);

			return factory;
		}

		static Mock<IAuthenticationController> ReplaceOrCreateMockControllers(ConcurrentDictionary<string, (IUserSession UserSession, IAuthenticationController AuthenticationController)> userSessionAndRelatedControllers, string key)
		{
			var mockUserSession = new UserSession(SessionType.General);

			if (userSessionAndRelatedControllers.TryGetValue(key, out var value))
			{
				mockUserSession = (UserSession)value.UserSession;
			}
			var mockAuthenticationController = new Mock<IAuthenticationController>(MockBehavior.Strict);

			var persistToFile = key == ActualUserKey;

			mockAuthenticationController
				.Setup(x => x.LoadAuthenticationStateAsync())
				.ReturnsAsync(() => new SessionAuthenticationDetails
				{
					SessionId = persistToFile ? PersistentSessionId : Guid.NewGuid(),
					AuthenticationTicketHeader = "Test",
					AuthenticationToken = persistToFile ? PersistentToken : $"{Env.CurrentUserPK}+{Env.CurrentBranchPK}+{Env.CurrentDepartmentPK}",
				});
			mockAuthenticationController
				.Setup(x => x.AuthenticateAsync(It.IsAny<IClientSessionService>()))
				.Callback<IClientSessionService>(clientSessionService =>
				{
					clientSessionService.BeginSessionWithCredentialsAsync("username", "password").Wait();
				})
				.Returns(() =>
				{
					mockUserSession.AuthenticationToken = $"{Env.CurrentUserPK}+{Env.CurrentBranchPK}+{Env.CurrentDepartmentPK}";
					return Task.CompletedTask;
				});
			mockAuthenticationController
				.Setup(x => x.SaveAuthenticationStateAsync(It.IsAny<SessionAuthenticationDetails>()))
				.Returns(Task.CompletedTask);

			userSessionAndRelatedControllers[key] = (mockUserSession, mockAuthenticationController.Object);
			return mockAuthenticationController;
		}

		IUserContext CreateMockUserContext(Guid userPk, Guid branchPk, Guid departmentPk)
		{
			var mockUserContext = new Mock<IUserContext>();
			var mockUser = new Mock<IUser>();
			mockUser.Setup(x => x.PK).Returns(userPk);
			mockUserContext.Setup(x => x.User).Returns(mockUser.Object);

			var mockBranch = new Mock<IBranch>();
			mockBranch.Setup(x => x.PK).Returns(branchPk);
			mockUserContext.Setup(x => x.Branch).Returns(mockBranch.Object);

			var mockDepartment = new Mock<IDepartment>();
			mockDepartment.Setup(x => x.PK).Returns(departmentPk);
			mockUserContext.Setup(x => x.Department).Returns(mockDepartment.Object);

			return mockUserContext.Object;
		}

		sealed class StubMessageHandler : HttpClientHandler
		{
			public Func<HttpStatusCode> ResponseStatusGetter { get; set; }

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var result = new HttpResponseMessage();
				if (ResponseStatusGetter != null)
				{
					result.StatusCode = ResponseStatusGetter();
				}

				return Task.FromResult(result);
			}
		}

		static readonly Uri BaseUri = new Uri("http://test.tst");
		static string actualBeginSessionRequestBody = string.Empty;
		static readonly Guid PersistentSessionId = Guid.NewGuid();
		const string PersistentToken = "PersistentToken";
		const string ActualUserKey = "ActualUserKey";
	}
}
