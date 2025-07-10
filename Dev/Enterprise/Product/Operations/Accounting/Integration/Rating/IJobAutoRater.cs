using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Integration;

public interface IJobAutoRater
{
	void AutoRate(IBusiness objectToAutoRate, ILogger logger, AutoRateOptions options);
}
