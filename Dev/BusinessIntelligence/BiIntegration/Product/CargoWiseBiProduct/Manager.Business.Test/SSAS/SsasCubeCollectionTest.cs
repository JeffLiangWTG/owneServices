using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(SsasCubeCollection))]
	class SsasCubeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SsasCubeCollection>
	{
		protected override SsasCubeCollection GetCollectionToTest()
		{
			return new SsasCubeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SsasCube("");
		}
	}
}
