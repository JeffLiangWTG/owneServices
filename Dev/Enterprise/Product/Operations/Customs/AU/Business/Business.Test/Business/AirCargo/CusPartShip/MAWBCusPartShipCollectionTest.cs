using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(MAWBCusPartShipCollection))]
	sealed class MAWBCusPartShipCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new MAWBCusPartShipCollection(Factory.New<CusMAWB>());
	}
}
