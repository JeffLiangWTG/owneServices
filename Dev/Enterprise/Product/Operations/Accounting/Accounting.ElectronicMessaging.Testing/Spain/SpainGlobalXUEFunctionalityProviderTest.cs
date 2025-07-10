using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Spain.Testing
{
	[TestedType(typeof(SpainGlobalXUEFunctionalityProvider))]
	class SpainGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new SpainEInvoicingObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(TransactionBatchEventMessageProcessor);
	}
}
