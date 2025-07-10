using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCADepotHouseCollection))]
	sealed class CusSCADepotHouseBizoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusSCADepotContainer container = Factory.New<CusSCADepotContainer>();
			return new CusSCADepotHouseCollection(container, Factory);
		}
	}
}
