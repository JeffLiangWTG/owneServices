using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.ServiceManager.Next.Launcher.Test.Fixture;
using CargoWise.ServiceManager.Next.Shared;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Enterprise.ServiceManager.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using AuthenticationSchemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

[UseSnapshotProtection]
class ProgramTest : TestCaseWithFactory
{
	CancellationTokenSource? cancellationTokenSource;
	WebApplicationFactory<Program>? factory;
	HttpClient? client;
	Mock<IProcessFactory>? processFactoryMock;
	Mock<IProcess>? processMock;
	TestNextLauncherOptions? testNextLauncherOptions;
	TestLoggerProvider? testLoggerProvider;
	WebApplicationFactory<Program>? webApplicationFactory;

	protected override void MasterSetUp()
	{
		base.MasterSetUp();
		// This configuration is required on DAT to avoid a System.InvalidOperationException: "Solution root could not be located using application root"
		// We cannot do it in ConfigureWebHost as it is resolved before WithWebHostBuilder action is called
		// To reproduce you need to remove MvcTestingAppManifest.json from the output folder (the file is generated during test compilation)
		// https://github.com/dotnet/aspnetcore/blob/9755a3af20e449aaec9c1fb9c933ac86160a469e/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L216
		// https://github.com/dotnet/aspnetcore/blob/9755a3af20e449aaec9c1fb9c933ac86160a469e/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L279
		var assemblyName = typeof(Program).Assembly.GetName().Name!;
		var settingSuffix = assemblyName.ToUpperInvariant().Replace(".", "_");
		var settingName = $"ASPNETCORE_TEST_CONTENTROOT_{settingSuffix}";
		Environment.SetEnvironmentVariable(settingName, AppContext.BaseDirectory, EnvironmentVariableTarget.Process);
	}

