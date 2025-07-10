using System;
using System.Linq;
using System.Reflection;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC560CMessageProcessor))]
sealed class CC560CMessageProcessorTest : MessageProcessorTestCase<CC560CMessageProcessor, ICC560CDataProvider>
{
	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC560C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC560CMessageInterpreter);

	protected override CC560CMessageProcessor Processor => processor;

	public void TestCheckMessageSequenceIsValid() => CombineAssertions(() =>
	{
		var delayStatuses = typeof(StatusCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(p => (string)p.GetValue(null)).ToList();
		delayStatuses.Remove("MRN");
		var message = incomingMessage;
		mockProvider.Setup(x => x.MRN).Returns("22BEE0000000001234");

		foreach (var delayStatus in delayStatuses)
		{
			message.EM_RetryCount = 0;
			entry.CH_EntryStatus = delayStatus;
			processor.PreProcessMessage(incomingMessage);
			AssertEquals($"Entry Status '{delayStatus}'", false, CheckMessageSequenceIsValid(message, logger));
		}

		entry.CH_EntryStatus = "MRN";
		processor.PreProcessMessage(incomingMessage);
		AssertEquals("Entry Status MRN", true, CheckMessageSequenceIsValid(message, logger));
	});

	public void TestMessageSequenceInvalidMessage()
	{
		var messageSequenceInvalidMessageProperty = typeof(CC560CMessageProcessor).GetProperty("MessageSequenceInvalidMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertEquals("Entry Status was not MRN", messageSequenceInvalidMessageProperty.GetValue(Processor));
	}

	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC560C }, processor.MessageTypesToInclude);
	}

	public void TestPreProcessCC560C_NonMatchingMRN() => AssertPreProcess("1234567890", EDIMessageStatusList.Codes.Failed);

	public void TestProcessMessage()
	{
		entry.CH_EntryStatus = StatusCodes.MRNAllocated;

		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("Entry status", StatusCodes.Control, entry.CH_EntryStatus);
			AssertEquals("Declaration log of type CIP, with correct event time and reference", true, entry.Declaration.Logs.Find(x => x.SL_SE_NKEvent.Equals(Events.CustomsImpedimentReceived.Code) && x.SL_EventTime.Equals(new ZDateTime(2023, 02, 13)) && x.SL_Reference.Equals("Control of Customs")).Any());
			AssertEquals("Declaration service of type CTL, with correct booked date", true, entry.Declaration.Services.Any(x => ((JobService)x).ES_Booked.Equals(new ZDateTime(2023, 02, 13)) && ((JobService)x).ES_ServiceCode.Equals(Core.Constants.FreightServiceType.Codes.ControlByCustoms)));
		});
	}

	void AssertPreProcess(string mrn, string expectedStatus)
	{
		mockProvider.Setup(x => x.MRN).Returns(mrn);
		Processor.PreProcessMessage(incomingMessage);

		AssertEquals("EDIMessage status should be " + expectedStatus, expectedStatus, incomingMessage.EM_Status);
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC560CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.LRN).Returns("23021011435200000001");
		mockProvider.Setup(m => m.MRN).Returns("22BEE0000000001234");
		mockProvider.Setup(m => m.NotificationType).Returns(NotificationTypesList.Codes.DecisionToControl);
		mockProvider.Setup(m => m.AnticipatedControlDate).Returns(new DateTime(2023, 02, 13));
		mockProvider.Setup(m => m.ControlNotificationDateTime).Returns(new DateTime(2023, 02, 09, 15, 02, 35));
		mockProvider.Setup(m => m.Text).Returns(string.Empty);
		var mockTypeOfControl = new Mock<ITypeOfControlsDataProvider>();
		mockTypeOfControl.Setup(x => x.Type).Returns(TypeOfControlsList.Codes.PhysicalControls);
		mockTypeOfControl.Setup(x => x.Text).Returns("Make sure container is at the scanner");
		mockProvider.Setup(m => m.TypeOfControls).Returns(new ITypeOfControlsDataProvider[] { mockTypeOfControl.Object });
		var mockRequestedDocument = new Mock<IRequestedDocumentDataProvider>();
		mockRequestedDocument.Setup(x => x.DocumentType).Returns("N380");
		mockRequestedDocument.Setup(x => x.Description).Returns("Value of the invoices will be checked");
		mockProvider.Setup(m => m.RequestedDocuments).Returns(new IRequestedDocumentDataProvider[] { mockRequestedDocument.Object });
		logger = new LoggingInformation();
		var mockProcessor = new Mock<CC560CMessageProcessor>(logger);
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		var declaration = Factory.New<JobDeclaration>();
		entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_BGMReference = "22045281480600000001";
		entry.MovementReferenceNumberSetter("22BEE0000000001234");
		Factory.Save();
	}

	Mock<ICC560CDataProvider> mockProvider;
	CC560CMessageProcessor processor;
	LoggingInformation logger;
	BEMessage incomingMessage;
	CusEntryHeader entry;
}
