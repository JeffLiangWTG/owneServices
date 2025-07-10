using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia.Testing
{
	[TestedType(typeof(MalaysiaGlobalXUEFunctionalityProvider))]
	class MalaysiaGlobalXUEFunctionalityProviderTest : GlobalXUEFunctionalityProviderTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new MalaysiaEInvoicingObjectFactory();

		public override void TestIsEventMessageProcessSupported()
		{
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocument, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail, true);

			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocument, true);
			AssertIsEventMessageProcessSupported(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail, true);
		}

		public override void TestGetEventMessageProcessor()
		{
			var objects = GetEventMessageProcessorObjects();

			AssertGetEventMessageProcessor(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, objects, typeof(EInvoicingEventMessageMYProcessor));
			AssertGetEventMessageProcessor(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, objects, typeof(EInvoicingEventMessageMYProcessor));
			AssertGetEventMessageProcessor(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocument, objects, typeof(EInvoicingEventMessageMYProcessor));
			AssertGetEventMessageProcessor(AutoEvents.InterchangeAcknowledgedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail, objects, typeof(EInvoicingEventMessageMYProcessor));

			AssertGetEventMessageProcessor(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction, objects, typeof(EInvoicingEventMessageMYProcessor));
			AssertGetEventMessageProcessor(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.GetSubmission, objects, typeof(EInvoicingEventMessageMYProcessor));
			AssertGetEventMessageProcessor(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocument, objects, typeof(EInvoicingEventMessageMYProcessor));
			AssertGetEventMessageProcessor(AutoEvents.InterchangeRejectedCode, MalaysiaEInvoiceAPICommandList.Codes.GetDocumentDetail, objects, typeof(EInvoicingEventMessageMYProcessor));
		}
	}
}
