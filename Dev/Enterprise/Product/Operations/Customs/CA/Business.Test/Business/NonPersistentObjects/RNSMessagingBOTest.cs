using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(RNSMessagingBO))]
	sealed class RNSMessagingBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "TEST1234567";
			var messaging = GetRNSMessagingBO(shipment);
			var requestData = (IRNSRequestData)messaging;
			var attachee = ((IEDIFACTMessageAttachee)messaging);

			AssertEquals("RN_ReleaseStatus", "Not Sent", messaging.RN_ReleaseStatus);
			AssertEquals("RN_ReleaseDate", ZDateTime.Empty, messaging.RN_ReleaseDate);
			AssertEquals("MessageStatus", string.Empty, attachee.MessageStatus);
			AssertEquals("HouseBillNumber", "TEST1234567", requestData.HouseBillNumber);

			var request = messaging.Messages.AddNew(typeof(RNSRequestMessage));
			request.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_Status = EDIMessage.Status.Sent;
			request.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var mf1 = messaging.Messages.AddNew(typeof(ACIHouseBillMessage));
			mf1.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
			mf1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mf1.EM_Status = EDIMessage.Status.Received;
			mf1.EM_SystemCreateTimeUtc = request.EM_SystemCreateTimeUtc.AddMinutes(5);

			AssertEquals("RN_ReleaseStatus", "Awaiting Warehouse Arrival Certification Message Original", messaging.RN_ReleaseStatus);
			AssertEquals("RN_ReleaseDate", ZDateTime.Empty, messaging.RN_ReleaseDate);
			AssertEquals("MessageStatus", MessageStatusList.Codes.AwaitingOriginal, attachee.MessageStatus);

			var response = messaging.Messages.AddNew(typeof(EDIReleaseMessage));
			response.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_Status = EDIMessage.Status.Received;
			response.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::125+12345000067897+11'DTM+58:201006221028:203'GIS+4'RFF+CN:CCN123456'UNT+6+1'";
			response.EM_SystemCreateTimeUtc = request.EM_SystemCreateTimeUtc.AddMinutes(10);

			var reject = messaging.Messages.AddNew(typeof(EDIReleaseMessage));
			reject.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.MessageContentRejected;
			reject.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			reject.EM_Status = EDIMessage.Status.Received;
			reject.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::489+12345000067897+11'DTM+58:201006221028:203'GIS+2'RFF+CN:CCN123456'UNT+6+1'";
			reject.EM_SystemCreateTimeUtc = response.EM_SystemCreateTimeUtc.AddMinutes(5);

			var error = messaging.Messages.AddNew(typeof(EDIReleaseMessage));
			error.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Error;
			error.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			error.EM_Status = EDIMessage.Status.Received;
			error.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::489+12345000067897+11'DTM+58:201006221028:203'GIS+14'RFF+CN:CCN123456'UNT+6+1'";
			error.EM_SystemCreateTimeUtc = response.EM_SystemCreateTimeUtc.AddMinutes(10);

			var syntax = messaging.Messages.AddNew(typeof(EDIReleaseMessage));
			syntax.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.SyntaxError;
			syntax.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			syntax.EM_Status = EDIMessage.Status.Received;
			syntax.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::489+12345000067897+11'DTM+58:201006221028:203'GIS+2'RFF+CN:CCN123456'UNT+6+1'";
			syntax.EM_SystemCreateTimeUtc = response.EM_SystemCreateTimeUtc.AddMinutes(15);

			var mf2 = messaging.Messages.AddNew(typeof(ACIHouseBillMessage));
			mf2.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
			mf2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mf2.EM_Status = EDIMessage.Status.Received;
			mf2.EM_SystemCreateTimeUtc = response.EM_SystemCreateTimeUtc.AddMinutes(20);

			AssertEquals("RN_ReleaseStatus", "4 - Goods Released", messaging.RN_ReleaseStatus);
			AssertEquals("RN_ReleaseDate", new ZDateTime(2010, 06, 22, 10, 28, 00), messaging.RN_ReleaseDate);
			AssertEquals("MessageStatus", string.Empty, attachee.MessageStatus);
		}

		public void TestMFOnlyMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var messaging = GetRNSMessagingBO(shipment);
			var attachee = ((IEDIFACTMessageAttachee)messaging);

			var mf = messaging.Messages.AddNew(typeof(ACIHouseBillMessage));
			mf.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
			mf.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mf.EM_Status = EDIMessage.Status.Received;
			mf.EM_SystemCreateTimeUtc = ZDateTime.Now;

			AssertEquals("RN_ReleaseStatus", "Not Sent", messaging.RN_ReleaseStatus);
			AssertEquals("RN_ReleaseDate", ZDateTime.Empty, messaging.RN_ReleaseDate);
			AssertEquals("MessageStatus", string.Empty, attachee.MessageStatus);
		}

		[TestDate(2014, 11, 19)]
		public void TestArrivalCertificationProperties()
		{
			AssertArrivalCertificationProperties(Factory.New<CFSShipment>());
			AssertArrivalCertificationProperties(Factory.New<ForwardingShipment>());
		}

		void AssertArrivalCertificationProperties(Freight.Business.CommonShipment shipment)
		{
			var messaging = GetRNSMessagingBO(shipment);
			var createTime = ZDateTime.UtcNow;

			AssertEquals("ArrivalCertificationStatus", "Not Sent", messaging.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", ZDateTime.Empty, messaging.ArrivalCertificationDate);

			var message1 = messaging.Messages.AddNew(typeof(RNSRequestMessage));
			message1.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_Status = EDIMessage.Status.Rejected;
			message1.EM_SystemCreateTimeUtc = createTime.AddDays(-1);

			Factory.Save();

			AssertEquals("ArrivalCertificationStatus", "Rejected", messaging.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", ZDateTime.Empty, messaging.ArrivalCertificationDate.Date);

			var message2 = messaging.Messages.AddNew(typeof(RNSRequestMessage));
			message2.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_Status = EDIMessage.Status.Sent;
			message2.EM_SystemCreateTimeUtc = createTime;

			var message3 = messaging.Messages.AddNew(typeof(RNSRequestMessage));
			message3.EM_MessageSubType = RNSMessageTypes.Codes.StatusQuery;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_SystemCreateTimeUtc = createTime;

			var message4 = messaging.Messages.AddNew(typeof(RNSRequestMessage));
			message4.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_SystemCreateTimeUtc = createTime;

			var message5 = messaging.Messages.AddNew(typeof(EDIReleaseMessage));
			message5.EM_SystemCreateTimeUtc = createTime;

			Factory.Save();

			AssertEquals("ArrivalCertificationStatus", "Sent", messaging.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", new DateTime(2014, 11, 19), messaging.ArrivalCertificationDate.Date);

			const string testNoteText = @"2015-12-30 15:26
TEST MANUAL RELEASE REASON
TESTLOGIN
2015-12-31 15:26";
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsManualStatus.Description, testNoteText);
			AssertEquals("RN_ReleaseDate", new ZDateTime(2015, 12, 30, 15, 26, 00), messaging.RN_ReleaseDate);
			AssertEquals("RN_ReleaseStatus", "TEST MANUAL RELEASE REASON", messaging.RN_ReleaseStatus);
		}

		public void TestMessagesCountChanged()
		{
			var shipment = Factory.New<CFSShipment>();
			var messaging = GetRNSMessagingBO(shipment);

			AssertEquals(0, messaging.Messages.Count);
			AssertEquals("RN_ReleaseStatus", "Not Sent", messaging.RN_ReleaseStatus);
			AssertEquals("RN_ReleaseDate", ZDateTime.Empty, messaging.RN_ReleaseDate);
			AssertEquals("ArrivalCertificationStatus", "Not Sent", messaging.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", ZDateTime.Empty, messaging.ArrivalCertificationDate);

			var message1 = Factory.New<EDIReleaseMessage>();
			message1.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_Status = EDIMessage.Status.Received;
			message1.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::125+12345000067897+11'DTM+58:201006221028:203'GIS+4'RFF+CN:CCN123456'UNT+6+1'";
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now;

			shipment.Messages.Add(message1);

			AssertEquals(1, messaging.Messages.Count);
			AssertEquals("RN_ReleaseStatus", "4 - Goods Released", messaging.RN_ReleaseStatus);
			AssertEquals("RN_ReleaseDate", new ZDateTime(2010, 06, 22, 10, 28, 00), messaging.RN_ReleaseDate);

			var message2 = Factory.New<RNSRequestMessage>();
			message2.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_Status = EDIMessage.Status.Rejected;
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now;

			shipment.Messages.Add(message2);

			AssertEquals(2, messaging.Messages.Count);
			AssertEquals("ArrivalCertificationStatus", "Rejected", messaging.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", ZDateTime.Empty, messaging.ArrivalCertificationDate.Date);

			var message3 = Factory.New<EDIReleaseMessage>();
			message3.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.MessageContentAccepted;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_Status = EDIMessage.Status.Received;
			message3.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::125+12345000067897+11'DTM+58:201006231028:203'GIS+1'RFF+CN:CCN123456'UNT+6+1'";
			message3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);

			((IEDIFACTMessageAttachee)messaging).AddMessage(message3);
			AssertEquals(3, messaging.Messages.Count);
			AssertEquals("RN_ReleaseStatus", "1 - Message Content Accepted", messaging.RN_ReleaseStatus);
			AssertEquals("RN_ReleaseDate", new ZDateTime(2010, 06, 23, 10, 28, 00), messaging.RN_ReleaseDate);
		}

		public void TestMessagesForDisplay()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, ZDateTime.Now);
			message1.EM_LinkedObject = shipment;
			message1.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			var messaging = GetRNSMessagingBO(shipment);
			var eventTime = ZDateTime.Now.AddMinutes(1);
			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime);
			UniversalEventMessageTest.LinkToBusinessObject(message2, shipment);
			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7598", messageNumber: "4");
			UniversalEventMessageTest.LinkToBusinessObject(message3, shipment);
			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "1");
			UniversalEventMessageTest.LinkToBusinessObject(message4, shipment);
			var message5 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "2");
			UniversalEventMessageTest.LinkToBusinessObject(message5, shipment);
			var message6 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime.AddHours(-1), interchangeNumber: "7599", messageNumber: "3");
			UniversalEventMessageTest.LinkToBusinessObject(message6, shipment);
			Factory.Save();

			var messages = messaging.MessagesForDisplay;
			AssertEquals(typeof(EDIMessageForDisplayCollection<EDIMessage>), messages.GetType());
			AssertEquals("Messages Count", 6, messages.Count);
			Assert("Contains Message", messages.Contains(message1));
			Assert("Contains Message", messages.Contains(message2));
			Assert("Contains Message", messages.Contains(message3));
			Assert("Contains Message", messages.Contains(message4));
			Assert("Contains Message", messages.Contains(message5));
			Assert("Contains Message", messages.Contains(message6));
			AssertEquals("LatestNoticeMessage", message5, messaging.LatestNoticeMessage);
			var message7 = Factory.New<ACIHouseBillMessage>();
			message7.EM_LinkedObject = shipment;
			message7.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message7.EM_MessageText = ZString.Format(@"UNH+1+GOVCBR:D:11B:UN'
BGM+23+10207000007531'
DTM+9:{0}:203'
RFF+AGO:857477707RM0001'
STS++2:::0001'
UNS+D'
HYN+3'
UNS+S'
UNT+9+1'
", ZDateTime.Now.AddMinutes(2).ToString("yyyyMMddHHmm")).Replace("\r\n", "");
			Factory.Save();
			messages.Reload(true);
			AssertEquals("Messages Count", 7, messages.Count);
			AssertEquals("LatestNoticeMessage", message7, messaging.LatestNoticeMessage);

			var message8 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime);
			UniversalEventMessageTest.LinkToBusinessObject(message8, shipment);
			message8.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message8.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			message8.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message9 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime);
			UniversalEventMessageTest.LinkToBusinessObject(message9, shipment);
			message9.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message9.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			Factory.Save();
			messages.Reload(true);
			AssertEquals("Messages Count", 9, messages.Count);
			Assert("Contains IID Message", messages.Contains(message8));
			Assert("Contains D4 Message", messages.Contains(message9));
		}

		internal static RNSMessagingBO GetRNSMessagingBO(BusinessObject job)
		{
			RNSMessagingBO result = null;
			if (job is ForwardingConsol)
			{
				result = new RNSMessagingBO(new RNSPlugInSupportConsolWrapper((ForwardingConsol)job));
			}
			else if (job is ForwardingShipment || job is CFSShipment)
			{
				result = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper((Freight.Business.CommonShipment)job));
			}
			return result;
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetRNSMessagingBO(Factory.New<ForwardingConsol>());
		}

		#endregion
	}
}
