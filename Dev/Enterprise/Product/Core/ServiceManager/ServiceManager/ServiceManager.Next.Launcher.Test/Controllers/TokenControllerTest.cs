using CargoWise.ServiceManager.Next.Launcher.Controllers;
using CargoWise.ServiceManager.Next.Launcher.Filters;
using CargoWise.ServiceManager.Next.Shared.Services;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Launcher.Test.Controllers;

public class TokenControllerTest
{
	CancellationTokenSource? cancellationTokenSource;
	Mock<INextProcessRunnerPool>? runnerPoolMock;

	[SetUp]
	public void Setup()
	{
		cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
		runnerPoolMock = new Mock<INextProcessRunnerPool>(MockBehavior.Strict);
	}

	[TearDown]
	public void TearDown()
	{
		cancellationTokenSource?.Dispose();
		runnerPoolMock?.VerifyNoOtherCalls();
	}

	[Test]
	public void TokenController_HasCorrectAttribute()
	{
		var type = typeof(TokenController);
		var attributes = type.GetCustomAttributes(inherit: false);
		Assert.That(
			attributes.Select(x => x.GetType()),
			Is.SupersetOf(new[] { typeof(ApiControllerAttribute), typeof(DbAccessTokenAuthorizationAttribute), typeof(RouteAttribute) }));
		Assert.That(attributes.OfType<RouteAttribute>().Select(x => x.Template), Is.EquivalentTo(new[] { "api/token" }));
	}

	[TestCaseSource(nameof(TestCases))]
	public async Task RunRequestAsync_InvokesRunnerPoolAndReturnsResponse<TRequest, TResponse>(TRequest request, TResponse response, TokenHandlerDelegate<TRequest,TResponse> handler)
	{
		runnerPoolMock!.Setup(x => x.RunAsync(It.IsAny<string>(), It.IsAny<TokenHandlerDelegate<TRequest, TResponse>>(), It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(response);

		var tokenController = new TokenController();
		dynamic req = request!; // this is a hack to avoid the compiler error since TRequest is not constrained to a valid type
		var cancellationToken = cancellationTokenSource!.Token;
		var actual = await tokenController.RunRequestAsync(req, runnerPoolMock.Object, cancellationToken);
		Assert.That(actual, Is.EqualTo(response));
		runnerPoolMock.Verify(m => m.RunAsync("token", handler, request, cancellationToken), Times.Once);
	}

	[TestCaseSource(nameof(TestCases))]
	public void RunRequestAsync_PropagateException<TRequest, TResponse>(TRequest request, TResponse response, TokenHandlerDelegate<TRequest, TResponse> handler)
	{
		var exception = new HubException("error for unit test");
		runnerPoolMock!
			.Setup(x => x.RunAsync(It.IsAny<string>(), It.IsAny<TokenHandlerDelegate<TRequest, TResponse>>(), It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(exception);

		var tokenController = new TokenController();
		dynamic req = request!; // this is a hack to avoid the compiler error since TRequest is not constrained to a valid type
		var cancellationToken = cancellationTokenSource!.Token;
		var e = Assert.ThrowsAsync<HubException>(() => tokenController.RunRequestAsync(req, runnerPoolMock.Object, cancellationToken));
		Assert.That(e, Is.EqualTo(exception));
		runnerPoolMock.Verify(m => m.RunAsync("token", handler, request, cancellationToken), Times.Once);
	}

	static IEnumerable<TestCaseData> TestCases() => new[]
	{
		new TestCaseData(
			new SignCwTokenRequest("audience1"),
			new SignCwTokenResponse("token1"),
			ICommandSender.SignCwTokenHandler).SetArgDisplayNames(nameof(ICommandSender.SignCwTokenHandler)),
		new TestCaseData(
			new PrepareNewCertificateRequest(),
			new PrepareNewCertificateResponse("csr"),
			ICommandSender.PrepareNewCertificateHandler).SetArgDisplayNames(nameof(ICommandSender.PrepareNewCertificateHandler)),
		new TestCaseData(
			new ResetAccessTokenRequest(),
			new ResetAccessTokenResponse(),
			ICommandSender.ResetAccessTokenHandler).SetArgDisplayNames(nameof(ICommandSender.ResetAccessTokenHandler)),
		new TestCaseData(
			new SetNewCertificateCredentialsRequest("opId", "tId", "cId", [1, 2, 3,]),
			new SetNewCertificateCredentialsResponse(),
			ICommandSender.SetNewCertificateCredentialsHandler).SetArgDisplayNames(nameof(ICommandSender.SetNewCertificateCredentialsHandler)),
		new TestCaseData(
			new SetOperationIdRequest("csr", "opId"),
			new SetOperationIdResponse(),
			ICommandSender.SetOperationIdHandler).SetArgDisplayNames(nameof(ICommandSender.SetOperationIdHandler)),
	};
}
