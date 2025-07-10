using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(RelatedArrivalMovementGenPivotCollection))]
class RelatedArrivalMovementGenPivotCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var movementHeader = Factory.New<NctsArrivalMovementHeader>();
		return new RelatedArrivalMovementGenPivotCollection(movementHeader);
	}
}
