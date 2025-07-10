using System.Collections.Generic;

namespace CargoWise.GraphEngine
{
	public interface IProcessedProvider<TEntity>
	{
		IEnumerable<TEntity> GetProcessed(IEnumerable<TEntity> entities);
	}
}
