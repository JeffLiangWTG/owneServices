using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocFormedPagesTopLevelPackCollection))]
	sealed class DocFormedPagesTopLevelPackCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocFormedPagesTopLevelPackCollection>
	{
		public void TestFilteredByMode()
		{
			var topLevelPack1 = CreateDocFormedPagesTopLevelPack(Constants.ContainerModes.RollOnRollOff);
			var topLevelPack2 = CreateDocFormedPagesTopLevelPack(Constants.ContainerModes.RollOnRollOff);
			var topLevelPack3 = CreateDocFormedPagesTopLevelPack(Constants.ContainerModes.BreakBulk);
			var topLevelPack4 = CreateDocFormedPagesTopLevelPack(Constants.ContainerModes.RollOnRollOff);

			var collection = new DocFormedPagesTopLevelPackCollection(Factory);
			collection.Add(topLevelPack1);
			collection.Add(topLevelPack2);
			collection.Add(topLevelPack3);
			collection.Add(topLevelPack4);

			AssertContainsExactElementsInAnyOrder(new[] { topLevelPack1, topLevelPack2, topLevelPack4 }, collection.FilteredByMode(Constants.ContainerModes.RollOnRollOff));
			AssertContainsExactElementsInAnyOrder(new[] { topLevelPack3 }, collection.FilteredByMode(Constants.ContainerModes.BreakBulk));
		}

		#region Implementation

		DocFormedPagesTopLevelPack CreateDocFormedPagesTopLevelPack(ZString mode)
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = mode;

			return new DocFormedPagesTopLevelPack(container);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipmentContainer>();
			return new DocFormedPagesTopLevelPack(container);
		}

		protected override DocFormedPagesTopLevelPackCollection GetCollectionToTest()
		{
			return new DocFormedPagesTopLevelPackCollection(Factory);
		}

		#endregion
	}
}
