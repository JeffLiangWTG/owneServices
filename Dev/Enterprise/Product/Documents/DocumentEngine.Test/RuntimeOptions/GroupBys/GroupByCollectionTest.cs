using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(GroupByCollection))]
	sealed class GroupByCollectionTest : NonPersistentBusinessObjectTestCase
	{
		GroupByCollection gbc;

		protected override void SetUp()
		{
			base.SetUp();
			gbc = new GroupByCollection();
		}

		[ExpectNoExceptions]
		public void TestEmptyOnConstruction()
		{
			Assert("Collection should be empty on creation", !((IEnumerable<IBindableBooleanItem>)new GroupByCollection()).GetEnumerator().MoveNext());
		}

		public void TestAdd()
		{
			gbc.Add("some description", "val1");
			gbc.Add("other description", "val2");
			gbc.Add("third description", "val3");

			IEnumerator<IBindableBooleanItem> en = ((IEnumerable<IBindableBooleanItem>)gbc).GetEnumerator();
			Assert(en.MoveNext());
			AssertEquals("some description", ((GroupBy)en.Current).DisplayName);
			AssertEquals("val1", ((GroupBy)en.Current).FieldList);

			Assert(en.MoveNext());
			AssertEquals("other description", ((GroupBy)en.Current).DisplayName);
			AssertEquals("val2", ((GroupBy)en.Current).FieldList);

			Assert(en.MoveNext());
			AssertEquals("third description", ((GroupBy)en.Current).DisplayName);
			AssertEquals("val3", ((GroupBy)en.Current).FieldList);

			Assert(!en.MoveNext());
		}

		public void TestNoneSelected()
		{
			AssertEquals("Empty collection", null, gbc.SelectedGroupBy.FieldList);
			gbc.Add("some description", "val1");
			gbc.Add("other description", "val2");
			gbc.Add("third description", "val3");
			AssertEquals("None selected", null, gbc.SelectedGroupBy.FieldList);
		}

		public void TestSelected()
		{
			GroupBy gb = new GroupBy("x", "x");
			gb.Selected = true;

			gbc.Add("some description", "val1");
			gbc.Add("other description", "val2");
			gbc.Add(gb);
			gbc.Add("fourth description", "val4");
			AssertEquals("Selected", gb, gbc.SelectedGroupBy);
		}

		public void TestCount()
		{
			AssertEquals("Count", 0, gbc.Count);
			gbc.Add("x", "y");
			gbc.Add("x", "y");
			gbc.Add("x", "y");
			AssertEquals("Count", 3, gbc.Count);
		}

		public void TestIndexIndexer()
		{
			gbc.Add("x", "y");
			AssertEquals("Display name", "x", gbc[0].DisplayName);
			AssertEquals("Field list", "y", gbc[0].FieldList);
		}

		public void TestDisplayNameIndexer()
		{
			gbc.Add("DisplayName1", "x");
			gbc.Add("DisplayName2", "y");
			AssertEquals("DisplayName1", gbc["DisplayName1"].DisplayName);
			AssertEquals("DisplayName2", gbc["DisplayName2"].DisplayName);
			AssertEquals(null, gbc["NonExistant"]);
		}

		public void TestEmpty()
		{
			gbc.Add("x", "y");
			gbc.Clear();
			AssertEquals("Count should be 0", 0, gbc.Count);
		}

		public void TestAddOneArgReturnsAddedOrder()
		{
			GroupBy gb = new GroupBy("x", "y");
			AssertSame("Add should return the object that was added", gb, gbc.Add(gb));
		}

		public void TestAddTwoArgReturnsAddedOrder()
		{
			GroupBy addResult = gbc.Add("x", "y");
			AssertSame("Add should return the object that was added", gbc[0], addResult);
		}

		public void TestEntriesRegisteredAsEditableChildren()
		{
			GroupBy gb = new GroupBy("x", "y");
			gbc.Add(gb);
			AssertEquals("Entry should be registered", true, gbc.IsRegisteredEditableChildObject(gb));
		}

		public void TestEntriesRegistrationInIndexerSetter()
		{
			GroupBy oldgb = new GroupBy("a", "b");
			gbc.Add(oldgb);
			AssertEquals("Entry should be registered", true, gbc.IsRegisteredEditableChildObject(oldgb));

			GroupBy gb = new GroupBy("x", "y");
			gbc[0] = gb;
			AssertEquals("New entry should be registered", true, gbc.IsRegisteredEditableChildObject(gb));
			AssertEquals("Old entry should be deregistered", false, gbc.IsRegisteredEditableChildObject(oldgb));
		}

		public void TestEntriesDeregistrationOnClear()
		{
			GroupBy gb = new GroupBy("x", "y");
			gbc.Add(gb);
			AssertEquals("Entry should be registered", true, gbc.IsRegisteredEditableChildObject(gb));

			gbc.Clear();
			AssertEquals("Entry should be deregistered", false, gbc.IsRegisteredEditableChildObject(gb));
		}

		public void TestDefaultGroupBy()
		{
			GroupBy gb1 = new GroupBy("x", "y");
			GroupBy gb2 = new GroupBy("a", "b");
			GroupBy gb3 = new GroupBy("c", "d");
			gb2.Selected = true;
			gbc.Add(gb1);
			gbc.Add(gb2);
			gbc.Add(gb3);
			gbc.UpdateDefaultGroupBy();
			AssertEquals(gb2, gbc.DefaultGroupBy);
		}
	}
}
