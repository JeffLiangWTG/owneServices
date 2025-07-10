using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	[TestedType(typeof(RomaniaGlobalXUEFunctionalityProvider))]
	class RomaniaGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new RomaniaEInvoicingObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(RomaniaEInvoicingEventMessageProcessor);

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported("IAK", "", false);
			AssertIsEventMessageProcessSupported("IAK", "XYZ", false);
			AssertIsEventMessageProcessSupported("IAK", "GEN", true);
			AssertIsEventMessageProcessSupported("IAK", "GEQ", true);

			AssertIsEventMessageProcessSupported("IRJ", "", false);
			AssertIsEventMessageProcessSupported("IRJ", "XYZ", false);
			AssertIsEventMessageProcessSupported("IRJ", "GEN", true);
			AssertIsEventMessageProcessSupported("IRJ", "GEQ", true);

			AssertIsEventMessageProcessSupported("OTR", "GEN", false);
			AssertIsEventMessageProcessSupported("OTR", "GEQ", false);
			AssertIsEventMessageProcessSupported("OTR", "XYZ", false);
			AssertIsEventMessageProcessSupported("OTR", "", false);
		}
	}
}
