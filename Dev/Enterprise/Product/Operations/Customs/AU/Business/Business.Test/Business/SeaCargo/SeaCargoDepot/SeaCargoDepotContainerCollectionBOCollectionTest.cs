using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoDepotContainerCollection))]
	sealed class SeaCargoDepotContainerCollectionBOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SeaCargoDepotContainerCollection>
	{
		protected override SeaCargoDepotContainerCollection GetCollectionToTest()
		{
			return new SeaCargoDepotContainerCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			return SeaCargoDepotContainer.Load(container);
		}
	}
}
