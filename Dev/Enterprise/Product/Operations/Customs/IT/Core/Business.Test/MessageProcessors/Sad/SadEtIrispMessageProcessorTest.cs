using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SadEtIrispMessageProcessorTest : SadIrispMessageProcessorTest
{
	public void TestReleasedIrispProcessing()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(positiveCLRMessageBundle.MessageNum, positiveCLRMessageBundle.MessageText, positiveCLRMessageBundle.ValidResponseFileName, ZGuid.NewZGuid().ToString(), positiveCLRMessageBundle.ValidIdocFileName);
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();
		AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ECC", positiveCLRMessageBundle.MessageText.Trim(), positiveCLRMessageBundle.MessageNum);

		AssertEntryNumbersAfterIrispOk(entryHeader);

		var clrEntryNumbers = CusEntryNumberHelperTest.GetEntryNumbers(Factory, entryHeader, "CLR");
		AssertEquals("CLR EntryNumbers Count", 1, clrEntryNumbers.Length);

		var clrEntryNumber = clrEntryNumbers.SingleOrDefault(x => x.CE_EntryType == "CLR");

		var expectedIssueDate = new ZDateTime(2019, 10, 3, 6, 47, 0);
		AssertNotNull($"Entry Number with CE_EntryType: CLR", clrEntryNumber);
		AssertEntryNumber(clrEntryNumber, "CLR", "QAAVS5", "CUS", "", expectedIssueDate);

		AssertEquals("CH_EntryReleaseDate", expectedIssueDate, entryHeader.CH_EntryReleaseDate);
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
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
		var entryHeader1 = jobDeclaration.CustomsEntryHeaders.AddNew();
		var entryHeader2 = jobDeclaration.CustomsEntryHeaders.AddNew();

		entryHeader1.CH_Status = "AWO";
		entryHeader2.CH_Status = "AWO";

		var messageNum1 = "601261";
		var sentIdocEdiMessage1 = entryHeader1.Messages.AddNew();
		sentIdocEdiMessage1.EM_MessageText = @"HERE THERE IS THE INTERCHANGE HEADER
		TET           60126100012345	A	0123456	01	02072019	HERE SOME OTHER DATA";
		sentIdocEdiMessage1.EM_Status = EDIInterchange.Status.Sent;
		sentIdocEdiMessage1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentIdocEdiMessage1.EM_MessageSubType = "ET";
		sentIdocEdiMessage1.EM_MessageType = IdocMessageType;
		sentIdocEdiMessage1.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNum1);

		var messageNum2 = "915956";
		var sentIdocEdiMessage2 = entryHeader2.Messages.AddNew();
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
		AssertProcessingResult(entryHeader1, receivedEdiMessage, receivedEdiInterchange, expectedEntryMessageStatus: "ERO", expectedEntryHeaderStatus: "", message1, messageNum1, expectedEntryMessagesCount: 2);
		AssertNull("EntryHeader1 -> CusEntryNumber", entryHeader1.CusEntryNumber);

		var message2 = @"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   03/10/19  06:47
RET          91595600137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2      310000.00                                                  
";
		AssertProcessingResult(entryHeader2, receivedEdiMessage, receivedEdiInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", message2, messageNum2, expectedEntryMessagesCount: 2);

		AssertEntryNumbersAfterIrispOk(entryHeader2);
	}

	public void TestProcessPositiveMainSadMessageWithNegativeNb()
	{
		var sentExportWithNbIdoc = $@"TET           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA
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

		(var entryHeader, var irispEdiMessage, var irispEdiInterchange, _) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, positiveSadMainMessageWithNegativeNbIrisp, "845A0210.XAL", ZGuid.NewZGuid().ToString(), "845A0210.RAL", sentExportWithNbIdoc);
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		entryLine.ZG_NBStatus = EntryLineCustomsStatusList.Codes.Sent;
		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();

		AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "NBR", positiveSadMainMessageWithNegativeNbIrisp, MessageNum);
		AssertEntryNumbersAfterIrispOk(entryHeader);
		AssertEquals("EntryLine has been rejected", EntryLineCustomsStatusList.Codes.Rejected, entryLine.ZG_NBStatus);
	}

	protected override ZString ValidResponseContentMessageText => PositiveIrispSampleText;

	protected override ZString ValidResponseFileName => "0RPE1003.X81";

	protected override ZString IdocMessageType => SADConstants.CustomsInterchangeType.IdocR;

	protected override ZString ReceivedInterchangeType => SADConstants.CustomsInterchangeType.IrispX;

	protected override ZString MessageSubType => "ET";
	protected override ZInt GetMessageNum() => 601261;

	protected override SadIrispMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SadIrispMessageProcessor(logger);

	protected override void AssertEntryNumbersAfterIrispOk(CusEntryHeader entryHeader)
	{
		AssertNotNull("EntryHeader -> CusEntryNumber", entryHeader.CusEntryNumber);
		AssertEquals("EntryNumber", "20ITQXT080007705T2", entryHeader.EntryNumber);

		var entryNumber = entryHeader.CusEntryNumber;
		AssertEntryNumber(entryNumber, "MRN", "20ITQXT080007705T2", "CUS", "137100", new ZDateTime(2020, 10, 19));

		var gtyEntryNumber = GetGtyEntryNumber(entryHeader);
		AssertNotNull("Guaranty Entry Number value", gtyEntryNumber);
		AssertEntryNumber(gtyEntryNumber, "GTY", "310000.00", "CUS", "", new ZDateTime(2020, 10, 19));
	}

	protected override ZString PositiveIrispSampleText => $@"0RPE            0RPE1003.X81190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  03/10/19 06:47,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          {MessageNum}00137100P8   00007705P191020014320X001352G231120 000000 000000                               20ITQXT080007705T2      310000.00                                                  ";

	protected ZString EccIrispSampleText => $@"037V            0RPE1003.X81210000046881137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  03/10/19 06:47,0RPE1003.X81
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          {MessageNum}00137100P1 T 00039036D191020000000 000000 000000 000000 000000QAAVS5SVINCOLATA               20ITQXT080007705T2                                                                 ";

	protected ZString EccIrispFileName => "0RPE1003.X81";
	protected override ZString PositiveIrispFileName => ValidResponseFileName;

	protected override ZString MessageType => "EXP";

	protected override ZString ValidIdocFileName => "0RPE1003.R81";

	protected override ZString ClearedIrispSampleText => EccIrispSampleText;

	protected override ZString ExpectedClearedEntryStatus => "ECC";

	(ZString ValidResponseFileName, ZString ValidIdocFileName, ZString MessageNum, ZString MessageText) positiveCLRMessageBundle =
	("037V0310.Xaf", "037V0310.Raf", "050005",
	$@"037V            037V0310.Xaf210000046881137100    02714410277     001 00005    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  03/10/19 06:47,037V0310.Raf
 RL=003,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          05000500137100P1 T 00039036D191020000000 000000 000000 000000 000000QAAVS5SVINCOLATA               20ITQXT080007705T2      310000.00                                                  ");

	CusEntryNumber GetGtyEntryNumber(CusEntryHeader entryHeader)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, "GTY");
		return Factory.LoadTop1<CusEntryNumber>(query);
	}
}
