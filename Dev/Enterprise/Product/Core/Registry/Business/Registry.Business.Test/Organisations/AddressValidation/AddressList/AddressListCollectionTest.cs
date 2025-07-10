using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressListCollection))]
	sealed class AddressListCollectionTest : RegistryBusinessObjectCollectionTestCase<AddressListCollection>
	{
		public void TestCollectionMembers()
		{
			var collection = new AddressListCollection();
			var element = collection.AddNew();

			element.AddressType = "typeOne";
			element.ControllerName = "Org";

			CombineAssertions(() =>
			{
				AssertEquals("typeOne", collection[0].AddressType);
				AssertEquals("Org", collection[0].ControllerName);
			});
		}

		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals("Must allow new rows", true, Collection.AllowNew);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AddressListCollection GetCollectionToTest() => new AddressListCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AddressListElement();

		#endregion
	}
}
