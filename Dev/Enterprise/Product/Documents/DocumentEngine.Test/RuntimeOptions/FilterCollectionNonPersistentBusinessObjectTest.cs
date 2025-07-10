using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(CollectionOfIFilter))]
	sealed class FilterCollectionNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			IFilterCollection = new CollectionOfIFilter();
		}
		CollectionOfIFilter IFilterCollection;

		public void TestFilterFieldsRegisteredAsEditableChildren()
		{
			TextField field = new TextField(Factory);
			IFilterCollection.Add(field);
			AssertEquals("Field should be registered", true, IFilterCollection.IsRegisteredEditableChildObject(field));
		}

		public void TestFilterFieldsRegistrationInIndexerSetter()
		{
			TextField oldField = new TextField(Factory);
			IFilterCollection.Add(oldField);
			AssertEquals("Field should be registered", true, IFilterCollection.IsRegisteredEditableChildObject(oldField));

			TextField newField = new TextField(Factory);
			IFilterCollection[0] = newField;
			AssertEquals("New Field should be registered", true, IFilterCollection.IsRegisteredEditableChildObject(newField));
			AssertEquals("Old Field should be deregistered", false, IFilterCollection.IsRegisteredEditableChildObject(oldField));
		}

		public void TestFilterFieldDeregistrationOnClear()
		{
			TextField field = new TextField(Factory);
			IFilterCollection.Add(field);
			AssertEquals("Field should be registered", true, IFilterCollection.IsRegisteredEditableChildObject(field));

			IFilterCollection.Clear();
			AssertEquals("Field should be deregistered", false, IFilterCollection.IsRegisteredEditableChildObject(field));
		}
	}
}
