using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SadIrispMessageProcessorTest : SadIncomingCustomsMessageProcessorTest<SadIrispMessageProcessor>
{
	public void TestNegativeIrispProcessing()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "ERO", expectedEntryHeaderStatus: "", NegativeIrispSampleText, MessageNum);
		}
	}

	public void TestWaitingForResponseIrispProcessing()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", PositiveIrispSampleText.Trim(), MessageNum);

			AssertEntryNumbersAfterIrispOk(entryHeader);
		}
	}

	public void TestRegisteredIrispAfterNegativeProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var entryHeader, var negativeIrispEdiMessage, _) = SetupDeclarationAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(negativeIrispEdiMessage);
			Factory.Save();

			AssertNull("EntryHeader -> CusEntryNumber", entryHeader.CusEntryNumber);
			AssertEquals("EntryNumber", "", entryHeader.EntryNumber);

			(var awoReceivedInterchangeMessage, var awoReceivedIrispEdiMessage) = GetReceivedInterchangeAndMessage(PositiveIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
			processor.ProcessMessage(awoReceivedIrispEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, awoReceivedIrispEdiMessage, awoReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", PositiveIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 3);

			AssertEntryNumbersAfterIrispOk(entryHeader);
		}
	}

	public void TestIrispProcessingWhenIncomingMessageIsNegativeAfterRegistered()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var awoIrispEdiMessage, var awoReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			processor.ProcessMessage(awoIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			(var eroReceivedInterchangeMessage, var eroReceivedEdiMessage) = GetReceivedInterchangeAndMessage(NegativeIrispSampleText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, "3");
			processor.ProcessMessage(eroReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, awoIrispEdiMessage, awoReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", PositiveIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			AssertInterchangeAndLoggerContainsInfoLogText(eroReceivedInterchangeMessage, $"Entry {entryHeaderReference} for job {declarationReference} not processed: Entry Status is not allowed.");
			AssertEquals("Waiting for Response EDIMessage Status", "RCV", eroReceivedEdiMessage.EM_Status);
		}
	}

	public void TestCustomsDeclarationDuplicatedIrispAwoProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var duplicatedIrispText = PositiveIrispSampleText;
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var icntrlEdiMessage, var orignalInterchange, var jobDeclaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, duplicatedIrispText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
		{
			jobDeclaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;
			processor.ProcessMessage(icntrlEdiMessage);
			Factory.Save();

			(var duplicatedIcntrlReceivedInterchangeMessage, var duplicatedIcntrlReceivedEdiMessage) = GetReceivedInterchangeAndMessage(duplicatedIrispText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
			processor.ProcessMessage(duplicatedIcntrlReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, icntrlEdiMessage, orignalInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", duplicatedIrispText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			AssertInterchangeAndLoggerContainsInfoLogText(duplicatedIcntrlReceivedInterchangeMessage, $"Entry {entryHeaderReference} for job {declarationReference} not processed: Entry Status is not allowed.");
			AssertEquals("Duplicated EDIMessage Status", "RCV", icntrlEdiMessage.EM_Status);
		}
	}

	public void TestCustomsDeclarationDuplicatedIrispEroProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var duplicatedIrispText = NegativeIrispSampleText;
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var icntrlEdiMessage, var orignalInterchange, var jobDeclaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, duplicatedIrispText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
		{
			jobDeclaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;
			processor.ProcessMessage(icntrlEdiMessage);
			Factory.Save();

			(var duplicatedIcntrlReceivedInterchangeMessage, var duplicatedIcntrlReceivedEdiMessage) = GetReceivedInterchangeAndMessage(duplicatedIrispText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
			processor.ProcessMessage(duplicatedIcntrlReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, icntrlEdiMessage, orignalInterchange, expectedEntryMessageStatus: "ERO", expectedEntryHeaderStatus: "", duplicatedIrispText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			AssertInterchangeAndLoggerContainsInfoLogText(duplicatedIcntrlReceivedInterchangeMessage, $"Entry {entryHeaderReference} for job {declarationReference} not processed: Entry Status is not allowed.");
			AssertEquals("Duplicated EDIMessage Status", "RCV", icntrlEdiMessage.EM_Status);
		}
	}

	public void TestIrispProcessingWhenCurrentEntryHasInvalidStatus()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			entryHeader.CH_EntryStatus = "XXX";
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", PositiveIrispSampleText.Trim(), MessageNum);
		}
	}

	public void TestFailingIrispProcessingWhenIncomingMessageDoesNotMatchWithIdoc()
	{
		var wrongMessageNumber = "703013";
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(wrongMessageNumber, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertFailMessage(irispEdiMessage, irispEdiInterchange, logText: "Incoming Customs Message (IRISP) does not contain the Message 703013");
		}
	}

	public void TestFailingIrispProcessingWhenUnableToIdentifyTheIrispProcessor()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		entryHeader.Declaration.JE_MessageType = "ABC";
		entryHeader.CH_BGMReference = "ENTRY-REF";
		processor.ProcessMessage(irispEdiMessage);
		entryHeader.Declaration.ResetMessageTypeChangeLogs();
		Factory.Save();
		AssertFailMessage(irispEdiMessage, irispEdiInterchange, logText: "Unable to identify the IRISP processor for entry 'ENTRY-REF'");
	}

	public void TestReleaseCodeIrispProcessingAfterRegistrationInfoIrispProcessing()
	{
		var sentInterchangeSessionGuid = ZGuid.NewZGuid().ToString();
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, sentInterchangeSessionGuid, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", PositiveIrispSampleText.Trim(), MessageNum);
			entryHeader.Messages.Reload(true);

			var singleWindowRequestMessages = entryHeader.Messages.Find(x => x.EM_MessageType == MessageProcessorConstants.InterchangeTypes.SingleWindowRequest);
			AssertEquals("Number of SWR auto-generated messages", 1, singleWindowRequestMessages.Count());
			singleWindowRequestMessages.ElementAt(0).EM_Status = EDIMessageStatusList.Codes.Sent;

			(var clearedIrispEdiInterchange, var clearedIrispEdiMessage) = GetReceivedInterchangeAndMessage(ClearedIrispSampleText, PositiveIrispFileName, sentInterchangeSessionGuid, interchangeNum: "3");
			processor.ProcessMessage(clearedIrispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, clearedIrispEdiMessage, clearedIrispEdiInterchange, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: ExpectedClearedEntryStatus, ClearedIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 3);
		}
	}

	protected CusEntryNumber[] GetEntryNumbers(CusEntryHeader entryHeader, ZString entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, entryHeader.CountryCode);
		query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
		return Factory.Load<CusEntryNumber>(query);
	}

	protected abstract void AssertEntryNumbersAfterIrispOk(CusEntryHeader entryHeader);
	protected abstract ZString PositiveIrispSampleText { get; }
	protected abstract ZString PositiveIrispFileName { get; }
	protected abstract ZString ClearedIrispSampleText { get; }
	protected abstract ZString ExpectedClearedEntryStatus { get; }
	protected abstract ZInt GetMessageNum();

	#region Implementation

	protected ZString MessageNum => ConvertToMessageNumForMessage(GetMessageNum());
	protected ZString ConvertToMessageNumForMessage(ZInt messageNum) => messageNum.ToString().PadLeft(6, '0').Substring(0, 6);

	protected ZString NegativeIrispSampleText => $@"54RL            0RPE1003.X81170000380397279100    13149600150     001 00006    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,0RPE1003.R81
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   26/10/17  05:43
R{MessageSubType}          {MessageNum}00279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]";
	protected ZString NegativeIrispFileName => "0RPE1003.X81";

	#endregion
}
