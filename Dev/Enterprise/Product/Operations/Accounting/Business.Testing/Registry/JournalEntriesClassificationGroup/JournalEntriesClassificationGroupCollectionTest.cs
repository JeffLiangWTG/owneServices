using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JournalEntriesClassificationGroupCollection))]
	public class JournalEntriesClassificationGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JournalEntriesClassificationGroupCollection>
	{
		public void TestAllowNewCore()
		{
			var collection = new JournalEntriesClassificationGroupCollectionForTest();
			AssertEquals(false, collection.AllowNewCoreForTest);
		}

		public void TestAllowRemoveCore()
		{
			var collection = new JournalEntriesClassificationGroupCollectionForTest();
			AssertEquals(false, collection.AllowRemoveCoreForTest);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override JournalEntriesClassificationGroupCollection GetCollectionToTest() => new JournalEntriesClassificationGroupCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new JournalEntriesClassificationGroup();

		#endregion

		public class JournalEntriesClassificationGroupCollectionForTest : JournalEntriesClassificationGroupCollection
		{
			public bool AllowNewCoreForTest => base.AllowNewCore;

			public bool AllowRemoveCoreForTest => base.AllowRemoveCore;
		}
	}
}
