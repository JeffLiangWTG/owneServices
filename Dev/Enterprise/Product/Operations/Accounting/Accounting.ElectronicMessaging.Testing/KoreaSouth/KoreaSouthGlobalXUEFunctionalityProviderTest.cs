using System;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthGlobalXUEFunctionalityProvider))]
	class KoreaSouthGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new KoreaSouthEInvoicingObjectFactory();

		protected override Type ExpectedEventMessageProcessor => typeof(KoreaSouthEInvoicingEventMessageProcessor);

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, "", false);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, "PQR", false);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, KoreaSouthEInvoiceAPICommandList.Codes.CallbackInvoceRequest, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, "", false);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, "PQR", false);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, KoreaSouthEInvoiceAPICommandList.Codes.CallbackInvoceRequest, true);
			AssertIsEventMessageProcessSupported("OTR", "GEN", false);
			AssertIsEventMessageProcessSupported("OTR", "PQR", false);
			AssertIsEventMessageProcessSupported("OTR", "", false);
		}
	}
}
