using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBufferTimespanCollection))]
	class BMBufferTimespanCollectionTest : ActiveBusinessObjectCollectionTestCase<BMBufferTimespanCollection>
	{
		protected override BMBufferTimespanCollection GetCollectionToTest()
		{
			return new BMBufferTimespanCollection(Factory);
		}

		public void TestInitCollection()
		{
			var collection = new BMBufferTimespanCollection(Factory);
			AssertEquals("Precondition: collection count", 0, collection.Count);
		}
	}
}
