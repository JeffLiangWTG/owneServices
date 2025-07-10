using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	class EntityPositionStrategyTest : TestCase
	{
		public void TestPositioning_DoNotLock()
		{
			var strategy = new EntityPositionStrategy();
			var newEntities = new[] { new ImmmutablePositionEntity(), new ImmmutablePositionEntity(), new ImmmutablePositionEntity() };

			strategy.SetPositionsForNewEntities(newEntities, newEntities, new Location(0, 0));

			Assert("This test will otherwise lock up.", true);
		}

		#region ImmutablePositionEntity

		class ImmmutablePositionEntity : Entity
		{
			public override double X
			{
				get { return 0; }
				set { }
			}

			public override double Y
			{
				get { return 0; }
				set { }
			}
		}

		#endregion
	}
}
