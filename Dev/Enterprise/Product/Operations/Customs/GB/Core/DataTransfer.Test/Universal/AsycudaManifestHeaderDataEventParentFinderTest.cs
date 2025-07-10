using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GVMS;
using Enterprise.Customs.GB.ICS;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.GB.GVMS.AsycudaManifestHeader;
using GBICSAsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeader;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class AsycudaManifestHeaderDataEventParentFinderTest : TestCaseWithFactory
	{
		public void TestUpdateManifestWithCorrelationId()
		{
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			var correlationId = "0987654321";
			var outgoingMessage = CreateManifestAndOutgoingMessage(eHubTrackingIdGuid);

			AssertNotEquals("Pre-req", correlationId, outgoingMessage.EM_ApplicationReference);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, icsAsycudaManifestXml, AsycudaEventMessageConstants.EventTypes.MRR, eHubTrackingIdGuid, correlationId));
			var subscriber = GetNewEventParentFinder();
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			var manifestHeader = (GBICSAsycudaManifestHeader)logParents[0];
			Factory.Save();
			manifestHeader.Reload();
			AssertEquals(2, manifestHeader.Messages.Count);
			var lastMessage = manifestHeader.Messages[1];
			AssertEquals(EDIMessage.Direction.Receive, lastMessage.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Received, lastMessage.EM_Status);
			AssertEquals("EM_ApplicationReference has been set to CorrelationID from eHub event", correlationId, lastMessage.EM_ApplicationReference);
			AssertEquals(MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
			AssertEquals("EM_LinkedObject should be set to the header", outgoingMessage.EM_LinkedObject, lastMessage.EM_LinkedObject);

			AssertContains($"Message {outgoingMessage.EM_MessageNum} received correlation ID {correlationId}", lastMessage.EM_MessageInterpretation);
		}

		EDIMessage CreateManifestAndOutgoingMessage(ZGuid eHubTrackingIdGuid)
		{
			var manifestHeader = Factory.New<GBICSAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN12345";
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.ICS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_SessionGUID = eHubTrackingIdGuid;
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "ICS";

			var outgoingMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(IcsNorthernIrelandEDIMessage));
			outgoingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.GbMessageICSGreatBritain;
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "999";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;

			manifestHeader.Messages.Add(outgoingMessage);
			outgoingMessage.EM_LinkedObject = manifestHeader;
			Factory.Save();

			return outgoingMessage;
		}

		public void TestUpdateManifestWithoutCorrelationId()
		{
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			var outgoingMessage = CreateManifestAndOutgoingMessage(eHubTrackingIdGuid);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, icsAsycudaManifestXmlWithoutCorrelationID, AsycudaEventMessageConstants.EventTypes.MRR, eHubTrackingIdGuid));
			var subscriber = GetNewEventParentFinder();
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			var manifestHeader = (GBICSAsycudaManifestHeader)logParents[0];

			AssertEquals(2, manifestHeader.Messages.Count);
			var lastMessage = manifestHeader.Messages[1];
			AssertEquals(EDIMessage.Direction.Receive, lastMessage.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Received, lastMessage.EM_Status);
			AssertEquals(MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
			AssertEquals("EM_LinkedObject should be set to the header", outgoingMessage.EM_LinkedObject, lastMessage.EM_LinkedObject);
		}

		public void TestIcsErrorResponse()
		{
			var eHubTrackingIdGuid = ZGuid.NewZGuid();
			var outgoingMessage = CreateManifestAndOutgoingMessage(eHubTrackingIdGuid);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, icsAsycudaManifestXmlErrorResponse, eHubTrackingIdGuid));
			var subscriber = GetNewEventParentFinder();
			var eventDataObject = xmlEvent as UniversalEvent;
			var logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			var manifestHeader = (GBICSAsycudaManifestHeader)logParents[0];

			CombineAssertions(() =>
			{
				AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Rejected, outgoingMessage.EM_Status);
				AssertEquals("Messages.Count", 2, manifestHeader.Messages.Count);
			});
			var lastMessage = manifestHeader.Messages[1];

			CombineAssertions(() =>
			{
				AssertEquals("manifestHeader.AMA_MessageStatus", MessageStatusCodeList.Codes.Error, manifestHeader.AMA_MessageStatus);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, lastMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Received, lastMessage.EM_Status);
				AssertEquals("EM_MessageText", errorResponseDecoded, lastMessage.EM_MessageText);
				AssertEquals("EM_MessageInterpretation", errorResponseInterpretation, lastMessage.EM_MessageInterpretation);
				AssertEquals("EM_LinkedObject should be set to the header", outgoingMessage.EM_LinkedObject, lastMessage.EM_LinkedObject);
			});
		}

		AsycudaManifestHeaderDataEventParentFinder GetNewEventParentFinder()
		{
			return new AsycudaManifestHeaderDataEventParentFinder(Factory, new AsycudaManifestHeaderDataContextManager(), new TestErrorLogger());
		}

		public void TestUpdateEntryWithNotificationMessageIdWhenOutgoingMessageStatusIsRejected()
		{
			SetupOutgoingMessage(EDIMessage.Status.Rejected);

			var logParents = ProcessMessage(validGvmsXML, sessionGuid, AutoEvents.MessageSentCode);
			var header = (AsycudaManifestHeader)logParents.First();
			AssertEquals(2, header.Messages.Count);
			var originalMessage = header.Messages[0];
			var notificationIdMessage = header.Messages[1];
			AssertEquals("654321", notificationIdMessage.EM_ApplicationReference);
			AssertEquals(GVMS.Constants.GVMSMessageSubTypes.NOTIFICATIONMESSAGEID, notificationIdMessage.EM_MessageSubType);
			// todo - pretty interpretation
			//AssertContains(expectedUpdateEntryWithConversationID.Replace("[SessionGUID]", sessionGuid.ToString()), notificationIdMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Rejected, originalMessage.EM_Status);
			AssertEquals("654321", originalMessage.EM_ApplicationReference);
			AssertEquals(EDIMessage.Status.Received, notificationIdMessage.EM_Status);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsGVMSManifest, notificationIdMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.Sent, header.AMA_MessageStatus);
		}
		public void TestUpdateEntryWithNotificatonMessageId()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(validGvmsXML, sessionGuid, AutoEvents.MessageSentCode);
			var header = (AsycudaManifestHeader)logParents.First();
			AssertEquals(2, header.Messages.Count);
			var originalMessage = header.Messages[0];
			var notificationMessageIdMessage = header.Messages[1];
			AssertEquals("654321", notificationMessageIdMessage.EM_ApplicationReference);
			AssertEquals(GVMS.Constants.GVMSMessageSubTypes.NOTIFICATIONMESSAGEID, notificationMessageIdMessage.EM_MessageSubType);
			// todo - pretty interpretation
			// AssertContains(expectedUpdateEntryWithnotificationMessageId.Replace("[SessionGUID]", sessionGuid.ToString()), conversationIdMessage.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Acknowledged, originalMessage.EM_Status);
			AssertEquals("654321", originalMessage.EM_ApplicationReference);
			AssertEquals(EDIMessage.Status.Received, notificationMessageIdMessage.EM_Status);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsGVMSManifest, notificationMessageIdMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.Sent, header.AMA_MessageStatus);
		}

		public void TestUpdateEntryWithNoEHubTrackingID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithoutEHubTrackingID, sessionGuid, AutoEvents.MessageSentCode);
			AssertNull(logParents);
		}

		public void TestUpdateEntryWithNoNotificationMessageId()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithoutNotificationMessageId, sessionGuid, AutoEvents.MessageSentCode);
			var header = (AsycudaManifestHeader)logParents.First();
			AssertEquals(1, header.Messages.Count);
			var originalMessage = header.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Sent);
		}

		public void TestUpdateEntryWithErrorResponseAndNotificationID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithErrorResponseAndNotificationMessageId, sessionGuid, AutoEvents.MessageRejectedCode);
			var header = (AsycudaManifestHeader)logParents.First();
			AssertEquals(3, header.Messages.Count);
			var originalMessage = header.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Rejected);

			var notificationMessageIdMessage = header.Messages[1];
			CombineAssertions("Notification Message", () =>
			{
				AssertEquals("654321", notificationMessageIdMessage.EM_ApplicationReference);
				AssertEquals(GVMS.Constants.GVMSMessageSubTypes.NOTIFICATIONMESSAGEID, notificationMessageIdMessage.EM_MessageSubType);
				AssertEquals(notificationMessageIdMessage.EM_Status, EDIMessage.Status.Received);
				AssertEquals(ApplicationCodeList.Codes.GbCustomsGVMSManifest, notificationMessageIdMessage.EM_ApplicationCode);
				AssertEquals("EM_LinkTable", AsycudaManifestHeader.Schema.TableName, notificationMessageIdMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", header.PK, notificationMessageIdMessage.EM_LinkUniqueID);
				AssertSame("EM_LinkedObject", header, notificationMessageIdMessage.EM_LinkedObject);
			});

			var errorMessage = header.Messages[2] as GVMSErrorResponseEDIMessage;
			CombineAssertions("Error Message", () =>
			{
				AssertEquals(GVMS.Constants.GVMSMessageSubTypes.EHUBERRORRESPONSE, errorMessage.EM_MessageType);
				AssertContains("<errorResponse>", errorMessage.EM_MessageText);
				AssertEquals(errorMessage.EM_Status, EDIMessage.Status.Received);
				AssertEquals(ApplicationCodeList.Codes.GbCustomsGVMSManifest, errorMessage.EM_ApplicationCode);
				AssertEquals("EM_LinkTable", AsycudaManifestHeader.Schema.TableName, errorMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", header.PK, errorMessage.EM_LinkUniqueID);
				AssertSame("EM_LinkedObject", header, errorMessage.EM_LinkedObject);
			});
		}

		public void TestUpdateEntryWithErrorResponseAndWithoutNotificationID()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlWithErrorResponseAndWithoutNotificationMessageId, sessionGuid, AutoEvents.MessageRejectedCode);
			var header = (AsycudaManifestHeader)logParents.First();
			AssertEquals(2, header.Messages.Count);
			var originalMessage = header.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Rejected);

			var errorMessage = header.Messages[1] as GVMSErrorResponseEDIMessage;
			AssertEquals(GVMS.Constants.GVMSMessageSubTypes.EHUBERRORRESPONSE, errorMessage.EM_MessageType);
			AssertContains("<errorResponse>", errorMessage.EM_MessageText);
			AssertEquals(errorMessage.EM_Status, EDIMessage.Status.Received);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsGVMSManifest, errorMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.ErrorReplace, header.AMA_MessageStatus);
		}

		public void TestErrorsWithoutResponseText()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(xmlErrorWithoutResponseText, sessionGuid, AutoEvents.MessageRejectedCode);
			var header = (AsycudaManifestHeader)logParents.First();
			AssertEquals(2, header.Messages.Count);
			var originalMessage = header.Messages[0];
			AssertEquals(originalMessage.EM_Status, EDIMessage.Status.Rejected);

			var errorMessage = header.Messages[1] as GVMSErrorResponseEDIMessage;
			AssertEquals(GVMS.Constants.GVMSMessageSubTypes.EHUBERRORRESPONSE, errorMessage.EM_MessageType);
			AssertContains("<errorResponse>", errorMessage.EM_MessageText);
			AssertEquals(errorMessage.EM_Status, EDIMessage.Status.Received);
			AssertEquals(ApplicationCodeList.Codes.GbCustomsGVMSManifest, errorMessage.EM_ApplicationCode);
			AssertEquals(MessageStatusList.Codes.ErrorReplace, header.AMA_MessageStatus);
		}

		public void TestErrorMessageQueuedWithValidJson()
		{
			SetupOutgoingMessage();

			var json64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonErrorDetails));
			var xml = xmlWithEmbeddedJsonErrorResponse.Replace("REPLACE_ME", json64);
			var logParents = ProcessMessage(xml, sessionGuid, AutoEvents.MessageRejectedCode);
			var header = (AsycudaManifestHeader)logParents.First();
			AssertEquals(2, header.Messages.Count);

			var errorMessage = header.Messages[1] as GVMSErrorResponseEDIMessage;
			AssertEquals(GVMS.Constants.GVMSMessageSubTypes.EHUBERRORRESPONSE, errorMessage.EM_MessageType);
			AssertContains(@"""messageId"": ""aaaaaaaa-bbbb-cccc-dddd-ffffffffffff""", errorMessage.EM_MessageText);
			AssertEquals(errorMessage.EM_Status, EDIMessage.Status.Queued);
		}

		public void TestCancellationMessage()
		{
			SetupOutgoingMessage(msgSubType: "CAN");

			var logParents = ProcessMessage(validGvmsXML, sessionGuid, AutoEvents.MessageSentCode);
			var header = (AsycudaManifestHeader)logParents.First();

			AssertEquals("CAN", header.RegistrationStatus);
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

		public void TestCDSTypeRefused()
		{
			SetupOutgoingMessage();
			var logParents = ProcessMessage(validCDSXML, sessionGuid, AutoEvents.MessageSentCode);
			AssertNull("CDS Style event - data target > type isolated", logParents);
		}

		[ExpectNoExceptions]
		public void TestOnlyProcessGVMSMessagesForRejections()
		{
			SetupOutgoingMessage();

			var logParents = ProcessMessage(validCTCRejection, sessionGuid, AutoEvents.MessageRejectedCode);
			AssertNull(logParents);
			logParents = ProcessMessage(invalidCTCRejection, sessionGuid, AutoEvents.MessageRejectedCode);
			AssertNull(logParents);
		}

		void SetupOutgoingMessage(string outgoingMessageStatus = EDIMessage.Status.Sent, string msgSubType = "")
		{
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN000001";
			header.AMA_ManifestType = "GVM";

			outgoingInterchange = Factory.New<EDIInterchange>();
			sessionGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = sessionGuid;
			outgoingInterchange.EI_HeaderText = "";
			outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(GVMSEDIMessage));
			header.Messages.Add(outgoingSentMessage);
			outgoingSentMessage.EM_LinkedObject = header;
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

			outgoingSentMessage.EM_MessageSubType = msgSubType;

			Factory.Save();
			outgoingSentMessage.EM_MessageNum = "999";
			Factory.Save();
		}

		AsycudaManifestHeaderDataEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger)
		{
			return new AsycudaManifestHeaderDataEventParentFinder(Factory, new GBGVMSAsycudaManifestHeaderDataContextManager(), logger);
		}

		AsycudaManifestHeader header;
		EDIInterchange outgoingInterchange;
		EDIMessage outgoingSentMessage;
		ZGuid sessionGuid;

		readonly ZString validGvmsXML = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{0}</EventType>
		<EventReference>1</EventReference>
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
		readonly ZString xmlWithoutEHubTrackingID = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{0}</EventType>
		<EventReference>1</EventReference>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>NotificationMessageId</Type>
            <Value>654321</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>
