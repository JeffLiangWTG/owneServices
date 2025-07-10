using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class DeltaIEInboundMessageCreatorTest : TestCaseWithFactory
	{
		public void TestCreateMessagesForInterchange()
		{
			var messages = AssertInterchangeCanBeProcessedToMessages(@"{""SchemaId"":""IE426"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":{""LRN"":""0000005856"",""declarationType"":""IM"",""additionalDeclarationType"":""F"",""presentationNotificationEstimatedDateAndTime"":""2023-04-03T00:00:00"",""languageCode"":""FR""},""CustomsOfficeOfPresentation"":{""referenceNumber"":""""},""SupervisingCustomsOffice"":{""referenceNumber"":""""},""CustomsOfficesOfDischarge"":{},""Importer"":{""identificationNumber"":""ESSSSS""},""Declarant"":{""identificationNumber"":""IT123654789""},""PersonProvidingAGuarantee"":{""identificationNumber"":""""},""PersonPayingCustomsDuty"":{""identificationNumber"":""""},""Representative"":{""identificationNumber"":"""",""status"":""2""},""CurrencyExchange"":{""internalCurrencyUnit"":""EUR""},""GoodsShipment"":[{""sequenceNumber"":""0"",""natureOfTransaction"":"""",""invoiceCurrency"":""EUR"",""dateOfAcceptance"":""2023-04-03T00:00:00"",""exchangeRate"":1.0,""Exporter"":{""identificationNumber"":"""",""name"":""SOBECA"",""Address"":{""streetAndNumber"":""BP53, "",""postcode"":""98845"",""city"":""NOUMEA CEDEX"",""country"":""NC""}},""DeliveryTerms"":{""incotermCode"":""FOB"",""UNLOCODE"":"""",""location"":"""",""country"":"""",""text"":""""},""CountryOfDispatch"":{""countryOfDispatch"":""NC""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""""},""Consignment"":{""containerIndicator"":"""",""inlandModeOfTransport"":"""",""modeOfTransportAtTheBorder"":"""",""referenceNumberUCR"":""3IT123654789-B221945"",""LocationOfGoods"":{""typeOfLocation"":"""",""qualifierOfIdentification"":""""},""ArrivalTransportMeans"":{""typeOfIdentification"":""0"",""identificationNumber"":""""},""ActiveBorderTransportMeans"":{""nationality"":""""}},""GoodsShipmentItem"":[{""sequenceNumber"":""1"",""declarationGoodsItemNumber"":""1"",""natureOfTransaction"":"""",""referenceNumberUCR"":"""",""dateOfAcceptance"":""2023-04-03T00:00:00"",""Procedure"":{""requestedProcedure"":"""",""previousProcedure"":""""},""Origin"":{""countryOfOrigin"":"""",""countryOfPreferentialOrigin"":""NC""},""CountryOfDispatch"":{""countryOfDispatch"":""NCNOU""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""FR""},""Commodity"":{""descriptionOfGoods"":""732497"",""CUSCode"":"""",""quotaOrderNumber"":"""",""CommodityCode"":{""harmonizedSystemSubheadingCode"":"""",""combinedNomenclatureCode"":"""",""taricCode"":"""",""TaricAdditionalCode"":[{""sequenceNumber"":"""",""taricAdditionalCode"":""""}],""NationalAdditionalCode"":[{""sequenceNumber"":"""",""nationalAdditionalCode"":"""",""ccQualifier"":""FR""}]},""GoodsMeasure"":{""nationalMeasurementUnitAndQualifier"":""""},""InvoiceLine"":{},""CalculationOfTaxes"":{""preference"":""""}},""Packaging"":[{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""}],""SupportingDocument"":[{""sequenceNumber"":""0"",""type"":""N380"",""ccQualifier"":""FR"",""referenceNumber"":""1"",""documentLineItemNumber"":""0"",""issuingAuthorityName"":"""",""dateOfValidity"":"""",""currency"":""""}],""CustomsValuation"":{""valuationMethod"":""1""},""ValuationAdjustment"":{""valuationIndicators"":""0000""}}]}]}}", "426");
		}

		public void TestCreateMessagesForInterchange_IE456_MultipleLRNShallBeParsedToDifferentMessages()
		{
			var multiLRNMessageText = @"{""SchemaId"":""IE456"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":[{""LRN"":""0000007767"",""customsRegistrationNumber"":""24FRD0000089108CR1"",""MRN"":""24FRD2300089108MR4"",""businessRejectionType"":""414"",""rejectionDateAndTime"":""2024-09-24T13:45:39"",""rejectionCode"":""4"",""rejectionReason"":""N/A""},{""LRN"":""0000007718"",""customsRegistrationNumber"":""24FRD0000088449CR4"",""MRN"":""24FRD2300088449MR7"",""businessRejectionType"":""414"",""rejectionDateAndTime"":""2024-09-24T13:45:39"",""rejectionCode"":""4"",""rejectionReason"":""N/A""}],""SupervisingCustomsOffice"":{""referenceNumber"":""FR002300""},""Declarant"":{""identificationNumber"":""FR33159700500064""},""FunctionalError"":[{""sequenceNumber"":""3"",""errorPointer"":""cc414/Declarant/identificationNumber"",""errorCode"":""99"",""errorReason"":""Nat_210"",""remarks"":""Vous ne pouvez pas annuler ou demander linvalidation dune declaration que vous navez pas deposee."",""originalAttributeValue"":""FR33159700500064""},{""sequenceNumber"":""1"",""errorPointer"":""cc414/ImportOperation[0]/invalidationMotivation"",""errorCode"":""99"",""errorReason"":""Nat_212"",""remarks"":""Vous ne pouvez pas annuler une declaration validee."",""originalAttributeValue"":""ANNUL""},{""sequenceNumber"":""2"",""errorPointer"":""cc414/ImportOperation[1]/invalidationMotivation"",""errorCode"":""99"",""errorReason"":""Nat_212"",""remarks"":""Vous ne pouvez pas annuler une declaration validee."",""originalAttributeValue"":""ANNUL""},{""sequenceNumber"":""4"",""errorPointer"":""cc414/ImportOperation/customsRegistrationNumber"",""errorCode"":""92"",""errorReason"":""N/A"",""remarks"":""L�tat : BONAENLEVER de la d�claration ne permet pas cette action pour le code dinvalidation : ANNUL"",""originalAttributeValue"":""24FRD0000089108CR1""},{""sequenceNumber"":""5"",""errorPointer"":""cc414/ImportOperation/customsRegistrationNumber"",""errorCode"":""92"",""errorReason"":""N/A"",""remarks"":""L�tat : BONAENLEVER de la d�claration ne permet pas cette action pour le code dinvalidation : ANNUL"",""originalAttributeValue"":""24FRD0000088449CR4""}]}}";
			var messages = AssertInterchangeCanBeProcessedToMessages(multiLRNMessageText, "456", 2);

			var startIndex = multiLRNMessageText.IndexOf(@"""SupervisingCustomsOffice""");
			var commonMessageText = multiLRNMessageText.Substring(startIndex, multiLRNMessageText.Length - startIndex - 1);
			var importOp1 = @"""ImportOperation"":[{""LRN"":""0000007767"",""customsRegistrationNumber"":""24FRD0000089108CR1"",""MRN"":""24FRD2300089108MR4"",""businessRejectionType"":""414"",""rejectionDateAndTime"":""2024-09-24T13:45:39"",""rejectionCode"":""4"",""rejectionReason"":""N/A""}]";
			var importOp2 = @"""ImportOperation"":[{""LRN"":""0000007718"",""customsRegistrationNumber"":""24FRD0000088449CR4"",""MRN"":""24FRD2300088449MR7"",""businessRejectionType"":""414"",""rejectionDateAndTime"":""2024-09-24T13:45:39"",""rejectionCode"":""4"",""rejectionReason"":""N/A""}]";

			AssertEquals(@$"{{{importOp1},{commonMessageText}", messages[0].EM_MessageText);
			AssertEquals("MessageNum should be EI_InterchangeNumber plus sequence suffix if only multiple messages created.", "237/1", messages[0].EM_MessageNum);

			AssertEquals(@$"{{{importOp2},{commonMessageText}", messages[1].EM_MessageText); 
			AssertEquals("MessageNum should be EI_InterchangeNumber plus sequence suffix if only multiple messages created.", "237/2", messages[1].EM_MessageNum);
		}

		public void TestCreateMessagesForInterchange_IE456_SingleLRNCanBeProcessedCorrectly()
		{
			AssertInterchangeCanBeProcessedToMessages(@"{""SchemaId"":""IE456"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":[{""LRN"":""0000007718"",""customsRegistrationNumber"":""24FRD0000088449CR4"",""MRN"":""24FRD2300088449MR7"",""businessRejectionType"":""414"",""rejectionDateAndTime"":""2024-09-24T13:45:39"",""rejectionCode"":""4"",""rejectionReason"":""N/A""}],""SupervisingCustomsOffice"":{""referenceNumber"":""FR002300""},""Declarant"":{""identificationNumber"":""FR33159700500064""},""FunctionalError"":[{""sequenceNumber"":""3"",""errorPointer"":""cc414/Declarant/identificationNumber"",""errorCode"":""99"",""errorReason"":""Nat_210"",""remarks"":""Vous ne pouvez pas annuler ou demander linvalidation dune declaration que vous navez pas deposee."",""originalAttributeValue"":""FR33159700500064""},{""sequenceNumber"":""1"",""errorPointer"":""cc414/ImportOperation[0]/invalidationMotivation"",""errorCode"":""99"",""errorReason"":""Nat_212"",""remarks"":""Vous ne pouvez pas annuler une declaration validee."",""originalAttributeValue"":""ANNUL""},{""sequenceNumber"":""2"",""errorPointer"":""cc414/ImportOperation[1]/invalidationMotivation"",""errorCode"":""99"",""errorReason"":""Nat_212"",""remarks"":""Vous ne pouvez pas annuler une declaration validee."",""originalAttributeValue"":""ANNUL""},{""sequenceNumber"":""4"",""errorPointer"":""cc414/ImportOperation/customsRegistrationNumber"",""errorCode"":""92"",""errorReason"":""N/A"",""remarks"":""L�tat : BONAENLEVER de la d�claration ne permet pas cette action pour le code dinvalidation : ANNUL"",""originalAttributeValue"":""24FRD0000089108CR1""},{""sequenceNumber"":""5"",""errorPointer"":""cc414/ImportOperation/customsRegistrationNumber"",""errorCode"":""92"",""errorReason"":""N/A"",""remarks"":""L�tat : BONAENLEVER de la d�claration ne permet pas cette action pour le code dinvalidation : ANNUL"",""originalAttributeValue"":""24FRD0000088449CR4""}]}}", "456");
		}

		EDIMessage[] AssertInterchangeCanBeProcessedToMessages(string bodyText, string expectedMessageSubType ,int expectedMessageCount = 1)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = bodyText;
			Factory.Save();

			var creator = new DeltaIEInboundMessageCreator() as IInboundMessageCreator;
			creator.CreateMessagesForInterchange(interchange);

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertEquals("Specific quantity of messages should be created.", expectedMessageCount, messages.Length);
			foreach (var message in messages)
			{
				AssertType<DeltaIEFREDIMessage>("All messages should be created as DeltaIEFREDIMessage.", message);
				AssertEquals("Message should be linked to interchange", interchange.PK, message.EM_EI);
				AssertEquals("EM_MessageType should be DEC.", MessageTypeList.Codes.DEC, message.EM_MessageType);
				AssertEquals("EM_MessageSubType should be retrieved from schemaID.", expectedMessageSubType, message.EM_MessageSubType);
			}
			AssertEquals("interchange.ContainedMessages should have same quantity.", expectedMessageCount, interchange.ContainedMessages.Count);
			AssertContainsExactElementsInAnyOrder("All messages should correctly be added into interchange.ContainedMessages.", messages.Select(m => m.PK), interchange.ContainedMessages.Select(x => x.PK));

			if (expectedMessageCount == 1)
			{
				var expectedMessageText = bodyText.Substring(bodyText.IndexOf(@"{""ImportOperation"), bodyText.Length - bodyText.IndexOf(@"{""ImportOperation") - 1);
				AssertEquals(expectedMessageText, messages[0].EM_MessageText);
				AssertEquals("MessageNum should be EI_InterchangeNumber if only one message created.", "237", messages[0].EM_MessageNum);
			}
			return messages;
		}
	}
}
