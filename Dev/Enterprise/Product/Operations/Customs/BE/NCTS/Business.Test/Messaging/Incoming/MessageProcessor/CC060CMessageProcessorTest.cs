using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC060CMessageProcessor))]
	sealed class CC060CMessageProcessorTest : MessageProcessorTestCase<CC060CMessageProcessor, ICC060CDataProvider>
	{
		public void TestMessageTypesToInclude()
		{
			AssertSequencesEqual(new ZString[] { BEIncomingMessageTypes.Codes.CC060C }, processor.MessageTypesToInclude);
		}

		#region TestValidateCC060CMessageDeclaration

		public void TestPreProcessCC060C_NonMatchingLRN()
		{
			mockProvider.Setup(x => x.LRN).Returns("1234567890");
			AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		}

		public void TestPreProcessCC060C_MatchingLRN_BadBusinessObject()
		{
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.Discarded);
		}

		public void TestPreProcessCC60C_MatchingLRN()
		{
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			movementHeader.BM_Phase = "015";
			movementHeader.BM_MessageStatus = "INV";
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
		}

		void AssertPreProcess(string expectedStatus)
		{
			processor.PreProcessMessage(incomingMessage);
			AssertEquals("EDIMEssage Status Should BE " + expectedStatus, expectedStatus, incomingMessage.EM_Status);
		}

		#endregion

		public void TestProcessCC060CMessageDeclaration_NotificationType_0()
		{
			TestProcessCC060CMessageDeclaration("0", NCTS5DepartureCustomsStatusList.Codes.DecisionToControl);
		}

		public void TestProcessCC060CMessageDeclaration_NotificationType_1()
		{
			TestProcessCC060CMessageDeclaration("1", NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest);
		}

		public void TestProcessCC060CMessageDeclaration_NotificationType_2()
		{
			TestProcessCC060CMessageDeclaration("2", NCTS5DepartureCustomsStatusList.Codes.IntentionToControl);
		}

		public void TestProcessCC060CMessageDeclaration_NotificationTypeInvalid()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);
		}

		public void TestProcessCC060CMessageDeclaration_PRE_013_ACC()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NctsMovementHeaderTransactionStatusList.Codes.Amendment, LogicalStatusList.Codes.Accepted);
		}
		public void TestProcessCC060CMessageDeclaration_PRE_013_INV()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NctsMovementHeaderTransactionStatusList.Codes.Amendment, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC060CMessageDeclaration_PRE_015_ACC()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NctsMovementHeaderTransactionStatusList.Codes.Declaration, LogicalStatusList.Codes.Accepted);
		}

		public void TestProcessCC060CMessageDeclaration_PRE_015_INV()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NctsMovementHeaderTransactionStatusList.Codes.Declaration, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC060CMessageDeclaration_MRN_013_ACC()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NctsMovementHeaderTransactionStatusList.Codes.Amendment, LogicalStatusList.Codes.Accepted);
		}
		public void TestProcessCC060CMessageDeclaration_MRN_013_INV()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NctsMovementHeaderTransactionStatusList.Codes.Amendment, LogicalStatusList.Codes.Invalid);
		}

		public void TestProcessCC060CMessageDeclaration_MRN_015_ACC()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NctsMovementHeaderTransactionStatusList.Codes.Declaration, LogicalStatusList.Codes.Accepted);
		}

		public void TestProcessCC060CMessageDeclaration_MRN_015_INV()
		{
			TestProcessCC060CMessageDeclaration("9", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NctsMovementHeaderTransactionStatusList.Codes.Declaration, LogicalStatusList.Codes.Invalid);
		}

		void TestProcessCC060CMessageDeclaration(string notificationType, string expectedCustomsStatus, string customsStatus = "MRN", string phase = "015", string messageStatus = "ACC")
		{
			var currentDateTime = DateTime.Now;
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			mockProvider.Setup(x => x.NotificationType).Returns(notificationType);
			mockProvider.Setup(x => x.ControlNotificationDateAndTimeUtc).Returns(currentDateTime);
			mockProvider.Setup(x => x.TypeOfControls).Returns(new Collection<TypeOfControlsXmlProvider>());
			mockProvider.Setup(x => x.RequestedDocument).Returns(new Collection<RequestedDocumentXmlProvider>());
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = customsStatus;
			movementHeader.BM_Phase = phase;
			movementHeader.BM_MessageStatus = messageStatus;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			CombineAssertions("Notification Type: notificationType", () =>
			{
				AssertEquals("EDIMEssage Status Should BE " + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals($"CusInBondMoveHeader Customs Status Should BE {expectedCustomsStatus}", expectedCustomsStatus, movementHeader.BM_CustomsStatus);
				AssertEquals("Message Status Should BE PRS", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("CusInBondHeader MRN Should BE 22BE000000000012J1", "22BE000000000012J1", nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
				AssertNotNull($"CusInBondHeader Should have a CIP event logged with date {currentDateTime}", nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && x.SL_Reference == movementHeader.BM_CustomsStatus && x.SL_EventTime == currentDateTime).SingleOrDefault());
				if (expectedCustomsStatus == NCTS5DepartureCustomsStatusList.Codes.DecisionToControl ||
					expectedCustomsStatus == NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest ||
					expectedCustomsStatus == NCTS5DepartureCustomsStatusList.Codes.IntentionToControl)
				{
					AssertNotNull($"CusInBondHeader Should have a CTL service logged with date {currentDateTime}", nctsHeader.Services.Find(x => x.ES_ParentID == nctsHeader.PK && x.ES_ServiceCode == Constants.ServiceTypes.CTL && x.ES_Booked == currentDateTime).SingleOrDefault());
				}
				else
				{
					AssertNull($"CusInBondHeader Should not have a CTL service logged with date {currentDateTime}", nctsHeader.Services.Find(x => x.ES_ParentID == nctsHeader.PK && x.ES_ServiceCode == Constants.ServiceTypes.CTL && x.ES_Booked == currentDateTime).SingleOrDefault());
				}
				AssertContains("ST_NoteText", "New Customs Status: Decision to Control Notification", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestDiscardedStatusAndNote()
		{
			var currentDateTime = DateTime.Now;
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			mockProvider.Setup(x => x.ControlNotificationDateAndTimeUtc).Returns(currentDateTime);
			mockProvider.Setup(x => x.TypeOfControls).Returns(new Collection<TypeOfControlsXmlProvider>());
			mockProvider.Setup(x => x.RequestedDocument).Returns(new Collection<RequestedDocumentXmlProvider>());
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_Phase = "015";
			movementHeader.BM_CustomsStatus = "XXX";
			movementHeader.BM_MessageStatus = "ABC";
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMessage status should be {EDIMessageStatusList.Codes.Discarded}", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
				var note = incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single();
				AssertContains("Note text", "The message with interchange was discarded, because the Status at Customs of the declaration is different from PRE/MRN, or the phase is different from 013/015 or the message status is different from ACC/INV.", note.ST_NoteText);
			});
		}

		public void TestPreProcessMessage_CheckMessageSequenceIsValid()
		{
			var currentDateTime = DateTime.Now;
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			incomingMessage.EM_RetryCount = 4;
			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("CheckMessageSequenceIsValid", false, CheckMessageSequenceIsValid(incomingMessage, Logger));
		}

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.CC060C.ToString();

		protected override CC060CMessageProcessor Processor => processor;

		protected override Type ExpectedMessageInterpreterType => typeof(CC060CMessageInterpreter);

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider = new Mock<ICC060CDataProvider>();
			mockProvider.CallBase = true;
			mockProcessor = new Mock<CC060CMessageProcessor>(Logger);
			mockProcessor.CallBase = true;
			provider = mockProvider.Object;
			mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(provider);
			processor = mockProcessor.Object;
			incomingMessage = CreateIncomingMessage(Factory);
			Factory.Save();
		}

		Mock<ICC060CDataProvider> mockProvider;
		Mock<CC060CMessageProcessor> mockProcessor;
		CC060CMessageProcessor processor;
		ICC060CDataProvider provider;
		BEMessage incomingMessage;

		LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
		LoggingInformation logger;
	}
}
