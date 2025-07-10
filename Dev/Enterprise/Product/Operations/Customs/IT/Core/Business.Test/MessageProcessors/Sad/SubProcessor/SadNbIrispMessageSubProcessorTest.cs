using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadNbIrispMessageSubProcessorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		var ediInterchange = Factory.New<EDIInterchange>();

		CombineAssertions("Test Constructor", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryHeader parameter is null", () => new SadNbIrispMessageSubProcessor(null, ediInterchange));
			AssertExceptionThrown<ArgumentNullException>("Should be exception when sentInterchange parameter is null", () => new SadNbIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader), null));
			AssertNoExceptionThrown("No exception expected", () => new SadNbIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader), ediInterchange));
		});
	}

	public void TestPerformActionsForMessageResponsesWithEmptyResponseMessageCollection()
	{
		var sadNbIrispMessageSubProcessor = new SadNbIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader), sentInterchange);

		AssertNoExceptionThrown("No exception expected when nbResponseMessages parameter is an Empty List", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(null));

		var emptyNbResponseMessageCollection = Enumerable.Empty<UnifiedDeclarationResponseMessage>();
		AssertNoExceptionThrown("No exception expected when nbResponseMessages parameter is an Empty List", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(emptyNbResponseMessageCollection));
	}

	public void TestPerformActionsForMessageResponses()
	{
		var sadNbIrispMessageSubProcessor = new SadNbIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader), sentInterchange);

		sentInterchange.EI_BodyText = ZString.Empty;
		AssertExceptionThrown<UnableToInterpretInterchangeException>("Should be exception when interchange.EI_BodyText is Empty", "Unable to interpret the sent Idoc interchange", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));

		sentInterchange.EI_BodyText = "XXXXX";
		AssertExceptionThrown<UnableToInterpretInterchangeException>("Should be exception when interchange.EI_BodyText is invalid", "Unable to interpret the sent Idoc interchange", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));

		const string idocInvalidFields = @"TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?IM1          22228500IM	1 DATA
?IM1          22228500IM	1 DATA
TNB           22228504IM	1 DATA";

		sentInterchange.EI_BodyText = idocInvalidFields;
		AssertExceptionThrown<UnableToInterpretInterchangeException>("Should be exception when interchange.EI_BodyText does not match with the sent Idoc", "Unable to interpret the sent Idoc interchange", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));

		const string idocValidButDoesntMatchWithNb = @"TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?IM1          22228500IM	1 DATA	A	Z	1	
?IM1          22228500IM	1 DATA	B	Z	2	
TNB           22228504IM	1 DATA	C	Z	3	";

		sentInterchange.EI_BodyText = idocValidButDoesntMatchWithNb;
		var expectedExceptionMessage = "The sent idoc does not contain a NB Message with Pan: 222285, Progressive number: 1";
		AssertExceptionThrown<IncomingMessageDoesNotMatchWithIdocException>("Should be exception when interchange.EI_BodyText does not match with the sent Idoc (No TNB message with Progressive Number 1,2,3)", expectedExceptionMessage, () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));

		const string idocValidButDoesntMatchWithEntryLine = @"TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?IM1          22228500IM	1 DATA	A	Z	1	
?IM1          22228500IM	1 DATA	B	Z	2	
TNB           22228501IM	1 DATA	C	Z	3	";

		entryHeader.CH_BGMReference = "A0001";
		sentInterchange.EI_BodyText = idocValidButDoesntMatchWithEntryLine;
		expectedExceptionMessage = "Entry: A0001 has not an EntryLine with LineNumber: 3";
		AssertExceptionThrown<CouldNotFindRelatedBusinessObjectException>("Should be exception when interchange.EI_BodyText does not match with the sent Declaration (No EntryLine No: 3)", expectedExceptionMessage, () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));

		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		var entryLine3 = entryHeader.MergedLines.AddNew();
		entryLine3.CL_LineNumber = 3;

		const string invalidIdocWithSameProgessionNumber = @"TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?IM1          22228500IM	1 DATA	A	Z	1	
?IM1          22228500IM	1 DATA	B	Z	2
?IM1          22228500IM	1 DATA	B	Z	3	
TNB           22228501IM	1 DATA	C	Z	1
TNB           22228501IM	2 DATA	C	Z	2
TNB           22228501IM	3 DATA	C	Z	3
";

		sentInterchange.EI_BodyText = invalidIdocWithSameProgessionNumber;
		AssertExceptionThrown<UnableToInterpretInterchangeException>("Should be exception when interchange.EI_BodyText has message with same progressive number", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));

		const string idocValid = @"TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?IM1          22228500IM	1 DATA	A	Z	1	
