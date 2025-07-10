using NUnit.Framework;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestsSubclassesOf(typeof(ExternalChargeableUsageProvider))]
	public abstract class ExternalChargeableUsageProviderTestCase<T> : TestCase
			where T : ExternalChargeableUsageProvider
	{
		public abstract void TestBulkCopyUsages();

		public abstract void TestLoadRawUsage();
	}
}
