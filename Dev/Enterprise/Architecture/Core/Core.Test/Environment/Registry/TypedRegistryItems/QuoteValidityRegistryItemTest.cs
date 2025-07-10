using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(QuoteValidityRegistryItem))]
	sealed class QuoteValidityRegistryItemTest : StronglyTypedRegistryItemTestCase<int>
	{
		protected override StronglyTypedRegistryItem<int, int> GetNewRegistryItem()
		{
			return new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 1);
		}

		public void TestHasExpiry()
		{
			AssertHasExpiry(1, true);
			AssertHasExpiry(-1, true);
			AssertHasExpiry(0, true);
			AssertHasExpiry(QuoteValidityRegistryItem.BlankValue, false);
		}

		void AssertHasExpiry(int period, bool expected)
		{
			var registryItem = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, period);
			AssertEquals(expected, registryItem.HasExpiry);
		}

		public void TestValidMonthsFromToday()
		{
			var registryItem = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 5);
			AssertEquals(5, registryItem.ValidMonthsFromToday);

			var registryItem2 = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, -2);
			AssertEquals(2, registryItem2.ValidMonthsFromToday);
		}

		public void TestTillEndOfMonth()
		{
			AssertTillEndOfMonth(1, false);
			AssertTillEndOfMonth(-1, true);
			AssertTillEndOfMonth(0, true);
			AssertTillEndOfMonth(QuoteValidityRegistryItem.BlankValue, false);
		}

		void AssertTillEndOfMonth(int period, bool expected)
		{
			var registryItem = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, period);
			AssertEquals(expected, registryItem.TillEndOfMonth);
		}
	}
}
