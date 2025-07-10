using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(EventRegistryBusinessObjectCollection))]
	public class EventRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EventRegistryBusinessObjectCollection>
	{
		public void TestFindByCodeAndReference()
		{
			EventRegistryBusinessObjectCollection collection = GetCollectionToTest();

			var entry1 = collection.AddNew();
			entry1.Code = "ABC";
			entry1.Reference = "123";
			var entry2 = collection.AddNew();
			entry2.Code = "ABC";
			entry2.Reference = "456";
			var entry3 = collection.AddNew();
			entry3.Code = "ZZZ";
			entry3.Reference = "";
			var entry4 = collection.AddNew();
			entry4.Code = "ZZZ";
			entry4.Reference = "999";

			AssertSame("Match on Code and Reference", entry1, collection.FindByCodeAndReference("ABC", "123"));
			AssertSame("Match on Code and Reference", entry2, collection.FindByCodeAndReference("ABC", "456"));
			AssertSame("Match on Code and Reference", entry3, collection.FindByCodeAndReference("ZZZ", ""));
			AssertSame("Match on Code ignoring Reference", entry3, collection.FindByCodeAndReference("ZZZ", "666"));
			AssertSame("Match on Code and Reference", entry4, collection.FindByCodeAndReference("ZZZ", "999"));
			AssertNull(collection.FindByCodeAndReference("ABC", "999"));
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override EventRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new EventRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EventRegistryBusinessObject();
		}
	}
}
