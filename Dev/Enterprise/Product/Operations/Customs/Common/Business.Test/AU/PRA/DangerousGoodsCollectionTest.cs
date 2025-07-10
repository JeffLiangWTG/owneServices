using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.Testing
{
	class DangerousGoodsCollectionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetErrors()
		{
			var collection = new DangerousGoodsCollection();
			var item1 = collection.AddNew();
			var expected = @"Dangerous Goods Info Missing:
    IMDG Class
    UNDG Number
    Hazard Technical Name
    Emergency Contact Name
    Emergency Contact Phone Number
";

			NUnit.Framework.Assert.That(collection.GetErrors(), Is.EqualTo(expected));
		}

		[ExpectNoExceptions]
		public void TestCollection()
		{
			var collection = new DangerousGoodsCollection();

			var item1 = collection.AddNew();
			NUnit.Framework.Assert.That(collection.Count, Is.EqualTo(1), "Should have one in the pot");
			NUnit.Framework.Assert.That(collection[0], Is.EqualTo(item1), "Should be the right object");

			var item2 = collection.AddNew();
			NUnit.Framework.Assert.That(collection.Count, Is.EqualTo(2), "Should have two in the pot");
			NUnit.Framework.Assert.That(collection[1], Is.EqualTo(item2), "Should be the right object");

			var counter = 0;
			foreach (ContainerMessagingDangerousGoods item in collection)
			{
				NUnit.Framework.Assert.That(item, Is.Not.EqualTo(default(ContainerMessagingDangerousGoods)), "Item should not be null - should not be [null]");
				counter += 1;
			}
			NUnit.Framework.Assert.That(counter, Is.EqualTo(2), "Should have found 2 objects in collection");
		}
	}
}