?IM1          22228500IM	1 DATA	B	Z	2
?IM1          22228500IM	1 DATA	B	Z	3	
TNB           22228501IM	1 DATA	C	Z	1
TNB           22228502IM	2 DATA	C	Z	2
TNB           22228503IM	3 DATA	C	Z	3
";

		sentInterchange.EI_BodyText = idocValid;
		entryHeader.CH_EntryStatus = "REG";
		AssertNoExceptionThrown("No exception expected", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));

		CombineAssertions("Test ZG_NBStatus Entry Lines", () =>
		{
			AssertEquals("EntryLine 1, ZG_NBStatus", "NBA", entryLine1.ZG_NBStatus);
			AssertEquals("EntryLine 2, ZG_NBStatus", "NBA", entryLine2.ZG_NBStatus);
			AssertEquals("EntryLine 3, ZG_NBStatus", "NBR", entryLine3.ZG_NBStatus);
		});
	}

	public void TestFailurePerformActionsForMessageResponsesWithInvalidStatus()
	{
		var sadNbIrispMessageSubProcessor = new SadNbIrispMessageSubProcessor(new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader), sentInterchange);

		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;

		const string validIdocText = @"TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?IM1          22228500IM	1 DATA	A	Z	1	
TNB           22228501IM	1 DATA	C	Z	1	";
		sentInterchange.EI_BodyText = validIdocText;

		const string irsipTextWithPositive = @"0020            00200119.XLR170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,00200119.XLR
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   15/04/20  14:13
RNB          22228501279100P";

		nbResponseMessages = GetNbResponseMessages(irsipTextWithPositive);

		entryLine1.ZG_NBStatus = "";
		sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages);

		AssertEquals("ZG_NBStatus", "NBA", entryLine1.ZG_NBStatus);

		entryHeader.CH_EntryStatus = "NBR";
		entryLine1.ZG_NBStatus = "NBS";
		sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages);

		AssertEquals("ZG_NBStatus", "NBA", entryLine1.ZG_NBStatus);

		const string irsipTextWithNegative = @"0020            00200119.XLR170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,00200119.XLR
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   15/04/20  14:13
RNB          22228501279100N
 013302 00L 00 Errore 0 : [Codice addizionale in input mancante]";

		nbResponseMessages = GetNbResponseMessages(irsipTextWithNegative);
		entryHeader.CH_EntryStatus = "NBR";
		entryLine1.ZG_NBStatus = "NBA";

		AssertExceptionThrown<IncomingMessageDoesNotMatchWithStatusException>("Should be exception when ZG_NBStatus is NBA and new status is NBS", () => sadNbIrispMessageSubProcessor.PerformActionsForMessageResponses(nbResponseMessages));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = "IMP";
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		sentInterchange = Factory.New<EDIInterchange>();
		nbResponseMessages = GetNbResponseMessages(IrispNbSampleText);
	}
	IEnumerable<UnifiedDeclarationResponseMessage> nbResponseMessages;
	CusEntryHeader entryHeader;
	EDIInterchange sentInterchange;

	IEnumerable<UnifiedDeclarationResponseMessage> GetNbResponseMessages(string irispText)
	{
		var irispNb = CustomsInterchange.LoadSafe<IrispTypeR>(irispText);
		return irispNb.Interchange.ResponseMessages.Where(x => x is SadNbPositiveResponseMessage || x is SadNbNegativeResponseMessage);
	}

	const string IrispNbSampleText = @"0020            00200119.XLR170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,00200119.XLR
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   19/01/17  09:39
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F230217 000000N7EWSXSVINCOLATA
ESEGUITO   15/04/20  14:13
RNB          22228501279100P
ESEGUITO   15/04/20  14:13
RNB          22228502279100P
ESEGUITO   15/04/20  14:13
RNB          22228503279100N
 013302 00L 00 Errore 0 : [Codice addizionale in input mancante]";
}

