using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class FRPortsResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCAEDResponse()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.BH_JobReference = "BH_JOBREFERENCE";
			var realDOAResponse = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.CAED_Response.xml");
			var outgoingInterchange = CreateInterchangeForTest("3386");
			var outgoingMessage = CreateMessageForTest(Factory, nctsHeader, EDIMessage.Direction.Transmit, "OutgoingMessage", EDIMessage.Status.Received, outgoingInterchange);
			var incomingMessage = CreateMessageForTest(Factory, null, EDIMessage.Direction.Receive, realDOAResponse, EDIMessage.Status.Queued);
			Factory.Save();

			var processor = new FRPortsResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			var headerLogs = nctsHeader.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageAcceptedCode).ToArray();
			AssertEquals(1, headerLogs.Length);
			AssertEquals("CAE", headerLogs[0].SL_Reference);
			AssertEquals("18-Feb-22 08:06:00 +01:00", headerLogs[0].EventTimeOffset.ToString());

			var reloadedHeader = new BusinessObjectFactory().Load<NctsHeader>(nctsHeader.PK);
			AssertEquals(2, reloadedHeader.Messages.Count);

			var lastIncomingMessage = reloadedHeader.Messages.LastIncomingMessage;

			AssertEquals(reloadedHeader, lastIncomingMessage.EM_LinkedObject);
			AssertEquals(FREDIMessage.ApplicationCodes.FRPortMessage, reloadedHeader.Messages.LastIncomingMessage.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.POR, reloadedHeader.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(MessageSubTypeList.Codes.CAED, reloadedHeader.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals(reloadedHeader.PK, reloadedHeader.Messages.LastIncomingMessage.EM_LinkUniqueID);
			AssertEquals(MessageStatusCodeList.Codes.OK, reloadedHeader.Messages.LastIncomingMessage.EM_Status);
			AssertEquals(reloadedHeader.Messages.LastOutgoingMessage.EM_Status, EDIMessage.Status.Acknowledged);
			AssertEquals("<p>Status : Valid</p><p>Status granted on: 18/02/2022 08:06</p>", reloadedHeader.Messages.LastIncomingMessage.EM_MessageInterpretation);
		}

		public void TestDOAResponseProcessingForValidResponse()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.BH_JobReference = "BH_JOBREFERENCE";
			var realDOAResponse = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DOA_Response.xml");
			var outgoingInterchange = CreateInterchangeForTest("3386");
			var outgoingMessage = CreateMessageForTest(Factory, nctsHeader, EDIMessage.Direction.Transmit, "OutgoingMessage", EDIMessage.Status.Received, outgoingInterchange);
			var incomingMessage = CreateMessageForTest(Factory, null, EDIMessage.Direction.Receive, realDOAResponse, EDIMessage.Status.Queued);
			Factory.Save();

			var processor = new FRPortsResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			var headerLogs = nctsHeader.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageAcceptedCode).ToArray();
			AssertEquals(1, headerLogs.Length);
			AssertEquals("DOA", headerLogs[0].SL_Reference);
			AssertEquals("18-Feb-22 08:06:00 +01:00", headerLogs[0].EventTimeOffset.ToString());

			var reloadedHeader = new BusinessObjectFactory().Load<NctsHeader>(nctsHeader.PK);
			AssertEquals(2, reloadedHeader.Messages.Count);

			var lastIncomingMessage = reloadedHeader.Messages.LastIncomingMessage;

			AssertEquals(reloadedHeader, lastIncomingMessage.EM_LinkedObject);
			AssertEquals(FREDIMessage.ApplicationCodes.FRPortMessage, reloadedHeader.Messages.LastIncomingMessage.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.POR, reloadedHeader.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(MessageSubTypeList.Codes.DOA, reloadedHeader.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals(reloadedHeader.PK, reloadedHeader.Messages.LastIncomingMessage.EM_LinkUniqueID);
			AssertEquals(MessageStatusCodeList.Codes.OK, reloadedHeader.Messages.LastIncomingMessage.EM_Status);
			AssertEquals(reloadedHeader.Messages.LastOutgoingMessage.EM_Status, EDIMessage.Status.Acknowledged);
			AssertEquals("<p>Tracking reference : 171614308</p><p>Status : Valid</p><p>Status granted on: 18/02/2022 08:06</p>", reloadedHeader.Messages.LastIncomingMessage.EM_MessageInterpretation);
		}

		public void TestDOAResponseProcessingForResponseWithErrors()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			nctsHeader.BH_JobReference = "BH_JOBREFERENCE";
			var realDOAResponse = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DOA_Response_WithErrors.xml");
			var outgoingInterchange = CreateInterchangeForTest("3386");
			var outgoingMessage = CreateMessageForTest(Factory, nctsHeader, EDIMessage.Direction.Transmit, "OutgoingMessage", EDIMessage.Status.Received, outgoingInterchange);
			var incomingMessage = CreateMessageForTest(Factory, nctsHeader, EDIMessage.Direction.Receive, realDOAResponse, EDIMessage.Status.Queued);
			Factory.Save();

			var processor = new FRPortsResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			var headerLogs = nctsHeader.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageAcceptedCode).ToArray();
			AssertEquals(1, headerLogs.Length);
			AssertEquals("DOA", headerLogs[0].SL_Reference);
			AssertEquals("18-Feb-22 08:06:00 +01:00", headerLogs[0].EventTimeOffset.ToString());

			var reloadedHeader = new BusinessObjectFactory().Load<NctsHeader>(nctsHeader.PK);
			AssertEquals(2, reloadedHeader.Messages.Count);

			var lastIncomingMessage = reloadedHeader.Messages.LastIncomingMessage;

			AssertEquals(reloadedHeader, lastIncomingMessage.EM_LinkedObject);
			AssertEquals(FREDIMessage.ApplicationCodes.FRPortMessage, reloadedHeader.Messages.LastIncomingMessage.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.POR, reloadedHeader.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(MessageSubTypeList.Codes.DOA, reloadedHeader.Messages.LastIncomingMessage.EM_MessageSubType);
			AssertEquals(reloadedHeader.PK, reloadedHeader.Messages.LastIncomingMessage.EM_LinkUniqueID);
			AssertEquals(MessageStatusCodeList.Codes.OK, reloadedHeader.Messages.LastIncomingMessage.EM_Status);
			AssertEquals(reloadedHeader.Messages.LastOutgoingMessage.EM_Status, EDIMessage.Status.Acknowledged);
			AssertEquals("<p>Tracking reference : 171614308</p><p>Status : Valid</p><p>Status granted on: 18/02/2022 08:06</p><p>Error HandlingUnits.SPI : No Handling Unit found with ID : BBI00000010976.</p><p>Error 856 :  La référence équipement ne correspond pas à lAMQ renseigné .</p>", reloadedHeader.Messages.LastIncomingMessage.EM_MessageInterpretation);
		}

		EDIMessage CreateMessageForTest(BusinessObjectFactory factory, NctsHeader nctsHeader, string receiveOrTransmit, string messageText, string status, EDIInterchange interchange = null)
		{
			var messageNum = "1";
			var mockMessage = factory.NewMoq<CreateMessageForTestEDIMessageDummy>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("111");
			var message = mockMessage.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRPortMessage;
			message.EM_MessageType = "";
			message.EM_MessageSubType = "";
			message.EM_ReceiveTransmit = receiveOrTransmit;
			message.EM_MessageText = messageText;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageNum = messageNum;
			message.EM_SendWithMessageErrors = false;

			if (nctsHeader != null)
			{
				nctsHeader.Messages.Add(message);
			}

			if (interchange != null)
			{
				interchange.ContainedMessages.Add(message);
			}

			return message;
		}

		EDIInterchange CreateInterchangeForTest(string interchangeNumber)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_InterchangeNum = interchangeNumber;
			interchange.EI_InterchangeType = ApplicationCodeList.Codes.FRPortMessage;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			return interchange;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	public class CreateMessageForTestEDIMessageDummy : EDIMessage
	{
		public CreateMessageForTestEDIMessageDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber() => "111";
	}
}
