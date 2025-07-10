using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DropDownCodeDescriptionBoolCollection))]
	sealed class DropDownCodeDescriptionBoolCollectionTest : RegistryBusinessObjectCollectionTestCase<DropDownCodeDescriptionBoolCollection>
	{
		public void TestAddSetsParent()
		{
			var collection = new DropDownCodeDescriptionBoolCollection();

			var item1 = collection.AddNew();
			AssertEquals(collection, item1.Parent);

			var item2 = new DropDownCodeDescriptionBool(new DropDownCodeDescriptionBoolCollection());
			collection.Add(item2);
			AssertEquals(collection, item2.Parent);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DropDownCodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new DropDownCodeDescriptionBoolCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DropDownCodeDescriptionBool(new DropDownCodeDescriptionBoolCollection());
		}
	}
}
