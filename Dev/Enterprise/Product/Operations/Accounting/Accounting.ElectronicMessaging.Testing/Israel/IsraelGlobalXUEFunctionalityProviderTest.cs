using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Israel.Testing
{
	[TestedType(typeof(IsraelGlobalXUEFunctionalityProvider))]
	class IsraelGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new IsraelEInvoicingObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(EInvoicingEventMessageILProcessor);

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported("IAK", "", false);
			AssertIsEventMessageProcessSupported("IAK", "***", false);
			AssertIsEventMessageProcessSupported("IAK", "GEN", true);

			AssertIsEventMessageProcessSupported("IRJ", "", false);
			AssertIsEventMessageProcessSupported("IRJ", "***", false);
			AssertIsEventMessageProcessSupported("IRJ", "GEN", true);

			AssertIsEventMessageProcessSupported("", "GEN", false);
			AssertIsEventMessageProcessSupported("ZZZ", "GEN", false);
		}
	}
}
