using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.Accounting.ElectronicMessaging.Common.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Registry.Testing
{
	[TestedType(typeof(SimultaneousInvoiceProcessingRateLimiterRegistryItem))]
	class SimultaneousInvoiceProcessingRateLimiterRegistryItemTest : StronglyTypedRegistryItemTestCase<int>
	{
		protected override StronglyTypedRegistryItem<int, int> GetNewRegistryItem()
		{
			return new SimultaneousInvoiceProcessingRateLimiterRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, new List<(string countryCode, int defaultNumberOfInvoicesProcessedSimultaneously)>() { }.ToImmutableList());
		}
	}
}
