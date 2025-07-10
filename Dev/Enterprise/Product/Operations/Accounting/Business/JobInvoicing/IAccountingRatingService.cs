using System;
using Enterprise.Integration;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IAccountingRatingService
	{
		RatingResults AutoRateAndCreateJobHeader(
			Guid operationsJobPk,
			string operationsJobTableCode,
			Guid staffPk, Guid branchPk,
			Guid departmentPk,
			Guid localClientPk,
			bool autorateRevenue = true,
			bool autorateCosts = true,
			bool isConsolLevelChargeExcluded = false,
			LogType maxLogLevel = LogType.Information);

		RatingResults SearchForRates(
			Guid operationsJobPk,
			string operationsJobTableCode,
			Guid staffPk,
			Guid branchPk,
			Guid departmentPk,
			Guid localClientPk,
			bool autorateRevenue = true,
			bool autorateCosts = true,
			bool isConsolLevelChargeExcluded = false,
			LogType maxLogLevel = LogType.Information);
	}
}
