using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(DummyBusinessObjectCollection))]
	sealed class DummyBusinessObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DummyBusinessObjectCollection(Factory);
		}
	}
}
