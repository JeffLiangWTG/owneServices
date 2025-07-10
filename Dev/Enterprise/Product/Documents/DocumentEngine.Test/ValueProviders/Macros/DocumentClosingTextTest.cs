using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DocumentClosingText))]
	sealed class DocumentClosingTextTest : DocumentTextAbstractTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <docment closing>", !ValueProviderToTest.IsResponsibleForReplacing("<docment closing>", Passes.FirstPass));
			Assert("should match < document      closing       >", ValueProviderToTest.IsResponsibleForReplacing("< document      closing       >", Passes.FirstPass));
			Assert("should match <DocumentClosing>", ValueProviderToTest.IsResponsibleForReplacing("<DocumentClosing>", Passes.FirstPass));
			Assert("should match < document      closing ()  >", ValueProviderToTest.IsResponsibleForReplacing("< document      closing ()  >", Passes.FirstPass));
			Assert("should match <DocumentClosing () >", ValueProviderToTest.IsResponsibleForReplacing("<DocumentClosing () >", Passes.FirstPass));
			Assert("should match <DocumentClosing( param ) >", ValueProviderToTest.IsResponsibleForReplacing("<DocumentClosing( param )>", Passes.FirstPass));
			Assert("should match <DocumentClosing( param1.param param2) >", ValueProviderToTest.IsResponsibleForReplacing("<DocumentClosing( param1.param param2) >", Passes.FirstPass));
			Assert("should match <DocumentClosing( param1.param param2, mode) >", ValueProviderToTest.IsResponsibleForReplacing("<DocumentClosing( param1.param param2, mode) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			SetDefaultOpeningClosingText(defaultText);
			AssertEquals(defaultText, ValueProviderToTest.GetReplacement("<documentclosing>", Report));
			AssertEquals(defaultText, ValueProviderToTest.GetReplacement("<documentclosing()>", Report));
			AssertEquals(defaultText, ValueProviderToTest.GetReplacement("<documentclosing(Test)>", Report));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirPreAlert;
			AssertDocumentText(Env.Registry.ImportAirPreAlert.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirArrivalNotice;
			AssertDocumentText(Env.Registry.ImportAirArrivalNotice.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirUltimateConsigneePreAlert;
			AssertDocumentText(Env.Registry.ImportAirUltimateConsigneePreAlert.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirUltimateConsigneeArrivalNotice;
			AssertDocumentText(Env.Registry.ImportAirUltimateConsigneeArrivalNotice.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirLetterToOverseasAgent;
			AssertDocumentText(Env.Registry.ExportAirLetterToOverseasAgent.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirShipperDepartureNotice;
			AssertDocumentText(Env.Registry.ExportAirFreightShipperDepartureNotice.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaPreAlert;
			AssertDocumentText(Env.Registry.ImportSeaFreightPreAlert.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaArrivalNotice;
			AssertDocumentText(Env.Registry.ImportSeaFreightArrivalNotice.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AgencyShipmentArrivalNotice;
			AssertDocumentText(DocumentsDataRegistry.Instance.AgencyShipmentArrivalNotice.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaDeliveryOrder;
			AssertDocumentText(Env.Registry.ImportSeaFreightDeliveryOrder.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaUltimateConsigneePreAlert;
			AssertDocumentText(Env.Registry.ImportSeaUltimateConsigneePreAlert.Text(DocumentTextType.Close));
		}

		public void TestOrderDocumentReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ImportOrderStatus;
			AssertDocumentText(Env.Registry.ImportOrderStatus.ClosingText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ExportOrderStatus;
			AssertDocumentText(Env.Registry.ExportOrderStatus.ClosingText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ImportOrderNotification;
			AssertDocumentText(Env.Registry.ImportOrderNotification.ClosingText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ExportOrderNotification;
			AssertDocumentText(Env.Registry.ExportOrderNotification.ClosingText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ImportOrderAdvice;
			AssertDocumentText(Env.Registry.ImportOrderAdvice.ClosingText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ExportOrderAdvice;
			AssertDocumentText(Env.Registry.ExportOrderAdvice.ClosingText);
		}

		public void TestContainerReleaseReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ContainerRelease;
			AssertDocumentText(DocumentsDataRegistry.Instance.ContainerRelease.Text(DocumentTextType.Close));
		}

		public void TestOneOffPricingPageMultipleCarriersReplacement()
			=> AssertDocumentText(Constants.DocumentNames.OneOffPricingPageMultipleCarriers, DocumentsDataRegistry.Instance.QuoteClosingText, "One Off Pricing Page - Closing Text");

		public void TestOneOffPricingPageReplacement()
			=> AssertDocumentText(Constants.DocumentNames.OneOffPricingPage, DocumentsDataRegistry.Instance.QuoteClosingText, "One Off Pricing Page - Closing Text");

		public void TestQuotationAcceptanceReplacement()
			=> AssertDocumentText(Constants.DocumentNames.QuotationAcceptance, DocumentsDataRegistry.Instance.AcceptancePageClosingText, "Acceptance Page - Closing Text");

		public void TestDetentionAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.DetentionAdvice;
			AssertDocumentText(DocumentsDataRegistry.Instance.DetentionAdviceClosingText.Value);
		}

		public void TestAgencyBookingConfirmationReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AgencyBookingConfirmation;
			AssertDocumentText(DocumentsDataRegistry.Instance.AgencyBookingConfirmationClosingText.Value);
		}

		public void TestShipmentCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdvice;
			AssertDocumentText(Env.Registry.CartageAdviceImport.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CartageAdviceImport.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdviceWithRouting;
			AssertDocumentText(Env.Registry.CartageAdviceImport.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdviceWithRoutingWithReceipt;
			AssertDocumentText(Env.Registry.CartageAdviceImport.Text(DocumentTextType.Close));
		}

		public void TestShipmentCartageAdviceWithRoutingReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdviceWithRouting;
			AssertDocumentText(Env.Registry.CartageAdviceImport.Text(DocumentTextType.Close));
		}

		public void TestCFSCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSCartageAdvice;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.Text(DocumentTextType.Close));
		}

		public void TestCFSShipmentCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSShipmentCartageAdvice;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSShipmentCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.Text(DocumentTextType.Close));
		}

		public void TestBookingCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.BookingCartageAdvice;
			AssertDocumentText(Env.Registry.BookingCartageAdvice.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.BookingCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.BookingCartageAdvice.Text(DocumentTextType.Close));
		}

		public void TestLocalCartageCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalCartageAdvice;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.Text(DocumentTextType.Close));
		}

		public void TestLocalTransportCartageAdviceReplacements_DocBuilder()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalCartageAdvice;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.Text(DocumentTextType.Close));
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalMultiContainerCartageAdvice;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.Text(DocumentTextType.Close));
		}

		public void TestCustomsDeclarationCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CustomsDeclarationCartageAdvice;
			AssertDocumentText(Env.Registry.CustomsDeclarationCartageAdvice.Text(DocumentTextType.Close));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CustomsDeclarationCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CustomsDeclarationCartageAdvice.Text(DocumentTextType.Close));
		}

		public void TestRequestForServiceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.RequestForService;
			AssertDocumentText(DocumentsDataRegistry.Instance.RequestForServiceClosingText.Value);
		}

		public void TestTransportBookingCartageAdvice()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CartageAdvice;
			AssertDocumentText(DocumentsDataRegistry.Instance.TransportBookingCartageAdviceClosingText.Value);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CartageAdviceWithReceipt;
			AssertDocumentText(DocumentsDataRegistry.Instance.TransportBookingCartageAdviceClosingText.Value);
		}

		public void TestAuthorisationForServiceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AuthorisationForService;
			AssertDocumentText(DocumentsDataRegistry.Instance.AuthorisationForServiceClosingText.Value);
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new DocumentClosingText();
		}

		protected override string macroString
		{
			get { return "<documentClosing(ReportName)>"; }
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			SetAirArrivalNoticeOpeningAndClosingText("Some closing text");
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("LEVEL", "Air"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TransportMode", "Air"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TransportStatus", "Arrival"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Note", "Notice"));
			Report.Name = "Arrival Notice";
		}

		#endregion
	}
}
