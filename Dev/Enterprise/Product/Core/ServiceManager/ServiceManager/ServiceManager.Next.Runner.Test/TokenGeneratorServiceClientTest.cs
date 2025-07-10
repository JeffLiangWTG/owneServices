using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Runner.Test;

class TokenGeneratorServiceClientTest : BaseTokenClientTest<TokenGeneratorServiceClient>
{
	Mock<ITokenGeneratorService>? tokenGeneratorServiceMock;

	[SetUp]
	public void SetUp()
	{
		tokenGeneratorServiceMock = new Mock<ITokenGeneratorService>(MockBehavior.Strict);
	}

	[TearDown]
	public void TearDown()
	{
		tokenGeneratorServiceMock?.VerifyNoOtherCalls();
	}

	protected override ServiceCollection BuildServiceCollection()
	{
		var serviceCollection = base.BuildServiceCollection();
		serviceCollection.AddScoped(sp => tokenGeneratorServiceMock!.Object);
		return serviceCollection;
	}

	[Test]
	public async Task RegisterTokenService_SignCwTokenAsync()
	{
		tokenGeneratorServiceMock!
			.Setup(x => x.SignCwTokenAsync(It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new SignCwTokenResponse("Test Token"));

		await RegisterTokenClientAndStartSignalR();

		var request = new SignCwTokenRequest("Test Audience");
		var response = await SendSignalRRequest<SignCwTokenRequest, SignCwTokenResponse>("SignCwToken", request);

		Assert.That(response, Is.EqualTo(new SignCwTokenResponse("Test Token")));

		tokenGeneratorServiceMock.Verify(m => m.SignCwTokenAsync(request, cancellationTokenSource!.Token), Times.Once());
		hostApplicationLifetimeMock!.Verify(m => m.ApplicationStopping, Times.Once());
	}
}
