using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryBusinessObjectCollectionTest : TestCase
	{
		public void TestCreateFromList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();

			list.AddPair("ABC", "ABC Description");
			list.AddPair("XYZ", "XYZ Description");

			DummyRegistryBusinessObjectCollection collection = new DummyRegistryBusinessObjectCollection(list, 55);

			AssertEquals("Count", 2, collection.Count);
			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].CodeMaxLength", 55, collection[0].CodeMaxLength);
			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].CodeMaxLength", 55, collection[1].CodeMaxLength);
		}

		public void TestCodeMaxLengthOnANewChild()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();

			list.AddPair("ABC", "ABC Description");
			list.AddPair("XYZ", "XYZ Description");

			DummyRegistryBusinessObjectCollection collection = new DummyRegistryBusinessObjectCollection(list, 55);
			DummyRegistryBusinessObject newBizObj = collection.AddNew();
			AssertEquals("CodeMaxLength on a new child", 55, newBizObj.CodeMaxLength);
		}

		#region TestFindByCode

		public void TestFindByCode()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();

			list.AddPair("ABC", "ABC Description");

			DummyRegistryBusinessObjectCollectionWithCaseIgnoring collection = new DummyRegistryBusinessObjectCollectionWithCaseIgnoring(list);

			collection.IgnoreCaseInCodesForTest = true;

			AssertEquals("ABC", collection.FindByCode("ABC").Code);
			AssertEquals("ABC", collection.FindByCode("abc").Code);
			AssertEquals("ABC", collection.FindByCode("Abc").Code);
			AssertNull(collection.FindByCode("XYZ"));

			collection.IgnoreCaseInCodesForTest = false;

			AssertEquals("ABC", collection.FindByCode("ABC").Code);
			AssertNull(collection.FindByCode("abc"));
			AssertNull(collection.FindByCode("Abc"));
			AssertNull(collection.FindByCode("XYZ"));
		}

		#endregion

		public void TestWithMultilingualCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("A", (NoResString)"Alpha");
			var collection = new DummyRegistryBusinessObjectCollection(list);
			AssertEquals("A", collection[0].Code);
			AssertEquals("Alpha", collection[0].Description);
		}

		public void TestCodeFromDescription()
		{
			DummyRegistryBusinessObjectCollection collection = new DummyRegistryBusinessObjectCollection();
			DummyRegistryBusinessObject bo1 = collection.AddNew();
			bo1.Code = "Cd1";
			bo1.Description = (NoResString)"Description1";
			DummyRegistryBusinessObject bo2 = collection.AddNew();
			bo2.Code = "Cd2";
			bo2.Description = (NoResString)"Description2";

			AssertEquals(bo1.Code, collection.GetCodeFromDescription(bo1.Description));
			AssertEquals(bo2.Code, collection.GetCodeFromDescription(bo2.Description));
		}
	}
}
