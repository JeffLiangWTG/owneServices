using CargoWise.ServiceManager.Next.Shared.Services;
using Microsoft.AspNetCore.SignalR;

namespace CargoWise.ServiceManager.Next.Launcher;

public class NextHub : Hub<ITokenHubClient>
{
	readonly ILogger logger;
	readonly ICommandSender commandSender;

	public NextHub(ILogger<NextHub> logger, ICommandSender commandSender)
	{
		this.logger = logger;
		this.commandSender = commandSender;
	}

	public override async Task OnConnectedAsync()
	{
		await base.OnConnectedAsync().ConfigureAwait(false);
		var connectionId = Context.ConnectionId;
		logger.LogInformation("Client connected: {ConnectionId}", connectionId);
		commandSender.AddConnection(connectionId, Context.ConnectionAborted);
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var connectionId = Context.ConnectionId;
		logger.LogInformation(exception, "Client disconnected: {ConnectionId}", connectionId);
		commandSender.RemoveConnection(connectionId);
		await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
	}
}
