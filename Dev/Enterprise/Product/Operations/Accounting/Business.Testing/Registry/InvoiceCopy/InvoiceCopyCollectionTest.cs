using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceCopyCollection))]
	public class InvoiceCopyCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<InvoiceCopyCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override InvoiceCopyCollection GetCollectionToTest()
		{
			return new InvoiceCopyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoiceCopy();
		}

		public void TestOrderWhenAddOrRemoveElement()
		{
			var collection = GetCollectionToTest();
			var copy1 = collection.AddNew();
			AssertEquals(1, copy1.Order);

			var copy2 = collection.AddNew();
			AssertEquals(2, copy2.Order);

			var copy3 = collection.AddNew();
			AssertEquals(3, copy3.Order);

			collection.RemoveAndDelete(copy2);
			AssertEquals(1, copy1.Order);
			AssertEquals(2, copy3.Order);
		}

		#endregion
	}
}
