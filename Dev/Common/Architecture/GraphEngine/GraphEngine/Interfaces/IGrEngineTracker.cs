using System.Collections.Generic;

namespace CargoWise.GraphEngine
{
	public interface IGrEngineTracker<TKey, TEntity>
	{
		void Loaded(IEnumerable<TEntity> entities);
		void VetexAdded(IEnumerable<TEntity> entities);
		void VetexRemoved(IEnumerable<TEntity> entities, int removedFromMiddle);
	}
}
