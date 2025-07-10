using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

[assembly: UsesConstants(typeof(CDSEDIMessageTypeList))]
namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class JobDeclarationEventParentFinderTest : TestCaseWithFactory
	{
		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestUpdateEntryWithConversationIDWhenOutgoingMessageStatusIsRejected()
		{
			SetupOutgoingMessage(EDIMessage.Status.Rejected);

			var logParents = ProcessMessage(validXML, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			var conversationIdMessage = entry.Messages[1];
			AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", conversationIdMessage.EM_ApplicationReference);
			AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, conversationIdMessage.EM_MessageSubType);
			AssertContains(expectedUpdateEntryWithConversationID.Replace("[SessionGUID]", sessionGuid.ToString()).Replace("[DUCR]", entry.CH_BGMReference), conversationIdMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Rejected, originalMessage.EM_Status);
			AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", originalMessage.EM_ApplicationReference);
			AssertEquals(EDIMessage.Status.Received, conversationIdMessage.EM_Status);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, conversationIdMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.Sent, entry.CH_Status);
		}

		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestUpdateEntryWithConversationID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(validXML, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			var conversationIdMessage = entry.Messages[1];
			AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", conversationIdMessage.EM_ApplicationReference);
			AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, conversationIdMessage.EM_MessageSubType);
			AssertContains(expectedUpdateEntryWithConversationID.Replace("[SessionGUID]", sessionGuid.ToString()).Replace("[DUCR]", entry.CH_BGMReference), conversationIdMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Acknowledged, originalMessage.EM_Status);
			AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", originalMessage.EM_ApplicationReference);
			AssertEquals(EDIMessage.Status.Received, conversationIdMessage.EM_Status);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, conversationIdMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.Sent, entry.CH_Status);
		}

		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestUpdateEntryWithConversationIDOnIMessageAttachee()
		{
			var houseBill = "TESTHOUSE";
			var header = Factory.New<Integration.Customs.GB.GBH7.IAsycudaManifestHeader>() as BusinessObject;
			header[AsycudaManifestHeaderSchema.AMA_JobReference] = "B00000001";

			var bill = Factory.New<Integration.Customs.GB.GBH7.IAsycudaBill>() as BusinessObject;
			bill[AsycudaBillSchema.ABL_AMA] = header.PK;
			bill[AsycudaBillSchema.ABL_BillNumber] = houseBill;
			bill["LocalReferenceNumber"] = "LRN123";

			var attachee = bill as IMessageAttachee;
			SetupOutgoingMessageForBill(attachee);

			var logParents = ProcessMessage(validXML, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			CombineAssertions(() =>
			{
				Assert("log parents", logParents.Single() is AsycudaBill);
				AssertEquals("messages count", 2, attachee.Messages.Count);

				AssertEquals("ABL_MessageStatus", MessageStatusList.Codes.Sent, (bill as AsycudaBill).ABL_MessageStatus);

				var originalMessage = attachee.Messages[0];
				AssertEquals("originalMessage.EM_Status", EDIMessage.Status.Acknowledged, originalMessage.EM_Status);
				AssertEquals("originalMessage.EM_ApplicationReference", "d03f84e3b5894aaaa1f8cafd744add8e", originalMessage.EM_ApplicationReference);

				var conversationIdMessage = attachee.Messages[1];
				AssertEquals("conversationIdMessage.EM_ApplicationReference", "d03f84e3b5894aaaa1f8cafd744add8e", conversationIdMessage.EM_ApplicationReference);
				AssertEquals("conversationIdMessage.EM_MessageSubType", CDSEDIMessageTypeList.Codes.ConversationID, conversationIdMessage.EM_MessageSubType);
				AssertContains("conversationIdMessage.EM_MessageInterpretation", expectedUpdateEntryWithConversationID.Replace("[SessionGUID]", sessionGuid.ToString()).Replace("[DUCR]", houseBill), conversationIdMessage.EM_MessageInterpretation);
				AssertEquals("conversationIdMessage.EM_Status", EDIMessage.Status.Received, conversationIdMessage.EM_Status);
				AssertEquals("conversationIdMessage.EM_ApplicationCode", ApplicationCodeList.Codes.GbCustomsDeclarationServices, conversationIdMessage.EM_ApplicationCode);
			});
		}

		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestUpdateEntryWithCIDAndCSP()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithCIDAndCSP, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			var conversationIdMessage = entry.Messages[1];
			AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", conversationIdMessage.EM_ApplicationReference);
			AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, conversationIdMessage.EM_MessageSubType);
			AssertContains(expectedUpdateEntryWithCIDAndCSP.Replace("[SessionGUID]", sessionGuid.ToString()).Replace("[DUCR]", entry.CH_BGMReference), conversationIdMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Acknowledged, originalMessage.EM_Status);
			AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", originalMessage.EM_ApplicationReference);
			AssertEquals(EDIMessage.Status.Received, conversationIdMessage.EM_Status);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, conversationIdMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.Sent, entry.CH_Status);
		}

		public void TestUpdateEntryWithNoEHubTrackingID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithoutEHubTrackingID, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(1, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Sent);
		}

		public void TestUpdateEntryWithNoConversationID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithoutConversationID, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(1, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Sent);
		}

		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestUpdateEntryWithErrorResponseAndCID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithErrorResponseAndCID, sessionGuid, CDS.Constants.EHubEventTypes.MessageRejected);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(3, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Rejected);

			var conversationIdMessage = entry.Messages[1];
			AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", conversationIdMessage.EM_ApplicationReference);
			AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, conversationIdMessage.EM_MessageSubType);
			AssertContains(expectedUpdateEntryWithCIDAndError.Replace("[SessionGUID]", sessionGuid.ToString()).Replace("[DUCR]", entry.CH_BGMReference), conversationIdMessage.EM_MessageInterpretation);
			AssertEquals(conversationIdMessage.EM_Status, EDIMessage.Status.Received);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, conversationIdMessage.EM_ApplicationCode);

			var errorMessage = entry.Messages[2] as CDSErrorResponseEDIMessage;
			AssertEquals(CDSEDIMessageTypeList.Codes.EHubErrorResponse, errorMessage.EM_MessageType);
			AssertContains("<errorResponse>", errorMessage.EM_MessageText);
			AssertEquals(errorMessage.EM_Status, EDIMessage.Status.Queued);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, errorMessage.EM_ApplicationCode);

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(errorMessage);
			var expectedMessage = ExpectedFriendlyError.Replace("[MessageNum]", originalMessage.EM_MessageNum);
			AssertEquals(expectedMessage, errorMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Received, errorMessage.EM_Status);
			AssertEquals("999E", errorMessage.EM_MessageNum);
		}

		public void TestUpdateEntryWithErrorResponseAndWithoutCID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithErrorResponseWithoutCID, sessionGuid, CDS.Constants.EHubEventTypes.MessageRejected);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Rejected);

			var errorMessage = entry.Messages[1] as CDSErrorResponseEDIMessage;
			AssertEquals(CDSEDIMessageTypeList.Codes.EHubErrorResponse, errorMessage.EM_MessageType);
			AssertContains("<errorResponse>", errorMessage.EM_MessageText);
			AssertEquals(errorMessage.EM_Status, EDIMessage.Status.Queued);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, errorMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(errorMessage);
			var expectedMessage = ExpectedFriendlyError.Replace("[MessageNum]", originalMessage.EM_MessageNum);
			AssertEquals(expectedMessage, errorMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Received, errorMessage.EM_Status);
		}

		public void TestProcessingWithNonXmlMessageAndErrorContexts()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithNonXMLErrorResponseAndContexts, sessionGuid, CDS.Constants.EHubEventTypes.MessageRejected);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(3, entry.Messages.Count);

			var errorMessage = entry.Messages[2] as CDSErrorResponseEDIMessage;
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, errorMessage.EM_ApplicationCode);
			AssertEquals(CDSEDIMessageTypeList.Codes.EHubErrorResponse, errorMessage.EM_MessageType);
			AssertContains("HTML EM_MessageText", "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Draft//EN\">", errorMessage.EM_MessageText);
			AssertContains("Heading from XML", "Message was rejected by an upstream system for the following reasons", errorMessage.EM_MessageInterpretation);
			AssertContains("Error Context from XML", "MCP Registration Failed", errorMessage.EM_MessageInterpretation);
			AssertContains("eHubTrackingID Context node from XML", $"<li>e Hub Tracking ID - {sessionGuid}</li>", errorMessage.EM_MessageInterpretation);
			AssertContains("Badge Context node from XML", "<li>Badge - FWC</li>", errorMessage.EM_MessageInterpretation);
			AssertContains("Topic Context node from XML", "<li>Topic - FWC9</li>", errorMessage.EM_MessageInterpretation);
			AssertContains("ConversationID Context node from XML", "<li>Conversation ID - d03f84e3-b589-4aaa-a1f8-cafd744add8e</li>", errorMessage.EM_MessageInterpretation);
			AssertContains("ResponseText Context from XML", "Error 401--Unauthorized", errorMessage.EM_MessageInterpretation);
			AssertEquals("Processed error message", errorMessage.EM_Status, EDIMessage.Status.ProcessedOK);
		}

		public void TestErrorsWithoutResponseText()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlErrorWithoutResponseText, sessionGuid, CDS.Constants.EHubEventTypes.MessageRejected);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Rejected);

			var errorMessage = entry.Messages[1] as CDSErrorResponseEDIMessage;
			AssertEquals(CDSEDIMessageTypeList.Codes.EHubErrorResponse, errorMessage.EM_MessageType);
			AssertContains("<errorResponse>", errorMessage.EM_MessageText);
			AssertEquals(errorMessage.EM_Status, EDIMessage.Status.Queued);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsDeclarationServices, errorMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(errorMessage);
			var expectedMessage = ExpectedFriendlyErrorWithoutResponseText.Replace("[MessageNum]", originalMessage.EM_MessageNum);
			AssertEquals(expectedMessage, errorMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Received, errorMessage.EM_Status);
		}

		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestUpdateEntryWithCSPID()
		{
			SetupOutgoingMessage();

			Factory.Save();

			var logParents = ProcessMessage(xmlWithCSPID, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var receivedMessage = entry.Messages[1];
			AssertEquals("AAA.BBBBBB.123456789", receivedMessage.EM_ApplicationReference);
			AssertEquals(CDSEDIMessageTypeList.Codes.CSPID, receivedMessage.EM_MessageSubType);
			AssertContains(expectedUpdateEntryWithCSPID.Replace("[SessionGUID]", sessionGuid.ToString()).Replace("[DUCR]", entry.CH_BGMReference), receivedMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Acknowledged, entry.Messages[0].EM_Status);
			AssertEquals(MessageStatusList.Codes.Sent, entry.CH_Status);
			AssertEquals("999R", receivedMessage.EM_MessageNum);
		}

		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestUpdateEntryWithCSPIDOnIMessageAttachee()
		{
			var houseBill = "TESTHOUSE";
			var header = Factory.New<Integration.Customs.GB.GBH7.IAsycudaManifestHeader>() as BusinessObject;
			header[AsycudaManifestHeaderSchema.AMA_JobReference] = "B00000001";

			var bill = Factory.New<Integration.Customs.GB.GBH7.IAsycudaBill>() as BusinessObject;
			bill[AsycudaBillSchema.ABL_AMA] = header.PK;
			bill[AsycudaBillSchema.ABL_BillNumber] = houseBill;
			bill["LocalReferenceNumber"] = "LRN123";

			var attachee = bill as IMessageAttachee;
			SetupOutgoingMessageForBill(attachee);

			var logParents = ProcessMessage(xmlWithCSPID, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);

			CombineAssertions(() =>
			{
				Assert("log parents", logParents.Single() is AsycudaBill);
				AssertEquals("messages count", 2, attachee.Messages.Count);

				AssertEquals("ABL_MessageStatus", MessageStatusList.Codes.Sent, (bill as AsycudaBill).ABL_MessageStatus);

				var originalMessage = attachee.Messages[0];
				AssertEquals("originalMessage.EM_Status", EDIMessage.Status.Acknowledged, originalMessage.EM_Status);

				var receivedMessage = attachee.Messages[1];
				AssertEquals("receivedMessage.EM_ApplicationReference", "AAA.BBBBBB.123456789", receivedMessage.EM_ApplicationReference);
				AssertEquals("receivedMessage.EM_MessageSubType", CDSEDIMessageTypeList.Codes.CSPID, receivedMessage.EM_MessageSubType);
				AssertContains("receivedMessage.EM_MessageInterpretation", expectedUpdateEntryWithCSPID.Replace("[SessionGUID]", sessionGuid.ToString()).Replace("[DUCR]", houseBill), receivedMessage.EM_MessageInterpretation);
				AssertEquals("receivedMessage.EM_Status", EDIMessage.Status.Received, receivedMessage.EM_Status);
				AssertEquals("receivedMessage.EM_ApplicationCode", ApplicationCodeList.Codes.GbCustomsDeclarationServices, receivedMessage.EM_ApplicationCode);
				AssertEquals("receivedMessage.EM_MessageNum", "999R", receivedMessage.EM_MessageNum);
			});
		}

		public void TestProcessQueryEvent()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456";

			var outgoingInterchange = Factory.New<EDIInterchange>();
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = eHubTrackingIdGuid;
			outgoingInterchange.EI_HeaderText = "";
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "GBCustomsTest";
			outgoingInterchange.EI_BodyText = "";

			var outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew();
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.EM_ApplicationCode = "";
			outgoingSentMessage.EM_ApplicationReference = "ABC123";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			outgoingSentMessage.EM_MessageNum = "999";
			outgoingSentMessage.EM_Status = EDIMessage.Status.Sent;

			entry.Messages.Add(outgoingSentMessage);

			Factory.Save();

			var responseText = @"<p:DeclarationStatusResponse xsi:schemaLocation=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"">
	<p:DeclarationStatusDetails>
		<p:Declaration>
			<p:ID>21GB3BF3RMN7MIB3R6</p:ID>
			<p:VersionID>1</p:VersionID>
			<p:ReceivedDateTime>
				<p:DateTimeString formatCode=""304"">20210325134724Z</p:DateTimeString>
			</p:ReceivedDateTime>
			<p:ROE>H</p:ROE>
			<p:ICS>14</p:ICS>
		</p:Declaration>
		<p2:Declaration>
			<p2:FunctionCode>9</p2:FunctionCode>
			<p2:TypeCode>IMD</p2:TypeCode>
			<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
			<p2:TotalPackageQuantity>10</p2:TotalPackageQuantity>
			<p2:Submitter>
				<p2:ID>GB159688953432</p2:ID>
			</p2:Submitter>
			<p2:GoodsShipment>
				<p2:PreviousDocument>
					<p2:ID>1GB896458895023-B00031630</p2:ID>
					<p2:TypeCode>DCR</p2:TypeCode>
				</p2:PreviousDocument>
				<p2:UCR>
					<p2:TraderAssignedReferenceID>1GB896458895023-B00031630</p2:TraderAssignedReferenceID>
				</p2:UCR>
			</p2:GoodsShipment>
		</p2:Declaration>
	</p:DeclarationStatusDetails>
</p:DeclarationStatusResponse>";
			var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseText));

			var logParents = ProcessMessage(xmlQueryResponse.Replace("Base64DataGoesHere", base64Content), eHubTrackingIdGuid, Events.ServiceRequested.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var receivedMessage = entry.Messages[1];
			AssertEquals(CDSEDIMessageTypeList.Codes.QueryResponse, receivedMessage.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Acknowledged, dec.ActiveEntryHeaders[0].Messages[0].EM_Status);
			AssertEquals("999R", receivedMessage.EM_MessageNum);

			AssertEquals(responseText, receivedMessage.EM_MessageText);
			AssertEquals("ABC123", receivedMessage.EM_ApplicationReference);
			AssertEquals(EDIMessage.Status.Queued, receivedMessage.EM_Status);
			AssertContains("CDS Declaration Query", receivedMessage.EM_MessageInterpretation);
		}

		BusinessObject[] ProcessMessage(ZString xmlMessage, ZGuid trackingID, ZString eventCode)
		{
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, xmlMessage, eventCode, trackingID));
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDataObject = xmlEvent as UniversalEvent;
			return subscriber.GetLogParentsForEvent(eventDataObject);
		}

		public void TestSecondSetOfMessagesWithCSPUsesCorrectCID()
		{
			SetupOutgoingMessage();
			var logParents = ProcessMessage(xmlWithCSPID, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			logParents = ProcessMessage(xmlWithErrorResponseAndCID, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(3, entry.Messages.Count);

			SetupOutgoingMessage2();
			logParents = ProcessMessage(xmlWithCSPID.Replace("AAA.BBBBBB.123456789", "CCC.DDDDDD.987654321"), sessionGuid2, Events.WarehouseJobCanNowBeFinalised.Code);
			logParents = ProcessMessage(xmlWithErrorResponseAndCID.Replace("d03f84e3-b589-4aaa-a1f8-cafd744add8e", "CC44346D-1AA6-44FE-B01E-8B0897AE6938"), sessionGuid2, Events.WarehouseJobCanNowBeFinalised.Code);
			dec = (JobDeclaration)logParents.First();
			AssertEquals(6, entry.Messages.Count);
			var receivedMessage = entry.Messages[4];
			AssertEquals("CCC.DDDDDD.987654321", receivedMessage.EM_ApplicationReference);
			AssertEquals(CDSEDIMessageTypeList.Codes.CSPID, receivedMessage.EM_MessageSubType);

			var cidMessage = entry.Messages[5];
			AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, cidMessage.EM_MessageSubType);
			AssertContains("HMRC Conversation ID = CC44346D1AA644FEB01E8B0897AE6938", cidMessage.EM_MessageInterpretation);
			AssertNotContains("it has not", cidMessage.EM_MessageInterpretation);

			Factory.Save();
		}

		public void TestGvmsTypeRefused()
		{
			SetupOutgoingMessage();
			var logParents = ProcessMessage(gvmsStyleXMLWithoutProvider, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			AssertNull("GVMS Style event - data target > type isolated", logParents);
			logParents = ProcessMessage(gvmsStyleXMLWithProvider, sessionGuid, Events.WarehouseJobCanNowBeFinalised.Code);
			AssertNull("GVMS Style event - data context > data provider provider isolated", logParents);
		}

		public void TestResponseWithoutContextCollectionDoesNotCauseException()
		{
			SetupOutgoingMessage();
			AssertNoExceptionThrown(() => ProcessMessage(xmlQueryWithoutContextCollection, sessionGuid, CDS.Constants.EHubEventTypes.MessageRejected));
		}

		public void TestInvalidBase64StringOnMessageResponseRejected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456";

			var outgoingInterchange = Factory.New<EDIInterchange>();
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = eHubTrackingIdGuid;
			outgoingInterchange.EI_HeaderText = "";
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "GBCustomsTest";
			outgoingInterchange.EI_BodyText = "";

			var outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew();
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.EM_ApplicationCode = "";
			outgoingSentMessage.EM_ApplicationReference = "ABC123";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			Factory.Save();

			outgoingSentMessage.EM_MessageNum = "999";
			outgoingSentMessage.EM_Status = EDIMessage.Status.Sent;

			entry.Messages.Add(outgoingSentMessage);

			Factory.Save();

			var logParents = ProcessMessage(string.Format(xmlInvalidBase64StringOnMessageResponseRejected, eHubTrackingIdGuid), eHubTrackingIdGuid, CDS.Constants.EHubEventTypes.MessageResponseRejected);
			var dec = (JobDeclaration)logParents.First();
			AssertEquals(2, entry.Messages.Count);
			var receivedMessage = entry.Messages[1];
			AssertEquals("Message status", EDIMessage.Status.Rejected, dec.ActiveEntryHeaders[0].Messages[0].EM_Status);
			AssertEquals("Message", ExpectedFriendlyErrorInvalidBase64StringOnMessageRejected, receivedMessage.EM_MessageText);
		}

		[TestDate(2019, 11, 11, 12, 0, 0)]
		public void TestInvalidBase64StringOnMessageRejected()
		{
			SetupOutgoingMessage();

			ProcessMessage(xmlInvalidBase64StringOnMessageRejected, sessionGuid, CDS.Constants.EHubEventTypes.MessageRejected);
			AssertEquals(3, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Rejected);

			var errorMessage = entry.Messages[2] as CDSErrorResponseEDIMessage;
			AssertEquals("Message type", CDSEDIMessageTypeList.Codes.EHubErrorResponse, errorMessage.EM_MessageType);
			AssertContains("Message", ExpectedFriendlyErrorInvalidBase64StringOnMessageRejected, errorMessage.EM_MessageText);
		}

		public void TestDocumentUploadConfirmation_WithFileUploadResponse_SingleFile()
		{
			var eHubTrackingIdGuid = SetupOutgoingMessage();

			var uploadedFileGuid = Guid.NewGuid();
			var uploadFile = string.Format(CultureInfo.InvariantCulture, dsnFileResponsePart, uploadedFileGuid);
			var uploadFileResponse = string.Format(CultureInfo.InvariantCulture, dsnFileUploadResponsePart, uploadFile, string.Empty);
			var uploadFileResponse64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(uploadFileResponse));
			string message = string.Format(CultureInfo.InvariantCulture, dsnResponse, declaration.JE_DeclarationReference, eHubTrackingIdGuid, uploadFileResponse64);

			var logParents = ProcessMessage(message, Guid.Empty, string.Empty);
			var dec = (JobDeclaration)logParents.First();

			CombineAssertions(() =>
			{
				AssertEquals(2, entry.Messages.Count);
				var dsnLogs = dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentSentCode));
				AssertEquals("One DSN event added to declaration", 1, dsnLogs.Length);
				AssertEquals("DSN event reference should match message record", GetFormattedEventReference(uploadedFileGuid), dsnLogs.FirstOrDefault()?.SL_Reference);
			});
		}

		public void TestDocumentUploadConfirmation_WithFileUploadResponse_MultipleFiles()
		{
			var eHubTrackingIdGuid = SetupOutgoingMessage();

			var uploadedFileGuid1 = Guid.NewGuid();
			var uploadFile1 = string.Format(CultureInfo.InvariantCulture, dsnFileResponsePart, uploadedFileGuid1);
			var uploadedFileGuid2 = Guid.NewGuid();
			var uploadFile2 = string.Format(CultureInfo.InvariantCulture, dsnFileResponsePart, uploadedFileGuid2);
			var uploadFileResponse = string.Format(CultureInfo.InvariantCulture, dsnFileUploadResponsePart, uploadFile1, uploadFile2);
			var uploadFileResponse64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(uploadFileResponse));
			string message = string.Format(CultureInfo.InvariantCulture, dsnResponse, declaration.JE_DeclarationReference, eHubTrackingIdGuid, uploadFileResponse64);

			var logParents = ProcessMessage(message, Guid.Empty, string.Empty);
			var dec = (JobDeclaration)logParents.First();

			CombineAssertions(() =>
			{
				AssertEquals(2, entry.Messages.Count);
				var dsnLogs = dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentSentCode));
				AssertEquals("One DSN event for each file confirmation", 2, dsnLogs.Length);
				AssertNotNull("DSN event reference should match message record 1", dsnLogs.FirstOrDefault(ev => ev.SL_Reference == GetFormattedEventReference(uploadedFileGuid1)));
				AssertNotNull("DSN event reference should match message record 2", dsnLogs.FirstOrDefault(ev => ev.SL_Reference == GetFormattedEventReference(uploadedFileGuid2)));
			});
		}

		public void TestDocumentUploadConfirmation_WithoutFileUploadResponse()
		{
			var eHubTrackingIdGuid = SetupOutgoingMessage();

			var uploadReply64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("File uploaded to https://fus-ro-dah.s3.awazon.com/referencecodeprovided"));
			string message = string.Format(CultureInfo.InvariantCulture, dsnResponse, declaration.JE_DeclarationReference, eHubTrackingIdGuid, uploadReply64);

			var logParents = ProcessMessage(message, Guid.Empty, string.Empty);
			var dec = (JobDeclaration)logParents.First();

			CombineAssertions(() =>
			{
				AssertEquals(2, entry.Messages.Count);
				AssertEquals("No DSN event added to declaration", 0, dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentSentCode)).Length);
			});
		}

		public void TestDocumentUploadConfirmationOnIMessageAttachee()
		{
			var header = Factory.New<Integration.Customs.GB.GBH7.IAsycudaManifestHeader>() as BusinessObject;
			header.FillWithValidTestData();

			var bill = Factory.New<Integration.Customs.GB.GBH7.IAsycudaBill>() as BusinessObject;
			bill[AsycudaBillSchema.ABL_AMA.Name] = header.PK;
			bill[AsycudaBillSchema.ABL_UCRNumber.Name] = "UC001";
			bill.FillWithValidTestData();

			var attachee = bill as IMessageAttachee;
			var eHubTrackingIdGuid = SetupOutgoingMessageForBill(attachee);
			var uploadedFileGuid = Guid.NewGuid();
			var uploadFile = string.Format(CultureInfo.InvariantCulture, dsnFileResponsePart, uploadedFileGuid);
			var uploadFileResponse = string.Format(CultureInfo.InvariantCulture, dsnFileUploadResponsePart, uploadFile, string.Empty);
			var uploadFileResponse64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(uploadFileResponse));
			string message = string.Format(CultureInfo.InvariantCulture, dsnResponse, bill[AsycudaBillSchema.ABL_BillNumber.Name], eHubTrackingIdGuid, uploadFileResponse64);

			var logParents = ProcessMessage(message, Guid.Empty, string.Empty);
			var logParent = (AsycudaBill)logParents.First();

			CombineAssertions(() =>
			{
				var dsnLogs = logParent.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentSentCode));
				AssertEquals("One DSN event added to bill", 1, dsnLogs.Length);
				AssertEquals("DSN event reference should match message record", GetFormattedEventReference(uploadedFileGuid), dsnLogs.FirstOrDefault()?.SL_Reference);
			});
		}

		public void TestDontProcessICSEvents()
		{
			BusinessObject[] ProcessMessageWithLogger(IXmlImportLogger logger, string xmlMessage, string dataProvider, ZGuid trackingID)
			{
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, xmlMessage, dataProvider, trackingID));
				var subscriber = GetNewEventParentFinderWithLogger(logger);
				var eventDataObject = xmlEvent as UniversalEvent;
				return subscriber.GetLogParentsForEvent(eventDataObject);
			}
			var logger = new TestErrorLogger();

			var outgoingInterchange = Factory.New<EDIInterchange>();
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = eHubTrackingIdGuid;
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;

			const string icsEventMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>AsycudaManifest</Type>
              <Key>MAN0000003</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2024-11-12T11:33:34+11:00</EventTime>
        <EventType>MRR</EventType>
        <DataContext>
          <DataSource>
            <DataProvider>{0}</DataProvider>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>";

			CombineAssertions(() =>
			{
				var logParents = ProcessMessageWithLogger(logger, icsEventMessage, Constants.ICSNI, eHubTrackingIdGuid);
				AssertNull("ICSNI result", logParents);
				AssertEquals("ICSNI logs", string.Empty, logger.Logs);

				logger.ClearLogs();
				logParents = ProcessMessageWithLogger(logger, icsEventMessage, Constants.ICSGB, eHubTrackingIdGuid);
				AssertNull("ICSGB result", logParents);
				AssertEquals("ICSGB logs", string.Empty, logger.Logs);
			});
		}

		public void TestUpdatePentantEntryWithConversationIDFromCIDOnly()
		{
			SetupOutgoingMessage();
			entry.Declaration.ZG_Gateway = GatewayList.Codes.Pentant;
			AssertEquals("Pre-requisite: Declaration.IsPentant", true, entry.Declaration.IsPentant);
			AssertEntryWithConversationIDFromCIDOnly();
		}

		public void TestUpdateMCPEntryWithConversationIDFromCIDOnly()
		{
			SetupOutgoingMessage();
			entry.Declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			AssertEquals("Pre-requisite: Declaration.IsMCP", true, entry.Declaration.IsMCP);
			AssertEntryWithConversationIDFromCIDOnly();
		}

		public void TestUpdateCNSEntryWithConversationIDFromCIDOnly()
		{
			SetupOutgoingMessage();
			entry.Declaration.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			AssertEquals("Pre-requisite: Declaration.IsCNS", true, entry.Declaration.IsCNS);
			AssertEntryWithConversationIDFromCIDOnly();
		}

		public void TestProcessDisQuery()
		{
			var outgoingInterchange = Factory.New<EDIInterchange>();
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = eHubTrackingIdGuid;
			outgoingInterchange.EI_HeaderText = "";
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "GBCustomsTest";
			outgoingInterchange.EI_BodyText = "";

			var outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(CDSDISQueryMessage));
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.EM_ApplicationReference = "ABC123";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingSentMessage.EM_MessageNum = "999";
			outgoingSentMessage.EM_Status = EDIMessage.Status.Sent;

			Factory.Save();

			var responseText = @"<p:DeclarationSearchResponse xsi:schemaLocation=""http://gov.uk/customs/declarationInformationRetrieval/declarationSummary/v1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/declarationInformationRetrieval/declarationSummary/v1"">
    <p:DeclarationSearchDetails>
        <p:Declaration>
            <p:ID>23GBB7KF5523KMKAR1</p:ID>
            <p:ReceivedDateTime>
                <p:DateTimeString formatCode=""304"">20231010102247Z</p:DateTimeString>
            </p:ReceivedDateTime>
            <p:ROE>H</p:ROE>
            <p:ICS>14</p:ICS>
            <p:LRN>HYEDUKCM20000000003421</p:LRN>
        </p:Declaration>
        <p2:Declaration>
            <p2:FunctionCode>9</p2:FunctionCode>
            <p2:TypeCode>IMD</p2:TypeCode>
            <p2:Submitter>
                <p2:ID>GB048834222514</p2:ID>
            </p2:Submitter>
            <p2:Declarant>
                <p2:ID>GB896458895015</p2:ID>
            </p2:Declarant>
            <p2:GoodsShipment>
                <p2:Consignment>
                    <p2:GoodsLocation>
                        <p2:Name>ABDABDABM</p2:Name>
                        <p2:TypeCode>A</p2:TypeCode>
                        <p2:Address>
                            <p2:TypeCode>U</p2:TypeCode>
                            <p2:CountryCode>GB</p2:CountryCode>
                        </p2:Address>
                    </p2:GoodsLocation>
                </p2:Consignment>
                <p2:Importer>
                    <p2:ID>GB896458895015</p2:ID>
                </p2:Importer>
            </p2:GoodsShipment>
        </p2:Declaration>
    </p:DeclarationSearchDetails>
    <p:DeclarationSearchDetails>
        <p:Declaration>
            <p:ID>23GBB7K6WRBIPQDAR8</p:ID>
            <p:ReceivedDateTime>
                <p:DateTimeString formatCode=""304"">20231010101623Z</p:DateTimeString>
            </p:ReceivedDateTime>
            <p:ROE>H</p:ROE>
            <p:ICS>14</p:ICS>
            <p:LRN>HYEDUKCM20000000003420</p:LRN>
        </p:Declaration>
        <p2:Declaration>
            <p2:FunctionCode>9</p2:FunctionCode>
            <p2:TypeCode>IMD</p2:TypeCode>
            <p2:Submitter>
                <p2:ID>GB048834222514</p2:ID>
            </p2:Submitter>
            <p2:Declarant>
                <p2:ID>GB896458895015</p2:ID>
            </p2:Declarant>
            <p2:GoodsShipment>
                <p2:Consignment>
                    <p2:GoodsLocation>
                        <p2:Name>ABDABDABM</p2:Name>
                        <p2:TypeCode>A</p2:TypeCode>
                        <p2:Address>
                            <p2:TypeCode>U</p2:TypeCode>
                            <p2:CountryCode>GB</p2:CountryCode>
                        </p2:Address>
                    </p2:GoodsLocation>
                </p2:Consignment>
                <p2:Importer>
                    <p2:ID>GB896458895015</p2:ID>
                </p2:Importer>
            </p2:GoodsShipment>
        </p2:Declaration>
    </p:DeclarationSearchDetails>
    <p:CurrentPageNumber>1</p:CurrentPageNumber>
    <p:TotalResultsAvailable>2</p:TotalResultsAvailable>
    <p:TotalPagesAvailable>1</p:TotalPagesAvailable>
    <p:NoResultsReturned>false</p:NoResultsReturned>
