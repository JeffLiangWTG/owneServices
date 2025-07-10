using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class DeltaIEInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestDeltaIEResponseMessage()
		{
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "EASYLOG2TEST_EAD";
			intchg.EI_To = "HYEDFRCMT";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;
			intchg.EI_InterchangeNum = "237";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = @"{""SchemaId"":""IEH1F"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":{""LRN"":""0000005856"",""declarationType"":""IM"",""additionalDeclarationType"":""F"",""presentationNotificationEstimatedDateAndTime"":""2023-04-03T00:00:00"",""languageCode"":""FR""},""CustomsOfficeOfPresentation"":{""referenceNumber"":""""},""SupervisingCustomsOffice"":{""referenceNumber"":""""},""CustomsOfficesOfDischarge"":{},""Importer"":{""identificationNumber"":""ESSSSS""},""Declarant"":{""identificationNumber"":""IT123654789""},""PersonProvidingAGuarantee"":{""identificationNumber"":""""},""PersonPayingCustomsDuty"":{""identificationNumber"":""""},""Representative"":{""identificationNumber"":"""",""status"":""2""},""CurrencyExchange"":{""internalCurrencyUnit"":""EUR""},""GoodsShipment"":[{""sequenceNumber"":""0"",""natureOfTransaction"":"""",""invoiceCurrency"":""EUR"",""dateOfAcceptance"":""2023-04-03T00:00:00"",""exchangeRate"":1.0,""Exporter"":{""identificationNumber"":"""",""name"":""SOBECA"",""Address"":{""streetAndNumber"":""BP53, "",""postcode"":""98845"",""city"":""NOUMEA CEDEX"",""country"":""NC""}},""DeliveryTerms"":{""incotermCode"":""FOB"",""UNLOCODE"":"""",""location"":"""",""country"":"""",""text"":""""},""CountryOfDispatch"":{""countryOfDispatch"":""NC""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""""},""Consignment"":{""containerIndicator"":"""",""inlandModeOfTransport"":"""",""modeOfTransportAtTheBorder"":"""",""referenceNumberUCR"":""3IT123654789-B221945"",""LocationOfGoods"":{""typeOfLocation"":"""",""qualifierOfIdentification"":""""},""ArrivalTransportMeans"":{""typeOfIdentification"":""0"",""identificationNumber"":""""},""ActiveBorderTransportMeans"":{""nationality"":""""}},""GoodsShipmentItem"":[{""sequenceNumber"":""1"",""declarationGoodsItemNumber"":""1"",""natureOfTransaction"":"""",""referenceNumberUCR"":"""",""dateOfAcceptance"":""2023-04-03T00:00:00"",""Procedure"":{""requestedProcedure"":"""",""previousProcedure"":""""},""Origin"":{""countryOfOrigin"":"""",""countryOfPreferentialOrigin"":""NC""},""CountryOfDispatch"":{""countryOfDispatch"":""NCNOU""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""FR""},""Commodity"":{""descriptionOfGoods"":""732497"",""CUSCode"":"""",""quotaOrderNumber"":"""",""CommodityCode"":{""harmonizedSystemSubheadingCode"":"""",""combinedNomenclatureCode"":"""",""taricCode"":"""",""TaricAdditionalCode"":[{""sequenceNumber"":"""",""taricAdditionalCode"":""""}],""NationalAdditionalCode"":[{""sequenceNumber"":"""",""nationalAdditionalCode"":"""",""ccQualifier"":""FR""}]},""GoodsMeasure"":{""nationalMeasurementUnitAndQualifier"":""""},""InvoiceLine"":{},""CalculationOfTaxes"":{""preference"":""""}},""Packaging"":[{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""}],""SupportingDocument"":[{""sequenceNumber"":""0"",""type"":""N380"",""ccQualifier"":""FR"",""referenceNumber"":""1"",""documentLineItemNumber"":""0"",""issuingAuthorityName"":"""",""dateOfValidity"":"""",""currency"":""""}],""CustomsValuation"":{""valuationMethod"":""1""},""ValuationAdjustment"":{""valuationIndicators"":""0000""}}]}]}}";
			Factory.Save();

			var processor = new DeltaIEIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);

			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));
			AssertEquals(1, msg1.Length);

			AssertEquals(1, intchg.ContainedMessages.Count);
			AssertEquals(msg1[0].PK, intchg.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg.PK, msg1[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg1[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.DEC, msg1[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", "1", msg1[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg1[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "237", msg1[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg1[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, msg1[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg1[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText body text", @"{""ImportOperation"":{""LRN"":""0000005856"",""declarationType"":""IM"",""additionalDeclarationType"":""F"",""presentationNotificationEstimatedDateAndTime"":""2023-04-03T00:00:00"",""languageCode"":""FR""},""CustomsOfficeOfPresentation"":{""referenceNumber"":""""},""SupervisingCustomsOffice"":{""referenceNumber"":""""},""CustomsOfficesOfDischarge"":{},""Importer"":{""identificationNumber"":""ESSSSS""},""Declarant"":{""identificationNumber"":""IT123654789""},""PersonProvidingAGuarantee"":{""identificationNumber"":""""},""PersonPayingCustomsDuty"":{""identificationNumber"":""""},""Representative"":{""identificationNumber"":"""",""status"":""2""},""CurrencyExchange"":{""internalCurrencyUnit"":""EUR""},""GoodsShipment"":[{""sequenceNumber"":""0"",""natureOfTransaction"":"""",""invoiceCurrency"":""EUR"",""dateOfAcceptance"":""2023-04-03T00:00:00"",""exchangeRate"":1.0,""Exporter"":{""identificationNumber"":"""",""name"":""SOBECA"",""Address"":{""streetAndNumber"":""BP53, "",""postcode"":""98845"",""city"":""NOUMEA CEDEX"",""country"":""NC""}},""DeliveryTerms"":{""incotermCode"":""FOB"",""UNLOCODE"":"""",""location"":"""",""country"":"""",""text"":""""},""CountryOfDispatch"":{""countryOfDispatch"":""NC""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""""},""Consignment"":{""containerIndicator"":"""",""inlandModeOfTransport"":"""",""modeOfTransportAtTheBorder"":"""",""referenceNumberUCR"":""3IT123654789-B221945"",""LocationOfGoods"":{""typeOfLocation"":"""",""qualifierOfIdentification"":""""},""ArrivalTransportMeans"":{""typeOfIdentification"":""0"",""identificationNumber"":""""},""ActiveBorderTransportMeans"":{""nationality"":""""}},""GoodsShipmentItem"":[{""sequenceNumber"":""1"",""declarationGoodsItemNumber"":""1"",""natureOfTransaction"":"""",""referenceNumberUCR"":"""",""dateOfAcceptance"":""2023-04-03T00:00:00"",""Procedure"":{""requestedProcedure"":"""",""previousProcedure"":""""},""Origin"":{""countryOfOrigin"":"""",""countryOfPreferentialOrigin"":""NC""},""CountryOfDispatch"":{""countryOfDispatch"":""NCNOU""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""FR""},""Commodity"":{""descriptionOfGoods"":""732497"",""CUSCode"":"""",""quotaOrderNumber"":"""",""CommodityCode"":{""harmonizedSystemSubheadingCode"":"""",""combinedNomenclatureCode"":"""",""taricCode"":"""",""TaricAdditionalCode"":[{""sequenceNumber"":"""",""taricAdditionalCode"":""""}],""NationalAdditionalCode"":[{""sequenceNumber"":"""",""nationalAdditionalCode"":"""",""ccQualifier"":""FR""}]},""GoodsMeasure"":{""nationalMeasurementUnitAndQualifier"":""""},""InvoiceLine"":{},""CalculationOfTaxes"":{""preference"":""""}},""Packaging"":[{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""}],""SupportingDocument"":[{""sequenceNumber"":""0"",""type"":""N380"",""ccQualifier"":""FR"",""referenceNumber"":""1"",""documentLineItemNumber"":""0"",""issuingAuthorityName"":"""",""dateOfValidity"":"""",""currency"":""""}],""CustomsValuation"":{""valuationMethod"":""1""},""ValuationAdjustment"":{""valuationIndicators"":""0000""}}]}]}", msg1[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg1[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg1[0].EM_LinkUniqueID);
		}
	}
}
