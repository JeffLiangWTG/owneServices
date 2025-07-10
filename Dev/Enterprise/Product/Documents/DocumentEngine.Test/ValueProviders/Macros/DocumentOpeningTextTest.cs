using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DocumentOpeningText))]
	sealed class DocumentOpeningTextTest : DocumentTextAbstractTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <docment openning>", !ValueProviderToTest.IsResponsibleForReplacing("<docment openning>", Passes.FirstPass));
			Assert("should match <documentopening>", ValueProviderToTest.IsResponsibleForReplacing("<documentopening>", Passes.FirstPass));
			Assert("should match < document      opening ()  >", ValueProviderToTest.IsResponsibleForReplacing("< document      opening ()  >", Passes.FirstPass));
			Assert("should match <DocumentOpening ()>", ValueProviderToTest.IsResponsibleForReplacing("<DocumentOpening ()>", Passes.FirstPass));
			Assert("should match <DocumentOpening( param ) >", ValueProviderToTest.IsResponsibleForReplacing("<DocumentOpening( param )>", Passes.FirstPass));
			Assert("should match <DocumentOpening( param1.param param2) >", ValueProviderToTest.IsResponsibleForReplacing("<DocumentOpening( param1.param param2) >", Passes.FirstPass));
			Assert("should match <DocumentOpening( param1.param param2, mode) >", ValueProviderToTest.IsResponsibleForReplacing("<DocumentOpening( param1.param param2, mode) >", Passes.FirstPass));
		}

		public void TestAirDocumentsOpeningTextReplacement()
		{
			SetDefaultOpeningClosingText("Please insert text here");
			AssertEquals("Please insert text here", ValueProviderToTest.GetReplacement("<documentopening(something)>", Report));

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirPreAlert;
			AssertDocumentText(Env.Registry.ImportAirPreAlert.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirArrivalNotice;
			AssertDocumentText(Env.Registry.ImportAirArrivalNotice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirUltimateConsigneePreAlert;
			AssertDocumentText(Env.Registry.ImportAirUltimateConsigneePreAlert.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirUltimateConsigneeArrivalNotice;
			AssertDocumentText(Env.Registry.ImportAirUltimateConsigneeArrivalNotice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirLetterToOverseasAgent;
			AssertDocumentText(Env.Registry.ExportAirLetterToOverseasAgent.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirShipperDepartureNotice;
			AssertDocumentText(Env.Registry.ExportAirFreightShipperDepartureNotice.OpeningText);
		}

		public void TestOrderDocumentsOpeningTextReplacement()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ImportOrderStatus;
			AssertDocumentText(Env.Registry.ImportOrderStatus.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ExportOrderStatus;
			AssertDocumentText(Env.Registry.ExportOrderStatus.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ImportOrderNotification;
			AssertDocumentText(Env.Registry.ImportOrderNotification.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ExportOrderNotification;
			AssertDocumentText(Env.Registry.ExportOrderNotification.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ImportOrderAdvice;
			AssertDocumentText(Env.Registry.ImportOrderAdvice.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ExportOrderAdvice;
			AssertDocumentText(Env.Registry.ExportOrderAdvice.OpeningText);
		}

		public void TestSeaDocumentsOpeningTextReplacement()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaPreAlert;
			AssertDocumentText(Env.Registry.ImportSeaFreightPreAlert.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaArrivalNotice;
			AssertDocumentText(Env.Registry.ImportSeaFreightArrivalNotice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AgencyShipmentArrivalNotice;
			AssertDocumentText(DocumentsDataRegistry.Instance.AgencyShipmentArrivalNotice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaDeliveryOrder;
			AssertDocumentText(Env.Registry.ImportSeaFreightDeliveryOrder.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaUltimateConsigneePreAlert;
			AssertDocumentText(Env.Registry.ImportSeaUltimateConsigneePreAlert.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaUltimateConsigneeArrivalNotice;
			AssertDocumentText(Env.Registry.ImportSeaUltimateConsigneeArrivalNotice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaAgentDepartureNotice;
			AssertDocumentText(Env.Registry.ExportSeaFreightAgentDepartureNotice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaShipperDepartureNotice;
			AssertDocumentText(Env.Registry.ExportSeaFreightShipperDepartureNotice.OpeningText);
		}

		public void TestSeaCargoWeightsAndMeasurementsReport()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.SeaCargoWeightsAndMeasurementsReport;
			AssertDocumentText(Env.Registry.SeaWeightsAndMeasurements.OpeningText);
		}

		public void TestContainerDetentionReminderDocument()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ContainerDetentionReminder;
			AssertDocumentText(DocumentsDataRegistry.Instance.ContainerDetentionReminderOpeningText.Value);
		}

		public void TestAirCargoWeightsAndMeasurementsReport()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AirCargoWeightsAndMeasurementsReport;
			AssertDocumentText(Env.Registry.AirWeightsAndMeasurements.OpeningText);
		}

		public void TestCartageAdviceTimeSlotRequest()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CartageAdviceTimeSlotRequest;
			AssertDocumentText(Env.Registry.CartageAdviceTimeSlotRequestImportOpeningText);
		}

		public void TestContainerReleaseReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ContainerRelease;
			AssertDocumentText(DocumentsDataRegistry.Instance.ContainerRelease.Text(DocumentTextType.Open));
		}

		public void TestOneOffPricingPageMultipleCarriersReplacement()
		{
			AssertStmMenuTemplatePivot(Factory, new Guid("0F5D50CA-6E42-4F06-9B47-92F4E7690F22"), Constants.DocumentNames.OneOffPricingPageMultipleCarriers);

			AssertDocumentText(Constants.DocumentNames.OneOffPricingPageMultipleCarriers, DocumentsDataRegistry.Instance.QuoteOpeningText, "One Off Pricing Page - Opening Text");
		}

		public void TestOneOffPricingPageReplacement()
		{
			AssertStmMenuTemplatePivot(Factory, new Guid("27fc73a6-a49f-4a6a-a948-22fa2304fe62"), Constants.DocumentNames.OneOffPricingPage);
			AssertStmMenuTemplatePivot(Factory, new Guid("8124f541-493d-434c-97fa-6ec8496d2404"), Constants.DocumentNames.OneOffPricingPage);

			AssertDocumentText(Constants.DocumentNames.OneOffPricingPage, DocumentsDataRegistry.Instance.QuoteOpeningText, "One Off Pricing Page - Opening Text");
		}

		public void TestQuotationAcceptanceReplacement()
		{
			AssertStmMenuTemplatePivot(Factory, new Guid("2205b610-7440-4a96-8310-605802a5b5cd"), Constants.DocumentNames.QuotationAcceptance);

			AssertDocumentText(Constants.DocumentNames.QuotationAcceptance, DocumentsDataRegistry.Instance.AcceptancePageOpeningText, "Acceptance Page - Opening Text");
		}

		static void AssertStmMenuTemplatePivot(BusinessObjectFactory factory, Guid stmMenuTemplatePK, string expected)
		{
			var pivot = factory.Load<StmMenuTemplatePivotBase>(new CargoWise.Types.ZGuid(stmMenuTemplatePK));
			AssertEquals
			(
				$"StmMenuTemplatePivot DocumentTitle with PK='{stmMenuTemplatePK}' should match",
				expected,
				pivot.SI_DocumentTitle.ToUpperInvariant()
			);
		}

		public void TestDetentionAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.DetentionAdvice;
			AssertDocumentText(DocumentsDataRegistry.Instance.DetentionAdviceOpeningText.Value);
		}

		public void TestAgencyBookingConfirmationReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AgencyBookingConfirmation;
			AssertDocumentText(DocumentsDataRegistry.Instance.AgencyBookingConfirmationOpeningText.Value);
		}

		public void TestShipmentCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdvice;
			AssertDocumentText(Env.Registry.CartageAdviceImport.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CartageAdviceImport.OpeningText);
		}

		public void TestShipmentCartageAdviceWithRoutingReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.ShipmentCartageAdviceWithRouting;
			AssertDocumentText(Env.Registry.CartageAdviceImport.OpeningText);
		}

		public void TestCFSCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSCartageAdvice;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.OpeningText);
		}

		public void TestCFSShipmentCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSShipmentCartageAdvice;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CFSShipmentCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CFSCartageAdvice.OpeningText);
		}

		public void TestBookingCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.BookingCartageAdvice;
			AssertDocumentText(Env.Registry.BookingCartageAdvice.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.BookingCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.BookingCartageAdvice.OpeningText);
		}

		public void TestLocalCartageCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalCartageAdvice;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.OpeningText);
		}

		public void TestLocalTransportCartageAdviceReplacements_DocBuilder()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalCartageAdvice;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.OpeningText);
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.LocalMultiContainerCartageAdvice;
			AssertDocumentText(Env.Registry.LocalCartageCartageAdvice.OpeningText);
		}

		public void TestCustomsDeclarationCartageAdviceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CustomsDeclarationCartageAdvice;
			AssertDocumentText(Env.Registry.CustomsDeclarationCartageAdvice.OpeningText);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CustomsDeclarationCartageAdviceWithReceipt;
			AssertDocumentText(Env.Registry.CustomsDeclarationCartageAdvice.OpeningText);
		}

		public void TestRequestForServiceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.RequestForService;
			AssertDocumentText(DocumentsDataRegistry.Instance.RequestForServiceOpeningText.Value);
		}

		public void TestTransportBookingCartageAdvice()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CartageAdvice;
			AssertDocumentText(DocumentsDataRegistry.Instance.TransportBookingCartageAdviceOpeningText.Value);

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CartageAdviceWithReceipt;
			AssertDocumentText(DocumentsDataRegistry.Instance.TransportBookingCartageAdviceOpeningText.Value);
		}

		public void TestAuthorisationForServiceReplacements()
		{
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.AuthorisationForService;
			AssertDocumentText(DocumentsDataRegistry.Instance.AuthorisationForServiceOpeningText.Value);
		}

		#region Implementation
		protected override ValueProvider GetNewValueProvider()
		{
			return new DocumentOpeningText();
		}

		protected override string macroString
		{
			get { return "<documentOpening(ReportName)>"; }
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			SetAirArrivalNoticeOpeningAndClosingText("Some opening text");
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("LEVEL", "Air"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TransportMode", "Air"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TransportStatus", "Arrival"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Note", "Notice"));
			Report.Name = "Arrival Notice";
		}

		#endregion
	}
}
