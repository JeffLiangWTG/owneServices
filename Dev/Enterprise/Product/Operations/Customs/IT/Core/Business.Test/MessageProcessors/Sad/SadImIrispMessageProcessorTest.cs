using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadImIrispMessageProcessorTest : SadWithNbMessageProcessorTest
{
	public void TestPartialPositiveIrispProcessing()
	{
		var partialPositiveIrispText = @"54RL            54RL0211.XAA170000380397279100    13149600150     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,54RL1026.RAA
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   26/10/17  05:43
RIM          13325700279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]
ESEGUITO   26/01/17  05:44
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F230217 000000      IN ATTESA DI ESITO                                                                                          ";

		string eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_ApplicationCode = "BLT";
		jobDeclaration.JE_MessageType = "IMP";
		var entryHeader1 = jobDeclaration.CustomsEntryHeaders.AddNew();
		var entryHeader2 = jobDeclaration.CustomsEntryHeaders.AddNew();

		entryHeader1.CH_Status = "AWO";
		entryHeader2.CH_Status = "AWO";

		var messageNum1 = "133257";
		var sentIdocEdiMessage1 = entryHeader1.Messages.AddNew();
		sentIdocEdiMessage1.EM_MessageText = @"HERE THERE IS THE INTERCHANGE HEADER
		TIM           13325700012345	A	0123456	01	02072019	HERE SOME OTHER DATA";
		sentIdocEdiMessage1.EM_Status = EDIInterchange.Status.Sent;
		sentIdocEdiMessage1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentIdocEdiMessage1.EM_MessageSubType = "IM";
		sentIdocEdiMessage1.EM_MessageType = IdocMessageType;
		sentIdocEdiMessage1.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNum1);

		var messageNum2 = "222285";
		var sentIdocEdiMessage2 = entryHeader2.Messages.AddNew();
		sentIdocEdiMessage2.EM_MessageText = @"HERE THERE IS THE INTERCHANGE HEADER
		TIM           22228500012345	A	0123456	01	02072019	HERE SOME OTHER DATA";
		sentIdocEdiMessage2.EM_Status = EDIInterchange.Status.Sent;
		sentIdocEdiMessage2.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentIdocEdiMessage2.EM_MessageSubType = "IM";
		sentIdocEdiMessage2.EM_MessageType = IdocMessageType;
		sentIdocEdiMessage2.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNum2);

		var sentEdiInterchange = Factory.New<EDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "1";
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_SessionGUID = new ZGuid(eHubTrackingIDFromSentInterchange);
		sentEdiInterchange.ContainedMessages.Add(sentIdocEdiMessage1);
		sentEdiInterchange.ContainedMessages.Add(sentIdocEdiMessage2);
		sentEdiInterchange.EI_HeaderText = GetSentHeaderText("54RL0211.RAA");
		Factory.Save();

		(var receivedEdiInterchange, var receivedEdiMessage) = GetReceivedInterchangeAndMessage(partialPositiveIrispText, receivedCustomsFileName: "54RL0211.XAA", eHubTrackingIDFromSentInterchange, interchangeNum: "2");
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
		{
			processor.ProcessMessage(receivedEdiMessage);
			Factory.Save();

			var message1 = @"54RL            54RL0211.XAA170000380397279100    13149600150     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,54RL1026.RAA
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   26/10/17  05:43
RIM          13325700279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]
";
			AssertProcessingResult(entryHeader1, receivedEdiMessage, receivedEdiInterchange, expectedEntryMessageStatus: "ERO", expectedEntryHeaderStatus: "", message1, messageNum1, expectedEntryMessagesCount: 2);
			AssertNull("EntryHeader1 -> CusEntryNumber", entryHeader1.CusEntryNumber);

			var message2 = @"54RL            54RL0211.XAA170000380397279100    13149600150     001 00008    INVIO IN AMBIENTE REALE       
 RICEVUTO  26/10/17 05:43,54RL1026.RAA
 RL=005,RS=000,ME=002,MS=000,PE=002,PS=000
