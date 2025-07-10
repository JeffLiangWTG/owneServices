using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt.Testing
{
	[TestedType(typeof(EgyptGlobalXUEFunctionalityProvider))]
	class EgyptGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new EgyptEInvoicingObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(EgyptTransactionBatchEventMessageProcessor);
	}
}
