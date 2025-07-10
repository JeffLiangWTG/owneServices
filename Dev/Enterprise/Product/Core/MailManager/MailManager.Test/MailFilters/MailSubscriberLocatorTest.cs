using System.Collections.Generic;
using System.Linq;
using Enterprise.MailManager.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.MailFilters.Testing
{
	sealed class MailSubscriberLocatorTest : TestCase
	{
		public void TestAllMailFilterCodesRegistered()
		{
			var locator = new MailFilterLocator(retrieveForAllClients: true);
			var allcodes = new HashSet<string>(MailFilterCodes.All);
			var registeredCodes = new HashSet<string>(locator.GetFiltersIncludingDisabled().Select(f => f.Code));

			var codesWithoutFilters = allcodes.Except(registeredCodes);
			AssertContainsExactElementsInAnyOrder("The following codes exist but a filter could not be found for them, have you set your attributes correctly?", Enumerable.Empty<string>(), codesWithoutFilters);

			var filtersWithoutCodes = registeredCodes.Except(allcodes);
			AssertContainsExactElementsInAnyOrder("Filters with the following codes were located. Please define the code in MailFilterCodes.", Enumerable.Empty<string>(), codesWithoutFilters);
		}

		public void TestDuplicates()
		{
			var duplicates = new MailFilterLocator(retrieveForAllClients: true)
				.GetFilters()
				.ToLookup(f => f.Code)
				.Where(l => l.Count() > 1)
				.Select(l => l.Key);

			AssertContainsExactElementsInAnyOrder("The following codes exist for more than one filter", Enumerable.Empty<string>(), duplicates);
		}

		public void TestCanLocate()
		{
			var locator = new MailFilterLocator();
			Assert("Should find the demo filters", locator.TryGetFilter("TS1", out var filter));
			AssertEquals("Should have the TS1 filter", "TS1", filter.Code);
		}

		public void TestCanLocateLazily()
		{
			var locator = new MailFilterLocator();
			Assert("Should find the demo filters", locator.TryGetFilter("TS1", out var filter));
			AssertEquals("Loading the TS1 filter should not evaluate the TS2 filter", 0, DemoClass.TS2Count.Value);
		}

		public void TestIncludesMessageFilters()
		{
			var locator = new MailFilterLocator();
			Assert("The MAP filters should also be included", locator.TryGetFilter("MAP", out var filter));
			Assert("Should be the correct filter", filter is MessageFilterMailFilter);
		}

		public void TestTryGetValueCodeMatchesFilterCode()
		{
			var locator = new MailFilterLocator();
			foreach (var filter in locator.GetFilters())
			{
				Assert("If we can find it through iteration then we should be able to TryGet it", locator.TryGetFilter(filter.Code, out var other));
				AssertEquals("It looks like your attribute doesn't match your filter code", filter.Code, other.Code);
			}
		}
	}
}
