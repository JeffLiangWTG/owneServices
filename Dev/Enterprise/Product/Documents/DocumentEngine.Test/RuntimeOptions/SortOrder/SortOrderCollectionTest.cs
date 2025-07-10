using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SortOrderCollection))]
	sealed class SortOrderCollectionTest : NonPersistentBusinessObjectTestCase
	{
		SortOrderCollection soc;

		protected override void SetUp()
		{
			base.SetUp();
			soc = new SortOrderCollection();
		}

		[ExpectNoExceptions]
		public void TestEmptyOnConstruction()
		{
			Assert("Collection should be empty on creation", !((IEnumerable<IBindableBooleanItem>)new SortOrderCollection()).GetEnumerator().MoveNext());
		}

		public void TestAdd()
		{
			soc.Add("some description", "val1");
			soc.Add("other description", "val2");
			soc.Add("third description", "val3");

			IEnumerator<IBindableBooleanItem> en = ((IEnumerable<IBindableBooleanItem>)soc).GetEnumerator();
			Assert(en.MoveNext());
			AssertEquals("some description", ((SortOrder)en.Current).DisplayName);
			AssertEquals("val1", ((SortOrder)en.Current).FieldList);

			Assert(en.MoveNext());
			AssertEquals("other description", ((SortOrder)en.Current).DisplayName);
			AssertEquals("val2", ((SortOrder)en.Current).FieldList);

			Assert(en.MoveNext());
			AssertEquals("third description", ((SortOrder)en.Current).DisplayName);
			AssertEquals("val3", ((SortOrder)en.Current).FieldList);

			Assert(!en.MoveNext());
		}

		public void TestNoneSelected()
		{
			AssertEquals("Empty collection", "NULL", soc.SelectedOrder.FieldList);
			soc.Add("some description", "val1");
			soc.Add("other description", "val2");
			soc.Add("third description", "val3");
			AssertEquals("None selected", "NULL", soc.SelectedOrder.FieldList);
		}

		public void TestSelected()
		{
			SortOrder so = new SortOrder("x", "x");
			so.Selected = true;

			soc.Add("some description", "val1");
			soc.Add("other description", "val2");
			soc.Add(so);
			soc.Add("fourth description", "val4");
			AssertEquals("Selected", so, soc.SelectedOrder);
		}

		public void TestCount()
		{
			AssertEquals("Count", 0, soc.Count);
			soc.Add("x", "y");
			soc.Add("x", "y");
			soc.Add("x", "y");
			AssertEquals("Count", 3, soc.Count);
		}

		public void TestIndexer()
		{
			soc.Add("x", "y");
			AssertEquals("Display name", "x", soc[0].DisplayName);
			AssertEquals("Field list", "y", soc[0].FieldList);
		}

		public void TestEmpty()
		{
			soc.Add("x", "y");
			soc.Clear();
			AssertEquals("Count should be 0", 0, soc.Count);
		}

		public void TestAddOneArgReturnsAddedOrder()
		{
			SortOrder so = new SortOrder("x", "y");
			AssertSame("Add should return the object that was added", so, soc.Add(so));
		}

		public void TestAddTwoArgReturnsAddedOrder()
		{
			SortOrder addResult = soc.Add("x", "y");
			AssertSame("Add should return the object that was added", soc[0], addResult);
		}

		public void TestEntriesRegisteredAsEditableChildren()
		{
			SortOrder so = new SortOrder("x", "y");
			soc.Add(so);
			AssertEquals("Entry should be registered", true, soc.IsRegisteredEditableChildObject(so));
		}

		public void TestEntriesRegistrationInIndexerSetter()
		{
			SortOrder oldso = new SortOrder("a", "b");
			soc.Add(oldso);
			AssertEquals("Entry should be registered", true, soc.IsRegisteredEditableChildObject(oldso));

			SortOrder so = new SortOrder("x", "y");
			soc[0] = so;
			AssertEquals("New entry should be registered", true, soc.IsRegisteredEditableChildObject(so));
			AssertEquals("Old entry should be deregistered", false, soc.IsRegisteredEditableChildObject(oldso));
		}

		public void TestEntriesDeregistrationOnClear()
		{
			SortOrder so = new SortOrder("x", "y");
			soc.Add(so);
			AssertEquals("Entry should be registered", true, soc.IsRegisteredEditableChildObject(so));

			soc.Clear();
			AssertEquals("Entry should be deregistered", false, soc.IsRegisteredEditableChildObject(so));
		}

		public void TestDefaultOrder()
		{
			SortOrder so1 = new SortOrder("x", "y");
			SortOrder so2 = new SortOrder("a", "b");
			SortOrder so3 = new SortOrder("c", "d");
			so2.Selected = true;
			soc.Add(so1);
			soc.Add(so2);
			soc.Add(so3);
			soc.UpdateDefaultOrder();
			AssertEquals(so2, soc.DefaultOrder);
		}
	}
}
