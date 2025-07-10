using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	[TestedType(typeof(FRCustomsRetrieverServiceTask))]
	sealed class FRCustomsRetrieverServiceTaskTest : ServiceTaskTestCase<FRCustomsRetrieverServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attr = GetHostedServiceAttributes().FirstOrDefault();
			AssertEquals("60Seconds", attr.MinimumPeriod);
			AssertEquals(true, attr.CanRunInAnyBranch);
			AssertEquals("15minutes", attr.DefaultScheduleRunEvery);
		}

		public void TestCheckRecipientIDRegistrySetting_HostServiceRequirementIsDefined()
		{
			var methodInfo = typeof(FRCustomsRetrieverServiceTask).GetMethod(nameof(FRCustomsRetrieverServiceTask.CheckRecipientIDRegistrySetting));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestCheckRecipientIDRegistrySetting_ShouldReturnNoErrorMsg_WhenRegistryValueIsSet()
		{
			using (FRCustomsDataRegistry.Instance.RecipientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EASYLOG2_EAD"))
			{
				var message = FRCustomsRetrieverServiceTask.CheckRecipientIDRegistrySetting();
				AssertEquals(message, string.Empty);
			}
		}

		public void TestCheckRecipientIDRegistrySetting_ShouldReturnErrorMsg_WhenRegistryValueIsNotSet()
		{
			using (FRCustomsDataRegistry.Instance.RecipientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var message = FRCustomsRetrieverServiceTask.CheckRecipientIDRegistrySetting();
				AssertEquals($"The registry setting '{FRCustomsDataRegistry.Instance.RecipientID.GetLocationInEnglish()}' has not been configured.", message);
			}
		}

		public void TestProcess()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var outgoingMessage = Factory.NewWithValidTestData<TestEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "FRC";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			entry.Messages.Add(outgoingMessage);

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.ServiceTasks.Testing.TestFiles.DeltaCImportValidResponseMessage.xml");

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;

			var cust = Factory.NewWithValidTestData<OrgHeader>();
			cust.OH_Code = "UNITTEST";

			var jobHeader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeFRC);
			jobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.SJH_JobReference = "UNITTEST";
			jobHeader.SJH_OH_Customer = cust.PK;

			var dec2 = jobHeader.CusTempStorageDec;
			var line2 = dec2.CusTempStorageLines.AddNew();
			line2.TSL_ReferenceNumber = "CIN-REF001";

			var message2 = Factory.New<CINImportResponseFREDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_ApplicationCode = "FRC";
			message2.EM_MessageType = MessageTypeList.Codes.CIN;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.CIN;
			message2.EM_MessageNum = "1";

			dec2.Messages.Add(message2);

			message2.EM_MessageText = string.Format(CultureInfo.InvariantCulture, @"<CinMessage type=""WarehouseMovement-In"">
  <Header from=""CIN"" to=""anything"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""{0}"" />
  <WarehouseMovementInResponse>Expecting some kind of response in this format but that has not been defined as yet</WarehouseMovementInResponse>
</CinMessage>", message2.EM_MessageNum);

			Factory.Save();

			InitialiseAndRunTaskSchedule(new FRCustomsRetrieverServiceTask());

			var newFactory = new BusinessObjectFactory();
			var reloadeMessage = newFactory.Load<DeltaCImportFREDIMessage>(message.PK);

			AssertEquals(entry.PK, reloadeMessage.EM_LinkedObject.PK);

			var reloadedMessage2 = newFactory.Load<CINImportResponseFREDIMessage>(message2.PK);
			AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, reloadedMessage2.EM_Status);
			AssertEquals(dec2.PK, reloadedMessage2.EM_LinkedObject.PK);
		}

		public void TestProcess_PNTSProcessor()
		{
			var tempStorageHeader = Factory.NewWithValidTestData<EU.Business.CusTempStorage.TemporaryStorageHeader>();
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_Category = "CUS";
			entryNum.CE_EntryType = "LRN";
			entryNum.CE_EntryNum = "IETS115INVALIDMESSAGE";
			entryNum.CE_ParentTable = "AsycudaManifestHeader";
			entryNum.CE_ParentID = tempStorageHeader.PK;
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_BodyText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.ServiceTasks.Testing.TestFiles.PNTS_IETS016ResponseMessage.xml");
			interchange.EI_ApplicationCode = "GMD";
			interchange.EI_InterchangeType = "FRS";
			interchange.EI_ReceiveTransmit = "RCV";
			interchange.EI_Status = "QUE";
			Factory.Save();

			InitialiseAndRunTaskSchedule(new FRCustomsRetrieverServiceTask());
			Factory.Save();

			var message = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK)).FirstOrDefault();
			CombineAssertions("Message is succesfully created and processed", () =>
			{
				AssertNotNull("Message is successfully created.", message);
				AssertEquals("Message is received.", "RCV", message.EM_ReceiveTransmit);
				AssertEquals("Message is for France Customs", "FRC", message.EM_ApplicationCode);
				AssertEquals("Message is processed.", MessageStatusCodeList.Codes.OK, message.EM_Status);
				AssertEquals("Message is linked to Temp. STO. Header.", AsycudaManifestHeaderSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("Message is linked to Temp. STO. Header.", tempStorageHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("Message Type should be STO", "STO", message.EM_MessageType);
				AssertEquals("Message SubType should 016", "016", message.EM_MessageSubType);
			});

			var newFactory = new BusinessObjectFactory();
			var reloadedEnrtyNum = newFactory.Load<CusEntryNumber>(entryNum.PK);
			CombineAssertions("Entry Number is updated accordingly", () =>
			{
				AssertEquals("Entry Number status is updated to the <status> node", "", reloadedEnrtyNum.CE_EntryStatus);
			});
		}

		public void TestProcessDeltaMessage()
		{
			var intchg1 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg1.EI_From = "EASYLOG2TEST_EAD";
			intchg1.EI_To = "HYEDFRCMT";
			intchg1.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg1.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg1.EI_InterchangeNum = "238";
			intchg1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg1.EI_Status = EDIInterchange.Status.Queued;
			intchg1.EI_IsActive = true;
			intchg1.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg1.EI_BodyText = "<Message><EnveloppeMessage><schemaID>MessageReponseCDecExp</schemaID><schemaVersion>01032013</schemaVersion><partyId>33159700500064</partyId><transactionId>HYEDFRCMT+DSE+13256</transactionId><numseq>12</numseq></EnveloppeMessage><ReponseDeclaration><Entete><refdos>0000000001</refdos></Entete><ReponseDatas><Erreur><ErreurGen><erreurCode>CORE1981</erreurCode><erreurDescription>CORE1981 : Le pays d'exportation/expédition et le pays de destination ne doivent pas figurer dans l'itinéraire.(AU)</erreurDescription></ErreurGen><ErreurGen><erreurCode>CORE1953</erreurCode><erreurDescription>CORE1953 : Si le bureau de sortie est différent du bureau de rattachement/présentation,le type de sortie ne doit pas être servi.</erreurDescription></ErreurGen><ErreurGen><erreurCode>COR1093</erreurCode><erreurDescription>[Article n°1] COR1093 : La masse nette doit être inférieure ou égale à la masse brute</erreurDescription></ErreurGen></Erreur></ReponseDatas></ReponseDeclaration></Message>";

			var intchg2 = Factory.NewWithValidTestData<EDIInterchange>();
			intchg2.EI_From = "EASYLOG2TEST_EAD";
			intchg2.EI_To = "HYEDFRCMT";
			intchg2.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg2.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE;
			intchg2.EI_InterchangeNum = "237";
			intchg2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg2.EI_Status = EDIInterchange.Status.Queued;
			intchg2.EI_IsActive = true;
			intchg2.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg2.EI_BodyText = @"{""SchemaId"":""IEH1F"",""TransactionId"":""0000005856"",""MessageJson"":{""ImportOperation"":{""LRN"":""0000005856"",""declarationType"":""IM"",""additionalDeclarationType"":""F"",""presentationNotificationEstimatedDateAndTime"":""2023-04-03T00:00:00"",""languageCode"":""FR""},""CustomsOfficeOfPresentation"":{""referenceNumber"":""""},""SupervisingCustomsOffice"":{""referenceNumber"":""""},""CustomsOfficesOfDischarge"":{},""Importer"":{""identificationNumber"":""ESSSSS""},""Declarant"":{""identificationNumber"":""IT123654789""},""PersonProvidingAGuarantee"":{""identificationNumber"":""""},""PersonPayingCustomsDuty"":{""identificationNumber"":""""},""Representative"":{""identificationNumber"":"""",""status"":""2""},""CurrencyExchange"":{""internalCurrencyUnit"":""EUR""},""GoodsShipment"":[{""sequenceNumber"":""0"",""natureOfTransaction"":"""",""invoiceCurrency"":""EUR"",""dateOfAcceptance"":""2023-04-03T00:00:00"",""exchangeRate"":1.0,""Exporter"":{""identificationNumber"":"""",""name"":""SOBECA"",""Address"":{""streetAndNumber"":""BP53, "",""postcode"":""98845"",""city"":""NOUMEA CEDEX"",""country"":""NC""}},""DeliveryTerms"":{""incotermCode"":""FOB"",""UNLOCODE"":"""",""location"":"""",""country"":"""",""text"":""""},""CountryOfDispatch"":{""countryOfDispatch"":""NC""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""""},""Consignment"":{""containerIndicator"":"""",""inlandModeOfTransport"":"""",""modeOfTransportAtTheBorder"":"""",""referenceNumberUCR"":""3IT123654789-B221945"",""LocationOfGoods"":{""typeOfLocation"":"""",""qualifierOfIdentification"":""""},""ArrivalTransportMeans"":{""typeOfIdentification"":""0"",""identificationNumber"":""""},""ActiveBorderTransportMeans"":{""nationality"":""""}},""GoodsShipmentItem"":[{""sequenceNumber"":""1"",""declarationGoodsItemNumber"":""1"",""natureOfTransaction"":"""",""referenceNumberUCR"":"""",""dateOfAcceptance"":""2023-04-03T00:00:00"",""Procedure"":{""requestedProcedure"":"""",""previousProcedure"":""""},""Origin"":{""countryOfOrigin"":"""",""countryOfPreferentialOrigin"":""NC""},""CountryOfDispatch"":{""countryOfDispatch"":""NCNOU""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""FR""},""Commodity"":{""descriptionOfGoods"":""732497"",""CUSCode"":"""",""quotaOrderNumber"":"""",""CommodityCode"":{""harmonizedSystemSubheadingCode"":"""",""combinedNomenclatureCode"":"""",""taricCode"":"""",""TaricAdditionalCode"":[{""sequenceNumber"":"""",""taricAdditionalCode"":""""}],""NationalAdditionalCode"":[{""sequenceNumber"":"""",""nationalAdditionalCode"":"""",""ccQualifier"":""FR""}]},""GoodsMeasure"":{""nationalMeasurementUnitAndQualifier"":""""},""InvoiceLine"":{},""CalculationOfTaxes"":{""preference"":""""}},""Packaging"":[{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""}],""SupportingDocument"":[{""sequenceNumber"":""0"",""type"":""N380"",""ccQualifier"":""FR"",""referenceNumber"":""1"",""documentLineItemNumber"":""0"",""issuingAuthorityName"":"""",""dateOfValidity"":"""",""currency"":""""}],""CustomsValuation"":{""valuationMethod"":""1""},""ValuationAdjustment"":{""valuationIndicators"":""0000""}}]}]}}";
			Factory.Save();

			InitialiseAndRunTaskSchedule(new FRCustomsRetrieverServiceTask());
			Factory.Save();

			intchg1.Reload();
			intchg2.Reload();

			var msg1 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			AssertEquals(1, msg1.Length);

			AssertEquals(1, intchg1.ContainedMessages.Count);
			AssertEquals(msg1[0].PK, intchg1.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg1.PK, msg1[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg1[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.EXC, msg1[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeList.Codes.EXC, msg1[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg1[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "238", msg1[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg1[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg1[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg1[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText", intchg1.EI_BodyText, msg1[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg1[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg1[0].EM_LinkUniqueID);

			var msg2 = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg2.PK));
			AssertEquals(1, msg2.Length);

			AssertEquals(1, intchg2.ContainedMessages.Count);
			AssertEquals(msg2[0].PK, intchg2.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", intchg2.PK, msg2[0].EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg2[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.DEC, msg2[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", "1", msg2[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg2[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "237", msg2[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg2[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, msg2[0].EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, msg2[0].EM_HeldUntilDate);
			AssertEquals("EM_MessageText body text", @"{""ImportOperation"":{""LRN"":""0000005856"",""declarationType"":""IM"",""additionalDeclarationType"":""F"",""presentationNotificationEstimatedDateAndTime"":""2023-04-03T00:00:00"",""languageCode"":""FR""},""CustomsOfficeOfPresentation"":{""referenceNumber"":""""},""SupervisingCustomsOffice"":{""referenceNumber"":""""},""CustomsOfficesOfDischarge"":{},""Importer"":{""identificationNumber"":""ESSSSS""},""Declarant"":{""identificationNumber"":""IT123654789""},""PersonProvidingAGuarantee"":{""identificationNumber"":""""},""PersonPayingCustomsDuty"":{""identificationNumber"":""""},""Representative"":{""identificationNumber"":"""",""status"":""2""},""CurrencyExchange"":{""internalCurrencyUnit"":""EUR""},""GoodsShipment"":[{""sequenceNumber"":""0"",""natureOfTransaction"":"""",""invoiceCurrency"":""EUR"",""dateOfAcceptance"":""2023-04-03T00:00:00"",""exchangeRate"":1.0,""Exporter"":{""identificationNumber"":"""",""name"":""SOBECA"",""Address"":{""streetAndNumber"":""BP53, "",""postcode"":""98845"",""city"":""NOUMEA CEDEX"",""country"":""NC""}},""DeliveryTerms"":{""incotermCode"":""FOB"",""UNLOCODE"":"""",""location"":"""",""country"":"""",""text"":""""},""CountryOfDispatch"":{""countryOfDispatch"":""NC""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""""},""Consignment"":{""containerIndicator"":"""",""inlandModeOfTransport"":"""",""modeOfTransportAtTheBorder"":"""",""referenceNumberUCR"":""3IT123654789-B221945"",""LocationOfGoods"":{""typeOfLocation"":"""",""qualifierOfIdentification"":""""},""ArrivalTransportMeans"":{""typeOfIdentification"":""0"",""identificationNumber"":""""},""ActiveBorderTransportMeans"":{""nationality"":""""}},""GoodsShipmentItem"":[{""sequenceNumber"":""1"",""declarationGoodsItemNumber"":""1"",""natureOfTransaction"":"""",""referenceNumberUCR"":"""",""dateOfAcceptance"":""2023-04-03T00:00:00"",""Procedure"":{""requestedProcedure"":"""",""previousProcedure"":""""},""Origin"":{""countryOfOrigin"":"""",""countryOfPreferentialOrigin"":""NC""},""CountryOfDispatch"":{""countryOfDispatch"":""NCNOU""},""Destination"":{""countryOfDestination"":"""",""regionOfDestination"":"""",""ccQualifier"":""FR""},""Commodity"":{""descriptionOfGoods"":""732497"",""CUSCode"":"""",""quotaOrderNumber"":"""",""CommodityCode"":{""harmonizedSystemSubheadingCode"":"""",""combinedNomenclatureCode"":"""",""taricCode"":"""",""TaricAdditionalCode"":[{""sequenceNumber"":"""",""taricAdditionalCode"":""""}],""NationalAdditionalCode"":[{""sequenceNumber"":"""",""nationalAdditionalCode"":"""",""ccQualifier"":""FR""}]},""GoodsMeasure"":{""nationalMeasurementUnitAndQualifier"":""""},""InvoiceLine"":{},""CalculationOfTaxes"":{""preference"":""""}},""Packaging"":[{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""},{""sequenceNumber"":"""",""typeOfPackages"":"""",""numberOfPackages"":""0"",""shippingMarks"":""""}],""SupportingDocument"":[{""sequenceNumber"":""0"",""type"":""N380"",""ccQualifier"":""FR"",""referenceNumber"":""1"",""documentLineItemNumber"":""0"",""issuingAuthorityName"":"""",""dateOfValidity"":"""",""currency"":""""}],""CustomsValuation"":{""valuationMethod"":""1""},""ValuationAdjustment"":{""valuationIndicators"":""0000""}}]}]}", msg2[0].EM_MessageText);
			AssertEquals("EM_LinkTable - this will be set by message processor", "", msg2[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this will be set by message processor", ZGuid.Empty, msg2[0].EM_LinkUniqueID);
		}

		public void TestProcessNCTSMessage()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.ServiceTasks.Testing.TestFiles.NCTS_CC004CResponseMessage.xml");

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(nctsHeader.MovementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, nctsHeader.CountryCode);
			entryNumber.CE_EntryNum = "0000007735";

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = EDIInterchangeTypeList.Codes.GenericMessageDelivery;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = messageText;
			Factory.Save();

			InitialiseAndRunTaskSchedule(new FRCustomsRetrieverServiceTask());

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertEquals(1, msg.Length);
			AssertType<NCTSFREDIMessage>(msg[0]);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(msg[0].PK, interchange.ContainedMessages[0].PK);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, msg[0].EM_ApplicationCode);
			AssertEquals("message linked to interchange", interchange.PK, msg[0].EM_EI);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.TP5, msg[0].EM_MessageType);
			AssertEquals("EM_MessageSubType", "004", msg[0].EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, msg[0].EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "237", msg[0].EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", msg[0].EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg[0].EM_Status);
			AssertEquals("EM_MessageText body text", messageText, msg[0].EM_MessageText);

			var newFactory = new BusinessObjectFactory();
			var reloadedHeader = newFactory.Load<NctsHeader>(nctsHeader.PK);
			AssertEquals("Header MRN should be updated as present in the message.", "MRN1", reloadedHeader.MovementReferenceNumber);
			AssertEquals("Message status should be updated to ACC.", LogicalStatusList.Codes.Accepted, reloadedHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Phase should be updated to 015.", "015", reloadedHeader.MovementHeader.BM_Phase);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"FR Customs interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"FR Customs CIN interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsCIN,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"FR Customs messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.FRCustomsMessage,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"FR Port messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.FRPortMessage,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"FR Ports interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRPorts,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"FR Delta IE interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"FR PNTS interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"FR TP5 interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery),
				};
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
