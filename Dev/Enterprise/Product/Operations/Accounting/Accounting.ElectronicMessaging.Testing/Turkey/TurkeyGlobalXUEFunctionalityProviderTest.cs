using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	[TestedType(typeof(TurkeyGlobalXUEFunctionalityProvider))]
	class TurkeyGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new TurkeyEInvoicingObjectFactory();

		public override void TestGetEventMessageProcessor()
		{
			var objects = GetEventMessageProcessorObjects();

			AssertGetEventMessageProcessor("IAK", "", objects, typeof(EInvoicingEventMessageTRProcessor));
			AssertGetEventMessageProcessor("IAK", "RIN", objects, typeof(EInvoicingEventMessageTRProcessor));
			AssertGetEventMessageProcessor("IAK", "RST", objects, typeof(EInvoicingEventMessageTRProcessor));
			AssertGetEventMessageProcessor("IRJ", "", objects, typeof(EInvoicingEventMessageTRProcessor));
			AssertGetEventMessageProcessor("IRJ", "RIN", objects, typeof(EInvoicingEventMessageTRProcessor));
			AssertGetEventMessageProcessor("IRJ", "RST", objects, typeof(EInvoicingEventMessageTRProcessor));
		}

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported("IAK", "", true);
			AssertIsEventMessageProcessSupported("IAK", "***", true);
			AssertIsEventMessageProcessSupported("IAK", "RIN", true);
			AssertIsEventMessageProcessSupported("IRJ", "", true);
			AssertIsEventMessageProcessSupported("IRJ", "***", true);
			AssertIsEventMessageProcessSupported("IRJ", "RIN", true);
			AssertIsEventMessageProcessSupported("ZZZ", "RIN", false);
		}
	}
}
