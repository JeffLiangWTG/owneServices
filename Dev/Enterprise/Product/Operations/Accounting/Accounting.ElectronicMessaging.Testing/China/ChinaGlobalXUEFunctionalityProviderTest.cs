using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	[TestedType(typeof(ChinaGlobalXUEFunctionalityProvider))]
	class ChinaGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new ChinaEInvoicingObjectFactory();

		protected override void AssertEventMessageChildrenProcessor(EInvoicingEventMessageProcessor childrenProcessor)
		{
			AssertNotNull(childrenProcessor);
			AssertEquals(typeof(EInvoicingEventMessageCNProcessorForEvent), childrenProcessor.GetType());
		}

		public override void TestGetEventMessageProcessor()
		{
			var objects = GetEventMessageProcessorObjects();

			AssertGetEventMessageProcessor("IAK", "", objects, typeof(EInvoicingEventMessageCNProcessor));
			AssertGetEventMessageProcessor("IAK", "GEN", objects, typeof(EInvoicingEventMessageCNProcessor));
			AssertGetEventMessageProcessor("IAK", "RDN", objects, typeof(EInvoicingEventMessageCNProcessor));
			AssertGetEventMessageProcessor("IRJ", "", objects, typeof(EInvoicingEventMessageCNProcessor));
			AssertGetEventMessageProcessor("IRJ", "GEN", objects, typeof(EInvoicingEventMessageCNProcessor));
			AssertGetEventMessageProcessor("IRJ", "RDN", objects, typeof(EInvoicingEventMessageCNProcessor));
		}

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported("IAK", "", true);
			AssertIsEventMessageProcessSupported("IAK", "GEN", true);
			AssertIsEventMessageProcessSupported("IAK", "RDN", true);
			AssertIsEventMessageProcessSupported("IRJ", "", true);
			AssertIsEventMessageProcessSupported("IRJ", "GEN", true);
			AssertIsEventMessageProcessSupported("IRJ", "RDN", true);
		}

		public override void TestIsEventMessageChildrenProcessSupported()
		{
			var isEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsEventMessageChildrenProcessSupported("IAK");
			Assert(isEventMessageProcessSupported);

			isEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsEventMessageChildrenProcessSupported("XXX");
			Assert(!isEventMessageProcessSupported);
		}

		public override void TestIsInvoiceEventMessageProcessSupported()
		{
			var isInvoiceEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsInvoiceEventMessageProcessSupported("SIU");
			Assert(isInvoiceEventMessageProcessSupported);

			isInvoiceEventMessageProcessSupported = GetGlobalXUEFunctionalityProvider.IsInvoiceEventMessageProcessSupported("XXX");
			Assert(!isInvoiceEventMessageProcessSupported);
		}
	}
}
