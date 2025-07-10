using System.Collections;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterFieldListTest : TestCase
	{
		[ExpectNoExceptions()]
		public void TestEmptyConstructor()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
		}

		public void TestConstructorWithFilterFieldList()
		{
			FilterFieldList testFilterFieldList1 = new FilterFieldList();
			testFilterFieldList1.Add(GetNewFilterField());
			testFilterFieldList1.Add(GetNewFilterField());
			testFilterFieldList1.Add(GetNewFilterField());

			FilterFieldList testFilterFieldList2 = new FilterFieldList(testFilterFieldList1);
			AssertEquals(3, testFilterFieldList2.Count);
		}

		public void TestConstructorWithArray()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList(new FilterField[] { GetNewFilterField(), GetNewFilterField() });
			AssertEquals(2, testFilterFieldList.Count);
		}

		public void TestIndexerGet()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			testFilterFieldList.Add(GetNewFilterField());
			AssertNotNull(testFilterFieldList[0]);
		}

		public void TestIndexerSet()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			FilterField testFilterField = GetNewFilterField();
			testFilterFieldList.Add(GetNewFilterField());
			testFilterFieldList[0] = testFilterField;
			AssertSame(testFilterField, testFilterFieldList[0]);
		}

		public void TestAdd()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			AssertEquals(0, testFilterFieldList.Count);
			testFilterFieldList.Add(GetNewFilterField());
			testFilterFieldList.Add(GetNewFilterField());
			testFilterFieldList.Add(GetNewFilterField());
			AssertEquals(3, testFilterFieldList.Count);
		}

		public void TestAddRangeFilterFieldList()
		{
			FilterFieldList testFilterFieldList1 = new FilterFieldList();
			testFilterFieldList1.Add(GetNewFilterField());
			testFilterFieldList1.Add(GetNewFilterField());
			testFilterFieldList1.Add(GetNewFilterField());

			FilterFieldList testFilterFieldList = new FilterFieldList();
			AssertEquals(0, testFilterFieldList.Count);
			testFilterFieldList.AddRange(testFilterFieldList1);
			AssertEquals(3, testFilterFieldList.Count);
		}

		public void TestAddRangeArray()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			AssertEquals(0, testFilterFieldList.Count);
			testFilterFieldList.AddRange(new FilterField[] { GetNewFilterField(), GetNewFilterField(), GetNewFilterField(), GetNewFilterField() });
			AssertEquals(4, testFilterFieldList.Count);
		}

		public void TestContaines()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			FilterField testFilterField = GetNewFilterField();
			testFilterFieldList.Add(testFilterField);
			Assert(testFilterFieldList.Contains(testFilterField));
		}

		public void TestCopyTo()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList(new FilterField[] { GetNewFilterField(), GetNewFilterField() });
			FilterField[] filterFieldList = new FilterField[3] { null, null, null };
			testFilterFieldList.CopyTo(filterFieldList, 1);
			AssertNull(filterFieldList[0]);
			AssertNotNull(filterFieldList[1]);
			AssertNotNull(filterFieldList[2]);
		}

		public void TestToArray()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList(new FilterField[] { GetNewFilterField(), GetNewFilterField() });
			FilterField[] filterFieldList = testFilterFieldList.ToArray();
			AssertEquals(2, filterFieldList.Length);
		}

		public void TestIndexOf()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			testFilterFieldList.Add(GetNewFilterField());
			testFilterFieldList.Add(GetNewFilterField());
			FilterField testFilterField = GetNewFilterField();
			testFilterFieldList.Add(testFilterField);
			AssertEquals(2, testFilterFieldList.IndexOf(testFilterField));
		}

		public void TestInsert()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			testFilterFieldList.Add(GetNewFilterField());
			testFilterFieldList.Add(GetNewFilterField());
			FilterField testFilterField = GetNewFilterField();
			testFilterFieldList.Insert(1, testFilterField);
			AssertEquals(1, testFilterFieldList.IndexOf(testFilterField));
		}

		public void TestRemove()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			FilterField testFilterField = GetNewFilterField();
			testFilterFieldList.Add(testFilterField);
			Assert(testFilterFieldList.Contains(testFilterField));
			testFilterFieldList.Remove(testFilterField);
			Assert(!testFilterFieldList.Contains(testFilterField));
		}

		public void TestCount()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList();
			AssertEquals(0, testFilterFieldList.Count);
			testFilterFieldList.Add(GetNewFilterField());
			testFilterFieldList.Add(GetNewFilterField());
			testFilterFieldList.Add(GetNewFilterField());
			AssertEquals(3, testFilterFieldList.Count);
		}

		public void TestGetEnumerator()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList(new FilterField[] { GetNewFilterField(), GetNewFilterField() });
			AssertNotNull(testFilterFieldList.GetEnumerator());
		}

		public void TestCurrent()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList(new FilterField[] { GetNewFilterField(), GetNewFilterField() });
			IEnumerator @enum = testFilterFieldList.GetEnumerator();
			@enum.MoveNext();
			AssertNotNull(@enum.Current);
		}

		public void TestMoveNext()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList(new FilterField[] { GetNewFilterField(), GetNewFilterField() });
			IEnumerator @enum = testFilterFieldList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
		}

		public void TestReset()
		{
			FilterFieldList testFilterFieldList = new FilterFieldList(new FilterField[] { GetNewFilterField(), GetNewFilterField() });
			IEnumerator @enum = testFilterFieldList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
			@enum.Reset();
			Assert(@enum.MoveNext());
		}

		FilterField GetNewFilterField()
		{
			return new TextField(new BusinessObjectFactory());
		}
	}
}