</p:DeclarationSearchResponse>
";
			var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseText));

			_ = ProcessMessage(disResponse.Replace("Base64DataGoesHere", base64Content), eHubTrackingIdGuid, Events.ServiceRequested.Code);

			var newMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationReference, outgoingSentMessage.EM_ApplicationReference);
			newMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "RCV");
			var receivedMessage = Factory.Load<EDIMessage>(newMessageQuery).FirstOrDefault();

			AssertNotNull("Received message not found", receivedMessage);
			AssertEquals(CDSEDIMessageTypeList.Codes.QueryResponse, receivedMessage.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Acknowledged, outgoingSentMessage.EM_Status);

			AssertEquals(responseText, receivedMessage.EM_MessageText);
			AssertEquals("ABC123", receivedMessage.EM_ApplicationReference);
			AssertEquals(EDIMessage.Status.Queued, receivedMessage.EM_Status);
			AssertEquals(expectedDisSearchQueryResponseInterpretation, receivedMessage.EM_MessageInterpretation);
		}

		void AssertEntryWithConversationIDFromCIDOnly()
		{
			_ = ProcessMessage(validXML, sessionGuid, Events.CargoReportSentCode);
			_ = ProcessMessage(xmlWithCSPID, sessionGuid, Events.CargoReportAcceptedCode);
			AssertEquals(3, entry.Messages.Count);
			var originalMessage = entry.Messages[0];
			var cidMessage = entry.Messages[1];
			var cspMessage = entry.Messages[2];
			CombineAssertions(() =>
			{
				AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", cidMessage.EM_ApplicationReference);
				AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, cidMessage.EM_MessageSubType);
				AssertEquals("AAA.BBBBBB.123456789", cspMessage.EM_ApplicationReference);
				AssertEquals(CDSEDIMessageTypeList.Codes.CSPID, cspMessage.EM_MessageSubType);
				AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", originalMessage.EM_ApplicationReference);
			});
			entry.Messages.RemoveAll();
			SetupOutgoingMessage2();
			_ = ProcessMessage(xmlWithCSPID, sessionGuid2, Events.CargoReportAcceptedCode);
			_ = ProcessMessage(validXML, sessionGuid2, Events.CargoReportSentCode);
			AssertEquals(3, entry.Messages.Count);
			originalMessage = entry.Messages[0];
			cspMessage = entry.Messages[1];
			cidMessage = entry.Messages[2];
			CombineAssertions(() =>
			{
				AssertEquals("AAA.BBBBBB.123456789", cspMessage.EM_ApplicationReference);
				AssertEquals(CDSEDIMessageTypeList.Codes.CSPID, cspMessage.EM_MessageSubType);
				AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", cidMessage.EM_ApplicationReference);
				AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, cidMessage.EM_MessageSubType);
				AssertEquals("d03f84e3b5894aaaa1f8cafd744add8e", originalMessage.EM_ApplicationReference);
			});
		}

		ZString GetFormattedEventReference(Guid fileReference)
		{
			var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ContentID, fileReference.ToString() }
			};
			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		ZGuid SetupOutgoingMessage(string outgoingMessageStatus = EDIMessage.Status.Sent)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.JE_HouseBill = "TESTHOUSE";

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456";
			entry.CH_EntryStatus = "CES";
			entry.LRN = "LRN123";

			outgoingInterchange = Factory.New<EDIInterchange>();
			sessionGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = sessionGuid;
			outgoingInterchange.EI_HeaderText = "";
			outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(CDSNewDeclarationEDIMessage));
			entry.Messages.Add(outgoingSentMessage);
			outgoingSentMessage.EM_EI = outgoingInterchange.PK;
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingSentMessage.EM_Status = outgoingMessageStatus;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "AAW";
			outgoingInterchange.EI_BodyText = "";
			Factory.Save();
			outgoingSentMessage.EM_MessageNum = "999";
			Factory.Save();

			return sessionGuid;
		}

		void SetupOutgoingMessage2()
		{
			outgoingInterchange2 = Factory.New<EDIInterchange>();
			sessionGuid2 = ZGuid.NewZGuid();
			outgoingInterchange2.EI_SessionGUID = sessionGuid2;
			outgoingInterchange2.EI_HeaderText = "";
			outgoingSentMessage2 = outgoingInterchange2.ContainedMessages.AddNew(typeof(CDSNewDeclarationEDIMessage));
			entry.Messages.Add(outgoingSentMessage2);
			outgoingSentMessage2.EM_EI = outgoingInterchange2.PK;
			outgoingSentMessage2.EM_MessageText = "";
			outgoingSentMessage2.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange2.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange2.EI_Status = EDIMessage.Status.Sent;
			outgoingSentMessage2.EM_Status = EDIMessage.Status.Sent;
			outgoingInterchange2.EI_From = "Sender";
			outgoingInterchange2.EI_To = "AAW";
			outgoingInterchange2.EI_BodyText = "";
			Factory.Save();
			outgoingSentMessage2.EM_MessageNum = "998";
			Factory.Save();
		}

		ZGuid SetupOutgoingMessageForBill(IMessageAttachee bill, string outgoingMessageStatus = EDIMessage.Status.Sent)
		{
			outgoingInterchange = Factory.New<EDIInterchange>();
			sessionGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = sessionGuid;
			outgoingInterchange.EI_HeaderText = "";
			outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(CDSNewDeclarationEDIMessage));
			bill.Messages.Add(outgoingSentMessage);
			outgoingSentMessage.EM_EI = outgoingInterchange.PK;
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingSentMessage.EM_Status = outgoingMessageStatus;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "AAW";
			outgoingInterchange.EI_BodyText = "";
			Factory.Save();
			outgoingSentMessage.EM_MessageNum = "999";
			Factory.Save();

			return sessionGuid;
		}

		JobDeclarationEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger)
		{
			return new JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), logger);
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		EDIInterchange outgoingInterchange;
		EDIMessage outgoingSentMessage;
		EDIInterchange outgoingInterchange2;
		EDIMessage outgoingSentMessage2;
		ZGuid sessionGuid;
		ZGuid sessionGuid2;
		const string ExpectedFriendlyError = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><h3>Message [MessageNum] was rejected by an upstream system for the following reasons</h3><li>Supervising office, coded - Value '' is not facet-valid with respect to pattern '.*[^\s].*'</li><li>Element 'ID' must have no element [children], and the value must be valid.</li>";

		readonly ZString validXML = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ConversationID</Type>
				<Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithoutEHubTrackingID = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>ConversationID</Type>
				<Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithoutConversationID = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithErrorResponseAndCID = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ConversationID</Type>
				<Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>PGVycm9yUmVzcG9uc2U+CiAgICAgIDxjb2RlPkJBRF9SRVFVRVNUPC9jb2RlPgogICAgICA8bWVzc2FnZT5QYXlsb2FkIGlzIG5vdCB2YWxpZCBhY2NvcmRpbmcgdG8gc2NoZW1hPC9tZXNzYWdlPgogICAgICA8ZXJyb3JzPgogICAgICAgIDxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLXBhdHRlcm4tdmFsaWQ6IFZhbHVlICcnIGlzIG5vdCBmYWNldC12YWxpZCB3aXRoIHJlc3BlY3QgdG8gcGF0dGVybiAnLipbXlxzXS4qJyBmb3IgdHlwZSAnI0Fub25UeXBlX1N1cGVydmlzaW5nT2ZmaWNlSWRlbnRpZmljYXRpb25JRFR5cGUnLjwvbWVzc2FnZT4KICAgICAgICA8L2Vycm9yPjxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLWNvbXBsZXgtdHlwZS4yLjI6IEVsZW1lbnQgJ0lEJyBtdXN0IGhhdmUgbm8gZWxlbWVudCBbY2hpbGRyZW5dLCBhbmQgdGhlIHZhbHVlIG11c3QgYmUgdmFsaWQuPC9tZXNzYWdlPgogICAgICAgIDwvZXJyb3I+CiAgICAgIDwvZXJyb3JzPgogICAgPC9lcnJvclJlc3BvbnNlPg==</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithErrorResponseWithoutCID = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>PGVycm9yUmVzcG9uc2U+CiAgICAgIDxjb2RlPkJBRF9SRVFVRVNUPC9jb2RlPgogICAgICA8bWVzc2FnZT5QYXlsb2FkIGlzIG5vdCB2YWxpZCBhY2NvcmRpbmcgdG8gc2NoZW1hPC9tZXNzYWdlPgogICAgICA8ZXJyb3JzPgogICAgICAgIDxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLXBhdHRlcm4tdmFsaWQ6IFZhbHVlICcnIGlzIG5vdCBmYWNldC12YWxpZCB3aXRoIHJlc3BlY3QgdG8gcGF0dGVybiAnLipbXlxzXS4qJyBmb3IgdHlwZSAnI0Fub25UeXBlX1N1cGVydmlzaW5nT2ZmaWNlSWRlbnRpZmljYXRpb25JRFR5cGUnLjwvbWVzc2FnZT4KICAgICAgICA8L2Vycm9yPjxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLWNvbXBsZXgtdHlwZS4yLjI6IEVsZW1lbnQgJ0lEJyBtdXN0IGhhdmUgbm8gZWxlbWVudCBbY2hpbGRyZW5dLCBhbmQgdGhlIHZhbHVlIG11c3QgYmUgdmFsaWQuPC9tZXNzYWdlPgogICAgICAgIDwvZXJyb3I+CiAgICAgIDwvZXJyb3JzPgogICAgPC9lcnJvclJlc3BvbnNlPg==</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithCSPID = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>CSPEntryTrackingID</Type>
				<Value>AAA.BBBBBB.123456789</Value>
					</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithCIDAndCSP = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ConversationID</Type>
				<Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
			</Context>
			<Context>
				<Type>CSPEntryTrackingID</Type>
				<Value>AAA.BBBBBB.123456789</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString gvmsStyleXMLWithoutProvider = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>MSN</EventType>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>NotificationMessageId</Type>
            <Value>654321</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>
