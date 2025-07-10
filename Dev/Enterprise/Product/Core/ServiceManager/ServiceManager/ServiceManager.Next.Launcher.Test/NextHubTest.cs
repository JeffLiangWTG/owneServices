using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

public class NextHubTest
{
	[Test]
	public async Task OnConnected_CallAddConnection()
	{
		var commandSenderMock = new Mock<ICommandSender>(MockBehavior.Strict);
		commandSenderMock.Setup(m => m.AddConnection(It.IsAny<string>(), It.IsAny<CancellationToken>()));
		using var testHubCallerContext = new TestHubCallerContext();
		using var nextHub = CreateNextHub(commandSenderMock, testHubCallerContext);
		await nextHub.OnConnectedAsync();
		commandSenderMock.Verify(m => m.AddConnection(testHubCallerContext.ConnectionId, testHubCallerContext.ConnectionAborted), Times.Once);
		commandSenderMock.VerifyNoOtherCalls();
	}

	[Test]
	public async Task OnDisconnected_CallRemoveConnection()
	{
		var commandSenderMock = new Mock<ICommandSender>(MockBehavior.Strict);
		commandSenderMock.Setup(m => m.RemoveConnection(It.IsAny<string>()));
		using var testHubCallerContext = new TestHubCallerContext();
		using var nextHub = CreateNextHub(commandSenderMock, testHubCallerContext);
		await nextHub.OnDisconnectedAsync(null);
		commandSenderMock.Verify(m => m.RemoveConnection(testHubCallerContext.ConnectionId), Times.Once);
		commandSenderMock.VerifyNoOtherCalls();
	}

	static NextHub CreateNextHub(Mock<ICommandSender> commandSenderMock, TestHubCallerContext testHubCallerContext)
	{
		var nextHub = new NextHub(new NullLogger<NextHub>(), commandSenderMock.Object);
		nextHub.Context = testHubCallerContext;
		return nextHub;
	}
}

class TestHubCallerContext : HubCallerContext, IDisposable
{
	readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

	public override void Abort()
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		cancellationTokenSource.Dispose();
	}

	public override string ConnectionId { get; } = Guid.NewGuid().ToString();
	public override string? UserIdentifier { get; }
	public override ClaimsPrincipal? User { get; }
	public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
	public override IFeatureCollection Features { get; } = new FeatureCollection();
	public override CancellationToken ConnectionAborted => cancellationTokenSource.Token;
}
