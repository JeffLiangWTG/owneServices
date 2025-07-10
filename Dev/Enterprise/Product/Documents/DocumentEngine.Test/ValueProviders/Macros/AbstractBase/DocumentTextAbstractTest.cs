using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	abstract class DocumentTextAbstractTest : ValueProviderTest
	{
		public void TestCarrierBookingRequestOpeningAndClosingText()
		{
			SetCarrierBookingRequestOpeningAndClosingText("carrier booking request text");
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CarrierBookingRequest;
			AssertDocumentText("carrier booking request text");
		}

		public void TestDeliveryInformationOpeningAndClosingText()
		{
			SetCarrierBookingRequestOpeningAndClosingText("delivery information request text");
			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.CarrierBookingRequest;
			AssertDocumentText("delivery information request text");
		}

		public void TestTransportBookingOpeningAndClosingText()
		{
			AssertEquals(Constants.DocumentNames.CartageAdvice, "CARTAGE ADVICE");
			AssertEquals(Constants.DocumentNames.CartageAdviceWithReceipt, "CARTAGE ADVICE WITH RECEIPT");

			SetTransportBookingOpeningAndClosingText("booking text");
			AssertDocumentTextForTransportBooking(Constants.DocumentNames.CartageAdvice, "booking text");
		}

		public void AssertDocumentTextForTransportBooking(string reportName, string expectedOutput)
		{
			((IReportForUnitTesting)Report).fName = reportName;
			AssertDocumentText(expectedOutput);

			((IReportForUnitTesting)Report).fName = reportName + " WITH RECEIPT";
			AssertDocumentText(expectedOutput);
		}

		public void TestCarrierBookingRequestOpeningAndClosingTextInFench()
		{
			Report.Parent.Language = Core.SharedConstants.Languages.French;
			TestCarrierBookingRequestOpeningAndClosingText();
		}

		public void TestInvalidModeTypeFallsBackToAirValueIfSeaValueEqualsToAirValue()
		{
			SetDefaultOpeningClosingText(defaultText);
			SetAirArrivalNoticeOpeningAndClosingText("I want to set this registry value!");
			SetSeaArrivalNoticeOpeningAndClosingText("I want to set this registry value!");

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("WrongMode", "TST"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ArrivalDocName", "ARRIVAL NOTICE"));

			string macroString = string.Format("<{0}(ArrivalDocName, WrongMode)>", this.ValueProviderToTest.GetType().Name);

			AssertEquals("Should use air value if sea equals to air", "I want to set this registry value!", ValueProviderToTest.GetReplacement(macroString, Report));
		}

		public void TestInvalidModeTypeFallsBackToDefaultValueIfSeaValueDoesNotEqualToAirValue()
		{
			SetDefaultOpeningClosingText(defaultText);
			SetAirArrivalNoticeOpeningAndClosingText("I want to set this registry value!");
			SetSeaArrivalNoticeOpeningAndClosingText("I want to set another registry value!");

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("WrongMode", "TST"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ArrivalDocName", "ARRIVAL NOTICE"));

			string macroString = string.Format("<{0}(ArrivalDocName, WrongMode)>", this.ValueProviderToTest.GetType().Name);

			AssertEquals("Should use default value if sea does not equal to air", defaultText, ValueProviderToTest.GetReplacement(macroString, Report));
		}

		public void TestInvalidRegistryKeyFallsBackToDefault()
		{
			SetDefaultOpeningClosingText(defaultText);
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("InvalidDocName", "INVALID DOCNAME"));

			string macroString = string.Format("<{0}(InvalidDocName)>", this.ValueProviderToTest.GetType().Name);
			AssertEquals("Should use default text", defaultText, ValueProviderToTest.GetReplacement(macroString, Report));
		}

		public void TestIfOverrideTextBlankThenUseDefaultDocumentText()
		{
			SetDefaultOpeningClosingText(defaultText);
			AssertEquals("Env.Registry.DefaultDocumentText.OpeningText", defaultText, Env.Registry.DefaultDocumentText.OpeningText);
			AssertEquals("Env.Registry.ImportAirArrivalNotice.OpeningText", "", Env.Registry.ImportAirArrivalNotice.OpeningText);
			AssertEquals("Env.Registry.DefaultDocumentText.ClosingText", defaultText, Env.Registry.DefaultDocumentText.ClosingText);
			AssertEquals("Env.Registry.ImportAirArrivalNotice.ClosingText", "", Env.Registry.ImportAirArrivalNotice.ClosingText);

			Report.MacroTranslator.RegisterValueProvider(ValueProviderToTest);
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("QueryParameter", Constants.DocumentNames.AirArrivalNotice));

			AssertEquals("Should use default text if override value is blank", defaultText, ValueProviderToTest.GetReplacement(macroString, Report));
		}

		public void TestRequestForMissingDocumentsTextDifferentBusinessContexts()
		{
			Env.Registry.CustomsRequestForMissingDocuments = new DocumentOpenCloseText("Customs Text", "Customs Text");
			Env.Registry.ShipmentRequestForMissingDocuments = new DocumentOpenCloseText("Shipment Text", "Shipment Text");

			((IReportForUnitTesting)Report).fName = Constants.DocumentNames.RequestForMissingDocuments;
			AssertDocumentText("Shipment Text");

			StmMenuItem menuItem = new BusinessObjectFactory().New<StmMenuItem>();
			DocumentPack pack = new DocumentPack(menuItem);
			Report report = new Report(pack, ExcelTemplate);
			((IReportForUnitTesting)report).fName = Constants.DocumentNames.RequestForMissingDocuments;

			report.MenuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			AssertDocumentText(report, Constants.DocumentNames.RequestForMissingDocuments, "Shipment Text");

			report.MenuItem.SU_BusinessContext = nameof(BusinessContext.Customs);
			AssertDocumentText(report, Constants.DocumentNames.RequestForMissingDocuments, "Customs Text");
		}

		protected string defaultText = "Default";

		protected abstract string macroString { get; }

		protected void SetDefaultOpeningClosingText(string text)
		{
			Env.Registry.RawRegistry.DocumentOpeningText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
			Env.Registry.RawRegistry.DocumentClosingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
		}

		protected void SetAirArrivalNoticeOpeningAndClosingText(string text)
		{
			((IRegistryItem)Env.Registry.RawRegistry.ImportAirArrivalNoticeOpeningText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
			((IRegistryItem)Env.Registry.RawRegistry.ImportAirArrivalNoticeClosingText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
		}

		protected void SetSeaArrivalNoticeOpeningAndClosingText(string text)
		{
			((IRegistryItem)Env.Registry.RawRegistry.ImportSeaFreightArrivalNoticeOpeningText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
			((IRegistryItem)Env.Registry.RawRegistry.ImportSeaFreightArrivalNoticeClosingText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
		}

		protected void SetCarrierBookingRequestOpeningAndClosingText(string text)
		{
			((IRegistryItem)DocumentsDataRegistry.Instance.CarrierBookingRequestOpeningText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
			((IRegistryItem)DocumentsDataRegistry.Instance.CarrierBookingRequestClosingText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
		}

		protected void SetDeliveryInformationOpeningAndClosingText(string text)
		{
			((IRegistryItem)DocumentsDataRegistry.Instance.DeliveryInformationOpeningText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
			((IRegistryItem)DocumentsDataRegistry.Instance.DeliveryInformationClosingText).SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, text);
		}

		protected void AssertDocumentText(string reportName, MultilingualStringRegistryItem registryItem, string registryValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				((IReportForUnitTesting)Report).fName = reportName;
				AssertDocumentText(registryItem.Value);
			}
		}

		protected void AssertDocumentText(string registryValue)
		{
			AssertDocumentText(Report, registryValue);
		}

		protected void AssertDocumentText(Report report, string registryValue)
		{
			AssertDocumentText(report, "", registryValue);
		}

		protected void AssertDocumentText(Report report, string assertMsg, string registryValue)
		{
			SetDefaultOpeningClosingText(defaultText);
			string expected = string.IsNullOrEmpty(registryValue) ? defaultText : registryValue;
			expected = expected.Trim().Replace("\r", "");

			report.MacroTranslator.RegisterValueProvider(ValueProviderToTest);
			AssertEquals(assertMsg, expected, ValueProviderToTest.GetReplacement(macroString, report));
		}

		void SetTransportBookingOpeningAndClosingText(string text)
		{
			((IRegistryItem)DocumentsDataRegistry.Instance.TransportBookingCartageAdviceClosingText).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, text);
			((IRegistryItem)DocumentsDataRegistry.Instance.TransportBookingCartageAdviceOpeningText).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, text);
		}
	}
}
