using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia.Testing
{
	[TestedType(typeof(SaudiArabiaGlobalXUEFunctionalityProvider))]
	class SaudiArabiaGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new SaudiArabiaEInvoicingObjectFactory();

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported("IAK", "", false);
			AssertIsEventMessageProcessSupported("IAK", "XYZ", false);
			AssertIsEventMessageProcessSupported("IAK", "GEN", true);
			AssertIsEventMessageProcessSupported("IRJ", "", true);
			AssertIsEventMessageProcessSupported("IRJ", "XYZ", true);
			AssertIsEventMessageProcessSupported("IRJ", "GEN", true);
			AssertIsEventMessageProcessSupported("OTR", "GEN", false);
			AssertIsEventMessageProcessSupported("OTR", "XYZ", false);
			AssertIsEventMessageProcessSupported("OTR", "", false);
		}
	}
}
