using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC917CMessageProcessor))]
	sealed class CC917CMessageProcessorTest : NctsMessageProcessorTestCase<CC917CMessageProcessor, ICC917CDataProvider>
	{
		#region TestValidateCC917CMessageDeclaration

		public void TestPreProcessCC917C_NonMatchingMRN()
		{
			mockProvider.Setup(x => x.MRN).Returns("FAKE");
			AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		}

		public void TestPreProcessCC917C_MatchingMRN_MultipleEntries_BadBusinessObject()
		{
			var nctsHeader1 = Factory.New<NctsHeader>();
			nctsHeader1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader1.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			var movementHeader1 = nctsHeader1.ArrivalMovementHeader;
			movementHeader1.BM_Phase = "";
			Extensions.CreateMovementReferenceNumber(nctsHeader1, "22BE000000000012J1");
			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader2.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			var movementHeader2 = nctsHeader2.ArrivalMovementHeader;
			movementHeader2.BM_Phase = "";
			Extensions.CreateMovementReferenceNumber(nctsHeader2, "22BE000000000012J1");
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		}

		public void TestPreProcessCC917C_MatchingMRN()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
		}

		public void TestPreProcessCC917C_MatchingMRN_MultipleEntries()
		{
			var nctsHeader1 = Factory.New<NctsHeader>();
			nctsHeader1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader1.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader1 = nctsHeader1.ArrivalMovementHeader;
			Extensions.CreateMovementReferenceNumber(nctsHeader1, "22BE000000000012J1");
			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader2.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			var movementHeader2 = nctsHeader2.ArrivalMovementHeader;
			Extensions.CreateMovementReferenceNumber(nctsHeader2, "22BE000000000012J1");
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
		}

		public void TestPreProcessCC917C_NonMatchingLRN()
		{
			mockProvider.Setup(x => x.LRN).Returns("1234567890");
			AssertPreProcess(EDIMessageStatusList.Codes.Failed);
		}

		public void TestPreProcessCC917C_MatchingLRN()
		{
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			Factory.Save();
			AssertPreProcess(EDIMessageStatusList.Codes.PreProcessedOK);
		}

		void AssertPreProcess(string expectedStatus)
		{
			processor.PreProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EDIMEssage Status Should BE " + expectedStatus, expectedStatus, incomingMessage.EM_Status);
				if (expectedStatus == EDIMessageStatusList.Codes.Failed)
				{
					var noteText = incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText;
					AssertContains("The processing of the message with interchange", noteText);
					AssertContains("failed because the message could not be linked to a NCTS declaration.", noteText);
				}
			});
		}

		#endregion

		public void TestProcessCC917CMessageDeclaration_ByMRN()
		{
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EDIMEssage Status Should BE PRS" + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("Message Status Should BE 'ERR'", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
				AssertContains("ST_NoteText", "New transaction status: XML error. Xml gives xsd errors.", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_CC007C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC007C", true);
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_CC044C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC044C", true);
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_CC013C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC013C", false);
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_CC014C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC014C", false);
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_CC015C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC015C", false);
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_CC054C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC054C", false);
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_C141C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC141C", false);
		}

		public void TestProcessCC917CMessageDeclaration_ByMRN_C170C()
		{
			AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType("CC170C", false);
		}

		void AssertProcessCC917CMessageDeclaration_ByMRN_AnswerToCorrectMovementType(string messageType, bool forArrival)
		{
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var errorItem = new XMLErrorXmlProvider();
			errorItem.ErrorPointer = "/" + messageType  + "/ consignment/departureTransportMeans[1]/typeOfIdentification";
			var errorList = new Collection<XMLErrorXmlProvider>();
			errorList.Add(errorItem);
			mockProvider.Setup(x => x.XMLErrorList).Returns(errorList);

			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeaderArrival = nctsHeaderArrival.ArrivalMovementHeader;
			movementHeaderArrival.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
			Extensions.CreateMovementReferenceNumber(nctsHeaderArrival, "22BE000000000012J1");

			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeaderDeparture = nctsHeaderArrival.MovementHeader;
			Extensions.CreateMovementReferenceNumber(nctsHeaderDeparture, "22BE000000000012J1");

			Factory.Save();
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals(messageType + " EDIMEssage Status Should BE PRS" + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals(messageType + " Message Status Should BE 'ERR'", LogicalStatusList.Codes.Error, forArrival ? nctsHeaderArrival.EffectiveMessageStatus : nctsHeaderDeparture.EffectiveMessageStatus);
				AssertContains(messageType + " ST_NoteText", "New transaction status: XML error. Xml gives xsd errors.", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessCC917CMessageDeclaration_ByLRN()
		{
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			Factory.Save();
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EDIMEssage Status Should BE " + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("Message Status Should BE 'ERR'", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
				AssertContains("ST_NoteText", "New transaction status: XML error. Xml gives xsd errors.", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessCC917CMessageDeclaration_ByInterchange()
		{
			mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
			SetupEDIInterchangeWithLink();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			AssertNotNull("The message should have a linked object", incomingMessage.EM_LinkedObject);
		}

		public void TestMessageStatusArray()
		{
			string[] messageStatusArray = new string[2] { LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent };
			AssertArrayEqualsByElements(messageStatusArray, new CC917CMessageProcessorForTest(new LoggingInformation()).messageStatusArray_exposed);
		}

		public void TestGetMessageDataProvider()
		{
			incomingMessage.EM_MessageText = "<?xml version=\"1.0\" encoding=\"utf-8\"?><CC917C></CC917C>";
			AssertType<CargoWise.Customs.BE.MessageContracts.MessageProviders.NCTS.CC917CDataProvider>(new CC917CMessageProcessorForTest(new LoggingInformation()).GetMessageDataProviderExposed(incomingMessage));
		}

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC917C;

		protected override Type ExpectedMessageInterpreterType => typeof(CC917CMessageInterpreter);

		protected override CC917CMessageProcessor Processor => processor;

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC917CDataProvider> MockProvider
		{
			get
			{
				if (mockProvider == null)
				{
					mockProvider = new Mock<ICC917CDataProvider>();
					mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
				}
				return mockProvider;
			}
		}
		Mock<ICC917CDataProvider> mockProvider;

		protected override Mock<CC917CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC917CMessageProcessor>(new LoggingInformation()));
		Mock<CC917CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageSubTypes.Codes.CC917C;

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.MrnAllocated };

		protected override bool SupportsSearchByEDIInterchange => true;

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider = new Mock<ICC917CDataProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
			mockProcessor = new Mock<CC917CMessageProcessor>(new LoggingInformation());
			mockProcessor.CallBase = true;
			provider = mockProvider.Object;
			mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(provider);
			processor = mockProcessor.Object;
			incomingMessage = CreateIncomingMessage(Factory);
			Factory.Save();
		}
		CC917CMessageProcessor processor;
		ICC917CDataProvider provider;
	}

	class CC917CMessageProcessorForTest : CC917CMessageProcessor
	{
		public CC917CMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public string[] messageStatusArray_exposed => this.messageStatusArray;

		public ICC917CDataProvider GetMessageDataProviderExposed(BEMessage message) => base.GetMessageDataProvider(message);
	}
}