";

		readonly ZString gvmsStyleXMLWithProvider = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>MSN</EventType>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>NotificationMessageId</Type>
            <Value>654321</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>
";

		readonly ZString expectedUpdateEntryWithConversationID = @"<style>
body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}
tr {
  height: 50px;
  vertical-align: center;
}
.Status {
padding-left: 40px;
padding-right: 10px;
}
.StepProgress-item {
  margin-top: 15px;
}
.StepProgress-item{
  content: '';
  width: 12px;
  height: 12px;
}
.StepProgress-item.is-done {
  font-size: 16px;
  color: green;
  text-align: center;
  font-weight: bold;
}
.StepProgress-item.current{
  font-size: 12px;
  text-align: center;
  color: grey;
}
.StepProgress-item.rejected {
  font-size: 16px;
  color: red;
  text-align: center;
  font-weight: bold;
}
</style><p><h4>Message 999 was uploaded to CDS and received Conversation ID d03f84e3b5894aaaa1f8cafd744add8e</h4></p><p><h5></h5</p><table>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Message created</td>
	<td><small>Message number = 999 at 11/11/2019 12:00 (UTC)</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to eHub</td>
	<td><small>eHub Tracking ID = [SessionGUID]</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to CDS</td>
	<td><small>HMRC Conversation ID = d03f84e3b5894aaaa1f8cafd744add8e</small></td>
