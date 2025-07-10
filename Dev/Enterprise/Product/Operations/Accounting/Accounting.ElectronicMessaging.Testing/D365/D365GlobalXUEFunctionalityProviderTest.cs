using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.D365.Testing
{
	[TestedType(typeof(D365GlobalXUEFunctionalityProvider))]
	class D365GlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new D365ObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(TransactionBatchEventMessageProcessor);
	}
}