";

		readonly ZString xmlWithoutNotificationMessageId = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{0}</EventType>
		<EventReference>1</EventReference>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>
";

		readonly ZString xmlWithErrorResponseAndNotificationMessageId = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{0}</EventType>
		<EventReference>1</EventReference>
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
	      <Context>
			<Type>ResponseText</Type>
			<Value>PGVycm9yUmVzcG9uc2U+CiAgICAgIDxjb2RlPkJBRF9SRVFVRVNUPC9jb2RlPgogICAgICA8bWVzc2FnZT5QYXlsb2FkIGlzIG5vdCB2YWxpZCBhY2NvcmRpbmcgdG8gc2NoZW1hPC9tZXNzYWdlPgogICAgICA8ZXJyb3JzPgogICAgICAgIDxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLXBhdHRlcm4tdmFsaWQ6IFZhbHVlICcnIGlzIG5vdCBmYWNldC12YWxpZCB3aXRoIHJlc3BlY3QgdG8gcGF0dGVybiAnLipbXlxzXS4qJyBmb3IgdHlwZSAnI0Fub25UeXBlX1N1cGVydmlzaW5nT2ZmaWNlSWRlbnRpZmljYXRpb25JRFR5cGUnLjwvbWVzc2FnZT4KICAgICAgICA8L2Vycm9yPjxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLWNvbXBsZXgtdHlwZS4yLjI6IEVsZW1lbnQgJ0lEJyBtdXN0IGhhdmUgbm8gZWxlbWVudCBbY2hpbGRyZW5dLCBhbmQgdGhlIHZhbHVlIG11c3QgYmUgdmFsaWQuPC9tZXNzYWdlPgogICAgICAgIDwvZXJyb3I+CiAgICAgIDwvZXJyb3JzPgogICAgPC9lcnJvclJlc3BvbnNlPg==</Value>
		  </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>