</tr>
</table><p/><table>
<tr style=""height: 20px;"">
<td>Status update time:      </td>
<td><small>13/08/2018 08:12 (UTC)</small></td>
</tr>
<tr style=""height: 20px;"">
<td>DUCR (2/4):</td>
<td><small>[DUCR]</small></td>
</tr>
<tr style=""height: 20px;"">
<td>LRN (2/5):</td>
<td><small>LRN123</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>B00000001</small></td>
</tr>
</table>
<p>
<h4>For further information please see our learning units</h4>
<ul>
<li>1BGB046 for direct communication with CDS</li>
<li>1BGB047 for CSP communication with CDS</li>
</ul>";

		readonly ZString expectedUpdateEntryWithCSPID = @"<style>
body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}
tr {
  height: 50px;
  vertical-align: center;
}
.Status {
padding-left: 40px;
padding-right: 10px;
}
.StepProgress-item {
  margin-top: 15px;
}
.StepProgress-item{
  content: '';
  width: 12px;
  height: 12px;
}
.StepProgress-item.is-done {
  font-size: 16px;
  color: green;
  text-align: center;
  font-weight: bold;
}
.StepProgress-item.current{
  font-size: 12px;
  text-align: center;
  color: grey;
}
.StepProgress-item.rejected {
  font-size: 16px;
  color: red;
  text-align: center;
  font-weight: bold;
}
</style><p><h4>Message 999 was uploaded to the CSP and received tracking ID AAA.BBBBBB.123456789; it has not yet necessarily reached CDS.</h4></p><table>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Message created</td>
	<td><small>Message number = 999 at 11/11/2019 12:00 (UTC)</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to eHub</td>
	<td><small>eHub Tracking ID = [SessionGUID]</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to CSP</td>
	<td><small>CSP Tracking ID = AAA.BBBBBB.123456789</small></td>
