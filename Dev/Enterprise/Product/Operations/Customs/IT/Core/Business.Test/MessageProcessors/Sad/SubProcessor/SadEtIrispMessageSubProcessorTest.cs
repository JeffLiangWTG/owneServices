using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SadEtIrispMessageSubProcessorTest : SadIrispMessageSubProcessorTest<SadEtIrispMessageSubProcessor>
{
	public void TestSpecificConstructor()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

		CombineAssertions("Test Constructor", () =>
		{
			jobDeclaration.JE_MessageType = "XXX";
			AssertExceptionThrown<ArgumentException>("Should be exception when Declaration is not export", "Entry must be related to an export job", () => new SadEtIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader)));
			jobDeclaration.JE_MessageType = MessageType;
			AssertNoExceptionThrown("No exception expected", () => new SadEtIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader)));
		});
	}

	public void TestSpecificPerformActionsForPositiveIrisp()
	{
		var sadEtIrispMessageSubProcessor = new SadEtIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader));

		const string positiveIrispWithToGuaranteeText = @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          60126100137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2                                                        ";

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(positiveIrispWithToGuaranteeText);
		entryHeader.CH_BGMReference = "A0001";
		jobDeclaration.JE_DeclarationReference = "B0001";
		entryHeader.CH_EntryStatus = "";
		sadEtIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);

		AssertNotNull("EntryHeader -> CusEntryNumber", entryHeader.CusEntryNumber);
		AssertEquals("EntryNumber", "20ITQXT080007705T2", entryHeader.EntryNumber);

		var mrnEntryNumber = entryHeader.CusEntryNumber;
		CombineAssertions("Assert MRN Entry Number", () =>
		{
			AssertEquals("CE_EntryType", "MRN", mrnEntryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", "20ITQXT080007705T2", mrnEntryNumber.CE_EntryNum);
			AssertEquals("CE_Category", "CUS", mrnEntryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", "137100", mrnEntryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2020, 10, 19), mrnEntryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, mrnEntryNumber.CE_EntryIsSystemGenerated);
		});

		var regEntryNumber = CusEntryNumberHelperTest.GetEntryNumber(Factory, "REG", entryHeader);
		CombineAssertions("Assert REG Entry Number", () =>
		{
			AssertEquals("CE_EntryType", "REG", regEntryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", "8 -7705P", regEntryNumber.CE_EntryNum);
			AssertEquals("CE_Category", "CUS", regEntryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", "137100", regEntryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2020, 10, 19), regEntryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, regEntryNumber.CE_EntryIsSystemGenerated);
		});

		var gtyEntryNumber = CusEntryNumberHelperTest.GetEntryNumber(Factory, "GTY", entryHeader);
		AssertNull("GTY Entry Number", gtyEntryNumber);
	}

	public void TestPerformActionsForPositiveIrispDoesNotSetEntryLineNBStatusAsAccepted()
	{
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine1.ZG_NBStatus = EntryLineCustomsStatusList.Codes.Sent;
		entryLine2.ZG_NBStatus = EntryLineCustomsStatusList.Codes.Rejected;

		var sadEtIrispMessageSubProcessor = GetNewSadMessageSubProcessor(entryHeader);
		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveIrispText);
		sadEtIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(entryLine1)}", EntryLineCustomsStatusList.Codes.Sent, entryLine1.ZG_NBStatus);
			AssertEquals($"{nameof(entryLine2)}", EntryLineCustomsStatusList.Codes.Rejected, entryLine2.ZG_NBStatus);
		});
	}

	public void TestPositiveIrispWithStatusEcc()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(entryHeader);
		AssertExceptionThrown<ArgumentNullException>("Should be exception when negativeResponseMessage is null", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(null));

		string irispText = $@"037V            037V0310.Xaf210000046881137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  10/03/21 18:39,037V0310.Raf
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   10/03/21  18:39
RET          05000500137100P1 T 00039036D100321000000 000000 000000 000000 000000QAAVS5SVINCOLATA               21ITQXT1T0039036E7                                                                 ";

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(irispText);

		entryHeader.CH_EntryStatus = "REG";
		entryHeader.CH_BGMReference = "A0001";
		jobDeclaration.JE_DeclarationReference = "B0001";
		AssertNoExceptionThrown("No exception expected", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

		AssertEquals("CH_EntryStatus", "ECC", entryHeader.CH_EntryStatus);
		AssertEquals("CH_Status", "CLO", entryHeader.CH_Status);
	}

	#region Override

	protected override SadEtIrispMessageSubProcessor GetNewSadMessageSubProcessor(CusEntryHeader entryHeader) => new SadEtIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader));

	protected override void AssertEntryNumbers(CusEntryHeader entryHeader)
	{
		AssertNotNull("EntryHeader -> CusEntryNumber", entryHeader.CusEntryNumber);
		AssertEquals("EntryNumber", "20ITQXT080007705T2", entryHeader.EntryNumber);

		var mrnEntryNumber = entryHeader.CusEntryNumber;
		CombineAssertions("Assert MRN Entry Number", () =>
		{
			AssertEquals("CE_EntryType", "MRN", mrnEntryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", "20ITQXT080007705T2", mrnEntryNumber.CE_EntryNum);
			AssertEquals("CE_Category", "CUS", mrnEntryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", "137100", mrnEntryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2020, 10, 19), mrnEntryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, mrnEntryNumber.CE_EntryIsSystemGenerated);
		});

		var gtyEntryNumber = CusEntryNumberHelperTest.GetEntryNumber(Factory, "GTY", entryHeader);
		AssertNotNull("GTY Entry Number", gtyEntryNumber);
		CombineAssertions("Assert MRN Entry Number", () =>
		{
			AssertEquals("CE_EntryType", "GTY", gtyEntryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", "310000.00", gtyEntryNumber.CE_EntryNum);
			AssertEquals("CE_Category", "CUS", gtyEntryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", "", gtyEntryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2020, 10, 19), gtyEntryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, gtyEntryNumber.CE_EntryIsSystemGenerated);
		});

		var regEntryNumber = CusEntryNumberHelperTest.GetEntryNumber(Factory, "REG", entryHeader);
		AssertNotNull("REG Entry Number", regEntryNumber);
		CombineAssertions("Assert REG Entry Number", () =>
		{
			AssertEquals("CE_EntryType", "REG", regEntryNumber.CE_EntryType);
			AssertEquals("CE_EntryNum", "8 -7705P", regEntryNumber.CE_EntryNum);
			AssertEquals("CE_Category", "CUS", regEntryNumber.CE_Category);
			AssertEquals("CE_EntryLineReference", "137100", regEntryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate", new ZDateTime(2020, 10, 19), regEntryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated", ZBool.True, regEntryNumber.CE_EntryIsSystemGenerated);
		});
	}

	protected override ZString MessageType => "EXP";

	protected override ZString PositiveIrispText => @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          60126100137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2      310000.00                                                  ";

	protected override ZString MessageSubType => "ET";

	protected override ZString ExpectedSingleWindowRequestMessageText =>
			"<richiesta_esito>" +
				"<dichiarazione>" +
					"<num_reg>7705</num_reg>" +
					"<cod_uff_dog>137100</cod_uff_dog>" +
					"<cod_reg>8</cod_reg>" +
					"<anno_reg>19102020</anno_reg>" +
				"</dichiarazione>" +
			"</richiesta_esito>";

	protected override ZString PositiveReleaseMessageText => @"037V            037V0310.Xaf210000046881137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  10/03/21 18:39,037V0310.Raf
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   10/03/21  18:39
RET          05000500137100P1 T 00039036D100321000000 000000 000000 000000 000000QAAVS5SVINCOLATA               21ITQXT1T0039036E7                                                                 ";

	protected override ZDateTime EntryReleaseDate => new ZDateTime(2021, 3, 10, 18, 39, 0);

	#endregion
}
