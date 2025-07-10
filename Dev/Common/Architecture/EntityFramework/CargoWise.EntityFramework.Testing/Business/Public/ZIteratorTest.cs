using System.Collections;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZIteratorTest : TestCaseWithFactory
	{
		public void TestIteratingNativeType()
		{
			char[] charArray1 = new char[] { 'l', 'a', 'u', 'g', 'h', 'i', 'n', 'g' };
			char[] charArray2 = new char[] { ' ', 'i', 's', ' ', 'g', 'o', 'o', 'd', ' ' };
			char[] charArray3 = new char[] { 'f', 'o', 'r', ' ', 'y', 'o', 'u' };

			ZIterator<char> charIterator = new ZIterator<char>(charArray1, charArray2, charArray3);
			string testString = "";
			foreach (char @char in charIterator)
			{
				testString += @char;
			}
			AssertEquals("Has to be iterating the characters in the order they are specified in the constructor", "laughing is good for you", testString);

			int[] intArray1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
			int[] intArray2 = new int[] { 9, 10, 11, 12, 13, 14, 15 };
			ZIterator<int> intIterator = new ZIterator<int>(intArray1, intArray2);
			int testIntTotal = 0;
			foreach (int testInt in intIterator)
			{
				testIntTotal += testInt;
			}
			AssertEquals(120, testIntTotal);
		}

		public void TestIteratingBusinessObjects()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject dummy1Child1 = dummy1.Collection.AddNew();
			DummyChildBusinessObject dummy1Child2 = dummy1.Collection.AddNew();
			DummyChildBusinessObject dummy1Child3 = dummy1.Collection.AddNew();

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject dummy3Child1 = dummy3.Collection.AddNew();

			ZIterator<BusinessObject> bizOIterator = new ZIterator<BusinessObject>(dummy1.Collection.ToArray(), dummy2.Collection.ToArray(), dummy3.Collection.ToArray());
			ArrayList bizOList = new ArrayList();
			foreach (BusinessObject bizO in bizOIterator)
			{
				bizOList.Add(bizO);
			}
			AssertEquals(4, bizOList.Count);
			AssertEquals(dummy1Child1, bizOList[0]);
			AssertEquals(dummy1Child2, bizOList[1]);
			AssertEquals(dummy1Child3, bizOList[2]);
			AssertEquals(dummy3Child1, bizOList[3]);
		}

		public void TestIteratingNativeTypeWithFirstElement()
		{
			ZIterator<string> stringIterator = new ZIterator<string>("MEH", new string[] { "has", "to", "be", "first" });
			string testString = "";
			foreach (string @string in stringIterator)
			{
				testString += @string + " ";
			}
			AssertEquals("First element has to be included", "MEH has to be first ", testString);
		}

		public void TestIteratingBusinessObjectWithFirstElement()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject dummy1Child1 = dummy1.Collection.AddNew();
			DummyChildBusinessObject dummy1Child2 = dummy1.Collection.AddNew();
			DummyChildBusinessObject dummy1Child3 = dummy1.Collection.AddNew();

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject dummy3Child1 = dummy3.Collection.AddNew();

			ZIterator<BusinessObject> bizOIterator = new ZIterator<BusinessObject>(dummy1, dummy1.Collection.ToArray(), dummy2.Collection.ToArray(), dummy3.Collection.ToArray());
			ArrayList bizOList = new ArrayList();
			foreach (BusinessObject bizO in bizOIterator)
			{
				bizOList.Add(bizO);
			}
			AssertEquals(5, bizOList.Count);
			AssertEquals(dummy1, bizOList[0]);
			AssertEquals(dummy1Child1, bizOList[1]);
			AssertEquals(dummy1Child2, bizOList[2]);
			AssertEquals(dummy1Child3, bizOList[3]);
			AssertEquals(dummy3Child1, bizOList[4]);
		}
	}
}
