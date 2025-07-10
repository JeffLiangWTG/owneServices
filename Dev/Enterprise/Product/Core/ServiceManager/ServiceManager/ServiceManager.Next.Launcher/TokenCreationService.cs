using CargoWise.Common;

namespace CargoWise.ServiceManager.Next.Launcher;

public class TokenCreationService : BackgroundService
{
	readonly IAccessTokenService accessTokenService;
	public TimeSpan Period { get; }
	readonly ILogger logger;

	public TokenCreationService(ILogger<TokenCreationService> logger, IAccessTokenService accessTokenService) : this(logger, accessTokenService, TimeSpan.FromMinutes(2)) { }

	internal TokenCreationService(ILogger<TokenCreationService> logger, IAccessTokenService accessTokenService, TimeSpan period)
	{
		this.accessTokenService = accessTokenService;
		this.logger = logger;
		Period = period;
	}

	void DoWork()
	{
		try
		{
			// This allows a buffer period when we start replacing the token and when the token expires so we never have no tokens
			// If we have 10 minutes expiry, this means that we will check every 2 minutes to see if the token will expired in less than 4 minutes.
			// That way it should generate a new token somewhere between 4 minutes and 2 minutes before the most recent token expires.
			if (accessTokenService.RotateToken(Period * 5, Period * 2))
			{
				logger.LogInformation("New DB access token has been generated");
			}
		}
		catch (Exception exception) when (!exception.IsCriticalException())
		{
			logger.LogWarning(exception, $"Exception caught while running {nameof(TokenCreationService)}");
		}
	}

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		await Task.Yield(); // notify that the initialization of the service is ready so that `StartAsync` can return
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				DoWork();
				await Task.Delay(Period, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{	// do nothing, same as if the while loop has ended.
		}
		logger.LogInformation("{BackgroundService} has been stopped", nameof(TokenCreationService));
	}
}
