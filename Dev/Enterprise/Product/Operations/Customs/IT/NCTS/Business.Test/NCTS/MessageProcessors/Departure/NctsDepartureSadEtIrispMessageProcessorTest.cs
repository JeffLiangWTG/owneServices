using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsDepartureSadEtIrispMessageProcessorTest : NctsDepartureSadIncomingCustomsMessageProcessorTest<SadIrispMessageProcessor>
{
	public void TestReleasedIrispProcessing()
	{
		(var nctsHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(PositiveCLRMessageBundle.MessageNum, PositiveCLRMessageBundle.MessageText, PositiveCLRMessageBundle.ValidResponseFileName, ZGuid.NewZGuid().ToString(), PositiveCLRMessageBundle.ValidIdocFileName);
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, PositiveCLRMessageBundle.MessageText.Trim(), PositiveCLRMessageBundle.MessageNum);

		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);

		var clearanceEntryNumbers = GetEntryNumbers(nctsHeader, "CLR");
		AssertEquals("CLR EntryNumbers Count", 1, clearanceEntryNumbers.Length);
		var clrEntryNumber = clearanceEntryNumbers.SingleOrDefault(x => x.CE_EntryType == "CLR");
		AssertEntryNumber(clrEntryNumber, "CLR", "QAAVS5", "CUS", "", new ZDateTime(2019, 10, 3, 6, 47, 0));
	}

	public void TestPartialPositiveIrispProcessing()
	{
		var partialPositiveIrispText = @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   03/10/19  06:48
RET          60126100137101N                                        
 014003 00F 00 Errore 781 : [Scheda Partita non convalidata]          
ESEGUITO   03/10/19  06:47
RET          91595600137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2      310000.00                                                  
";

		string eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var nctsHeader1 = Factory.NewDepartureNctsHeader();
		var nctsHeader2 = Factory.NewDepartureNctsHeader();

		nctsHeader1.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;
		nctsHeader2.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;

		var messageNum1 = "601261";
		var sentIdocEdiMessage1 = nctsHeader1.Messages.AddNew();
		sentIdocEdiMessage1.EM_MessageText = @"HERE THERE IS THE INTERCHANGE HEADER
		TET           60126100012345	A	0123456	01	02072019	HERE SOME OTHER DATA";
		sentIdocEdiMessage1.EM_Status = EDIInterchange.Status.Sent;
		sentIdocEdiMessage1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentIdocEdiMessage1.EM_MessageSubType = "ET";
		sentIdocEdiMessage1.EM_MessageType = IdocMessageType;
		sentIdocEdiMessage1.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNum1);

		var messageNum2 = "915956";
		var sentIdocEdiMessage2 = nctsHeader2.Messages.AddNew();
		sentIdocEdiMessage2.EM_MessageText = @"HERE THERE IS THE INTERCHANGE HEADER
		TET           91595600012345	A	0123456	01	02072019	HERE SOME OTHER DATA";
		sentIdocEdiMessage2.EM_Status = EDIInterchange.Status.Sent;
		sentIdocEdiMessage2.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentIdocEdiMessage2.EM_MessageSubType = "ET";
		sentIdocEdiMessage2.EM_MessageType = IdocMessageType;
		sentIdocEdiMessage2.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNum2);

		var sentEdiInterchange = Factory.New<EDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "1";
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_SessionGUID = new ZGuid(eHubTrackingIDFromSentInterchange);
		sentEdiInterchange.ContainedMessages.Add(sentIdocEdiMessage1);
		sentEdiInterchange.ContainedMessages.Add(sentIdocEdiMessage2);
		sentEdiInterchange.EI_HeaderText = GetSentHeaderText("0RPE1003.R81");
		Factory.Save();

		(var receivedEdiInterchange, var receivedEdiMessage) = GetReceivedInterchangeAndMessage(partialPositiveIrispText, receivedCustomsFileName: "0RPE1003.X81", eHubTrackingIDFromSentInterchange, interchangeNum: "2");
		processor.ProcessMessage(receivedEdiMessage);
		Factory.Save();

		var message1 = @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   03/10/19  06:48