ESEGUITO   26/01/17  05:44
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F230217 000000      IN ATTESA DI ESITO                                                                                          ";
			AssertProcessingResult(entryHeader2, receivedEdiMessage, receivedEdiInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", message2.Trim(), messageNum2, expectedEntryMessagesCount: 2);

			AssertEntryNumbersAfterIrispOk(entryHeader2);
		}
	}

	public void TestReleasedIrispProcessing()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, IccIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ICC", IccIrispSampleText.Trim(), MessageNum);

			AssertEntryNumbersAfterIrispOk(entryHeader);

			var clrEntryNumbers = CusEntryNumberHelperTest.GetEntryNumbers(Factory, entryHeader, "CLR");
			AssertEquals("CLR EntryNumbers Count", 1, clrEntryNumbers.Length);

			var clrEntryNumber = clrEntryNumbers.SingleOrDefault(x => x.CE_EntryType == "CLR");
			AssertNotNull($"Entry Number with CE_EntryType: CLR", clrEntryNumber);
			AssertEntryNumber(clrEntryNumber, "CLR", "N7EWSX", "CUS", "", new ZDateTime(2017, 01, 19, 9, 39, 0));

			AssertEquals("CH_EntryReleaseDate", new ZDateTime(2017, 1, 19, 9, 39, 0), entryHeader.CH_EntryReleaseDate);
		}
	}

	public void TestReleasedIrispProcessingWithA93()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, IccIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate("A00", 100m).CF_MethodOfPayment = "G";
			entryLine.Fees.AddOrUpdate("A10", 25m).CF_MethodOfPayment = "G";
			entryLine.Fees.AddOrUpdate("A20", 34.54m).CF_MethodOfPayment = "F";
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ICC", IccIrispSampleText.Trim(), MessageNum);

			var a93EntryNumbers = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().OrderBy(x => x.C9_PaymentParty).ToArray();
			AssertEquals("EntryPayInfo Count", 3, a93EntryNumbers.Length);

			CombineAssertions("EntryPayInfo at 0", () => a93EntryNumbers[0].AssertEntryPayInfo("000081", "F", 34.54m, "4 T", new ZDateTime(2017, 02, 24), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
			CombineAssertions("EntryPayInfo at 1", () => a93EntryNumbers[1].AssertEntryPayInfo("000081", "G", 125m, "4 T", new ZDateTime(2017, 02, 23), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
			CombineAssertions("EntryPayInfo at 2", () => a93EntryNumbers[2].AssertEntryPayInfo("000081", "T", 0m, "4 T", new ZDateTime(2017, 02, 25), Customs.Business.CusEntryPayInfoStatusList.Codes.Pending));
		}
	}

	public void TestNotReleasableIrispProcessing()
	{
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest(MessageNum, UclIrispSampleText, PositiveIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "UCL", UclIrispSampleText.Trim(), MessageNum);

			AssertEntryNumbersAfterIrispOk(entryHeader);
		}
	}

	public void TestReleasedIrispAfterAwoIrispProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var entryHeader, var awoIrispEdiMessage, _) = SetupDeclarationAndGetDataForTest(MessageNum, AwoIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(awoIrispEdiMessage);
			Factory.Save();
			(var iccReceivedInterchangeMessage, var iccReceivedEdiMessage) = GetReceivedInterchangeAndMessage(IccIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
			processor.ProcessMessage(iccReceivedEdiMessage);
			Factory.Save();
			AssertProcessingResult(entryHeader, iccReceivedEdiMessage, iccReceivedInterchangeMessage, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ICC", IccIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 3);

			AssertEntryNumbersAfterIrispOk(entryHeader);
		}
	}

	public void TestReleasedIrispAfterAwoAndUclIrispProcessing()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(var entryHeader, var awoIrispEdiMessage, _) = SetupDeclarationAndGetDataForTest(MessageNum, AwoIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(awoIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			(var uclReceivedInterchangeMessage, var uclReceivedEdiMessage) = GetReceivedInterchangeAndMessage(UclIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "3");
			processor.ProcessMessage(uclReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, uclReceivedEdiMessage, uclReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "UCL", UclIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 3);

			(var iccReceivedInterchangeMessage, var iccReceivedEdiMessage) = GetReceivedInterchangeAndMessage(IccIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, interchangeNum: "4");
			processor.ProcessMessage(iccReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, iccReceivedEdiMessage, iccReceivedInterchangeMessage, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ICC", IccIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 4);

			var clrEntryNumbers = CusEntryNumberHelperTest.GetEntryNumbers(Factory, entryHeader, "CLR");
			AssertEquals("CLR EntryNumbers Count", 1, clrEntryNumbers.Length);

			var clrEntryNumber = clrEntryNumbers.SingleOrDefault(x => x.CE_EntryType == "CLR");
			AssertNotNull($"Entry Number with CE_EntryType: CLR", clrEntryNumber);
			AssertEntryNumber(clrEntryNumber, "CLR", "N7EWSX", "CUS", "", new ZDateTime(2017, 01, 19, 9, 39, 0));
		}
	}

	public void TestIrispProcessingWhenIncomingMessageIsWaitingForResponseAfterNotReleased()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var uclReceivedEdiMessage, var uclReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, UclIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			processor.ProcessMessage(uclReceivedEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			(var awoReceivedInterchangeMessage, var awoReceivedEdiMessage) = GetReceivedInterchangeAndMessage(AwoIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, "3");
			processor.ProcessMessage(awoReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, uclReceivedEdiMessage, uclReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "UCL", UclIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 2);

			AssertInterchangeAndLoggerContainsInfoLogText(awoReceivedInterchangeMessage, $"Entry {entryHeaderReference} for job {declarationReference} not processed: Entry Status is not allowed.");
			AssertEquals("Under Control EDIMessage Status", "RCV", awoReceivedEdiMessage.EM_Status);
		}
	}

	public void TestIrispProcessingWhenIncomingMessageIsWaitingForResponseAfterReleased()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var iccIrispEdiMessage, var iccReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, IccIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			processor.ProcessMessage(iccIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			(var awoReceivedInterchangeMessage, var awoReceivedEdiMessage) = GetReceivedInterchangeAndMessage(AwoIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, "3");
			processor.ProcessMessage(awoReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, iccIrispEdiMessage, iccReceivedInterchangeMessage, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ICC", IccIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			AssertInterchangeAndLoggerContainsInfoLogText(awoReceivedInterchangeMessage, $"Entry {entryHeaderReference} for job {declarationReference} not processed: Entry Status is not allowed.");
			AssertEquals("Waiting for Response EDIMessage Status", "RCV", awoReceivedEdiMessage.EM_Status);
		}
	}

	public void TestIrispProcessingWhenIncomingMessageIsNotReleasedAfterReleased()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var iccIrispEdiMessage, var iccReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, IccIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			processor.ProcessMessage(iccIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			(var uclReceivedInterchangeMessage, var uclReceivedEdiMessage) = GetReceivedInterchangeAndMessage(UclIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, "3");
			processor.ProcessMessage(uclReceivedEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, iccIrispEdiMessage, iccReceivedInterchangeMessage, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ICC", IccIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			AssertInterchangeAndLoggerContainsInfoLogText(uclReceivedInterchangeMessage, $"Entry {entryHeaderReference} for job {declarationReference} not processed: Entry Status is not allowed.");
			AssertEquals("Waiting for Response EDIMessage Status", "RCV", uclReceivedEdiMessage.EM_Status);
		}
	}

	public void TestIccIrispProcessWithNbMessagePositive()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		var iccIrispWithNbPositiveText = CombineIrispWithPositiveNbIrispText(IccIrispSampleText);

		(var entryHeader, var iccWithNbIrispEdiMessage, var iccWithNbReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, iccIrispWithNbPositiveText, receivedCustomsFileName: "00200119.XLR", eHubTrackingIDFromSentInterchange, sentIdocFileName: "00200119.RLR", sentIdocEdiInterchageText: GetIdocPlusTwoNB());
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			entryLine1.ZG_NBStatus = "NBS";
			entryLine2.ZG_NBStatus = "NBS";

			processor.ProcessMessage(iccWithNbIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			AssertProcessingResult(entryHeader, iccWithNbIrispEdiMessage, iccWithNbReceivedInterchangeMessage, expectedEntryMessageStatus: "CLO", expectedEntryHeaderStatus: "ICC", iccIrispWithNbPositiveText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBA", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBA", entryLine2.ZG_NBStatus);
			});
		}
	}

	#region Edge Cases

	public void TestFailingIrispProcessingWhenIncomingMessageHasUnexpectedReleaseNotes()
	{
		var irispSampleTextWithUnexpectedReleaseNote = @"0020            0RPE1003.X81170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,00200119.RLR
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   19/01/17  09:39
RIM          22228500279100P4 T 00061689G190117010994Q000081G230217F230217 000000      SOTTO CONTROLLO                                                                                            ";
		(var entryHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDeclarationAndGetDataForTest("222285", irispSampleTextWithUnexpectedReleaseNote, receivedCustomsFileName: "54RL1026.XGI", ZGuid.NewZGuid().ToString(), sentIdocFileName: "54RL1026.RGI");
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, false))
		{
			processor.ProcessMessage(irispEdiMessage);
			Factory.Save();
			AssertFailMessage(irispEdiMessage, irispEdiInterchange, logText: "Unable to manage the SAD IRISP Release Note: \"SOTTO CONTROLLO\"");
		}
	}

	#endregion

	protected override SadIrispMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SadIrispMessageProcessor(logger);

	protected override void AssertEntryNumbersAfterIrispOk(CusEntryHeader entryHeader)
	{
		var registrationInfoEntryNumber = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfoEntryNumber);
		AssertEntryNumber(registrationInfoEntryNumber, "REG", "4 T-61689G", "CUS", "279100", new ZDateTime(2017, 01, 19));
	}

	protected override ZString IdocMessageType => SADConstants.CustomsInterchangeType.IdocR;
	protected override ZString ReceivedInterchangeType => SADConstants.CustomsInterchangeType.IrispX;
	protected override ZString ValidResponseContentMessageText => AwoIrispSampleText;
	protected override ZString ValidResponseFileName => PositiveIrispFileName;
	protected override ZString MessageSubType => "IM";
	protected override ZInt GetMessageNum() => 222285;

	protected override ZString PositiveIrispSampleText => AwoIrispSampleText;
	protected override ZString PositiveIrispFileName => "0RPE1003.X81";

	protected override ZString MessageType => "IMP";

	string UclIrispSampleText => $@"0020            0RPE1003.X81170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   19/01/17  09:39
RIM          {MessageNum}00279100P4 T 00061689G190117010994Q000081G230217F230217 000000      NON SVINCOLABILE                                                                                           ";

	string AwoIrispSampleText => $@"0020            0RPE1003.X81170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   19/01/17  09:39
RIM          {MessageNum}00279100P4 T 00061689G190117010994Q000081G230217F230217 000000      IN ATTESA DI ESITO                                                                                          ";

	string IccIrispSampleText => $@"0020            0RPE1003.X81170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,0RPE1003.R81
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   19/01/17  09:39
RIM          {MessageNum}00279100P4 T 00061689G190117010994Q000081G230217F240217T250217N7EWSXSVINCOLATA                                                                                                  ";

	protected override ZString ValidIdocFileName => "0RPE1003.R81";

	protected override ZString ClearedIrispSampleText => IccIrispSampleText;

	protected override ZString ExpectedClearedEntryStatus => "ICC";
}
