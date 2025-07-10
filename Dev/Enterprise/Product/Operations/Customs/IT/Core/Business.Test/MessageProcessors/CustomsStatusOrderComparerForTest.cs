using System.Collections.Generic;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.IT.Business.Testing;

class CustomsStatusOrderComparerForTest : IEqualityComparer<CustomsStatusOrder>
{
	public bool Equals(CustomsStatusOrder x, CustomsStatusOrder y) => x.EntryStatusCode == y.EntryStatusCode && x.Order == y.Order;

	public int GetHashCode(CustomsStatusOrder obj) => FixedHashCodeOnlyForObjectEqualityComparison;
	const int FixedHashCodeOnlyForObjectEqualityComparison = 1;
}

public static class CustomsStatusOrderTestHelper
{
	public static void AssertCollection(CustomsStatusOrder[] expectedValues, CustomsStatusOrder[] actualValues)
	{
		AssertContainsExactElementsInAnyOrder(new CustomsStatusOrderComparerForTest(), (obj) => $"{obj.EntryStatusCode} - {obj.Order}", expectedValues, actualValues);
	}
}
