using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	[TestedType(typeof(HungaryGlobalXUEFunctionalityProvider))]
	class HungaryGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new HungaryEInvoicingObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(EInvoicingEventMessageHUProcessor);
	}
}
