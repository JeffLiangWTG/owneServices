using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureSadEtIrispMessageSubProcessorTest : TestCaseWithFactory
{
	public void TestSpecificConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsHeader parameter is null", () => GetNewSadMessageSubProcessor(null));
		AssertNoExceptionThrown("No exception expected", () => GetNewSadMessageSubProcessor(nctsHeader));
	}

	public void TestSpecificPerformActionsForPositiveIrisp()
	{
		var sadEtIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);

		const string positiveIrispWithToGuaranteeText = @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          60126100137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2                                                        ";

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(positiveIrispWithToGuaranteeText);
		nctsHeader.BH_JobReference = "A0001";
		nctsHeader.MovementHeader.BM_CustomsStatus = "";
		sadEtIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);

		AssertEntryNumber(nctsHeader.MovementReferenceEntryNumber, "MRN", "20ITQXT080007705T2", "CUS", "137100", new ZDateTime(2020, 10, 19));

		var regEntryNumber = GetEntryNumber("REG", nctsHeader);
		AssertNotNull("REG Entry Number", regEntryNumber);
		AssertEntryNumber(regEntryNumber, "REG", "8 -7705P", "CUS", "137100", new ZDateTime(2020, 10, 19));

		var gtyEntryNumber = GetEntryNumber("GTY", nctsHeader);
		AssertNull("GTY Entry Number", gtyEntryNumber);
	}

	public void TestPerformActionsForPositiveIrispDoesNotSetEntryLineNBStatusAsAccepted()
	{
		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem1.BY_Status = EntryLineCustomsStatusList.Codes.Sent;
		goodsItem2.BY_Status = EntryLineCustomsStatusList.Codes.Rejected;

		var sadEtIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);
		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveIrispText);
		sadEtIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(goodsItem1.BY_Status), EntryLineCustomsStatusList.Codes.Sent, goodsItem1.BY_Status);
			AssertEquals(nameof(goodsItem2.BY_Status), EntryLineCustomsStatusList.Codes.Rejected, goodsItem2.BY_Status);
		});
	}

	public void TestPositiveIrispWithStatusEcc()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);
		AssertExceptionThrown<ArgumentNullException>("Should be exception when negativeResponseMessage is null", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(null));

		string irispText = $@"037V            037V0310.Xaf210000046881137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  10/03/21 18:39,037V0310.Raf
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   10/03/21  18:39
RET          05000500137100P1 T 00039036D100321000000 000000 000000 000000 000000QAAVS5SVINCOLATA               21ITQXT1T0039036E7                                                                 ";

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(irispText);

		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		nctsHeader.BH_JobReference = "A0001";
		AssertNoExceptionThrown("No exception expected", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

		AssertEquals(nameof(nctsHeader.MovementHeader.BM_CustomsStatus), NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, nctsHeader.MovementHeader.BM_CustomsStatus);
		AssertEquals(nameof(nctsHeader.BH_MessageStatus), NctsMessageStatusList.Codes.Ok, nctsHeader.BH_MessageStatus);
	}

	public void TestSingleWindowRequestCreationAfterPositiveIrispWithRegistrationNumber()
	{
		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT137100");
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);
		AssertEquals("[PRE-CONDITION]: nctsHeader as no messages", 0, nctsHeader.Messages.Count);

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveIrispText);
		sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			nctsHeader.Messages.Reload(true);
			AssertEquals("[POST-CONDITION]: nctsHeader has one message", 1, nctsHeader.Messages.Count);
			var message = nctsHeader.Messages[0];
			AssertEquals(nameof(message.EM_ApplicationCode), Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.ITCustoms, message.EM_ApplicationCode);
			AssertEquals(nameof(message.EM_MessageType), MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, message.EM_MessageType);
			AssertEquals(nameof(message.EM_MessageSubType), MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, message.EM_MessageSubType);
			Assert(nameof(message.IsTransmitMessage), message.IsTransmitMessage);
			AssertEquals(nameof(message.EM_ApplicationReference), nctsHeader.GetApplicationReference(), message.EM_ApplicationReference);
			AssertEquals(nameof(message.EM_Status), EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(nameof(message.EM_LinkTable), NctsHeader.Schema.TableName, message.EM_LinkTable);
			AssertEquals(nameof(message.EM_LinkUniqueID), nctsHeader.PK, message.EM_LinkUniqueID);
			AssertMultilineASCIIEquals(nameof(message.EM_MessageText), ExpectedSingleWindowRequestMessageText, message.EM_MessageText);
			AssertEquals(nameof(message.EM_MessageNum), ZString.Empty, message.EM_MessageNum);
		});
	}

	public void TestReleasedIrispProcessing()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveReleaseMessageText);
		nctsHeader.BH_JobReference = "A0001";
		nctsHeader.MovementHeader.BM_CustomsStatus = "";
		sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);

		var clrEntryNumber = GetEntryNumber("CLR", nctsHeader);
		AssertNotNull($"Entry Number with CE_EntryType: CLR", clrEntryNumber);
		AssertEquals(nameof(clrEntryNumber.CE_IssueDate), EntryReleaseDate, clrEntryNumber.CE_IssueDate);
	}

	public void TestPerformActionsForPositiveIrisp()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);
		AssertExceptionThrown<ArgumentNullException>("Should be exception when positiveResponseMessage is null", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(null));

		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveIrispText);
		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		nctsHeader.BH_JobReference = "A0001";
		AssertExceptionThrown<IncomingMessageDoesNotMatchWithStatusException>("Should be exception when BM_CustomsStatus is DMA", "Entry A0001 for job A0001 not processed: Entry Status is not allowed.", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

		nctsHeader.MovementHeader.BM_CustomsStatus = "";
		AssertNoExceptionThrown("No exception expected", () => sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage));

		AssertEntryNumbers(nctsHeader);
	}

	public void TestPerformActionsForNegativeIrisp()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);
		AssertExceptionThrown<ArgumentNullException>("Should be exception when negativeResponseMessage is null", () => sadIrispMessageSubProcessor.PerformActionsForNegativeIrisp(null));

		string negativeIrispText = $@"54RL            54RL0211.XAA170000380397279100    13149600150     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,54RL1026.RAA
 RL=005,RS=000,ME=001,MS=000,PE=000,PS=001
