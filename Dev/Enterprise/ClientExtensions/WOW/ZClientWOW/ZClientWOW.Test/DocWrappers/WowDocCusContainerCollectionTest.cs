using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowDocCusContainerCollection))]
	class WowDocCusContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WowDocCusContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusContainer container = Factory.New<CusContainer>();
			return WowDocCusContainer.New(container, Factory);
		}

		protected override WowDocCusContainerCollection GetCollectionToTest()
		{
			return new WowDocCusContainerCollection(Factory);
		}
	}
}