sealed class SadNbIrispMessageSubProcessorOutboundInterchangeAssumptionTest : TestCaseWithFactory
{
	public void TestITOutgoingMessageProcessorCreatesAnInterchangeForEachNBStandaloneMessage()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "11111111111", 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_CustomsProfile = "1234-DEC1";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var outNbStandaloneMsg1 = CreateQueuedOutboundNbStandaloneMessage("01");
		outNbStandaloneMsg1.EM_LinkedObject = entryHeader;
		var outNbStandaloneMsg2 = CreateQueuedOutboundNbStandaloneMessage("02");
		outNbStandaloneMsg2.EM_LinkedObject = entryHeader;
		var outNbStandaloneMsg3 = CreateQueuedOutboundNbStandaloneMessage("03");
		outNbStandaloneMsg3.EM_LinkedObject = entryHeader;

		Factory.Save();

		CombineAssertions("[PRE-CONDITION] Check messages are NB Standalone and queued", () =>
		{
			AssertEquals("Message 1 SubType", SADConstants.MessageSubTypes.NB, outNbStandaloneMsg1.EM_MessageSubType);
			AssertEquals("Message 2 SubType", SADConstants.MessageSubTypes.NB, outNbStandaloneMsg2.EM_MessageSubType);
			AssertEquals("Message 3 SubType", SADConstants.MessageSubTypes.NB, outNbStandaloneMsg3.EM_MessageSubType);

			AssertEquals("Message 1 Status", EDIMessage.Status.Queued, outNbStandaloneMsg1.EM_Status);
			AssertEquals("Message 2 Status", EDIMessage.Status.Queued, outNbStandaloneMsg2.EM_Status);
			AssertEquals("Message 3 Status", EDIMessage.Status.Queued, outNbStandaloneMsg3.EM_Status);
		});

		var testProcessor = new SadOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
		testProcessor.ProcessMessage(CancellationToken.None);

		outNbStandaloneMsg1.Reload();
		outNbStandaloneMsg2.Reload();
		outNbStandaloneMsg3.Reload();

		CombineAssertions("Check messages were processed", () =>
		{
			AssertEquals("Message 1 Status", EDIMessage.Status.Sent, outNbStandaloneMsg1.EM_Status);
			AssertEquals("Message 2 Status", EDIMessage.Status.Sent, outNbStandaloneMsg2.EM_Status);
			AssertEquals("Message 3 Status", EDIMessage.Status.Sent, outNbStandaloneMsg3.EM_Status);
		});

		CombineAssertions("Check each message was packed into a separate interchange", () =>
		{
			AssertNotNull("Message1 Interchange", outNbStandaloneMsg1.Interchange);
			AssertNotNull("Message2 Interchange", outNbStandaloneMsg2.Interchange);
			AssertNotNull("Message3 Interchange", outNbStandaloneMsg3.Interchange);

			var areAnyOfTheMessageInterchangesTheSame =
				outNbStandaloneMsg1.EM_EI == outNbStandaloneMsg2.EM_EI
				|| outNbStandaloneMsg1.EM_EI == outNbStandaloneMsg3.EM_EI
				|| outNbStandaloneMsg2.EM_EI == outNbStandaloneMsg3.EM_EI;

			AssertEquals(
				"NB IRISP Message Processing relies on the fact that multiple NB Standalone messages are not grouped in a single interchange.\r\nWere any messages packed into the same interchange?",
				false,
				areAnyOfTheMessageInterchangesTheSame
			);
		});

		CombineAssertions("Check interchange text contains information about its respective message", () =>
		{
			AssertInterchangeTextContainsMessageText(outNbStandaloneMsg1);
			AssertInterchangeTextContainsMessageText(outNbStandaloneMsg2);
			AssertInterchangeTextContainsMessageText(outNbStandaloneMsg3);
		});
	}

	void AssertInterchangeTextContainsMessageText(ITEDIMessage message)
	{
		Assert(
			$"EI_InterchangeText should contain [{message.EM_MessageText}], but instead it is [{message.Interchange.EI_InterchangeText}]",
			message.Interchange.EI_InterchangeText.Contains(message.EM_MessageText)
		);
	}

	ITEDIMessage CreateQueuedOutboundNbStandaloneMessage(string messageNumber)
	{
		var message = Factory.New<ITEDIMessage>();
		message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		message.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message.EM_MessageSubType = SADConstants.MessageSubTypes.NB;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageNum = messageNumber;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber);
		message.EM_MessageText = $"TEST MESSAGE {messageNumber} TEXT";
		return message;
	}
}
