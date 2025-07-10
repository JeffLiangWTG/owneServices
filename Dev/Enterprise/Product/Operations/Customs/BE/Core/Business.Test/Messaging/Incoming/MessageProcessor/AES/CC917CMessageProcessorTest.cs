using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC917CMessageProcessor))]
sealed class CC917CMessageProcessorTest : MessageProcessorTestCase<CC917CMessageProcessor, ICC917CDataProvider>
{
	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC917C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC917CMessageInterpreter);

	protected override CC917CMessageProcessor Processor => processor;

	public void TestMessagesTypeToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC917C }, processor.MessageTypesToInclude);
	}

	public void TestPreProcessCC917C_NonMatchingMRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("FAKE");

		AssertPreProcess(EDIMessageStatusList.Codes.Failed, false);
	}

	public void TestPreProcessCC917C_MatchingMRN()
	{
		var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry.CountryCode);
		mrn.CE_EntryNum = "22045281480600000002";
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("22045281480600000002");
		Factory.Save();

		TestPossibleStatusToPreProcess();
	}

	public void TestPreProcessCC917C_NonMatchingLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("FAKE");

		AssertPreProcess(EDIMessageStatusList.Codes.Failed, false);
	}

	public void TestPreProcessCC917C_MatchingLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		mockProvider.Setup(x => x.MRN).Returns("FAKE");

		TestPossibleStatusToPreProcess();
	}

	public void TestPreProcessCC917C_NonMatchingSessionID()
	{
		mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
		MessageProcessorTestHelper.SetupEDIInterchangeWithLink(entry, incomingMessage, true);

		Processor.PreProcessMessage(incomingMessage);
		Processor.ProcessMessage(incomingMessage);

		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("FAKE");

		AssertPreProcess(EDIMessageStatusList.Codes.Failed, false);
	}

	public void TestPreProcessCC917C_MatchingSessionID()
	{
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("FAKE");
		mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
		MessageProcessorTestHelper.SetupEDIInterchangeWithLink(entry, incomingMessage);

		TestPossibleStatusToPreProcess();
	}

	public void TestProcessCC917CMessageDeclaration_ByMRN()
	{
		var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry.CountryCode);
		mrn.CE_EntryNum = "22045281480600000002";
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("22045281480600000002");
		mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
		Factory.Save();

		TestPossibleStatusToProcess();
	}

	public void TestProcessCC917CMessageDeclaration_ByLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		mockProvider.Setup(x => x.MRN).Returns("FAKE");
		mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());

		TestPossibleStatusToProcess();
	}

	public void TestProcessCC917CMessageDeclaration_BySessionID()
	{
		mockProvider.Setup(x => x.XMLErrorList).Returns(new Collection<XMLErrorXmlProvider>());
		MessageProcessorTestHelper.SetupEDIInterchangeWithLink(entry, incomingMessage);

		TestPossibleStatusToProcess();
	}

	void TestPossibleStatusToProcess()
	{
		processor.PreProcessMessage(incomingMessage);

		foreach (var element in processStatusDictionairy)
		{
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			entry.CH_EntryStatus = element.Key;
			entry.CH_Status = EDIMessageStatusList.Codes.Sent;
			AssertProcess(element.Value);
		}
	}

	void AssertProcess(string expectedStatus)
	{
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EDI status should be PRS", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals($"CusEntryHeader status should be {expectedStatus}", expectedStatus, entry.CH_EntryStatus);
			AssertEquals($"CusEntryHeader message status should be ERR", EDIMessageStatusList.Codes.Error, entry.CH_Status);
		});
	}

	void TestPossibleStatusToPreProcess()
	{
		foreach (var element in preProcessStatusDictionairy)
		{
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			entry.CH_Status = element.Key;
			AssertPreProcess(element.Value);
		}
	}

	void AssertPreProcess(string expectedStatus, bool linked = true)
	{
		processor.PreProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals($"EDIMEssage Status Should BE {expectedStatus}", expectedStatus, incomingMessage.EM_Status);
			AssertEquals("Message linked object", linked, incomingMessage.EM_LinkedObject != null);
			if (!linked)
			{
				AssertEquals("The processing of the message with interchange failed because the message could not be linked to a declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC917CDataProvider>();
		mockProvider.CallBase = true;
		var mockProcessor = new Mock<CC917CMessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		Factory.Save();
	}
	Mock<ICC917CDataProvider> mockProvider;
	EDIMessage incomingMessage;
	CC917CMessageProcessor processor;
	CusEntryHeader entry;

	readonly Dictionary<string, string> processStatusDictionairy = new Dictionary<string, string>
	{
			{ "", StatusCodes.Rejected  },
			{ StatusCodes.InvalidationRequest, StatusCodes.RejectedInvalidation },
			{ StatusCodes.AmendmentRequest, StatusCodes.RejectedAmendment },
	};

	readonly Dictionary<string, string> preProcessStatusDictionairy = new Dictionary<string, string>
	{
			{ EDIMessageStatusList.Codes.Sent, EDIMessageStatusList.Codes.PreProcessedOK  },
			{ "", EDIMessageStatusList.Codes.Discarded }
	};
}
