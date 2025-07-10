using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureSadNbIrispMessageProcessorTest : NctsDepartureSadEtIrispMessageProcessorTest
{
	public void TestProcessNegativeIrispWithEntryLinesPending()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var sentIdoc = GetIdocPlusTwoNB();

		(var nctsHeader, var negativeIrispEdiMessage, var negativeIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName, sentIdoc);

		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;

		goodsItem1.BY_Status = "NBS";
		goodsItem2.BY_Status = "NBS";

		processor.ProcessMessage(negativeIrispEdiMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, negativeIrispEdiMessage, negativeIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", NegativeIrispSampleText, MessageNum);
		CombineAssertions("GoodsItem Line status", () =>
		{
			AssertEquals($"Goods Item 1, {nameof(NctsDepartureCargoDesc.BY_Status)}", "NBR", goodsItem1.BY_Status);
			AssertEquals($"Goods Item 2, {nameof(NctsDepartureCargoDesc.BY_Status)}", "NBR", goodsItem2.BY_Status);
		});
	}

	public void TestIrispProcessWithPartialNbMessagePositiveAndNextStandaloneNb()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();

		var sentIdoc = GetIdocPlusTwoNB();

		var positiveIrispWithPartialPositiveNbText = $@"{PositiveIrispSampleText}
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}01279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}02279100P";

		(var nctsHeader, var partialPositiveIrispMessage, var partialPositiveIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, positiveIrispWithPartialPositiveNbText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName, sentIdoc);

		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;

		goodsItem1.BY_Status = "NBS";
		goodsItem2.BY_Status = "NBS";

		processor.ProcessMessage(partialPositiveIrispMessage);
		Factory.Save();

		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);

		AssertProcessingResult(nctsHeader, partialPositiveIrispMessage, partialPositiveIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: "NBR", positiveIrispWithPartialPositiveNbText, MessageNum, expectedEntryMessagesCount: 2);
		CombineAssertions("GoodsItem Line status", () =>
		{
			AssertEquals($"Goods Item 1, {nameof(NctsDepartureCargoDesc.BY_Status)}", "NBR", goodsItem1.BY_Status);
			AssertEquals($"Goods Item 2, {nameof(NctsDepartureCargoDesc.BY_Status)}", "NBA", goodsItem2.BY_Status);
		});

		//SENDING NB STANDALONE TO FIX NB MESSAGE
		var nbStandaloneMessageNum = (ZInt.Parse(MessageNum) + 1).ToString();
		string standaloneNbIdocText = $"TNB           {nbStandaloneMessageNum}004 T	61689	D	19912017	1	OTHER DATA";
		var eHubTrackingIDFromSentNBInterchange = ZGuid.NewZGuid().ToString();

		var sentNbIdocEdiMessage = nctsHeader.Messages.AddNew();
		sentNbIdocEdiMessage.EM_MessageText = standaloneNbIdocText;
		sentNbIdocEdiMessage.EM_Status = EDIInterchange.Status.Sent;
		sentNbIdocEdiMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentNbIdocEdiMessage.EM_MessageSubType = "NB";
		sentNbIdocEdiMessage.EM_MessageType = IdocMessageType;
		sentNbIdocEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(nbStandaloneMessageNum);
		sentNbIdocEdiMessage.EM_MessageNum = nbStandaloneMessageNum;

		var sentEdiInterchange = Factory.New<EDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "3";
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_SessionGUID = new ZGuid(eHubTrackingIDFromSentNBInterchange);
		sentEdiInterchange.EI_BodyText = standaloneNbIdocText;
		sentEdiInterchange.EI_HeaderText = GetSentHeaderText("0RPE1003.R82");
		sentEdiInterchange.ContainedMessages.Add(sentNbIdocEdiMessage);
		Factory.Save();

		var standaloneNbIrispText = $@"0RPE            0RPE1003.X82190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R82
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RNB          {nbStandaloneMessageNum}00279100P ";

		(var receivedStandaloneNbInterchange, var receivedStandaloneIrispMessage) = GetReceivedInterchangeAndMessage(standaloneNbIrispText, "0RPE1003.X82", eHubTrackingIDFromSentNBInterchange, nbStandaloneMessageNum);
		Factory.Save();

		processor.ProcessMessage(receivedStandaloneIrispMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, receivedStandaloneIrispMessage, receivedStandaloneNbInterchange, NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, standaloneNbIrispText.Trim(), nbStandaloneMessageNum, expectedEntryMessagesCount: 4, expectedMessageSubType: "NB");
		CombineAssertions("Entry Lines status", () =>
		{
			AssertEquals("ZG_NBStatus Entry Line 1", "NBA", goodsItem1.BY_Status);
			AssertEquals("ZG_NBStatus Entry Line 2", "NBA", goodsItem2.BY_Status);
		});
	}

	public void TestNegativeStandaloneNbMessage()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var firstNbMessageNum = (ZInt.Parse(MessageNum) + 1).ToString();
		var secondNbMessageNum = (ZInt.Parse(MessageNum) + 2).ToString();

		var idocNbStandalone = $@"TNB           {firstNbMessageNum}00ET	{MessageNum}			1	OTHER DATA