";

		readonly ZString xmlWithErrorResponseAndWithoutNotificationMessageId = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{0}</EventType>
		<EventReference>1</EventReference>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
	      <Context>
			<Type>ResponseText</Type>
			<Value>PGVycm9yUmVzcG9uc2U+CiAgICAgIDxjb2RlPkJBRF9SRVFVRVNUPC9jb2RlPgogICAgICA8bWVzc2FnZT5QYXlsb2FkIGlzIG5vdCB2YWxpZCBhY2NvcmRpbmcgdG8gc2NoZW1hPC9tZXNzYWdlPgogICAgICA8ZXJyb3JzPgogICAgICAgIDxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLXBhdHRlcm4tdmFsaWQ6IFZhbHVlICcnIGlzIG5vdCBmYWNldC12YWxpZCB3aXRoIHJlc3BlY3QgdG8gcGF0dGVybiAnLipbXlxzXS4qJyBmb3IgdHlwZSAnI0Fub25UeXBlX1N1cGVydmlzaW5nT2ZmaWNlSWRlbnRpZmljYXRpb25JRFR5cGUnLjwvbWVzc2FnZT4KICAgICAgICA8L2Vycm9yPjxlcnJvcj4KICAgICAgICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPgogICAgICAgICAgPG1lc3NhZ2U+Y3ZjLWNvbXBsZXgtdHlwZS4yLjI6IEVsZW1lbnQgJ0lEJyBtdXN0IGhhdmUgbm8gZWxlbWVudCBbY2hpbGRyZW5dLCBhbmQgdGhlIHZhbHVlIG11c3QgYmUgdmFsaWQuPC9tZXNzYWdlPgogICAgICAgIDwvZXJyb3I+CiAgICAgIDwvZXJyb3JzPgogICAgPC9lcnJvclJlc3BvbnNlPg==</Value>
		  </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>
