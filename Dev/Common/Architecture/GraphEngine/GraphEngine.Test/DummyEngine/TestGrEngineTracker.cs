using System.Collections.Generic;
using System.Linq;

namespace CargoWise.GraphEngine.Test
{
	public class TestGrEngineTracker : IGrEngineTracker<DummyKey, DummyEntity>
	{
		public int Loads { get; private set; }

		public int VertexAdds { get; private set; }

		public int VertexRemoves { get; private set; }

		void IGrEngineTracker<DummyKey, DummyEntity>.Loaded(IEnumerable<DummyEntity> entities)
		{
			Loads += entities.Count();
		}

		void IGrEngineTracker<DummyKey, DummyEntity>.VetexAdded(IEnumerable<DummyEntity> entities)
		{
			VertexAdds += entities.Count();
		}

		void IGrEngineTracker<DummyKey, DummyEntity>.VetexRemoved(IEnumerable<DummyEntity> entities, int removedFromMiddle)
		{
			VertexRemoves += entities.Count();
		}
	}
}
