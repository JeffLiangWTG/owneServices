using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadImIrispMessageSubProcessorTest : SadIrispMessageSubProcessorTest<SadImIrispMessageSubProcessor>
{
	public void TestSpecificConstructor()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

		CombineAssertions("Test Constructor", () =>
		{
			AssertExceptionThrown<ArgumentException>("Should be exception when Declaration is not import", "Entry must be related to an import job", () => new SadImIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader)));
			jobDeclaration.JE_MessageType = "IMP";
			AssertNoExceptionThrown("No exception expected", () => new SadImIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader)));
		});
	}

	public void TestSpecificPerformActionsForPositiveIrisp()
	{
		var sadImIrispMessageSubProcessor = new SadImIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader));

		const string positiveIrispWithInvalidReleaseNoteText = @"54RL            54RL0211.XAA170000380397279100    13149600150     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,54RL1026.RAA
 RL=005,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   26/10/17  05:43
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F230217 000000      XXXXXX                                                                                                      ";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
		{
			var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(positiveIrispWithInvalidReleaseNoteText);

			entryHeader.CH_EntryStatus = "";
			AssertExceptionThrown<CustomsMessageProcessorException>("Should be exception when irisp has an invalid release note", "Unable to manage the SAD IRISP Release Note: \"XXXXXX\"", () => sadImIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

			sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveIrispText);
			entryHeader.CH_EntryStatus = "ICC";
			entryHeader.CH_BGMReference = "A0001";
			jobDeclaration.JE_DeclarationReference = "B0001";
			AssertExceptionThrown<IncomingMessageDoesNotMatchWithStatusException>("Should be exception when CH_EntryStatus is ICC", "Entry A0001 for job B0001 not processed: Entry Status is not allowed.", () => sadImIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

			entryHeader.CH_EntryStatus = "";
			AssertNoExceptionThrown("No exception expected", () => sadImIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

			AssertEntryNumbers(entryHeader);
		}
	}

	protected override SadImIrispMessageSubProcessor GetNewSadMessageSubProcessor(CusEntryHeader entryHeader) => new SadImIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader));

	protected override void AssertEntryNumbers(CusEntryHeader entryHeader)
	{
		var registrationInfoEntryNumber = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfoEntryNumber);

		var entryNumber = registrationInfoEntryNumber;
		CombineAssertions("Cus Entry Number", () =>
		{
			AssertEquals("CE_EntryType", "REG", entryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", "4 T-61689G", entryNumber.CE_EntryNum);
			AssertEquals("CE_Category", "CUS", entryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", "279100", entryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2017, 01, 19), entryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, entryNumber.CE_EntryIsSystemGenerated);
		});
	}

	protected override ZString MessageType => "IMP";

	protected override ZString MessageSubType => "IM";

	protected override ZString PositiveIrispText => @"54RL            54RL0211.XAA170000380397279100    13149600150     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,54RL1026.RAA
 RL=005,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   26/10/17  05:43
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F230217 000000      IN ATTESA DI ESITO                                                                                     ";

	protected override ZString ExpectedSingleWindowRequestMessageText =>
		"<richiesta_esito>" +
			"<dichiarazione>" +
				"<num_reg>61689</num_reg>" +
				"<cod_uff_dog>137100</cod_uff_dog>" +
				"<cod_reg>4 T</cod_reg>" +
				"<anno_reg>19012017</anno_reg>" +
			"</dichiarazione>" +
		"</richiesta_esito>";

	protected override ZString PositiveReleaseMessageText => $@"0020            0RPE1003.X81170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   19/01/17  09:39
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F240217T250217N7EWSXSVINCOLATA                                                                                                  ";

	protected override ZDateTime EntryReleaseDate => new ZDateTime(2017, 1, 19, 9, 39, 0);
}
