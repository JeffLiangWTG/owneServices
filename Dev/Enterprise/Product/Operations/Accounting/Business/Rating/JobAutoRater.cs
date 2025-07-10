using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business;

public class JobAutoRater : IJobAutoRater
{
	public void AutoRate(IBusiness objectToAutoRate, ILogger logger, AutoRateOptions options)
	{
		var starter = new AutoRatingStarter(new[] { objectToAutoRate }, logger);
		starter.ExecuteAutorating(options);
	}
}