";
		readonly ZString xmlErrorWithoutResponseText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{0}</EventType>
		<EventReference>1</EventReference>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
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
</UniversalEvent>
";
		readonly ZString validCDSXML = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
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
				<Value>MD2 Eample Rejections</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString validCTCRejection = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>NCT00000013</Key>
					<Type>NctsHeader</Type>
				</DataTarget>
				<DataSource>
				<DataProvider>CTCGB</DataProvider>
			</DataSource>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-10-25T14:14:11.783</EventTime>
		<EventType>{0}</EventType>
		<ContextCollection>
			<Context>
				<Type>DepartureId</Type>
				<Value>13579</Value>
			</Context>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>MD2 Eample Message 21</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString invalidCTCRejection = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>NCT00000013</Key>
					<Type>NctsHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-10-25T14:14:11.783</EventTime>
		<EventType>{0}</EventType>
		<DataContext>
			<DataSource>
				<DataProvider>CTCGB</DataProvider>
			</DataSource>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>DepartureId</Type>
				<Value>13579</Value>
			</Context>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>MD2 Eample Message 21</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly ZString xmlWithEmbeddedJsonErrorResponse = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{0}</EventType>
		<EventReference>1</EventReference>
        <ContextCollection>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{1}</Value>
          </Context>
	      <Context>
			<Type>ResponseText</Type>
			<Value>REPLACE_ME</Value>
		  </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

		readonly ZString jsonErrorDetails = @"{
  ""messageId"": ""aaaaaaaa-bbbb-cccc-dddd-ffffffffffff"",
  ""gmrId"": ""GMRO0000F2KW"",
  ""gmrStatusVersion"": 3,
  ""createdDateTime"": ""2021-09-11T10:58:12.384Z"",
  ""updatedDateTime"": ""2021-09-24T04:23:50.384Z"",
  ""state"": ""NOT_FINALISABLE"",
  ""inspectionRequired"": true,
  ""reportToLocationId"": ""L00013""
}";

		readonly ZString icsAsycudaManifestXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AsycudaManifest</Type>
					<Key>MAN12345</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>
        <DataContext>
          <DataSource>
            <DataProvider>ICSGB</DataProvider>
          </DataSource>
        </DataContext>
		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>CorrelationID</Type>
				<Value>{2}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly string icsAsycudaManifestXmlWithoutCorrelationID = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AsycudaManifest</Type>
					<Key>MAN12345</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-08-13T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<DataContext>
			<DataSource>
				<DataProvider>ICSGB</DataProvider>
			</DataSource>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>EHubTrackingID</Type>
				<Value>{1}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly string icsAsycudaManifestXmlErrorResponse = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
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
		<EventType>MRJ</EventType>
		<DataContext>
			<DataSource>
				<DataProvider>ICSGB</DataProvider>
			</DataSource>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{0}</Value>
			</Context>
			<Context>
				<Type>Error</Type>
				<Value>
				</Value>
			</Context>
			<Context>
				<Type>ErrorSummary</Type>
				<Value>
                                                        HTTP Status (400): BadRequest. Response content: [lt;err:ErrorResponse SchemaVersion=quot;2.0quot; xmlns:dsl=quot;http://decisionsoft.com/rim/errorExtensionquot; xmlns:err=quot;http://www.govtalk.gov.uk/CM/errorresponsequot;gt;lt;err:Applicationgt;lt;err:MessageCountgt;2lt;/err:MessageCountgt;lt;/err:Applicationgt;lt;err:Errorgt;lt;err:RaisedBygt;HMRClt;/err:RaisedBygt;lt;err:Numbergt;4065lt;/err:Numbergt;lt;err:Typegt;schemalt;/err:Typegt;lt;err:Textgt;Invalid content was found starting with element apos;IdeOfMeaOfTraCroHEA85apos;. One of apos;{{TraModAtBorHEA76}}apos; is expected.lt;/err:Textgt;lt;err:Locationgt;/q1:CC315A[1]/HEAHEA[1]lt;/err:Locationgt;lt;/err:Errorgt;lt;err:Errorgt;lt;err:RaisedBygt;HMRClt;/err:RaisedBygt;lt;err:Numbergt;4066lt;/err:Numbergt;lt;err:Typegt;schemalt;/err:Typegt;lt;err:Textgt;The content of element apos;CUSOFFFENT730apos; is not complete. One of apos;{{ExpDatOfArrFIRENT733}}apos; is expected.lt;/err:Textgt;lt;err:Locationgt;/q1:CC315A[1]/CUSOFFFENT730[1]lt;/err:Locationgt;lt;/err:Errorgt;lt;/err:ErrorResponsegt;]. 
                                                    </Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGVycjpFcnJvclJlc3BvbnNlIFNjaGVtYVZlcnNpb249IjIuMCIgeG1sbnM6ZHNsPSJodHRwOi8vZGVjaXNpb25zb2Z0LmNvbS9yaW0vZXJyb3JFeHRlbnNpb24iIHhtbG5zOmVycj0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay9DTS9lcnJvcnJlc3BvbnNlIj48ZXJyOkFwcGxpY2F0aW9uPjxlcnI6TWVzc2FnZUNvdW50PjI8L2VycjpNZXNzYWdlQ291bnQ+PC9lcnI6QXBwbGljYXRpb24+PGVycjpFcnJvcj48ZXJyOlJhaXNlZEJ5PkhNUkM8L2VycjpSYWlzZWRCeT48ZXJyOk51bWJlcj40MDY1PC9lcnI6TnVtYmVyPjxlcnI6VHlwZT5zY2hlbWE8L2VycjpUeXBlPjxlcnI6VGV4dD5JbnZhbGlkIGNvbnRlbnQgd2FzIGZvdW5kIHN0YXJ0aW5nIHdpdGggZWxlbWVudCAnSWRlT2ZNZWFPZlRyYUNyb0hFQTg1Jy4gT25lIG9mICd7VHJhTW9kQXRCb3JIRUE3Nn0nIGlzIGV4cGVjdGVkLjwvZXJyOlRleHQ+PGVycjpMb2NhdGlvbj4vcTE6Q0MzMTVBWzFdL0hFQUhFQVsxXTwvZXJyOkxvY2F0aW9uPjwvZXJyOkVycm9yPjxlcnI6RXJyb3I+PGVycjpSYWlzZWRCeT5ITVJDPC9lcnI6UmFpc2VkQnk+PGVycjpOdW1iZXI+NDA2NjwvZXJyOk51bWJlcj48ZXJyOlR5cGU+c2NoZW1hPC9lcnI6VHlwZT48ZXJyOlRleHQ+VGhlIGNvbnRlbnQgb2YgZWxlbWVudCAnQ1VTT0ZGRkVOVDczMCcgaXMgbm90IGNvbXBsZXRlLiBPbmUgb2YgJ3tFeHBEYXRPZkFyckZJUkVOVDczM30nIGlzIGV4cGVjdGVkLjwvZXJyOlRleHQ+PGVycjpMb2NhdGlvbj4vcTE6Q0MzMTVBWzFdL0NVU09GRkZFTlQ3MzBbMV08L2VycjpMb2NhdGlvbj48L2VycjpFcnJvcj48L2VycjpFcnJvclJlc3BvbnNlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly string errorResponseDecoded = @"<err:ErrorResponse SchemaVersion=""2.0"" xmlns:dsl=""http://decisionsoft.com/rim/errorExtension"" xmlns:err=""http://www.govtalk.gov.uk/CM/errorresponse""><err:Application><err:MessageCount>2</err:MessageCount></err:Application><err:Error><err:RaisedBy>HMRC</err:RaisedBy><err:Number>4065</err:Number><err:Type>schema</err:Type><err:Text>Invalid content was found starting with element 'IdeOfMeaOfTraCroHEA85'. One of '{TraModAtBorHEA76}' is expected.</err:Text><err:Location>/q1:CC315A[1]/HEAHEA[1]</err:Location></err:Error><err:Error><err:RaisedBy>HMRC</err:RaisedBy><err:Number>4066</err:Number><err:Type>schema</err:Type><err:Text>The content of element 'CUSOFFFENT730' is not complete. One of '{ExpDatOfArrFIRENT733}' is expected.</err:Text><err:Location>/q1:CC315A[1]/CUSOFFFENT730[1]</err:Location></err:Error></err:ErrorResponse>";
		readonly string errorResponseInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><h3>Response Errors</h3><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>HMRC</td></tr><tr><td>Error Number</td><td>4065</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>Invalid content was found starting with element 'IdeOfMeaOfTraCroHEA85'. One of '{TraModAtBorHEA76}' is expected.</td></tr><tr><td>Location</td><td>/q1:CC315A[1]/HEAHEA[1]</td></tr></table></ul><br><ul><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td><strong>Key</strong></td><td><strong>Value</strong></td></tr><tr><td>Raised By</td><td>HMRC</td></tr><tr><td>Error Number</td><td>4066</td></tr><tr><td>Error Type</td><td>schema</td></tr><tr><td>Error Text</td><td>The content of element 'CUSOFFFENT730' is not complete. One of '{ExpDatOfArrFIRENT733}' is expected.</td></tr><tr><td>Location</td><td>/q1:CC315A[1]/CUSOFFFENT730[1]</td></tr></table></ul><br>";
	}
}
