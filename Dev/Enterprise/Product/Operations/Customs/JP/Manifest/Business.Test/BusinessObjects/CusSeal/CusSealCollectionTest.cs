using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(CusSealCollection))]
	sealed class CusSealCollectionTest : Common.Testing.CusSealCollectionAbstractTest<CusSealCollection, CusSeal>
	{
		protected override int ExpectedMaxRowCount => 3;

		protected override CusSealCollection GetCollectionToTest()
		{
			var container = Factory.New<AsycudaContainer>();
			return new CusSealCollection(container);
		}
	}
}
