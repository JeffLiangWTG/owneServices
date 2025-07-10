using System.Collections.Generic;
using System.Linq;

namespace CargoWise.GraphEngine.Test
{
	public class DummyKeyProvider : IKeyProvider<DummyKey, DummyEntity>
	{
		IEnumerable<IGrouping<DummyEntity, DummyKey>> IKeyProvider<DummyKey, DummyEntity>.GetKeys(IEnumerable<DummyEntity> entities)
		{
			return entities.Select(e => new Group<DummyEntity, DummyKey>(e, i => i.Keys));
		}
	}

	public class DummyGraphEngine : GrEngine<DummyKey, DummyEntity>
	{
		public TestGrEngineTracker TestTracker { get; } = new TestGrEngineTracker();

		public List<DummyEntity> Dummies { get; } = new List<DummyEntity>();

		protected override IKeyProvider<DummyKey, DummyEntity> GetKeyProvider()
		{
			return new DummyKeyProvider();
		}

		protected override IComparer<DummyEntity> GetOrderComparer()
		{
			return new DummyOrderComparer();
		}

		protected override IProcessedProvider<DummyEntity> GetProcessedProvider()
		{
			return new DummyProcessedProvider();
		}

		protected override IGrEngineTracker<DummyKey, DummyEntity> GetTracker()
		{
			return TestTracker;
		}

		protected override IEnumerable<DummyEntity> LoadEntitiesCore()
		{
			foreach (var entity in Dummies)
			{
				yield return entity;
			}

			Dummies.Clear();
		}

		#region Internal class

		class DummyOrderComparer : IComparer<DummyEntity>
		{
			public int Compare(DummyEntity x, DummyEntity y)
			{
				return x.ID.CompareTo(y.ID);
			}
		}

		#endregion
	}
}