ESEGUITO   26/10/17  05:43
R{MessageSubType}          13325700279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]";

		var sadNegativeResponseMessage = GetSadResponseMessage<SadNegativeResponseMessage>(negativeIrispText);

		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		nctsHeader.BH_JobReference = "A0001";
		AssertExceptionThrown<IncomingMessageDoesNotMatchWithStatusException>("Should be exception when CH_EntryStatus is REG", "Entry A0001 for job A0001 not processed: Entry Status is not allowed.", () => sadIrispMessageSubProcessor.PerformActionsForNegativeIrisp(sadNegativeResponseMessage));

		nctsHeader.MovementHeader.BM_CustomsStatus = "";
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
		AssertNoExceptionThrown("No exception expected", () => sadIrispMessageSubProcessor.PerformActionsForNegativeIrisp(sadNegativeResponseMessage));

		AssertEquals(nameof(nctsHeader.BH_MessageStatus), NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, nctsHeader.BH_MessageStatus);
	}

	public void TestUpdateEntryStatusAfterChildMessagesProcessing()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);

		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem1.BY_Status = EntryLineCustomsStatusList.Codes.Rejected;
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem2.BY_Status = EntryLineCustomsStatusList.Codes.Approved;

		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		sadIrispMessageSubProcessor.UpdateEntryStatusAfterChildMessagesProcessing();
		AssertEquals(nameof(nctsHeader.MovementHeader.BM_CustomsStatus), NctsTransitStatusList.Codes.NbRejected, nctsHeader.MovementHeader.BM_CustomsStatus);

		goodsItem1.BY_Status = EntryLineCustomsStatusList.Codes.Approved;
		sadIrispMessageSubProcessor.UpdateEntryStatusAfterChildMessagesProcessing();
		AssertEquals(nameof(nctsHeader.MovementHeader.BM_CustomsStatus), NctsTransitStatusList.Codes.DeclarationMrnAllocated, nctsHeader.MovementHeader.BM_CustomsStatus);
	}

	public void TestTadPrintedWhenReceivingClearance()
	{
		var sadIrispMessageSubProcessor = GetNewSadMessageSubProcessor(nctsHeader);
		var sadPositiveResponseMessage = GetSadResponseMessage<SadPositiveResponseMessage>(PositiveReleaseMessageText);

		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
		nctsHeader.BH_JobReference = "A0001";
		sadIrispMessageSubProcessor.PerformActionsForPositiveIrisp(sadPositiveResponseMessage);

		AssertEquals("When Receiving Positive Irisp with Clearance, Should have a document", 1, Factory.GetTadPrintedJobs(nctsHeader).Length);
	}

	TCustomsMessage GetSadResponseMessage<TCustomsMessage>(string irispText)
	   where TCustomsMessage : UnifiedDeclarationResponseMessage
	{
		var irispX = CustomsInterchange.LoadSafe<IrispTypeR>(irispText);
		return irispX.Interchange.ResponseMessages.OfType<TCustomsMessage>().Single();
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	NctsHeader nctsHeader;

	SadEtIrispMessageSubProcessor GetNewSadMessageSubProcessor(NctsHeader nctsHeader) => new SadEtIrispMessageSubProcessor(new NctsHeaderCustomsLinkedObjectAdapter(nctsHeader));

	ZString MessageSubType => "ET";

	ZString PositiveIrispText => @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          60126100137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2      310000.00                                                  ";

	void AssertEntryNumbers(NctsHeader nctsHeader)
	{
		AssertEntryNumber(nctsHeader.MovementReferenceEntryNumber, "MRN", "20ITQXT080007705T2", "CUS", "137100", new ZDateTime(2020, 10, 19));

		var gtyEntryNumber = GetEntryNumber("GTY", nctsHeader);
		AssertNotNull("GTY Entry Number", gtyEntryNumber);
		AssertEntryNumber(gtyEntryNumber, "GTY", "310000.00", "CUS", "", new ZDateTime(2020, 10, 19));

		var regEntryNumber = GetEntryNumber("REG", nctsHeader);
		AssertNotNull("REG Entry Number", regEntryNumber);
		AssertEntryNumber(regEntryNumber, "REG", "8 -7705P", "CUS", "137100", new ZDateTime(2020, 10, 19));
	}

	ZString PositiveReleaseMessageText => @"037V            037V0310.Xaf210000046881137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  10/03/21 18:39,037V0310.Raf
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   10/03/21  18:39
RET          05000500137100P1 T 00039036D100321000000 000000 000000 000000 000000QAAVS5SVINCOLATA               21ITQXT1T0039036E7                                                                 ";

	ZDateTime EntryReleaseDate => new ZDateTime(2021, 3, 10, 18, 39, 0);

	ZString ExpectedSingleWindowRequestMessageText =>
			"<richiesta_esito>" +
				"<dichiarazione>" +
					"<num_reg>7705</num_reg>" +
					"<cod_uff_dog>137100</cod_uff_dog>" +
					"<cod_reg>8</cod_reg>" +
					"<anno_reg>19102020</anno_reg>" +
				"</dichiarazione>" +
			"</richiesta_esito>";

	CusEntryNumber GetEntryNumber(ZString entryType, NctsHeader nctsHeader)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		return Factory.LoadTop1<CusEntryNumber>(query);
	}

	void AssertEntryNumber(CusEntryNumber entryNumber, ZString expectedEntryType, ZString expectedEntryNum, ZString expectedCategory, ZString expectedEntryLineReference, ZDateTime expectedIssueDate)
	{
		CombineAssertions($"Asserting entry number: '{entryNumber.CE_EntryType}'", () =>
		{
			AssertEquals(nameof(entryNumber.CE_EntryType), expectedEntryType, entryNumber.CE_EntryType);
			AssertEquals(nameof(entryNumber.CE_EntryNum), expectedEntryNum, entryNumber.CE_EntryNum);
			AssertEquals(nameof(entryNumber.CE_Category), expectedCategory, entryNumber.CE_Category);
			AssertEquals(nameof(entryNumber.CE_EntryLineReference), expectedEntryLineReference, entryNumber.CE_EntryLineReference);
			AssertEquals(nameof(entryNumber.CE_IssueDate), expectedIssueDate, entryNumber.CE_IssueDate);
			AssertEquals(nameof(entryNumber.CE_EntryIsSystemGenerated), ZBool.True, entryNumber.CE_EntryIsSystemGenerated);
		});
	}
}