</tr>
<tr>
	<td class=""StepProgress-item current"">&#10004;</td>
	<td class=""Status"">Sent to CDS</td>
	<td>
		<small></small>
	</td>
</tr>
</table><p/><table>
<tr style=""height: 20px;"">
<td>Status update time:      </td>
<td><small>13/08/2018 08:12 (UTC)</small></td>
</tr>
<tr style=""height: 20px;"">
<td>DUCR (2/4):</td>
<td><small>[DUCR]</small></td>
</tr>
<tr style=""height: 20px;"">
<td>LRN (2/5):</td>
<td><small>LRN123</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>B00000001</small></td>
</tr>
</table>
<p>
<h4>For further information please see our learning units</h4>
<ul>
<li>1BGB046 for direct communication with CDS</li>
<li>1BGB047 for CSP communication with CDS</li>
</ul>";

		readonly ZString expectedUpdateEntryWithCIDAndCSP = @"<style>
body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}
tr {
  height: 50px;
  vertical-align: center;
}
.Status {
padding-left: 40px;
padding-right: 10px;
}
.StepProgress-item {
  margin-top: 15px;
}
.StepProgress-item{
  content: '';
  width: 12px;
  height: 12px;
}
.StepProgress-item.is-done {
  font-size: 16px;
  color: green;
  text-align: center;
  font-weight: bold;
}
.StepProgress-item.current{
  font-size: 12px;
  text-align: center;
  color: grey;
}
.StepProgress-item.rejected {
  font-size: 16px;
  color: red;
  text-align: center;
  font-weight: bold;
}
</style><p><h4>Message 999 was uploaded to CDS and received Conversation ID d03f84e3b5894aaaa1f8cafd744add8e</h4></p><p><h5></h5</p><table>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Message created</td>
	<td><small>Message number = 999 at 11/11/2019 12:00 (UTC)</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to eHub</td>
	<td><small>eHub Tracking ID = [SessionGUID]</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to CSP</td>
	<td><small>CSP Tracking ID = AAA.BBBBBB.123456789</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to CDS</td>
	<td><small>HMRC Conversation ID = d03f84e3b5894aaaa1f8cafd744add8e</small></td>
