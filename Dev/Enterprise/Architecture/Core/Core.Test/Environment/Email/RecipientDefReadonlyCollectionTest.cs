using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RecipientDefReadonlyCollectionTest : TestCase
	{
		RecipientDefReadonlyCollection Collection;
		RecipientDef rec1;
		RecipientDef rec2;
		RecipientDef rec3;

		protected override void SetUp()
		{
			List<RecipientDef> list = new List<RecipientDef>();
			rec1 = new RecipientDef("aaa");
			rec2 = new RecipientDef("Daniel <b@a.com>", true);
			rec3 = new RecipientDef("c@c.com", false);
			list.Add(rec1);
			list.Add(rec2);
			list.Add(rec3);
			Collection = new RecipientDefReadonlyCollection(list);
			base.SetUp();
		}

		public void TestGetElement()
		{
			AssertEquals("First element is rec1", rec1, Collection[0]);
			AssertEquals("Second element is rec2", rec2, Collection[1]);
			AssertEquals("Third element is rec3", rec3, Collection[2]);
		}

		public void TestToStringCollection()
		{
			StringCollection col = Collection.ToStringCollection();
			AssertEquals("First element is email of rec1", rec1.Email, col[0]);
			AssertEquals("Second element is email of rec2", rec2.Email, col[1]);
			AssertEquals("Third element is email of rec3", rec3.Email, col[2]);
		}

		public void TestClear()
		{
			AssertEquals("Precondition - 3 elements", 3, Collection.Count);
			Collection.Clear();
			AssertEquals("All cleared", 0, Collection.Count);
		}

		public void TestContains()
		{
			Assert("Should contain aaa address", Collection.Contains("aaa"));
			Assert("Should contain bbb address", Collection.Contains("Daniel <b@a.com>"));
			Assert("Should contain ccc address", Collection.Contains("c@c.com"));
			Assert("Should not contain ddd address", !Collection.Contains("ddd"));
		}

		public void TestCount()
		{
			AssertEquals("Precondition - 3 elements", 3, Collection.Count);
		}

		public void TestRecipientsAsCommaSeparatedList()
		{
			AssertEquals("aaa; Daniel <b@a.com>; c@c.com", Collection.RecipientsAsDelimitedString());
			AssertEquals("aaa~Daniel <b@a.com>~c@c.com", Collection.RecipientsAsDelimitedString("~"));
		}
	}
}
