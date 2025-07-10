using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DevTools;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(BizoDiffPropertyCollection))]
	public class BizoDiffPropertyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BizoDiffPropertyCollection>
	{
		protected override BizoDiffPropertyCollection GetCollectionToTest()
		{
			return new BizoDiffPropertyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BizoProperty("Name", "Value");
		}

		public void TestGetAllSelectedFields()
		{
			var collection = new BizoDiffPropertyCollection();
			collection.Add(new BizoProperty("Name1", "Value1"));
			collection.Add(new BizoProperty("Name2", "Value2"));

			collection[0].KeyField = true;

			AssertEquals(1, collection.KeyFields.Count);
			AssertEquals("Name1", collection.KeyFields[0]);
		}
	}
}
