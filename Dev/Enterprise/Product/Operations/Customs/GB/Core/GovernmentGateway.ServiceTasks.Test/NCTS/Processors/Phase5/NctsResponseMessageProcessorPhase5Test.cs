using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class NctsResponseMessageProcessorPhase5Test : TestCaseWithFactory
	{
		public void TestProcessReceivedMessage_InvalidXMLWithValidMessageSubType()
		{
			var invalidXMLMessage = responseHelper.GetEmbeddedResourceFile("InvalidXML.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, invalidXMLMessage, "29C");
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			var messages = (headerReloaded as Business.NctsHeader).LinkedMessages;
			AssertEquals(2, messages.Count);

			var incomingMessage = messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_ReceiveTransmit == "RCV");
			AssertEquals("QUE", incomingMessage.EM_Status);

			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			var processor = new NctsResponseMessageProcessorPhase5(serviceLogger);

			for (var i = 0; i < retryAttempts - 2; i++)
			{
				processor.ExecuteBatch();
				var incomingMessageReload = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
				AssertEquals("QUE", incomingMessageReload.EM_Status);
			}

			processor.ExecuteBatch();
			AssertEquals("Error Processing Incoming EDI Message: GBN-GB-13", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			var failedIncomingMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
			AssertEquals("FAL", failedIncomingMessage.EM_Status);
		}

		public void TestProcessReceivedMessage_InvalidXMLWithInvalidMessageSubType()
		{
			var invalidXMLMessage = responseHelper.GetEmbeddedResourceFile("InvalidXML.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, invalidXMLMessage, "XXX");
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			var messages = (headerReloaded as Business.NctsHeader).LinkedMessages;
			AssertEquals(2, messages.Count);
			var incomingMessage = messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_ReceiveTransmit == "RCV");
			AssertEquals("FAL", incomingMessage.EM_Status);
		}

		public void TestProcessReceivedMessage_UnknownMessage()
		{
			var unknownCC999ZMessage = responseHelper.GetEmbeddedResourceFile("UnknownMessage.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, unknownCC999ZMessage, "999");
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			AssertEquals(2, (headerReloaded as Business.NctsHeader).LinkedMessages.Count);
			AssertContains("Error|Processor for NCTS Phase 5 message type 999 not found", serviceLogger.ToString());
		}

		public void TestProcessCanPickUpMessages_Without_EM_MessageType_Filter()
		{
			var interChangeMessage = responseHelper.GetEmbeddedResourceFile("TestResponseInterchangeFromEhub.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, interChangeMessage, "19C");
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			var messages = (headerReloaded as Business.NctsHeader).LinkedMessages;
			var lastIncomingMessage = messages.LastIncomingMessage;
			AssertEquals(2, messages.Count);
			AssertEquals("19C", lastIncomingMessage.EM_MessageSubType);
			AssertEquals(headerReloaded.MovementHeader.PK, lastIncomingMessage.EM_LinkUniqueID);
			AssertEquals(EDIMessage.Direction.Receive, lastIncomingMessage.EM_ReceiveTransmit);
			AssertEquals("QUE", lastIncomingMessage.EM_Status);
			AssertEquals("RCV", lastIncomingMessage.EM_ReceiveTransmit);
			AssertEquals("GB", lastIncomingMessage.EM_MessageType);
			AssertEquals(EDIMessage.ApplicationCodes.GbCustomsNCTS, lastIncomingMessage.EM_ApplicationCode);
		}

		public void TestProcessWillOnlyPickUpApplicationCodeGBN()
		{
			responseHelper.MessageApplicationCode = EDIInterchange.ApplicationCodes.GbCommonTransitConvention;

			var cc051message = responseHelper.GetEmbeddedResourceFile("CC051C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc051message, "51C");

			var header = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
			var messages = (header as Business.NctsHeader).LinkedMessages;
			AssertEquals("QUE", messages.LastIncomingMessage.EM_Status);
		}

		public void TestProcessReceivedMessage_CC004C()
		{
			var cc004message = responseHelper.GetEmbeddedResourceFile("CC004C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc004message, "04C",
				transitStatus: NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
				mrn: "23GB000246JHSAY3J5");
			AssertReceivedMessage(departure, 2, "004");
		}

		public void TestProcessReceivedMessage_CC009C()
		{
			var cc009message = responseHelper.GetEmbeddedResourceFile("CC009C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc009message, "09C");
			AssertReceivedMessage(departure, 2, "009");
		}

		public void TestProcessReceivedMessage_CC019C()
		{
			var cc019message = responseHelper.GetEmbeddedResourceFile("CC019C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc019message, "19C");
			AssertReceivedMessage(departure, 2, "019");
		}

		public void TestProcessReceivedMessage_CC022C()
		{
			var cc022message = responseHelper.GetEmbeddedResourceFile("CC022C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc022message, "22C");
			AssertReceivedMessage(departure, 2, "022");
		}

		public void TestProcessReceivedMessage_CC025C()
		{
			var cc025message = responseHelper.GetEmbeddedResourceFile("CC025C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.DepartureAndArrival, cc025message, "25C",
				mrn: "23XI000081RN3DBHJ0", outgoingMessageSubType: "007");
			AssertReceivedMessage(departure, 2, "025");
		}

		public void TestProcessReceivedMessage_CC028C()
		{
			var cc028message = responseHelper.GetEmbeddedResourceFile("CC028C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc028message, "28C",
				transitStatus: NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
			AssertReceivedMessage(departure, 2, "028");
		}

		public void TestProcessReceivedMessage_CC029C()
		{
			var cc029message = responseHelper.GetEmbeddedResourceFile("CC029C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc029message, "29C");
			AssertReceivedMessage(departure, 2, "029");
		}

		public void TestProcessReceivedMessage_CC035C()
		{
			var cc035message = responseHelper.GetEmbeddedResourceFile("CC035C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.DepartureAndArrival, cc035message, "35C");
			AssertReceivedMessage(departure, 2, "035");
		}

		public void TestProcessReceivedMessage_CC043C()
		{
			var cc043message = responseHelper.GetEmbeddedResourceFile("CC043C_Message.xml");
			var arrival = SetupAndRunMessageProcessor(NctsMovementType.Codes.Arrival, cc043message, "43C");
			AssertReceivedMessage(arrival, 2, "043");
		}

		public void TestProcessReceivedMessage_CC045C()
		{
			var cc045message = responseHelper.GetEmbeddedResourceFile("CC045C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc045message, "45C");
			AssertReceivedMessage(departure, 2, "045");
		}

		public void TestProcessReceivedMessage_CC051C()
		{
			var cc051message = responseHelper.GetEmbeddedResourceFile("CC051C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc051message, "51C");
			AssertReceivedMessage(departure, 2, "051");
		}

		public void TestProcessReceivedMessage_CC055C()
		{
			var cc055message = responseHelper.GetEmbeddedResourceFile("CC055C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc055message, "55C",
				transitStatus: NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
				mrn: "23GB000246F5YWI4J2");
			AssertReceivedMessage(departure, 2, "055");
		}

		public void TestProcessReceivedMessage_CC056C()
		{
			var cc056message = responseHelper.GetEmbeddedResourceFile("CC056C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc056message, "56C",
				phase: NCTS5DeparturePhaseList.Codes.Declaration,
				lrn: "TRATESTGB262307050928");
			AssertReceivedMessage(departure, 2, "056");
		}

		public void TestProcessReceivedMessage_CC057C()
		{
			var cc057message = responseHelper.GetEmbeddedResourceFile("CC057C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.DepartureAndArrival, cc057message, "57C",
				mrn: "23GB000060KNEJKEJ3", outgoingMessageSubType: "007");
			AssertReceivedMessage(departure, 2, "057");
		}

		public void TestProcessReceivedMessage_CC060C()
		{
			var cc060message = responseHelper.GetEmbeddedResourceFile("CC060C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc060message, "60C",
				transitStatus: NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
				lrn: "TRATESTGB142308011437");
			AssertReceivedMessage(departure, 2, "060");
		}

		public void TestProcessReceivedMessage_CC182C()
		{
			var cc182message = responseHelper.GetEmbeddedResourceFile("CC182C_Message.xml");
			var arrival = SetupAndRunMessageProcessor(NctsMovementType.Codes.DepartureAndArrival, cc182message, "182");
			AssertReceivedMessage(arrival, 2, "182");
		}

		public void TestProcessReceivedMessage_CC928C()
		{
			var cc928message = responseHelper.GetEmbeddedResourceFile("CC928C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc928message, "928", "015");
			AssertReceivedMessage(departure, 2, "928");
		}

		void AssertReceivedMessage(NctsHeader header, int messageCount, ZString messageType)
		{
			header = new BusinessObjectFactory().Load<NctsHeader>(header.PK);
			var messages = (header as Business.NctsHeader).LinkedMessages;
			AssertEquals(messageCount, messages.Count);
			AssertEquals(messageType, messages.LastIncomingMessage.EM_MessageType);
			var pk = header.IsPhase5Departure ? header.MovementHeader.PK : header.PK;
			AssertEquals(pk, messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		public void TestProcessReceivedMessage_UsingDepartureId()
		{
			var cc029message = responseHelper.GetEmbeddedResourceFile("CC029C_Message.xml");
			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.Departure, cc029message, "29C", mrn: "", lrn: "1234");
			AssertReceivedMessage(departure, 2, "029");
		}

		public void TestProcessReceivedMessage_UsingArrivalId()
		{
			var cc043message = responseHelper.GetEmbeddedResourceFile("CC043C_Message.xml");
			var arrival = SetupAndRunMessageProcessor(NctsMovementType.Codes.Arrival, cc043message, "43C", mrn: "", lrn: "1234");
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(arrival.PK);
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("043", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		public void TestProcessReceivedMessage_Phase51()
		{
			const string originalPhaseID = "PhaseID=\"NCTS5.1\"";
			const string replacementPhaseID = "PhaseID=\" NCTS5.1\"";
			var cc057message = responseHelper.GetEmbeddedResourceFile("CC057C_Message.xml");
			AssertContains("Pre-requisite", originalPhaseID, cc057message);
			cc057message = cc057message.Replace(originalPhaseID, replacementPhaseID);

			var departure = SetupAndRunMessageProcessor(NctsMovementType.Codes.DepartureAndArrival, cc057message, "57C",
				mrn: "23GB000060KNEJKEJ3", outgoingMessageSubType: "007");
			var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK).MovementHeader;
			AssertEquals(2, headerReloaded.Messages.Count);
			AssertEquals("057", headerReloaded.Messages.LastIncomingMessage.EM_MessageType);
			AssertEquals(headerReloaded.PK, headerReloaded.Messages.LastIncomingMessage.EM_LinkUniqueID);
		}

		NctsHeader SetupAndRunMessageProcessor(ZString movementType, string incomingMessageToTest, string messageSubType = "",
			string outgoingMessageSubType = "", string transitStatus = "", string phase = "", string mrn = "1234",
			string lrn = "TRATESTGB12308021209")
		{
			incomingMessageToTest = incomingMessageToTest.Replace("BH_JOBREFERENCE", "NCT00050167");
			if (string.IsNullOrEmpty(outgoingMessageSubType))
			{
				outgoingMessageSubType = movementType == NctsMovementType.Codes.Arrival ? "007" : "015";
			}
			var header = responseHelper.SetupPhase5MessagesForTest(Factory, movementType, incomingMessageToTest,
				transitStatus, string.Empty, messageSubType, outgoingMessageSubType, jobNo: "NCT00050167",
				applicationReference: "REMOVEME1234");
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			if (header.IsDepartureMovement)
			{
				header.MovementHeader.BM_PaperlessInbondNum = lrn;
				header.MovementHeader.BM_Phase = phase;
			}
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			mrnEntryNumber.CE_EntryNum = mrn;
			Factory.Save();

			var processor = new NctsResponseMessageProcessorPhase5(serviceLogger);
			processor.ExecuteBatch();

			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			responseHelper = new CtcNctsResponseHelperTest
			{
				MessageApplicationCode = EDIInterchange.ApplicationCodes.GbCustomsNCTS,
				ResourceNamePrefix = "Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Testing.NCTS.Processors.Phase5.TestFiles."
			};
			serviceLogger = new TestServiceLogger();
		}

		CtcNctsResponseHelperTest responseHelper;
		TestServiceLogger serviceLogger;
	}
}
