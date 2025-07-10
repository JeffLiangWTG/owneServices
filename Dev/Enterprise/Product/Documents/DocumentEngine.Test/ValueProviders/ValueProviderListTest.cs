using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	sealed class ValueProviderListTest : TestCase
	{
		[ExpectNoExceptions()]
		public void TestEmptyConstructor()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
		}

		public void TestConstructorWithValueProviderList()
		{
			ValueProviderList testValueProviderList1 = new ValueProviderList();
			testValueProviderList1.Add(GetNewValueProvider());
			testValueProviderList1.Add(GetNewValueProvider());
			testValueProviderList1.Add(GetNewValueProvider());

			ValueProviderList testValueProviderList2 = new ValueProviderList(testValueProviderList1);
			AssertEquals(3, testValueProviderList2.Count);
		}

		public void TestConstructorWithArray()
		{
			ValueProviderList testValueProviderList = new ValueProviderList(new ValueProvider[] { GetNewValueProvider(), GetNewValueProvider() });
			AssertEquals(2, testValueProviderList.Count);
		}

		public void TestIndexerGet()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			testValueProviderList.Add(GetNewValueProvider());
			AssertNotNull(testValueProviderList[0]);
		}

		public void TestIndexerSet()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			ValueProvider testValueProvider = GetNewValueProvider();
			testValueProviderList.Add(GetNewValueProvider());
			testValueProviderList[0] = testValueProvider;
			AssertSame(testValueProvider, testValueProviderList[0]);
		}

		public void TestAdd()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			AssertEquals(0, testValueProviderList.Count);
			testValueProviderList.Add(GetNewValueProvider());
			testValueProviderList.Add(GetNewValueProvider());
			testValueProviderList.Add(GetNewValueProvider());
			AssertEquals(3, testValueProviderList.Count);
		}

		public void TestAddRangeValueProviderList()
		{
			ValueProviderList testValueProviderList1 = new ValueProviderList();
			testValueProviderList1.Add(GetNewValueProvider());
			testValueProviderList1.Add(GetNewValueProvider());
			testValueProviderList1.Add(GetNewValueProvider());

			ValueProviderList testValueProviderList = new ValueProviderList();
			AssertEquals(0, testValueProviderList.Count);
			testValueProviderList.AddRange(testValueProviderList1);
			AssertEquals(3, testValueProviderList.Count);
		}

		public void TestAddRangeArray()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			AssertEquals(0, testValueProviderList.Count);
			testValueProviderList.AddRange(new ValueProvider[] { GetNewValueProvider(), GetNewValueProvider(), GetNewValueProvider(), GetNewValueProvider() });
			AssertEquals(4, testValueProviderList.Count);
		}

		public void TestContaines()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			ValueProvider testValueProvider = GetNewValueProvider();
			testValueProviderList.Add(testValueProvider);
			Assert(testValueProviderList.Contains(testValueProvider));
		}

		public void TestCopyTo()
		{
			ValueProviderList testValueProviderList = new ValueProviderList(new ValueProvider[] { GetNewValueProvider(), GetNewValueProvider() });
			ValueProvider[] valueProviderList = new ValueProvider[3] { null, null, null };
			testValueProviderList.CopyTo(valueProviderList, 1);
			AssertNull(valueProviderList[0]);
			AssertNotNull(valueProviderList[1]);
			AssertNotNull(valueProviderList[2]);
		}

		public void TestToArray()
		{
			ValueProviderList testValueProviderList = new ValueProviderList(new ValueProvider[] { GetNewValueProvider(), GetNewValueProvider() });
			ValueProvider[] valueProviderList = testValueProviderList.ToArray();
			AssertEquals(2, valueProviderList.Length);
		}

		public void TestIndexOf()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			testValueProviderList.Add(GetNewValueProvider());
			testValueProviderList.Add(GetNewValueProvider());
			ValueProvider testValueProvider = GetNewValueProvider();
			testValueProviderList.Add(testValueProvider);
			AssertEquals(2, testValueProviderList.IndexOf(testValueProvider));
		}

		public void TestInsert()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			testValueProviderList.Add(GetNewValueProvider());
			testValueProviderList.Add(GetNewValueProvider());
			ValueProvider testValueProvider = GetNewValueProvider();
			testValueProviderList.Insert(1, testValueProvider);
			AssertEquals(1, testValueProviderList.IndexOf(testValueProvider));
		}

		public void TestRemove()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			ValueProvider testValueProvider = GetNewValueProvider();
			testValueProviderList.Add(testValueProvider);
			Assert(testValueProviderList.Contains(testValueProvider));
			testValueProviderList.Remove(testValueProvider);
			Assert(!testValueProviderList.Contains(testValueProvider));
		}

		public void TestCount()
		{
			ValueProviderList testValueProviderList = new ValueProviderList();
			AssertEquals(0, testValueProviderList.Count);
			testValueProviderList.Add(GetNewValueProvider());
			testValueProviderList.Add(GetNewValueProvider());
			testValueProviderList.Add(GetNewValueProvider());
			AssertEquals(3, testValueProviderList.Count);
		}

		#region Implementation

		ValueProvider GetNewValueProvider()
		{
			return new DBOrBOValueProvider();
		}

		#endregion
	}
}