TNB           {secondNbMessageNum}00{MessageSubType}	{MessageNum}			2	OTHER DATA";

		var irispNbStandaloneNegative = $@"845A            845A0521.X07200000124458279100    13149600150     003 00009    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  21/05/20 12:42,845A0521.R07
 RL=002,RS=000,ME=002,MS=000,PE=000,PS=000
ESEGUITO   21/05/20  12:42
RNB          {firstNbMessageNum}00279100N                                        
 000J00 00L 00 Errore 364 : [Colli incoerenti con archivio]           
ESEGUITO   21/05/20  12:42
RNB          {secondNbMessageNum}00279100N                                        
 000J00 00L 00 Errore 364 : [Colli incoerenti con archivio]           
";
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Ok;

		var sentNbIdocEdiMessage = nctsHeader.Messages.AddNew();
		sentNbIdocEdiMessage.EM_MessageText = irispNbStandaloneNegative;
		sentNbIdocEdiMessage.EM_Status = EDIInterchange.Status.Sent;
		sentNbIdocEdiMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentNbIdocEdiMessage.EM_MessageSubType = "NB";
		sentNbIdocEdiMessage.EM_MessageType = IdocMessageType;
		sentNbIdocEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(firstNbMessageNum);
		sentNbIdocEdiMessage.EM_MessageNum = firstNbMessageNum;

		var sentEdiInterchange = Factory.New<EDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "3";
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_SessionGUID = new ZGuid(eHubTrackingIDFromSentInterchange);
		sentEdiInterchange.EI_BodyText = idocNbStandalone;
		sentEdiInterchange.EI_HeaderText = GetSentHeaderText(idocFileName: "845A0521.R07");
		sentEdiInterchange.ContainedMessages.Add(sentNbIdocEdiMessage);
		Factory.Save();

		(var receivedStandaloneNbInterchange, var receivedStandaloneIrispMessage) = GetReceivedInterchangeAndMessage(irispNbStandaloneNegative, "845A0521.X07", eHubTrackingIDFromSentInterchange, firstNbMessageNum);
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Ok;
		nctsHeader.MovementHeader.BM_CustomsStatus = "NBR";
		nctsHeader.MovementHeader.BM_EntryDate = ZDateTime.Today;

		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;

		goodsItem1.BY_Status = "NBS";
		goodsItem2.BY_Status = "NBS";

		Factory.Save();

		processor.ProcessMessage(receivedStandaloneIrispMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, receivedStandaloneIrispMessage, receivedStandaloneNbInterchange, NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.NbRejected, irispNbStandaloneNegative, firstNbMessageNum, expectedEntryMessagesCount: 2, expectedMessageSubType: "NB");
		CombineAssertions("Entry Lines status", () =>
		{
			AssertEquals("ZG_NBStatus Entry Line 1", "NBR", goodsItem1.BY_Status);
			AssertEquals("ZG_NBStatus Entry Line 2", "NBR", goodsItem2.BY_Status);
		});

		AssertNotEquals("BM_EntryDate should not be cleared by an  negative NB standalone message", ZDate.Empty, nctsHeader.MovementHeader.BM_EntryDate);
	}

	ZString GetIdocPlusTwoNB()
	{
		return $@"TET           {MessageNum}00012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?ET1          {MessageNum}00IM1 DATA
?ET1          {MessageNum}00IM1 DATA
TNB           {MessageNum}01ET	{MessageNum}			1	OTHER DATA
TNB           {MessageNum}02ET	{MessageNum}			2	OTHER DATA";
	}
}