</tr>
</table><p/><table>
<tr style=""height: 20px;"">
<td>Status update time:      </td>
<td><small>13/08/2018 08:12 (UTC)</small></td>
</tr>
<tr style=""height: 20px;"">
<td>DUCR (2/4):</td>
<td><small>[DUCR]</small></td>
</tr>
<tr style=""height: 20px;"">
<td>LRN (2/5):</td>
<td><small>LRN123</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>B00000001</small></td>
</tr>
</table>
<p>
<h4>For further information please see our learning units</h4>
<ul>
<li>1BGB046 for direct communication with CDS</li>
<li>1BGB047 for CSP communication with CDS</li>
</ul>";
		readonly ZString expectedUpdateEntryWithCIDAndError = @"<style>
body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}
tr {
  height: 50px;
  vertical-align: center;
}
.Status {
padding-left: 40px;
padding-right: 10px;
}
.StepProgress-item {
  margin-top: 15px;
}
.StepProgress-item{
  content: '';
  width: 12px;
  height: 12px;
}
.StepProgress-item.is-done {
  font-size: 16px;
  color: green;
  text-align: center;
  font-weight: bold;
}
.StepProgress-item.current{
  font-size: 12px;
  text-align: center;
  color: grey;
}
.StepProgress-item.rejected {
  font-size: 16px;
  color: red;
  text-align: center;
  font-weight: bold;
}
</style><p><h4>Message 999 was rejected by CDS and received Conversation ID d03f84e3b5894aaaa1f8cafd744add8e</h4></p><p><h5>Please refer to other messages for details of the failure</h5</p><table>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Message created</td>
	<td><small>Message number = 999 at 11/11/2019 12:00 (UTC)</small></td>
