using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DeliveryInstructionsCollection))]
	sealed class DeliveryInstructionsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DeliveryInstructionsCollection>
	{
		[ExpectNoExceptions]
		public void TestConstructors()
		{
			DeliveryInstructionsCollection collection = new DeliveryInstructionsCollection();
			collection = new DeliveryInstructionsCollection(Factory);
		}

		protected override DeliveryInstructionsCollection GetCollectionToTest()
		{
			return new DeliveryInstructionsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DeliveryInstructions();
		}
	}
}
