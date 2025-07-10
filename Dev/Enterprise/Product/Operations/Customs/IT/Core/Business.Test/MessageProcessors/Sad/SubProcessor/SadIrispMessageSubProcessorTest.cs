using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SadIrispMessageSubProcessorTest<T> : TestCaseWithFactory
	where T : SadIrispMessageSubProcessor
{
	public void TestSingleWindowRequestCreationAfterPositiveIrispWithRegistrationNumber()
	{
		entryHeader.Declaration.JE_CustomsOffice = "IT137100";
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(entryHeader);
		AssertEquals("[PRE-CONDITION]: entryHeader as no messages", 0, entryHeader.Messages.Count);

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveIrispText);
		sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			entryHeader.Messages.Reload(true);
			AssertEquals("[POST-CONDITION]: entryHeader has one message", 1, entryHeader.Messages.Count);
			var message = entryHeader.Messages[0];
			AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.ITCustoms, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, message.EM_MessageSubType);
			Assert("IsTransmitMessage", message.IsTransmitMessage);
			AssertEquals("EM_ApplicationReference", entryHeader.Declaration.GetApplicationReference(), message.EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_LinkTable", CusEntryHeader.Schema.TableName, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
			AssertMultilineASCIIEquals("EM_MessageText", ExpectedSingleWindowRequestMessageText, message.EM_MessageText);
			AssertEquals("EM_MessageNum", ZString.Empty, message.EM_MessageNum);
		});

		AssertEntryNumbers(entryHeader);
	}

	public void TestReleasedIrispProcessing()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(entryHeader);

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveReleaseMessageText);
		entryHeader.CH_BGMReference = "A0001";
		jobDeclaration.JE_DeclarationReference = "B0001";
		entryHeader.CH_EntryStatus = "";
		sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);

		var clrEntryNumbers = CusEntryNumberHelperTest.GetEntryNumbers(Factory, entryHeader, "CLR");
		AssertEquals("CLR EntryNumbers Count", 1, clrEntryNumbers.Length);

		var clrEntryNumber = clrEntryNumbers.SingleOrDefault(x => x.CE_EntryType == "CLR");
		AssertNotNull($"Entry Number with CE_EntryType: CLR", clrEntryNumber);
		AssertEquals("CE_IssueDate", EntryReleaseDate, clrEntryNumber.CE_IssueDate);
	}

	public void TestPerformActionsForPositiveIrisp()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(entryHeader);
		AssertExceptionThrown<ArgumentNullException>("Should be exception when positiveResponseMessage is null", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(null));

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveIrispText);
		entryHeader.CH_EntryStatus = "REG";
		entryHeader.CH_BGMReference = "A0001";
		jobDeclaration.JE_DeclarationReference = "B0001";
		AssertExceptionThrown<IncomingMessageDoesNotMatchWithStatusException>("Should be exception when CH_EntryStatus is REG", "Entry A0001 for job B0001 not processed: Entry Status is not allowed.", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

		entryHeader.CH_EntryStatus = "";
		AssertNoExceptionThrown("No exception expected", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

		AssertEntryNumbers(entryHeader);
	}

	public void TestConstructor()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

		CombineAssertions("Test Constructor", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryHeader parameter is null", () => GetNewSadMessageSubProcessor(null));
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryHeader.Declaration is null", () => GetNewSadMessageSubProcessor(Factory.New<CusEntryHeader>()));
		});
	}

	public void TestPerformActionsForNegativeIrisp()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(entryHeader);
		AssertExceptionThrown<ArgumentNullException>("Should be exception when negativeResponseMessage is null", () => sadIrispMessageSubProcessor.PerformActionsForNegativeIrisp(null));

		string negativeIrispText = $@"54RL            54RL0211.XAA170000380397279100    13149600150     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,54RL1026.RAA
 RL=005,RS=000,ME=001,MS=000,PE=000,PS=001
ESEGUITO   26/10/17  05:43
R{MessageSubType}          13325700279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]";

		var sadNegativeResponseMessage = GetSadResponseMessage<SadNegativeResponseMessage>(negativeIrispText);

		entryHeader.CH_EntryStatus = "REG";
		entryHeader.CH_BGMReference = "A0001";
		jobDeclaration.JE_DeclarationReference = "B0001";
		AssertExceptionThrown<IncomingMessageDoesNotMatchWithStatusException>("Should be exception when CH_EntryStatus is REG", "Entry A0001 for job B0001 not processed: Entry Status is not allowed.", () => sadIrispMessageSubProcessor.PerformActionsForNegativeIrisp(sadNegativeResponseMessage));

		entryHeader.CH_EntryStatus = "";
		entryHeader.CH_Status = "AWO";
		AssertNoExceptionThrown("No exception expected", () => sadIrispMessageSubProcessor.PerformActionsForNegativeIrisp(sadNegativeResponseMessage));

		AssertEquals("CH_Status", "ERO", entryHeader.CH_Status);
	}
	public void TestUpdateEntryStatusAfterChildMessagesProcessing()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(entryHeader);

		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.ZG_NBStatus = "NBR";
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.ZG_NBStatus = "NBA";

		entryHeader.CH_EntryStatus = ITEntryStatusList.Codes.Registered;
		sadIrispMessageSubProcessor.UpdateEntryStatusAfterChildMessagesProcessing();

		AssertEquals("CH_EntryStatus", "NBR", entryHeader.CH_EntryStatus);

		entryLine1.ZG_NBStatus = "NBA";
		sadIrispMessageSubProcessor.UpdateEntryStatusAfterChildMessagesProcessing();

		AssertEquals("CH_EntryStatus", "REG", entryHeader.CH_EntryStatus);
	}

	protected TCustomsMessage GetSadResponseMessage<TCustomsMessage>(string irispText)
		where TCustomsMessage : UnifiedDeclarationResponseMessage
	{
		var irispX = CustomsInterchange.LoadSafe<IrispTypeR>(irispText);
		return irispX.Interchange.ResponseMessages.OfType<TCustomsMessage>().Single();
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_ApplicationCode = "BLT";
		jobDeclaration.JE_MessageType = MessageType;

		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
	}

	protected JobDeclaration jobDeclaration;
	protected CusEntryHeader entryHeader;

	protected abstract T GetNewSadMessageSubProcessor(CusEntryHeader entryHeader);
	protected abstract ZString MessageType { get; }
	protected abstract ZString MessageSubType { get; }
	protected abstract ZString PositiveIrispText { get; }
	protected abstract void AssertEntryNumbers(CusEntryHeader entryHeader);

	protected abstract ZString PositiveReleaseMessageText { get; }
	protected abstract ZDateTime EntryReleaseDate { get; }
	protected abstract ZString ExpectedSingleWindowRequestMessageText { get; }
}
