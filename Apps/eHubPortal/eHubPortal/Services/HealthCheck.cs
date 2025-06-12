using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace eServices.eHubPortal.Services;

public class HealthCheck(IDbContextFactory<eHubTransactionsContext> DbFactory) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		using var dbContext = DbFactory.CreateDbContext();
		await dbContext.Database.OpenConnectionAsync(cancellationToken);
		await dbContext.Database.ExecuteSqlRawAsync("SELECT @@SERVERNAME", cancellationToken);
		return new HealthCheckResult(HealthStatus.Healthy);
	}
}