</tr>
<tr>
	<td class=""StepProgress-item is-done"">&#10004;</td>
	<td class=""Status"">Sent to eHub</td>
	<td><small>eHub Tracking ID = [SessionGUID]</small></td>
</tr>
<tr>
	<td class=""StepProgress-item rejected"">&#x2716;</td>
	<td class=""Status"">Sent to CDS</td>
	<td><small>HMRC Conversation ID = d03f84e3b5894aaaa1f8cafd744add8e</small></td>
</tr>
</table><p/><table>
<tr style=""height: 20px;"">
<td>Status update time:      </td>
<td><small>13/08/2018 08:12 (UTC)</small></td>
</tr>
<tr style=""height: 20px;"">
<td>DUCR (2/4):</td>
<td><small>[DUCR]</small></td>
</tr>
<tr style=""height: 20px;"">
<td>LRN (2/5):</td>
<td><small>LRN123</small></td>
</tr>
<tr style=""height: 20px;"">
<td>Job number:</td>
<td><small>B00000001</small></td>
</tr>
</table>
<p>
<h4>For further information please see our learning units</h4>
<ul>
<li>1BGB046 for direct communication with CDS</li>
<li>1BGB047 for CSP communication with CDS</li>
</ul>";

		readonly ZString xmlErrorWithoutResponseText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key />
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-02-25T08:09:10.123</EventTime>
		<EventType>{0}</EventType>
		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
			  <Type>ErrorSummary</Type>
			  <Value>Timeout</Value>
			</Context>
			<Context>
			  <Type>ErrorDescription</Type>
			  <Value>An unexpected failure occurred while processing a message. The text associated with the exception is ""One or more errors occurred."".</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string ExpectedFriendlyErrorWithoutResponseText = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><h3>Message [MessageNum] was rejected by an upstream system for the following reasons</h3><li>An unexpected failure occurred while processing a message. The text associated with the exception is ""One or more errors occurred."".</li>";

		readonly ZString xmlQueryResponse = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>ABC123</EventReference>

		<ContextCollection>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>DUCR</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>ABC12300005</Value>
			</Context>
			<Context>
				<Type>NotificationType</Type>
				<Value>status</Value>
			</Context>

			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>Base64DataGoesHere</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlQueryWithoutContextCollection = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>ABC123</EventReference>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithNonXMLErrorResponseAndContexts = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00000001</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2022-03-22T09:27:33</EventTime>
		<EventType>{0}</EventType>
		<ContextCollection>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>PCFET0NUWVBFIEhUTUwgUFVCTElDICItLy9XM0MvL0RURCBIVE1MIDQuMCBEcmFmdC8vRU4iPgo8SFRNTD4KPEhFQUQ+CjxUSVRMRT5FcnJvciA0MDEtLVVuYXV0aG9yaXplZDwvVElUTEU+CjwvSEVBRD4KPEJPRFkgYmdjb2xvcj0id2hpdGUiPgo8Rk9OVCBGQUNFPUhlbHZldGljYT48QlIgQ0xFQVI9YWxsPgo8VEFCTEUgYm9yZGVyPTAgY2VsbHNwYWNpbmc9NT48VFI+PFREPjxCUiBDTEVBUj1hbGw+CjxGT05UIEZBQ0U9IkhlbHZldGljYSIgQ09MT1I9ImJsYWNrIiBTSVpFPSIzIj48SDI+RXJyb3IgNDAxLS1VbmF1dGhvcml6ZWQ8L0gyPgo8L0ZPTlQ+PC9URD48L1RSPgo8L1RBQkxFPgo8VEFCTEUgYm9yZGVyPTAgd2lkdGg9MTAwJSBjZWxscGFkZGluZz0xMD48VFI+PFREIFZBTElHTj10b3AgV0lEVEg9MTAwJSBCR0NPTE9SPXdoaXRlPjxGT05UIEZBQ0U9IkNvdXJpZXIgTmV3Ij48Rk9OVCBGQUNFPSJIZWx2ZXRpY2EiIFNJWkU9IjMiPjxIMz5Gcm9tIFJGQyAyMDY4IDxpPkh5cGVydGV4dCBUcmFuc2ZlciBQcm90b2NvbCAtLSBIVFRQLzEuMTwvaT46PC9IMz4KPC9GT05UPjxGT05UIEZBQ0U9IkhlbHZldGljYSIgU0laRT0iMyI+PEg0PjEwLjQuMiA0MDEgVW5hdXRob3JpemVkPC9IND4KPC9GT05UPjxQPjxGT05UIEZBQ0U9IkNvdXJpZXIgTmV3Ij5UaGUgcmVxdWVzdCByZXF1aXJlcyB1c2VyIGF1dGhlbnRpY2F0aW9uLiBUaGUgcmVzcG9uc2UgTVVTVCBpbmNsdWRlIGEgV1dXLUF1dGhlbnRpY2F0ZSBoZWFkZXIgZmllbGQgKHNlY3Rpb24gMTQuNDYpIGNvbnRhaW5pbmcgYSBjaGFsbGVuZ2UgYXBwbGljYWJsZSB0byB0aGUgcmVxdWVzdGVkIHJlc291cmNlLiBUaGUgY2xpZW50IE1BWSByZXBlYXQgdGhlIHJlcXVlc3Qgd2l0aCBhIHN1aXRhYmxlIEF1dGhvcml6YXRpb24gaGVhZGVyIGZpZWxkIChzZWN0aW9uIDE0LjgpLiBJZiB0aGUgcmVxdWVzdCBhbHJlYWR5IGluY2x1ZGVkIEF1dGhvcml6YXRpb24gY3JlZGVudGlhbHMsIHRoZW4gdGhlIDQwMSByZXNwb25zZSBpbmRpY2F0ZXMgdGhhdCBhdXRob3JpemF0aW9uIGhhcyBiZWVuIHJlZnVzZWQgZm9yIHRob3NlIGNyZWRlbnRpYWxzLiBJZiB0aGUgNDAxIHJlc3BvbnNlIGNvbnRhaW5zIHRoZSBzYW1lIGNoYWxsZW5nZSBhcyB0aGUgcHJpb3IgcmVzcG9uc2UsIGFuZCB0aGUgdXNlciBhZ2VudCBoYXMgYWxyZWFkeSBhdHRlbXB0ZWQgYXV0aGVudGljYXRpb24gYXQgbGVhc3Qgb25jZSwgdGhlbiB0aGUgdXNlciBTSE9VTEQgYmUgcHJlc2VudGVkIHRoZSBlbnRpdHkgdGhhdCB3YXMgZ2l2ZW4gaW4gdGhlIHJlc3BvbnNlLCBzaW5jZSB0aGF0IGVudGl0eSBNQVkgaW5jbHVkZSByZWxldmFudCBkaWFnbm9zdGljIGluZm9ybWF0aW9uLiBIVFRQIGFjY2VzcyBhdXRoZW50aWNhdGlvbiBpcyBleHBsYWluZWQgaW4gc2VjdGlvbiAxMS48L0ZPTlQ+PC9QPgo8L0ZPTlQ+PC9URD48L1RSPgo8L1RBQkxFPgoKPC9CT0RZPgo8L0hUTUw+Cg==</Value>
			</Context>
			<Context>
				<Type>Error</Type>
				<Value>MCP Registration Failed</Value>
			</Context>
			<Context>
				<Type>Badge</Type>
				<Value>FWC</Value>
			</Context>
			<Context>
				<Type>Topic</Type>
				<Value>FWC9</Value>
			</Context>
			<Context>
				<Type>ConversationID</Type>
				<Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlInvalidBase64StringOnMessageResponseRejected = @"<UniversalEvent	xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00000000</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2022-09-23T08:30:23</EventTime>
		<EventType>MRJ</EventType>
		<ContextCollection>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>INVENTORY-REFERENCE</Value>
			</Context>
			<Context>
				<Type>EntryNumber</Type>
				<Value>HSLS0000000000000000000</Value>
			</Context>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{0}</Value>
			</Context>
			<Context>
				<Type>FailedAction</Type>
				<Value>Invalid Request</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>The request failed with the following code: 401 - Unauthorized</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>&lt;?xml version='1.0' encoding='UTF-8'?&gt;
