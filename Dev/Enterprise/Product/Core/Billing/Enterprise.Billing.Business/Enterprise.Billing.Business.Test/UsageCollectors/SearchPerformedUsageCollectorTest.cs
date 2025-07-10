using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Billing.Business.Testing
{
	[TestedType(typeof(SearchPerformedUsageCollector))]
	public class SearchPerformedUsageCollectorTest : TransactionedTestCase
	{
		public void TestUsageCollector_WhenEnableGlowIndexSearchUsageCollectorRegistry_ShouldGenerateEDI()
		{
			using (GlowRegistry.Instance.GlowIndexSearchUsageCollector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				new SearchPerformedUsageCollector().Report("ModuleIndexSearch", "GlbStaff", "FullName");

				var properties = new List<(string name, object value)>
				{
					("SearchType", "ModuleIndexSearch"),
					("ModuleID", "GlbStaff"),
					("Filters", "FullName"),
				};
				Assert("Contains message.", Helper.AssertUsageMessagesContains("SPF", properties));
			}
		}

		public void TestUsageCollector_WhenDisableGlowIndexSearchUsageCollectorRegistry_ShouldNotGenerateEDI()
		{
			using (GlowRegistry.Instance.GlowIndexSearchUsageCollector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				new SearchPerformedUsageCollector().Report("ModuleIndexSearch", "GlbStaff", "FullName");
				AssertEquals("Not contain message.", true, Helper.AssertUsageMessagesCount("SPF", 0));
			}
		}

		UsageCollectorTestHelper Helper => helper ?? (helper = new UsageCollectorTestHelper(new BusinessObjectFactory()));
		UsageCollectorTestHelper helper;
	}
}
