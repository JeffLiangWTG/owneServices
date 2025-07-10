using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZClientWebCargoWiseEDI;
using NUnit.Framework;

namespace Testing
{
	[TestedType(typeof(BulkUpdateModeCollection))]
	public class BulkUpdateModeCollectionTest : NonPersistentBusinessObjectTestCase
	{
		BulkUpdateModeCollection collection;
		protected override void SetUp()
		{
			base.SetUp();
			collection = new BulkUpdateModeCollection();
		}

		[ExpectNoExceptions]
		public void TestEmptyOnConstruction()
		{
			AssertEquals("Collection should be empty on creation", false, new BulkUpdateModeCollection().GetEnumerator().MoveNext());
		}

		public void TestAdd()
		{
			collection.Add("some description", 1);
			collection.Add("other description", 2);
			collection.Add("third description", 3);
			IEnumerator<IBindableBooleanItem> en = ((IEnumerable<IBindableBooleanItem>)collection).GetEnumerator();
			Assert(en.MoveNext());
			AssertEquals("some description", ((BulkUpdateMode)en.Current).DisplayName);
			AssertEquals(1, ((BulkUpdateMode)en.Current).RecordsAffected);
			Assert(en.MoveNext());
			AssertEquals("other description", ((BulkUpdateMode)en.Current).DisplayName);
			AssertEquals(2, ((BulkUpdateMode)en.Current).RecordsAffected);
			Assert(en.MoveNext());
			AssertEquals("third description", ((BulkUpdateMode)en.Current).DisplayName);
			AssertEquals(3, ((BulkUpdateMode)en.Current).RecordsAffected);
			Assert(!en.MoveNext());
		}

		public void TestSelected()
		{
			BulkUpdateMode so = new BulkUpdateMode("x", 5);
			so.Selected = true;
			collection.Add("some description", 1);
			collection.Add("other description", 2);
			collection.Add(so);
			collection.Add("fourth description", 4);
			AssertEquals("Selected", so, collection.SelectedOrder);
		}

		public void TestCount()
		{
			AssertEquals("Count", 0, collection.Count);
			collection.Add("x", 4);
			collection.Add("x", 4);
			collection.Add("x", 4);
			AssertEquals("Count", 3, collection.Count);
		}

		public void TestIndexer()
		{
			AssertEquals("Count", 0, collection.Count);
			var m1 = collection.Add(new BulkUpdateMode("name1", 0, "M1"));
			var m2 = collection.Add(new BulkUpdateMode("name2", 0, "M2"));
			var m3 = collection.Add(new BulkUpdateMode("name3", 0, "M3"));
			var m4 = collection.Add(new BulkUpdateMode("name4", 0));
			var m5 = collection.Add(new BulkUpdateMode("name5", 0));
			AssertEquals("Count", 5, collection.Count);
			AssertEquals(m1, collection["M1"]);
			AssertEquals(m2, collection["M2"]);
			AssertEquals(m3, collection["M3"]);
			AssertEquals(m4, collection[""]);
		}
	}
}
