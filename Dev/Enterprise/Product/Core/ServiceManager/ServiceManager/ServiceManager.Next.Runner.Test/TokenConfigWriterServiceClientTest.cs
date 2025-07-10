using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Runner.Test;

class TokenConfigWriterServiceClientTest : BaseTokenClientTest<TokenConfigWriterServiceClient>
{
	Mock<ITokenConfigWriterService>? tokenConfigWriterServiceMock;

	[SetUp]
	public void SetUp()
	{
		tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
	}

	[TearDown]
	public void TearDown()
	{
		tokenConfigWriterServiceMock?.VerifyNoOtherCalls();
	}

	protected override ServiceCollection BuildServiceCollection()
	{
		var serviceCollection = base.BuildServiceCollection();
		serviceCollection.AddScoped(sp => tokenConfigWriterServiceMock!.Object);
		return serviceCollection;
	}

	[Test]
	public async Task RegisterTokenClient_PrepareNewCertificate()
	{
		var capturedRequests = new List<PrepareNewCertificateRequest>();
		tokenConfigWriterServiceMock!
			.Setup(x => x.PrepareNewCertificateAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PrepareNewCertificateResponse("Test CSR"));

		await RegisterTokenClientAndStartSignalR();
		var request = new PrepareNewCertificateRequest();
		var response = await SendSignalRRequest<PrepareNewCertificateRequest, PrepareNewCertificateResponse>("PrepareNewCertificate", request);

		Assert.Multiple(() =>
		{
			Assert.That(response, Is.EqualTo(new PrepareNewCertificateResponse("Test CSR")));
			Assert.That(capturedRequests.Select(Serialize), Is.EquivalentTo(new[] { "{}", }));
		});

		tokenConfigWriterServiceMock.Verify(
			m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), cancellationTokenSource!.Token),
			Times.Once());
		hostApplicationLifetimeMock!.Verify(m => m.ApplicationStopping, Times.Once());
	}

	[Test]
	public async Task RegisterTokenClient_ResetAccessToken()
	{
		var capturedRequests = new List<ResetAccessTokenRequest>();
		tokenConfigWriterServiceMock!
			.Setup(x => x.ResetAccessTokenAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ResetAccessTokenResponse());

		await RegisterTokenClientAndStartSignalR();
		var request = new ResetAccessTokenRequest();
		var response = await SendSignalRRequest<ResetAccessTokenRequest, ResetAccessTokenResponse>("ResetAccessToken", request);

		Assert.Multiple(() =>
		{
			Assert.That(Serialize(response), Is.EqualTo("{}"));
			Assert.That(capturedRequests.Select(Serialize), Is.EquivalentTo(new[] { "{}", }));
		});

		tokenConfigWriterServiceMock.Verify(
			m => m.ResetAccessTokenAsync(	It.IsAny<ResetAccessTokenRequest>(), cancellationTokenSource!.Token),
			Times.Once());
		hostApplicationLifetimeMock!.Verify(m => m.ApplicationStopping, Times.Once());
	}

	[Test]
	public async Task RegisterTokenClient_SetOperationId()
	{
		var capturedRequests = new List<SetOperationIdRequest>();
		tokenConfigWriterServiceMock!
			.Setup(x => x.SetOperationIdAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))	
			.ReturnsAsync(new SetOperationIdResponse());

		await RegisterTokenClientAndStartSignalR();

		var request = new SetOperationIdRequest("Test CSR", "Test OperationId");
		var response = await SendSignalRRequest<SetOperationIdRequest, SetOperationIdResponse>("SetOperationId", request);

		Assert.Multiple(() =>
		{
			Assert.That(Serialize(response), Is.EqualTo("{}"));
			Assert.That(capturedRequests, Is.EquivalentTo(new[] { request, }));
		});

		tokenConfigWriterServiceMock.Verify(
			m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), cancellationTokenSource!.Token),
			Times.Once());
		hostApplicationLifetimeMock!.Verify(m => m.ApplicationStopping, Times.Once());
	}

	[Test]
	public async Task RegisterTokenClient_SetNewCertificateCredentials()
	{
		var capturedRequests = new List<SetNewCertificateCredentialsRequest>();
		tokenConfigWriterServiceMock!
			.Setup(x => x.SetNewCertificateCredentialsAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new SetNewCertificateCredentialsResponse());

		await RegisterTokenClientAndStartSignalR();

		var certificate = "Test Certificate"u8.ToArray();
		var request = new SetNewCertificateCredentialsRequest("Test OperationId", "Test TenantId", "Test ClientId", certificate);
		var response = await SendSignalRRequest<SetNewCertificateCredentialsRequest, SetNewCertificateCredentialsResponse>("SetNewCertificateCredentials", request);

		Assert.Multiple(() =>
		{
			Assert.That(Serialize(response), Is.EqualTo("{}"));
			Assert.That(capturedRequests, Is.EquivalentTo(new[] { request, }));
		});

		tokenConfigWriterServiceMock.Verify(
			m => m.SetNewCertificateCredentialsAsync(
				It.Is<SetNewCertificateCredentialsRequest>(x => Serialize(x) == Serialize(request)),
				cancellationTokenSource!.Token),
			Times.Once());
		hostApplicationLifetimeMock!.Verify(m => m.ApplicationStopping, Times.Once());
	}
}
