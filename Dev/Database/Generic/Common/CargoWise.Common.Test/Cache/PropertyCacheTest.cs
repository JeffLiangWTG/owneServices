using CargoWise.Common.Cache;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test.DocumentPrinting
{
	public class PropertyCacheTest : TestCase
	{
		public PropertyCacheTest()
		{
			PropertyCache1 = new PropertyCache<string>(getProperty: () => { PropertyCacheCounter1++; return "PropertyCache1"; });
			PropertyCache2 = new PropertyCache<string>(getProperty: () => { PropertyCacheCounter2++; return "PropertyCache2"; });
		}

		public void TestGet()
		{
			AssertGet(message: "Get key11 1st time", actualValue: PropertyCache1.Get("key11"), expectedValue: "PropertyCache1", actualCounter: PropertyCacheCounter1, expectedCounter: 1);
			AssertGet(message: "Get key11 2nd time", actualValue: PropertyCache1.Get("key11"), expectedValue: "PropertyCache1", actualCounter: PropertyCacheCounter1, expectedCounter: 1);

			AssertGet(message: "Get key21 1st time", actualValue: PropertyCache2.Get("key21"), expectedValue: "PropertyCache2", actualCounter: PropertyCacheCounter2, expectedCounter: 1);
			AssertGet(message: "Get key21 2nd time", actualValue: PropertyCache2.Get("key21"), expectedValue: "PropertyCache2", actualCounter: PropertyCacheCounter2, expectedCounter: 1);

			AssertGet(message: "Get key12 1st time", actualValue: PropertyCache1.Get("key12"), expectedValue: "PropertyCache1", actualCounter: PropertyCacheCounter1, expectedCounter: 2);

			AssertGet(message: "Get key22 1st time", actualValue: PropertyCache2.Get("key22"), expectedValue: "PropertyCache2", actualCounter: PropertyCacheCounter2, expectedCounter: 2);
		}

		static void AssertGet(string message, string actualValue, string expectedValue, int actualCounter, int expectedCounter)
			=> CombineAssertions(message, () =>
			{
				AssertEquals("Value", expectedValue, actualValue);
				AssertEquals("Counter", expectedCounter, actualCounter);
			});

		PropertyCache<string> PropertyCache1 { get; }
		int PropertyCacheCounter1;

		PropertyCache<string> PropertyCache2 { get; }
		int PropertyCacheCounter2;
	}
}
