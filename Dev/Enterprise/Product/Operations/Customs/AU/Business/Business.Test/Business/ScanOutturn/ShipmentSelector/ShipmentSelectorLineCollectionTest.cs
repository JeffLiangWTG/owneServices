using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ShipmentSelectorLineCollection))]
	sealed class ShipmentSelectorLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShipmentSelectorLineCollection>
	{
		protected override ShipmentSelectorLineCollection GetCollectionToTest()
		{
			return new ShipmentSelectorLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SeaShipmentSelectorLine(Factory.New<CusSCAOceanBill>());
		}
	}
}
