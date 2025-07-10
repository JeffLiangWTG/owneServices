using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusSealCollection))]
	sealed class CusSealCollectionTest : Common.Testing.CusSealCollectionAbstractTest<CusSealCollection, CusSeal>
	{
		protected override int ExpectedMaxRowCount => 4;

		protected override CusSealCollection GetCollectionToTest()
		{
			var container = Factory.New<CusContainer>();
			return new CusSealCollection(container);
		}
	}
}
