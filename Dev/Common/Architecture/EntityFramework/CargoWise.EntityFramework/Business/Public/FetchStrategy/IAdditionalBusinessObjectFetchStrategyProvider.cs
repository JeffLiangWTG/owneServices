using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IAdditionalBusinessObjectFetchStrategyProvider
	{
		IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies();
	}
}