&lt;errorResponse&gt;
      &lt;code&gt;UNAUTHORIZED&lt;/code&gt;
      &lt;message&gt;Unauthorised request&lt;/message&gt;
      
    &lt;/errorResponse&gt;</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlInvalidBase64StringOnMessageRejected = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CustomsDeclaration</Type>
					<Key>B00000001</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ConversationID</Type>
				<Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>&lt;?xml version='1.0' encoding='UTF-8'?&gt;
&lt;errorResponse&gt;
      &lt;code&gt;UNAUTHORIZED&lt;/code&gt;
      &lt;message&gt;Unauthorised request&lt;/message&gt;
      
    &lt;/errorResponse&gt;</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string ExpectedFriendlyErrorInvalidBase64StringOnMessageRejected = @"<?xml version='1.0' encoding='UTF-8'?>
<errorResponse>
      <code>UNAUTHORIZED</code>
      <message>Unauthorised request</message>
      
    </errorResponse>";

		readonly ZString dsnResponse = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>CustomsDeclaration</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2022-06-07T14:55:40</EventTime>
        <EventType>DSN</EventType>
        <ContextCollection>
          <Context>
            <Type>ConversationID</Type>
            <Value>d03f84e3-b589-4aaa-a1f8-cafd744add8e</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
          <Context>
            <Type>ResponseText</Type>
            <Value>
				{2}
			</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>";
		const string dsnFileResponsePart = @"<File>
            <Reference>{0}</Reference>
            <UploadRequest>
              <Href>https://fus-ro-dah.s3.awsazon.com</Href>
              <Fields>
				<randomdatafields/>
              </Fields>
            </UploadRequest>
          </File>";
		const string dsnFileUploadResponsePart = @"<FileUploadResponse xmlns=""hmrc:fileupload"">
        <Files>
          {0}
		  {1}
        </Files>
      </FileUploadResponse>";

		readonly ZString disResponse = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>B60007163</Key>
             <Type>CustomsDeclaration</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2023-10-25T12:34:36</EventTime>
        <EventType>SVR</EventType>
        <ContextCollection>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
          <Context>
            <Type>ResponseText</Type>
            <Value>Base64DataGoesHere</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>";

		const string expectedDisSearchQueryResponseInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}table, tr, td {border: none;}hr.rounded {border-top: 4px solid #bbb;border-radius: 5px;</style><H4>Page 1 of 1: 2 results</H4><p><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td width=""33%"">MRN: 23GBB7KF5523KMKAR1</td><td width=""33%"">LRN: HYEDUKCM20000000003421</td><td width=""33%"">Date: 10-Oct-23 10:22:47</td></tr><tr><td>Status: route H, ICS 14</td><td>Type: IMD</td><td>Location: GBAUABDABDABM</td></tr><tr><td>Importer: GB896458895015</td><td>Declarant: GB896458895015</td><td>Submitter: GB048834222514</td></tr></table></p><hr class=""rounded""><p><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td width=""33%"">MRN: 23GBB7K6WRBIPQDAR8</td><td width=""33%"">LRN: HYEDUKCM20000000003420</td><td width=""33%"">Date: 10-Oct-23 10:16:23</td></tr><tr><td>Status: route H, ICS 14</td><td>Type: IMD</td><td>Location: GBAUABDABDABM</td></tr><tr><td>Importer: GB896458895015</td><td>Declarant: GB896458895015</td><td>Submitter: GB048834222514</td></tr></table></p><hr class=""rounded"">";
	}
}