RET          60126100137101N                                        
 014003 00F 00 Errore 781 : [Scheda Partita non convalidata]          
";
		AssertProcessingResult(nctsHeader1, receivedEdiMessage, receivedEdiInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", message1, messageNum1, expectedEntryMessagesCount: 2);
		AssertEquals(nameof(nctsHeader1.MovementReferenceNumber), "", nctsHeader1.MovementReferenceNumber);

		var message2 = @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   03/10/19  06:47
RET          91595600137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2      310000.00                                                  
";
		AssertProcessingResult(nctsHeader2, receivedEdiMessage, receivedEdiInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, message2, messageNum2, expectedEntryMessagesCount: 2);
		AssertEntryNumbersAfterPositiveIrisp(nctsHeader2);
	}

	public void TestProcessPositiveMainSadMessageWithNegativeNb()
	{
		var sentExportWithNbIdoc = $@"TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?ET1          {MessageNum}00  	1 DATA	A	Z	1	
TNB           {MessageNum}01  	1 DATA	C	Z	1";

		var positiveSadMainMessageWithNegativeNbIrisp = $@"845A            845A0210.XAL210000029505137100    13149600150     003 00008    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  10/02/21 12:16,845A0210.RAL
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   10/02/21  12:16
RET          {MessageNum}00137100P3   00000054X191020000000 000000 000000 000000 000000                               20ITQXT080007705T2      310000.00                                                  
ESEGUITO   10/02/21  12:16
RNB          {MessageNum}01137100N                                        
 000000 00L 00 Errore 101 : [Scheda partita non trovata]-Nota: Partita N.7 T 3627P 1 137100";

		(var nctsHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, positiveSadMainMessageWithNegativeNbIrisp, "845A0210.XAL", ZGuid.NewZGuid().ToString(), "845A0210.RAL", sentExportWithNbIdoc);
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem.BY_LineNo = 1;
		goodsItem.BY_Status = EntryLineCustomsStatusList.Codes.Sent;
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.NbRejected, positiveSadMainMessageWithNegativeNbIrisp, MessageNum);
		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);
		AssertEquals(nameof(goodsItem.BY_Status), EntryLineCustomsStatusList.Codes.Rejected, goodsItem.BY_Status);
	}

	public void TestProcessNegativeMainSadMessageWithGoodsItemPending()
	{
		var sentExportWithNbIdoc = $@"TET           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?ET1          {MessageNum}00  	1 DATA	A	Z	1	
TNB           {MessageNum}01  	1 DATA	C	Z	1";

		var negativeSadMainMessage = $@"0RPE            845A0210.XAL190015527541119101    02046110033     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   03/10/19  06:48
RET          {MessageNum}00137101N                                        
 014003 00F 00 Errore 781 : [Scheda Partita non convalidata]          
";

		(var nctsHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, negativeSadMainMessage, "845A0210.XAL", ZGuid.NewZGuid().ToString(), "845A0210.RAL", sentExportWithNbIdoc);
		nctsHeader.MovementHeader.BM_EntryDate = ZDate.Today;
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem.BY_LineNo = 1;
		goodsItem.BY_Status = EntryLineCustomsStatusList.Codes.Sent;
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", negativeSadMainMessage, MessageNum);
		AssertEquals(nameof(goodsItem.BY_Status), EntryLineCustomsStatusList.Codes.Rejected, goodsItem.BY_Status);
		AssertEquals("Negavite Irisp message with NB should clear BM_EntryDate", ZDate.Empty, nctsHeader.MovementHeader.BM_EntryDate);
	}

	public void TestProcessPositiveIrispWithNbMessagesPositive()
	{
		var sentExportWithNbIdoc = $@"TET           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?ET1          {MessageNum}00  	1 DATA	A	Z	1	
TNB           {MessageNum}01  	1 DATA	C	Z	1";

		var positiveSadMainMessageWithPositiveNbIrisp = $@"845A            845A0210.XAL210000029505137100    13149600150     003 00008    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  10/02/21 12:16,845A0210.RAL
 RL=003,RS=000,ME=002,MS=000,PE=001,PS=000
ESEGUITO   10/02/21  12:16
RET          {MessageNum}00137100P3   00000054X191020000000 000000 000000 000000 000000                               20ITQXT080007705T2      310000.00                                                  
ESEGUITO   10/02/21  12:16
RNB          {MessageNum}01279100P";

		(var nctsHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, positiveSadMainMessageWithPositiveNbIrisp, "845A0210.XAL", ZGuid.NewZGuid().ToString(), "845A0210.RAL", sentExportWithNbIdoc);
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem.BY_LineNo = 1;
		goodsItem.BY_Status = EntryLineCustomsStatusList.Codes.Sent;
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, positiveSadMainMessageWithPositiveNbIrisp, MessageNum);
		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);
		AssertEquals(nameof(goodsItem.BY_Status), EntryLineCustomsStatusList.Codes.Approved, goodsItem.BY_Status);
	}

	public void TestNegativeIrispProcessing()
	{
		(var nctsHeader, var negativeIrispMessage, var negativeIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		processor.ProcessMessage(negativeIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, negativeIrispMessage, negativeIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", NegativeIrispSampleText, MessageNum);
	}

	public void TestNegativeIrispProcessingAfterIcntrlOk()
	{
		(var nctsHeader, var negativeIrispMessage, var negativeIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Ok;
		nctsHeader.MovementHeader.BM_EntryDate = ZDate.Today;

		processor.ProcessMessage(negativeIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, negativeIrispMessage, negativeIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", NegativeIrispSampleText, MessageNum);
		AssertEquals("Negavite Irisp message should clear BM_EntryDate", ZDate.Empty, nctsHeader.MovementHeader.BM_EntryDate);
	}

	public void TestPositiveIrispProcessingAfterIcntrlOk()
	{
		(var nctsHeader, var positiveIrispMessage, var positiveIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Ok;

		processor.ProcessMessage(positiveIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, positiveIrispMessage, positiveIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, PositiveIrispSampleText.Trim(), MessageNum);
		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);
	}

	public void TestWaitingForResponseIrispProcessing()
	{
		(var nctsHeader, var positiveIrispMessage, var positiveIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		processor.ProcessMessage(positiveIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, positiveIrispMessage, positiveIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, PositiveIrispSampleText.Trim(), MessageNum);
		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);
	}

	public void TestRegisteredIrispAfterNegativeProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var nctsHeader, var negativeIrispEdiMessage, _) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		processor.ProcessMessage(negativeIrispEdiMessage);
		Factory.Save();

		(var positiveIrispInterchange, var positiveIrispMessage) = GetReceivedInterchangeAndMessage(PositiveIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
		processor.ProcessMessage(positiveIrispMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, positiveIrispMessage, positiveIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, PositiveIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 3);
		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);
	}

	public void TestIrispProcessingWhenIncomingMessageIsNegativeAfterRegistered()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var entryReference = "B0001";

		(var nctsHeader, var positiveIrispMessage, var positiveIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		nctsHeader.BH_JobReference = entryReference;

		processor.ProcessMessage(positiveIrispMessage);
		Factory.Save();
		AssertEntryNumbersAfterPositiveIrisp(nctsHeader);

		(var negativeIrispInterchange, var negativeIrispMessage) = GetReceivedInterchangeAndMessage(NegativeIrispSampleText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, "3");
		processor.ProcessMessage(negativeIrispMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, positiveIrispMessage, positiveIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, PositiveIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
		AssertInterchangeAndLoggerContainsInfoLogText(negativeIrispInterchange, $"Entry {entryReference} for job {entryReference} not processed: Entry Status is not allowed.");
		AssertEquals(nameof(negativeIrispMessage.EM_Status), "RCV", negativeIrispMessage.EM_Status);
	}

	public void TestCustomsDeclarationDuplicatedPositiveIrispProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var duplicatedIrispText = PositiveIrispSampleText;
		var entryReference = "B0001";

		(var nctsHeader, var positiveIrispMessage, var positiveIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, duplicatedIrispText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		nctsHeader.BH_JobReference = entryReference;
		processor.ProcessMessage(positiveIrispMessage);
		Factory.Save();

		(var duplicatedPositiveIrispInterchange, var duplicatedPositiveIrispMessage) = GetReceivedInterchangeAndMessage(duplicatedIrispText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
		processor.ProcessMessage(duplicatedPositiveIrispMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, positiveIrispMessage, positiveIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, duplicatedIrispText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
		AssertInterchangeAndLoggerContainsInfoLogText(duplicatedPositiveIrispInterchange, $"Entry {entryReference} for job {entryReference} not processed: Entry Status is not allowed.");
		AssertEquals(nameof(duplicatedPositiveIrispMessage.EM_Status), "RCV", duplicatedPositiveIrispMessage.EM_Status);
	}

	public void TestCustomsDeclarationDuplicatedNegativeIrispProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var duplicatedIrispText = NegativeIrispSampleText;
		var entryReference = "B0001";

		(var nctsHeader, var negativeIrispMessage, var negativeIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, duplicatedIrispText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		nctsHeader.BH_JobReference = entryReference;
		processor.ProcessMessage(negativeIrispMessage);
		Factory.Save();

		(var duplicatedNegativeIrispInterchange, var duplicatedNegativeIrispMessage) = GetReceivedInterchangeAndMessage(duplicatedIrispText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
		processor.ProcessMessage(duplicatedNegativeIrispMessage);
		Factory.Save();

		AssertProcessingResult(nctsHeader, negativeIrispMessage, negativeIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", duplicatedIrispText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
		AssertInterchangeAndLoggerContainsInfoLogText(duplicatedNegativeIrispInterchange, $"Entry {entryReference} for job {entryReference} not processed: Entry Status is not allowed.");
		AssertEquals(nameof(duplicatedNegativeIrispMessage.EM_Status), "RCV", duplicatedNegativeIrispMessage.EM_Status);
	}

	public void TestIrispProcessingWhenCurrentEntryHasInvalidStatus()
	{
		(var nctsHeader, var positiveIrispMessage, var positiveIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		nctsHeader.MovementHeader.BM_CustomsStatus = "XXX";
		processor.ProcessMessage(positiveIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, positiveIrispMessage, positiveIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.DeclarationMrnAllocated, PositiveIrispSampleText.Trim(), MessageNum);
	}

	public void TestFailingIrispProcessingWhenIncomingMessageDoesNotMatchWithIdoc()
	{
		var wrongMessageNumber = "703013";
		(_, var irispEdiMessage, var irispEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(wrongMessageNumber, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();
		AssertFailMessage(irispEdiMessage, irispEdiInterchange, logText: "Incoming Customs Message (IRISP) does not contain the Message 703013");
	}

	public void TestFailingIrispProcessingWhenGenerateDocumentThrowsException()
	{
		var nctsHeader = Factory.New<NctsHeaderForFailingDocumentsGeneration>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		(_, var irispEdiMessage, var irispEdiInterchange) = SetupNctsHeaderAndGetDataForTest(nctsHeader, MessageNum, PositiveIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();
		AssertFailMessage(irispEdiMessage, irispEdiInterchange, logText: "Generation documents process failed");
	}

	public void TestReleasedIrispProcessingWithA93()
	{
		var irispWithA93Info = $@"037V            037V0120.X09220000013333137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  20/01/22 14:21,037V0120.R09
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   20/01/22  14:21
RET          {MessageNum}00137100P8   00002689U200122013712K000375G230222F230222 000000                               22ITQXT080002689T3                                                                 ";

		(var nctsHeader, var positiveIrispMessage, var positiveIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, irispWithA93Info, "037V0120.X09", ZGuid.NewZGuid().ToString(), "037V0120.R09");

		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		AddNewFee(goodsItem1, "G", 25m);
		AddNewFee(goodsItem1, "F", 34.54m);
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		AddNewFee(goodsItem2, "G", 112.01m);
		AddNewFee(goodsItem2, "F", 13.2m);

		processor.ProcessMessage(positiveIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, positiveIrispMessage, positiveIrispInterchange, expectedEntryMessageStatus: "MOK", expectedEntryHeaderStatus: "DMA", irispWithA93Info.Trim(), MessageNum);

		var a93EntryNumbers = nctsHeader.MovementHeader.PayInfoCollection.Cast<CusInBondPayInfo>().OrderBy(x => x.BPI_MethodOfPayment).ToArray();
		AssertEquals("EntryPayInfo Count", 2, a93EntryNumbers.Length);

		CombineAssertions("EntryPayInfo at 0", () => AssertCusInBondPayInfo(a93EntryNumbers[0], "000375", "F", 47.74m, "8", new ZDateTime(2022, 02, 23), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
		CombineAssertions("EntryPayInfo at 1", () => AssertCusInBondPayInfo(a93EntryNumbers[1], "000375", "G", 137.01m, "8", new ZDateTime(2022, 02, 23), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));

		void AddNewFee(NctsDepartureCargoDesc goodsItem, ZString methodOfPayment, ZDecimal amount)
		{
			var fee = goodsItem.Fees.AddNew();
			fee.BFE_ChargeType = "149";
			fee.BFE_MethodOfCalculation = "X";
			fee.BFE_MethodOfPayment = methodOfPayment;
			fee.BFE_ChargeAmount = amount;
		}

		void AssertCusInBondPayInfo(CusInBondPayInfo cusPayInfo, ZString incomingPayResponseNo, ZString methodOfPayment, ZDecimal paymentAmount, ZString declarationRegistry, ZDateTime expirationDate, ZString paymentStatus)
		{
			AssertEquals("BPI_IncomingPayResponseNo", incomingPayResponseNo, cusPayInfo.BPI_IncomingPayResponseNo);
			AssertEquals("BPI_MethodOfPayment", methodOfPayment, cusPayInfo.BPI_MethodOfPayment);
			AssertEquals("BPI_PaymentAmount", paymentAmount, cusPayInfo.BPI_PaymentAmount);
			AssertEquals("BPI_TransactionType", declarationRegistry, cusPayInfo.BPI_TransactionType);
			AssertEquals("BPI_PaymentDate", expirationDate, cusPayInfo.BPI_PaymentDate);
			AssertEquals("BPI_PaymentStatus", paymentStatus, cusPayInfo.BPI_PaymentStatus);
		}
	}

	public void TestReleaseCodeIrispProcessingAfterRegistrationInfoIrispProcessing()
	{
		var sentInterchangeSessionGuid = ZGuid.NewZGuid().ToString();
		(var nctsHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, sentInterchangeSessionGuid, ValidIdocFileName);
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "MOK", expectedEntryHeaderStatus: "DMA", PositiveIrispSampleText.Trim(), MessageNum);
		nctsHeader.Messages.Reload(true);

		var singleWindowRequestMessages = nctsHeader.Messages.Find(x => x.EM_MessageType == MessageProcessorConstants.InterchangeTypes.SingleWindowRequest);
		AssertEquals("Number of SWR auto-generated messages", 1, singleWindowRequestMessages.Count());
		singleWindowRequestMessages.ElementAt(0).EM_Status = EDIMessageStatusList.Codes.Sent;

		(var clearedIrispEdiInterchange, var clearedIrispEdiMessage) = GetReceivedInterchangeAndMessage(PositiveCLRMessageBundle.MessageText, PositiveCLRMessageBundle.ValidResponseFileName, sentInterchangeSessionGuid, interchangeNum: "3");
		processor.ProcessMessage(clearedIrispEdiMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, clearedIrispEdiMessage, clearedIrispEdiInterchange, expectedEntryMessageStatus: "MOK", expectedEntryHeaderStatus: "DRL", PositiveCLRMessageBundle.MessageText.Trim(), MessageNum, expectedEntryMessagesCount: 3);
	}

	protected override ZString ValidResponseContentMessageText => PositiveIrispSampleText;
	protected override ZString ValidResponseFileName => "0RPE1003.X81";
	protected override ZString IdocMessageType => SADConstants.CustomsInterchangeType.IdocR;
	protected override ZString ReceivedInterchangeType => SADConstants.CustomsInterchangeType.IrispX;
	protected override ZString MessageSubType => "ET";
	protected override SadIrispMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SadIrispMessageProcessor(logger);

	protected override ZString ValidIdocFileName => "0RPE1003.R81";

	#region Implementation

	protected ZString MessageNum => "601261";

	protected ZString PositiveIrispSampleText => $@"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          {MessageNum}00137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2      310000.00                                                  ";

	protected ZString PositiveIrispFileName => ValidResponseFileName;

	protected ZString NegativeIrispSampleText => $@"54RL            0RPE1003.X81170000380397279100    13149600150     001 00006    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,0RPE1003.R81
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   26/10/17  05:43
R{MessageSubType}          {MessageNum}00279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]";

	protected ZString NegativeIrispFileName => "0RPE1003.X81";

	protected (ZString ValidResponseFileName, ZString ValidIdocFileName, ZString MessageNum, ZString MessageText) PositiveCLRMessageBundle =>
("0RPE1003.X81", "0RPE1003.R81", "601261",
$@"037V            0RPE1003.X81210000046881137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  03/10/19 06:47,037V0310.Raf
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          60126100137100P1 T 00039036D191020000000 000000 000000 000000 000000QAAVS5SVINCOLATA               20ITQXT080007705T2      310000.00                                                  ");

	CusEntryNumber[] GetEntryNumbers(NctsHeader nctsHeader, ZString entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, nctsHeader.CountryCode);
		query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
		return Factory.Load<CusEntryNumber>(query);
	}

	protected void AssertEntryNumbersAfterPositiveIrisp(NctsHeader nctsHeader)
	{
		AssertEquals(nameof(nctsHeader.MovementReferenceNumber), "20ITQXT080007705T2", nctsHeader.MovementReferenceNumber);
		AssertEntryNumber(nctsHeader.MovementReferenceEntryNumber, "MRN", "20ITQXT080007705T2", "CUS", "137100", new ZDateTime(2020, 10, 19));

		var gtyEntryNumber = GetEntryNumbers(nctsHeader, "GTY").SingleOrDefault();
		AssertNotNull(nameof(gtyEntryNumber), gtyEntryNumber);
		AssertEntryNumber(gtyEntryNumber, "GTY", "310000.00", "CUS", "", new ZDateTime(2020, 10, 19));
	}

	#endregion
}

class NctsHeaderForFailingDocumentsGeneration : NctsHeader, ICustomsLinkedObjectAdapterProvider
{
	public NctsHeaderForFailingDocumentsGeneration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ISingleWindowCustomsLinkedObjectAdapter GetNewSingleWindowCustomsLinkedObjectAdapter()
	{
		return new NctsHeaderCustomsLinkedObjectAdapterForFailingDocumentGeneration(this);
	}

	public ISadCustomsLinkedObjectAdapter GetSadCustomsLinkedObjectAdapter()
	{
		return new NctsHeaderCustomsLinkedObjectAdapterForFailingDocumentGeneration(this);
	}
}

class NctsHeaderCustomsLinkedObjectAdapterForFailingDocumentGeneration : NctsHeaderCustomsLinkedObjectAdapter, ICustomsLinkedObjectAdapter
{
	public NctsHeaderCustomsLinkedObjectAdapterForFailingDocumentGeneration(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	void ICustomsLinkedObjectAdapter.GenerateDocuments() => throw new NotSupportedException("Mock exception for test");
}
