using CargoWise.EntityFramework.Testing;
using CargoWise.ServiceManager.Next.Runner.Test.Fixture;
using CargoWise.ServiceManager.Next.Runner.TokenSigning;
using CargoWise.SystemToSystemTrust.DataContracts;
using Enterprise.Registry.Business;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CargoWise.ServiceManager.Next.Runner.Test.TokenSigning;

class TokenGeneratorServiceTest : TestCaseWithFactory
{
	CancellationTokenSource? cancellationTokenSource;
	Mock<IHttpClientFactory>? httpClientFactoryMock;

	protected override void SetUp()
	{
		base.SetUp();
		httpClientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
		cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
	}

	protected override void TearDown()
	{
		cancellationTokenSource?.Dispose();
		base.TearDown();
		httpClientFactoryMock?.VerifyNoOtherCalls();
	}

	public void TestSignCwTokenAsync_RegistrySet_GenerateValidToken()
	{
		var audience = Guid.NewGuid().ToString();
		var request = new SignCwTokenRequest(audience);
		using var oauthClientAssertionFixture = new OAuthClientAssertionFixture(audience);
		using var registryOverride = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oauthClientAssertionFixture.SystemToSystemTrustInfo);
		using var client = oauthClientAssertionFixture.CreateFakeHttpClient();
		httpClientFactoryMock!.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(client);
		var response = CallAsync(() => SignCwTokenAsync(request, httpClientFactoryMock.Object, cancellationTokenSource!.Token));
		AssertNotNull(response);
		oauthClientAssertionFixture.VerifyCall(response!.Token);
		httpClientFactoryMock.Verify(m => m.CreateClient("TokenGeneratorService"), Times.Once());
	}

	public void TestSignCwTokenAsync_RegistryNotSet_Throws()
	{
		var audience = Guid.NewGuid().ToString();
		var request = new SignCwTokenRequest(audience);
		using var oauthClientAssertionFixture = new OAuthClientAssertionFixture(audience);
		var (companyPk, branchPk, departmentPk) = (Guid.Empty, Guid.Empty, Guid.Empty);
		var defaultValue = SystemDataRegistry.Instance.SystemToSystemCertificate.Inner.GetDefaultValue(companyPk, branchPk, departmentPk);
		using var registryOverride = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(companyPk, branchPk, departmentPk, defaultValue);
		using var client = oauthClientAssertionFixture.CreateFakeHttpClient();
		httpClientFactoryMock!.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(client);
		var exception = CallAsync<ArgumentException, SignCwTokenResponse>(() => SignCwTokenAsync(request, httpClientFactoryMock.Object, cancellationTokenSource!.Token));
		CombineAssertions(() =>
		{
			oauthClientAssertionFixture.VerifyNoCall();
			AssertNull(nameof(exception.ParamName), exception.ParamName);
			AssertEquals("Value cannot be null or empty. Parameter name: tokenEndpoint", exception.Message);
		});
	}

	public void TestSignCwTokenAsync_AudienceNotSet_Throws()
	{
		var audience = "";
		var request = new SignCwTokenRequest(audience);
		using var oauthClientAssertionFixture = new OAuthClientAssertionFixture(audience);
		using var registryOverride = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oauthClientAssertionFixture.SystemToSystemTrustInfo);
		using var client = oauthClientAssertionFixture.CreateFakeHttpClient();
		httpClientFactoryMock!.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
		var exception = CallAsync<ArgumentException, SignCwTokenResponse>(() => SignCwTokenAsync(request, httpClientFactoryMock.Object, cancellationTokenSource!.Token));
		CombineAssertions(() =>
		{
			oauthClientAssertionFixture.VerifyNoCall();
			AssertEquals("Value cannot be null or empty. Parameter name: aud", exception.Message);
		});
	}

	T? CallAsync<T>(Func<Task<T>> codeToRun)
	{
		T? res = default;

		AssertNoExceptionThrown(() => Task.Run(async () => res = await codeToRun()).GetAwaiter().GetResult());

		return res;
	}

	TException CallAsync<TException, TResult>(Func<Task<TResult>> codeToRun) where TException : Exception
	{
		return AssertExceptionThrown<TException>(() => Task.Run(async () => await codeToRun()).GetAwaiter().GetResult());
	}

	async Task<SignCwTokenResponse> SignCwTokenAsync(SignCwTokenRequest request, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken)
	{
		var tokenGeneratorService = new TokenGeneratorService(new NullLogger<TokenGeneratorService>(), httpClientFactory);
		return await tokenGeneratorService.SignCwTokenAsync(request, cancellationToken);
	}
}
