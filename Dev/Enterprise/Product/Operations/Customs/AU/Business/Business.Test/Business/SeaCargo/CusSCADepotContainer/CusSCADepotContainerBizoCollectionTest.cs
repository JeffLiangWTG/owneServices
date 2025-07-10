using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCADepotContainerCollection))]
	sealed class CusSCADepotContainerBizoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			return new CusSCADepotContainerCollection(container, Factory);
		}
	}
}
