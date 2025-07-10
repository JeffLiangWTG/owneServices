using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Poland.Testing
{
	[TestedType(typeof(PolandGlobalXUEFunctionalityProvider))]
	class PolandGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new PolandEInvoicingObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(TransactionBatchEventMessageProcessor);
	}
}
