using System.Collections.Generic;
using System.Linq;

namespace CargoWise.GraphEngine.Test
{
	class DummyProcessedProvider : IProcessedProvider<DummyEntity>
	{
		public IEnumerable<DummyEntity> GetProcessed(IEnumerable<DummyEntity> entities)
		{
			return entities.Where(e => e.IsProcessed);
		}
	}
}
