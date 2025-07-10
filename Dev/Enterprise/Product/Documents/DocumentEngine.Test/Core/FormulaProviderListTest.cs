using System.Collections;
using Enterprise.DocumentEngine.FlexCelInterface;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class FormulaProviderListTest : TestCase
	{
		public FormulaProviderListTest() : base()
		{
		}

		FormulaProvider GetNewFormulaProvider()
		{
			return new FormulaProvider(new ExcelWorkSheet(null, ""));
		}

		[ExpectNoExceptions()]
		public void TestEmptyConstructor()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
		}

		public void TestConstructorWithFormulaProviderList()
		{
			FormulaProviderList testFormulaProviderList1 = new FormulaProviderList();
			testFormulaProviderList1.Add(GetNewFormulaProvider());
			testFormulaProviderList1.Add(GetNewFormulaProvider());
			testFormulaProviderList1.Add(GetNewFormulaProvider());

			FormulaProviderList testFormulaProviderList2 = new FormulaProviderList(testFormulaProviderList1);
			AssertEquals(3, testFormulaProviderList2.Count);
		}

		public void TestConstructorWithArray()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider() });
			AssertEquals(2, testFormulaProviderList.Count);
		}

		public void TestIndexerGet()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			testFormulaProviderList.Add(GetNewFormulaProvider());
			AssertNotNull(testFormulaProviderList[0]);
		}

		public void TestIndexerSet()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			FormulaProvider testFormulaProvider = GetNewFormulaProvider();
			testFormulaProviderList.Add(GetNewFormulaProvider());
			testFormulaProviderList[0] = testFormulaProvider;
			AssertSame(testFormulaProvider, testFormulaProviderList[0]);
		}

		public void TestAdd()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			AssertEquals(0, testFormulaProviderList.Count);
			testFormulaProviderList.Add(GetNewFormulaProvider());
			testFormulaProviderList.Add(GetNewFormulaProvider());
			testFormulaProviderList.Add(GetNewFormulaProvider());
			AssertEquals(3, testFormulaProviderList.Count);
		}

		public void TestAddRangeFormulaProviderList()
		{
			FormulaProviderList testFormulaProviderList1 = new FormulaProviderList();
			testFormulaProviderList1.Add(GetNewFormulaProvider());
			testFormulaProviderList1.Add(GetNewFormulaProvider());
			testFormulaProviderList1.Add(GetNewFormulaProvider());

			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			AssertEquals(0, testFormulaProviderList.Count);
			testFormulaProviderList.AddRange(testFormulaProviderList1);
			AssertEquals(3, testFormulaProviderList.Count);
		}

		public void TestAddRangeArray()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			AssertEquals(0, testFormulaProviderList.Count);
			testFormulaProviderList.AddRange(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider(), GetNewFormulaProvider(), GetNewFormulaProvider() });
			AssertEquals(4, testFormulaProviderList.Count);
		}

		public void TestContaines()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			FormulaProvider testFormulaProvider = GetNewFormulaProvider();
			testFormulaProviderList.Add(testFormulaProvider);
			Assert(testFormulaProviderList.Contains(testFormulaProvider));
		}

		public void TestCopyTo()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider() });
			FormulaProvider[] formulaProviderList = new FormulaProvider[3] { null, null, null };
			testFormulaProviderList.CopyTo(formulaProviderList, 1);
			AssertNull(formulaProviderList[0]);
			AssertNotNull(formulaProviderList[1]);
			AssertNotNull(formulaProviderList[2]);
		}

		public void TestToArray()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider() });
			FormulaProvider[] formulaProviderList = testFormulaProviderList.ToArray();
			AssertEquals(2, formulaProviderList.Length);
		}

		public void TestIndexOf()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			testFormulaProviderList.Add(GetNewFormulaProvider());
			testFormulaProviderList.Add(GetNewFormulaProvider());
			FormulaProvider testFormulaProvider = GetNewFormulaProvider();
			testFormulaProviderList.Add(testFormulaProvider);
			AssertEquals(2, testFormulaProviderList.IndexOf(testFormulaProvider));
		}

		public void TestInsert()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			testFormulaProviderList.Add(GetNewFormulaProvider());
			testFormulaProviderList.Add(GetNewFormulaProvider());
			FormulaProvider testFormulaProvider = GetNewFormulaProvider();
			testFormulaProviderList.Insert(1, testFormulaProvider);
			AssertEquals(1, testFormulaProviderList.IndexOf(testFormulaProvider));
		}

		public void TestRemove()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			FormulaProvider testFormulaProvider = GetNewFormulaProvider();
			testFormulaProviderList.Add(testFormulaProvider);
			Assert(testFormulaProviderList.Contains(testFormulaProvider));
			testFormulaProviderList.Remove(testFormulaProvider);
			Assert(!testFormulaProviderList.Contains(testFormulaProvider));
		}

		public void TestCount()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList();
			AssertEquals(0, testFormulaProviderList.Count);
			testFormulaProviderList.Add(GetNewFormulaProvider());
			testFormulaProviderList.Add(GetNewFormulaProvider());
			testFormulaProviderList.Add(GetNewFormulaProvider());
			AssertEquals(3, testFormulaProviderList.Count);
		}

		public void TestGetEnumerator()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider() });
			AssertNotNull(testFormulaProviderList.GetEnumerator());
		}

		public void TestCurrent()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider() });
			IEnumerator @enum = testFormulaProviderList.GetEnumerator();
			@enum.MoveNext();
			AssertNotNull(@enum.Current);
		}

		public void TestMoveNext()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider() });
			IEnumerator @enum = testFormulaProviderList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
		}

		public void TestReset()
		{
			FormulaProviderList testFormulaProviderList = new FormulaProviderList(new FormulaProvider[] { GetNewFormulaProvider(), GetNewFormulaProvider() });
			IEnumerator @enum = testFormulaProviderList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
			@enum.Reset();
			Assert(@enum.MoveNext());
		}
	}
}