	protected override void SetUp()
	{
		base.SetUp();
		cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		processMock = new Mock<IProcess>(MockBehavior.Loose);
		processMock.Setup(m => m.Start()).Returns(true);
		processFactoryMock = new Mock<IProcessFactory>(MockBehavior.Strict);
		processFactoryMock.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>())).Returns(processMock.Object);
		testNextLauncherOptions = new TestNextLauncherOptions();
		testLoggerProvider = new TestLoggerProvider();
		webApplicationFactory = new WebApplicationFactory<Program>();
		factory = webApplicationFactory.WithWebHostBuilder(builder => ConfigureWebHost(builder, processFactoryMock.Object, testNextLauncherOptions, testLoggerProvider));
		client = factory.CreateClient();
	}

	protected override void TearDown()
	{
		client?.Dispose();
		factory?.Dispose();
		webApplicationFactory?.Dispose();
		testLoggerProvider?.Dispose();
		cancellationTokenSource?.Dispose();
		processFactoryMock?.VerifyNoOtherCalls();
		base.TearDown();
	}

	public void Test_WtgEndpointAsync_ValidMethod_ReadyGet()
	{
		WtgEndpointAsync_ValidMethodCore("/wtg/ready", HttpMethod.Get);
	}

	public void Test_WtgEndpointAsync_ValidMethod_HeadGet()
	{
		WtgEndpointAsync_ValidMethodCore("/wtg/ready", HttpMethod.Head);
	}

	public void Test_WtgEndpointAsync_ValidMethod_StatusGet()
	{
		WtgEndpointAsync_ValidMethodCore("/wtg/status", HttpMethod.Get);
	}

	public void Test_WtgEndpointAsync_ValidMethod_StatusHead()
	{
		WtgEndpointAsync_ValidMethodCore("/wtg/status", HttpMethod.Head);
	}

	void WtgEndpointAsync_ValidMethodCore(string endpoint, HttpMethod httpMethod)
	{
		AssertNotNull(client);
		using var httpRequestMessage = new HttpRequestMessage(httpMethod, endpoint);
		var response = client!.SendAsync(httpRequestMessage, cancellationTokenSource!.Token).Result;
		AssertEquals($"Unexpected response: {response}", HttpStatusCode.OK, response.StatusCode);
		var content = response.Content.ReadAsStringAsync().Result;
		AssertNullOrEmpty(content);
	}

	public void Test_WtgEndpointAsync_InvalidMethod_ReadyDelete()
	{
		WtgEndpointAsync_InvalidMethodCore("/wtg/ready", HttpMethod.Delete);
	}

	public void Test_WtgEndpointAsync_InvalidMethod_ReadyPost()
	{
		WtgEndpointAsync_InvalidMethodCore("/wtg/ready", HttpMethod.Post);
	}

	public void Test_WtgEndpointAsync_InvalidMethod_ReadyTrace()
	{
		WtgEndpointAsync_InvalidMethodCore("/wtg/ready", HttpMethod.Trace);
	}

	public void Test_WtgEndpointAsync_InvalidMethod_StatusDelete()
	{
		WtgEndpointAsync_InvalidMethodCore("/wtg/status", HttpMethod.Delete);
	}

	public void Test_WtgEndpointAsync_InvalidMethod_StatusPost()
	{
		WtgEndpointAsync_InvalidMethodCore("/wtg/status", HttpMethod.Post);
	}

	public void Test_WtgEndpointAsync_InvalidMethod_StatusTrace()
	{
		WtgEndpointAsync_InvalidMethodCore("/wtg/status", HttpMethod.Trace);
	}

	void WtgEndpointAsync_InvalidMethodCore(string endpoint, HttpMethod httpMethod)
	{
		AssertNotNull(client);
		using var httpRequestMessage = new HttpRequestMessage(httpMethod, endpoint);
		var response = client!.SendAsync(httpRequestMessage, cancellationTokenSource!.Token).Result;
		AssertEquals($"Unexpected response: {response}", HttpStatusCode.MethodNotAllowed, response.StatusCode);
		var content = response.Content.ReadAsStringAsync().Result;
		AssertNullOrEmpty(content);
	}

	public void Test_PostNextTokenSignCw_Audience()
	{
		PostNextTokenSignCw(audience => $$"""
		{"Audience":"{{audience}}"}
		""");
	}

	public void Test_PostNextTokenSignCw_AudienceAndOther()
	{
		PostNextTokenSignCw(audience => $$"""
		{"Audience":"{{audience}}","Other":"Value"}
		""");
	}

	void PostNextTokenSignCw(Func<string, string> queryBuilder)
	{
		var audience = Guid.NewGuid().ToString();
		SendRequestAndCheckResponse(
			"signCW",
			queryBuilder(audience),
			(request) => Task.FromResult(new SignCwTokenResponse(token: $"Mock token for {request.Audience}")),
			"SignCwToken",
			new SignCwTokenRequest(audience),
			$$"""
			  {"token":"Mock token for {{audience}}"}
			  """
		).Wait();
	}

	public void Test_PostNextPrepareNewCertificateAsync()
	{
		SendRequestAndCheckResponse(
			"prepareNewCertificate",
			"{}",
			(request) => Task.FromResult(new PrepareNewCertificateResponse($"Mock CSR for {request}")),
			"PrepareNewCertificate",
			new PrepareNewCertificateRequest(),
			"""
				{"csr":"Mock CSR for PrepareNewCertificateRequest { }"}
				"""
		).Wait();
	}

	public void Test_PostNextResetAccessTokenAsync()
	{
		SendRequestAndCheckResponse(
			"resetAccessToken",
			"{}",
			(request) => Task.FromResult(new ResetAccessTokenResponse()),
			"ResetAccessToken",
			new ResetAccessTokenRequest(),
			"{}"
		).Wait();
	}

	public void Test_PostNextSetNewCertificateCredentialsAsync()
	{
		var operationId = Guid.NewGuid().ToString();
		var tenantId = Guid.NewGuid().ToString();
		var clientId = Guid.NewGuid().ToString();
		var certificate = Guid.NewGuid().ToByteArray();
		var json = $$"""
		{
			"OperationId":"{{operationId}}",
			"TenantId":"{{tenantId}}",
			"ClientId":"{{clientId}}",
			"Certificate":"{{Convert.ToBase64String(certificate)}}"
		}
		""";

		SendRequestAndCheckResponse(
			"setNewCertificateCredentials",
			json,
			(request) => Task.FromResult(new SetNewCertificateCredentialsResponse()),
			"SetNewCertificateCredentials",
			new SetNewCertificateCredentialsRequest(operationId, tenantId, clientId, certificate),
			"{}"
		).Wait();
	}

	public void Test_PostNextSetOperationIdAsync()
	{
		var csr = Guid.NewGuid().ToString();
		var operationId = Guid.NewGuid().ToString();
		var json = $$"""
		{
			"Csr":"{{csr}}",
			"OperationId":"{{operationId}}"
		}
		""";
		SendRequestAndCheckResponse(
			"setOperationId",
			json,
			(request) => Task.FromResult(new SetOperationIdResponse()),
			"SetOperationId",
			new SetOperationIdRequest(csr, operationId),
			"{}"
		).Wait();
	}

	async Task SendRequestAndCheckResponse<TRequest, TResponse>(string endpoint, string query, Func<TRequest, Task<TResponse>> callback, string expectedMethod, TRequest expectedRequest, string expectedResponse)
	{
		var requests = new List<TRequest>();
		var responses = new List<TResponse?>();
		var testServer = factory!.Server;
		bool hasExited = false;
		bool hasBeenDisposed = false;
		await using var hubConnection = new HubConnectionBuilder()
			.WithUrl(
				new Uri(client!.BaseAddress!, testNextLauncherOptions!.LauncherHubEndpoint),
				o => o.HttpMessageHandlerFactory = handler => testServer.CreateHandler())
			.Build();
		var pid = TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
		processMock!.Setup(m => m.Id).Returns(pid);
		processMock!.Setup(m => m.HasExited).Returns(() => hasExited);
		processMock!.Setup(m => m.Dispose()).Callback(() => hasBeenDisposed = true);
		hubConnection.On("Close", () =>
		{
			hasExited = true;
			return hasExited;
		});
		hubConnection.On<int>("ProcessId", () => pid);
		hubConnection.On<TRequest, TResponse>(expectedMethod, async request =>
		{
			requests.Add(request);
			var response = await callback(request);
			responses.Add(response);
			return response;
		});
		await hubConnection.StartAsync(cancellationTokenSource!.Token).ConfigureAwait(false);
		AssertNotNull(client);
		AssertNotNull(processFactoryMock);
		await SetAuthorizationFromNewlyCreatedDbTokenAsync(client, cancellationTokenSource.Token);
		var runnerCode = "token";
		using var requestBody = CreateContentForPostRequest(query);

		var response = await client!.PostAsync($"api/{runnerCode}/{endpoint}", requestBody, cancellationTokenSource!.Token);
		AssertEquals(
			$"Unexpected response: {response}.{Environment.NewLine}Full logs:{Environment.NewLine}{string.Join(Environment.NewLine, testLoggerProvider!.LogMessages)}",
			HttpStatusCode.OK,
			response.StatusCode);
		var content = await response.Content.ReadAsByteArrayAsync();
		AssertEquals(expectedResponse, Encoding.UTF8.GetString(content));
		AssertEquals("application/json", response.Content.Headers.ContentType?.MediaType);
		processFactoryMock!.Verify(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()), Times.Once);
		processMock.Verify(m => m.Id, Times.AtLeastOnce);
		Assert("Process was not disposed in time.", SpinWait.SpinUntil(() => hasBeenDisposed, TimeSpan.FromSeconds(5)));
		Assert("Process was not closed gracefully.", hasExited);

		processMock!.Invocations.Clear(); // this is to ensure we do not have issue due to loose mock
		AssertContainsExactElementsInExactOrder(new[] { expectedRequest }, requests);

		var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(Encoding.UTF8.GetString(content));
		AssertNotNull(responseObj);
		AssertContainsExactElementsInExactOrder("There was an issue with deserialization", responses, new[] { responseObj });
	}

	public void Test_PostInvalidNextRequest_TokenSignCw()
	{
		SetAuthorizationFromNewlyCreatedDbToken();
		PostInvalidNextRequestCore("token", "signCW", "{}", "Audience:[The Audience field is required.]", HttpStatusCode.BadRequest);
	}

	public void Test_PostInvalidNextRequest_ApiKeyMissing()
	{
		PostInvalidNextRequestCore("token", "signCW", "{}", "Api key is missing.", HttpStatusCode.Unauthorized);
		PostInvalidNextRequestCore("token", "setOperationId", "{}", "Api key is missing.", HttpStatusCode.Unauthorized);
		PostInvalidNextRequestCore("token", "prepareNewCertificate", "{}", "Api key is missing.", HttpStatusCode.Unauthorized);
		PostInvalidNextRequestCore("token", "resetAccessToken", "{}", "Api key is missing.", HttpStatusCode.Unauthorized);
		PostInvalidNextRequestCore("token", "setNewCertificateCredentials", "{}", "Api key is missing.", HttpStatusCode.Unauthorized);
	}

	public void Test_PostInvalidNextRequest_SignCw_NoAudience()
	{
		SetAuthorizationFromNewlyCreatedDbToken();
		var json = """
			{"Aud":"123"}
			""";
		PostInvalidNextRequestCore("token", "signCW", json, "Audience:[The Audience field is required.]", HttpStatusCode.BadRequest);
	}

	public void Test_PostInvalidNextRequest_SetOperationId_NoCsr()
	{
		SetAuthorizationFromNewlyCreatedDbToken();
		var json = """
			{"CertificateSigningRequest":"Csr","OperationId":"123"}
			""";
		PostInvalidNextRequestCore("token", "setOperationId", json, "Csr:[The Csr field is required.]", HttpStatusCode.BadRequest);
	}

	public void Test_PostInvalidNextRequest_SetNewCertificateCredentials_NoCertificate()
	{
		SetAuthorizationFromNewlyCreatedDbToken();
		var json = """
			{"TenantId":"tId","OperationId":"oId","ClientId":"cId"}
			""";
		PostInvalidNextRequestCore("token", "setNewCertificateCredentials", json, "Certificate:[The Certificate field is required.]", HttpStatusCode.BadRequest);
	}

	public void Test_PostInvalidNextRequest_SetNewCertificateCredentials_NotBase64Certificate()
	{
		SetAuthorizationFromNewlyCreatedDbToken();
		var json = """
			{"TenantId":"tId","OperationId":"oId","ClientId":"cId","Certificate":"NotBase64"}
			""";
		PostInvalidNextRequestCore("token", "setNewCertificateCredentials", json, "Certificate:[The Certificate field is not a valid Base64 encoding.]", HttpStatusCode.BadRequest);
	}

	public void Test_PostInvalidNextRequest_UnknownSlug()
	{
		SetAuthorizationFromNewlyCreatedDbToken();
		var audience = Guid.NewGuid().ToString();
		PostInvalidNextRequestCore("token", $"unknown?audience={audience}", "{}", $"Invalid request: token/unknown?audience={audience}.", HttpStatusCode.InternalServerError);
	}

	public void Test_PostInvalidNextRequest_UnknownRunnerCode()
	{
		SetAuthorizationFromNewlyCreatedDbToken();
		var audience = Guid.NewGuid().ToString();
		PostInvalidNextRequestCore("unknown", $"signCW?audience={audience}", "{}", $"Invalid request: unknown/signCW?audience={audience}.", HttpStatusCode.InternalServerError);
	}

	void PostInvalidNextRequestCore(string runnerCode, string endpoint, string json, string expectedError, HttpStatusCode expectedCode)
	{
		AssertNotNull(client);
		AssertNotNull(processFactoryMock);
		var logFile = expectedCode == HttpStatusCode.InternalServerError ? GetLogFileNameAndDeleteContentIfExists() : null;
		using var requestBody = CreateContentForPostRequest(json);
		var requestUri = $"api/{runnerCode}/{endpoint}";
		var response = client!.PostAsync(requestUri, requestBody, cancellationTokenSource!.Token).Result;
		CombineAssertions(
			$"Error for {requestUri}: {json}",
			() =>
			{
				AssertEquals($"Unexpected response: {response}", expectedCode, response.StatusCode);
				var content = response.Content.ReadAsStringAsync().Result;
				if (string.IsNullOrEmpty(expectedError))
				{
					AssertEquals("Content should be empty", expectedError, content);
				}
				else if (expectedCode == HttpStatusCode.BadRequest)
				{
					HttpValidationProblemDetails? problemDetails = null;
					AssertNoExceptionThrown($"cannot deserialize {content}", () => problemDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<HttpValidationProblemDetails>(content));
					AssertNotNull(problemDetails);
					AssertContainsExactElementsInExactOrder(new[] { expectedError }, problemDetails?.Errors?.Select(x => $"{x.Key}:[{string.Join(",", x.Value)}]"));
				}
				else
				{
					ProblemDetails? problemDetails = null;
					AssertNoExceptionThrown($"cannot deserialize {content}", () => problemDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<ProblemDetails>(content));
					AssertNotNull($"{nameof(problemDetails)} is null when deserializing {content}", problemDetails);
					if (expectedCode == HttpStatusCode.InternalServerError)
					{
						AssertLogToFileSystem(logFile!, expectedError);
					}
					else
					{
						AssertNotNull($"{nameof(problemDetails)}.{nameof(problemDetails.Detail)} is null when deserializing {content}", problemDetails?.Detail);
						AssertStartsWith($"Content should start with expected error but was {content}", expectedError, problemDetails?.Detail);
					}
				}
				processFactoryMock!.Verify(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()), Times.Never);
			});
	}

	static HttpContent CreateContentForPostRequest(string json)
	{
		var result = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
		result.Headers.ContentType = new MediaTypeHeaderValue("application/json")
		{
			CharSet = "utf-8",
		};
		return result;
	}

	public void Test_CreateNextHub()
	{
		var testServer = factory!.Server;
		var pid = TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
		var hubConnection = new HubConnectionBuilder()
			.WithUrl(
				new Uri(client!.BaseAddress!, testNextLauncherOptions!.LauncherHubEndpoint),
				o => o.HttpMessageHandlerFactory = handler => testServer.CreateHandler())
			.Build();
		hubConnection.On<int>("ProcessId", () => pid);
		hubConnection.On("Close", async () =>
		{
			await hubConnection.StopAsync(cancellationTokenSource!.Token);
			return true;
		});
		hubConnection.StartAsync(cancellationTokenSource!.Token);
		SpinWait.SpinUntil(() => hubConnection.State == HubConnectionState.Connected || cancellationTokenSource.Token.IsCancellationRequested, TimeSpan.FromSeconds(10));
		AssertEquals(HubConnectionState.Connected, hubConnection.State);

		var commandSender = testServer.Services.GetRequiredService<ICommandSender>();
		var nextProcessRunnerMock = new Mock<INextProcessRunner>(MockBehavior.Strict);
		nextProcessRunnerMock.Setup(x => x.ProcessId).Returns(pid);
		nextProcessRunnerMock.Setup(x => x.HasExited).Returns(() => hubConnection.State == HubConnectionState.Disconnected);
		nextProcessRunnerMock.Setup(x => x.Dispose());
		nextProcessRunnerMock.Setup(x => x.RunnerCode).Returns("token");
		do
		{
			var closeRunnerTask = commandSender.CloseRunnerAsync(nextProcessRunnerMock.Object, cancellationTokenSource!.Token);
			SpinWait.SpinUntil(() => closeRunnerTask.IsCompleted || cancellationTokenSource.Token.IsCancellationRequested, TimeSpan.FromSeconds(10));
			Assert("CloseRunnerAsync should have completed in time.", closeRunnerTask.IsCompleted);
		} while (!cancellationTokenSource.Token.IsCancellationRequested && hubConnection.State != HubConnectionState.Disconnected);
		AssertEquals(HubConnectionState.Disconnected, hubConnection.State);

		var disposeTask = hubConnection.DisposeAsync();
		SpinWait.SpinUntil(() => disposeTask.IsCompleted || (cancellationTokenSource?.Token.IsCancellationRequested is true), TimeSpan.FromSeconds(10));
	}

	public void Test_CheckHttpSysOptions()
	{
		var testServer = factory!.Server;
		CombineAssertions(() =>
		{
			var httpSysOptions = testServer.Services.GetService<IOptions<HttpSysOptions>>();
			AssertNotNull(httpSysOptions);
			AssertContainsExactElementsInAnyOrder(new[] { testNextLauncherOptions!.ExpectedListenerPrefix, }, httpSysOptions?.Value.UrlPrefixes);
			AssertEquals("AllowAnonymous", true, httpSysOptions?.Value.Authentication.AllowAnonymous);
			AssertEquals(AuthenticationSchemes.None, httpSysOptions?.Value.Authentication.Schemes);

			var servers = testServer.Services.GetServices<IServer>();
			AssertCollectionContains("Microsoft.AspNetCore.Server.HttpSys.MessagePump", servers.Select(x => x.GetType().ToString()));
		});
	}

	public void TestLogger_Program_LogToFileSystem()
	{
		var logger = factory!.Server.Services.GetRequiredService<ILogger<Program>>();
		Check_LogToFileSystem(logger);
	}

	public void TestLogger_Default_LogToFileSystem()
	{
		var logger = factory!.Server.Services.GetRequiredService<ILogger>();
		Check_LogToFileSystem(logger);
	}

	public void TestLogger_LoggerFactory_LogToFileSystem()
	{
		var loggerFactory = factory!.Server.Services.GetRequiredService<ILoggerFactory>();
		var logger = loggerFactory.CreateLogger("TestLogger");
		Check_LogToFileSystem(logger);
	}

	void Check_LogToFileSystem(ILogger logger)
	{
		var logFileName = GetLogFileNameAndDeleteContentIfExists();

		AssertEquals("Logger should be enabled", true, logger.IsEnabled(LogLevel.Information));
		logger.LogInformation("This is a test log from {TestCase}.", Name);
		var expected = $"This is a test log from {Name}.";
		AssertLogToFileSystem(logFileName, expected);
	}

	static string GetLogFileNameAndDeleteContentIfExists()
	{
		var logDir = ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName);
		var logFileName = Path.Combine(logDir, $"WEB_{DateTime.UtcNow:yyyyMMdd}.txt");
		if (File.Exists(logFileName))
		{
			File.Delete(logFileName);
		}

		return logFileName;
	}

	void AssertLogToFileSystem(string logFileName, string expected)
	{
		Assert("Log file does not exists", File.Exists(logFileName));
		using var stream = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var sr = new StreamReader(stream);
		var content = sr.ReadToEnd();
		AssertContains("Log file does not contains expected value", expected, content);
	}

	static void ConfigureWebHost(IWebHostBuilder builder, IProcessFactory processFactoryOverride, INextLauncherOptions nextLauncherOptions, ILoggerProvider extraLoggerProvider)
	{
		if (bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting) && datIsTesting)
		{
			// This configuration is required to avoid a System.IO.DirectoryNotFoundException when creating the host on DAT
			// This is because we do not have access to the source and .NET is doing an optimization in development environment to load assets
			// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/?view=aspnetcore-8.0&tabs=windows#content-root
			// https://github.com/dotnet/aspnetcore/blob/79ef5e329b1e31c3775a1977798253cc8f7da6cc/src/DefaultBuilder/src/WebHost.cs#L219C8-L225C12
			builder.UseEnvironment("DAT");
		}

		builder.ConfigureLogging(logging => logging.AddProvider(extraLoggerProvider));

		builder.ConfigureTestServices(services =>
		{
			OverrideServices<IProcessFactory>(services, ServiceLifetime.Singleton, sp => processFactoryOverride);
			OverrideServices<INextLauncherOptions>(services, ServiceLifetime.Singleton, sp => nextLauncherOptions);
		});
	}

	static void OverrideServices<TService>(IServiceCollection services, ServiceLifetime serviceLifetime, Func<IServiceProvider, object> implementationFactory)
	{
		var existingServiceDescriptor = services.SingleOrDefault(e => e.ServiceType == typeof(TService));
		AssertEquals(existingServiceDescriptor?.Lifetime, serviceLifetime);

		var overrideServiceDescriptor = new ServiceDescriptor(typeof(TService), implementationFactory, serviceLifetime);
		services.Add(overrideServiceDescriptor);
	}

	void SetAuthorizationFromNewlyCreatedDbToken()
	{
		SetAuthorizationFromNewlyCreatedDbTokenAsync(client!, cancellationTokenSource!.Token).GetAwaiter().GetResult();
	}

	static async Task SetAuthorizationFromNewlyCreatedDbTokenAsync(HttpClient client, CancellationToken cancellationToken)
	{
		var dbAccessTokenRetriever = ObjectFactory.Get<IDbAccessTokenRetriever>();
		AssertNotNull(nameof(dbAccessTokenRetriever), dbAccessTokenRetriever);
		// Read token from DB until we have a value or the cancellationToken is cancelled
		string? accessToken;
		do
		{
			accessToken = await GetNewlyCreatedDbToken(dbAccessTokenRetriever, cancellationToken);
			if (accessToken is null)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(100), CancellationToken.None);
			}
		} while (accessToken is null && !cancellationToken.IsCancellationRequested);
		Assert("TokenCreationService background worker should have generated a valid token", accessToken is not null);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", accessToken);
	}

	static async Task<string?> GetNewlyCreatedDbToken(IDbAccessTokenRetriever dbAccessTokenRetriever, CancellationToken cancellationToken)
	{
		using var disposableActionForDb = Db.DisposableActionForDbConnection();
		return await dbAccessTokenRetriever.GetDbAccessTokenAsync(cancellationToken);
	}

	sealed class TestLoggerProvider : ILoggerProvider
	{
		readonly List<string> logMessages = [];
		readonly Dictionary<string, TestLogger> loggers = new();
		public IReadOnlyList<string> LogMessages => logMessages;

		public void Dispose()
		{
			loggers.Clear();
			logMessages.Clear();
		}

		public ILogger CreateLogger(string categoryName)
		{
			if (loggers.TryGetValue(categoryName, out var logger))
			{
				return logger;
			}

			logger = new TestLogger(categoryName, logMessages);
			loggers.Add(categoryName, logger);
			return logger;
		}

		sealed class TestLogger(string categoryName, ICollection<string> logMessages) : ILogger
		{
			public IDisposable? BeginScope<TState>(TState state) where TState : notnull
			{
				logMessages.Add($"[{categoryName}] BeginScope: {state}");
				return new DisposableAction(() => logMessages.Add($"[{categoryName}] EndScope: {state}"));
			}

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
			{
				var message = formatter(state, exception);
				if (exception is not null)
				{
					message += Environment.NewLine + exception;
				}
				logMessages.Add($"[{categoryName}] {logLevel}: {message}");
			}

			sealed class DisposableAction(Action action) : IDisposable
			{
				public void Dispose() => action();
			}
		}
	}
}

