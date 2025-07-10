using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Logging.CW.Test;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

public class TokenCreationServiceTest
{
	Mock<IAccessTokenService>? accessTokenServiceMock;
	TokenCreationService? tokenCreationService;
	Mock<ILogger<TokenCreationService>>? logger;

	[Test]
	public async Task StopsWhenCancellationTokenIsSetTest()
	{
		using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(2500));
		var cancellationToken = cancellationTokenSource.Token;

		var callCount = 0;

		accessTokenServiceMock!.Setup(x => x.RotateToken(It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>())).Callback(() => { callCount++; });

		await tokenCreationService!.StartAsync(cancellationToken);
		var taskEnded = SpinWait.SpinUntil(() => tokenCreationService.ExecuteTask!.IsCompleted, TimeSpan.FromSeconds(4));

		Assert.Multiple(() =>
		{
			Assert.That(taskEnded, Is.True, "taskEnded");
			Assert.That(callCount, Is.EqualTo(3), "callCount");
		});
		accessTokenServiceMock.Verify(x => x.RotateToken(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2)), Times.Exactly(3));
		logger.VerifyLog(LogLevel.Information, "TokenCreationService has been stopped", Times.Once);
	}

	[TestCase(typeof(Exception), false)]
	[TestCase(typeof(InvalidOperationException), false)]
	[TestCase(typeof(OutOfMemoryException), true)]
	public async Task OnlyCriticalExceptionStopsLoopTest(Type typeOfException, bool isCriticalException)
	{
		using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(2500));
		var cancellationToken = cancellationTokenSource.Token;

		var callCount = 0;
		var exception = (Exception?)Activator.CreateInstance(typeOfException) ?? new Exception();
		var callback = () => { callCount++; if (callCount > 1) { throw exception; }  };

		accessTokenServiceMock!.Setup(x => x.RotateToken(It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>())).Callback(callback);

		await tokenCreationService!.StartAsync(cancellationToken);
		var taskEnded = SpinWait.SpinUntil(() => tokenCreationService!.ExecuteTask!.IsCompleted, TimeSpan.FromSeconds(4));
		Assert.Multiple(() =>
		{
			Assert.That(taskEnded, Is.True, "taskEnded");
			Assert.That(callCount, Is.EqualTo(isCriticalException ? 2 : 3), "callCount");
			Assert.That(tokenCreationService!.ExecuteTask!.Status, Is.EqualTo(isCriticalException ? TaskStatus.Faulted : TaskStatus.RanToCompletion), "taskStatus");
			Assert.That(cancellationToken.IsCancellationRequested, Is.EqualTo(!isCriticalException), "cancellationRequested");
		});
		accessTokenServiceMock.Verify(x => x.RotateToken(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2)), Times.AtLeastOnce);
		logger.VerifyLog(LogLevel.Warning, $"Exception caught while running {nameof(TokenCreationService)}", exception, isCriticalException ? Times.Never : Times.AtLeastOnce);
		logger.VerifyLog(LogLevel.Information, "TokenCreationService has been stopped", isCriticalException ? Times.Never : Times.Once);
	}

	[Test]
	public void DefaultPeriodIsTwoMinutesTest()
	{
		tokenCreationService = new TokenCreationService(logger!.Object, accessTokenServiceMock!.Object);
		Assert.That(TimeSpan.FromMinutes(2), Is.EqualTo(tokenCreationService.Period), "period");
	}

	[Test]
	public async Task RotateTokenIsCalledWithExpectedValuesTest([Values] bool rotateTokenResult)
	{
		var period = TimeSpan.FromSeconds(1);
		tokenCreationService = new TokenCreationService(logger!.Object, accessTokenServiceMock!.Object, period);
		Assert.That(period, Is.EqualTo(tokenCreationService.Period));
		using var cancellationTokenSource = new CancellationTokenSource(period);
		var cancellationToken = cancellationTokenSource.Token;
		accessTokenServiceMock!
			.Setup(x => x.RotateToken(It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
			.Returns(rotateTokenResult);
		await tokenCreationService!.StartAsync(cancellationToken);
		var taskEnded = SpinWait.SpinUntil(() => tokenCreationService!.ExecuteTask!.IsCompleted, TimeSpan.FromSeconds(4));
		Assert.Multiple(() =>
		{
			Assert.That(taskEnded, Is.True, "taskEnded");
			Assert.That(tokenCreationService!.ExecuteTask!.Status, Is.EqualTo(TaskStatus.RanToCompletion), "taskStatus");
			Assert.That(cancellationToken.IsCancellationRequested, Is.True, "cancellationRequested");
		});
		accessTokenServiceMock.Verify(x => x.RotateToken(5 * period, 2 * period), Times.AtLeastOnce);
		logger.VerifyLog(LogLevel.Information, "TokenCreationService has been stopped", Times.Once);
		logger.VerifyLog(LogLevel.Information, "New DB access token has been generated", rotateTokenResult ? Times.AtLeastOnce : Times.Never);
	}

	[SetUp]
	public void SetUp()
	{
		accessTokenServiceMock = new Mock<IAccessTokenService>();
		logger = new Mock<ILogger<TokenCreationService>>();
		tokenCreationService = new TokenCreationService(logger.Object, accessTokenServiceMock.Object, TimeSpan.FromSeconds(1));
	}

	[TearDown]
	public void TearDown()
	{
		accessTokenServiceMock!.VerifyNoOtherCalls();
		logger!.VerifyNoOtherCalls();
	}
}
