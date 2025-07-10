using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SadWithNbMessageProcessorTest : SadIrispMessageProcessorTest
{
	public void TestProcessPositiveIrispWhenEntryLinesArePending()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var regWithNbIrispEdiMessage, var regWithNbReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, PositiveIrispSampleText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, sentIdocFileName: ValidIdocFileName, GetIdocPlusTwoNB());
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			entryLine1.ZG_NBStatus = "NBS";
			entryLine2.ZG_NBStatus = "NBS";

			processor.ProcessMessage(regWithNbIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			AssertProcessingResult(entryHeader, regWithNbIrispEdiMessage, regWithNbReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", PositiveIrispSampleText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBS", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBS", entryLine2.ZG_NBStatus);
			});
		}
	}

	public void TestProcessPositiveIrispWithNbMessagesPositive()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";
		var irispWithNbPositiveText = GetRegisteredPositiveIrispWithPositiveNbText();

		(var entryHeader, var iccWithNbIrispEdiMessage, var iccWithNbReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, irispWithNbPositiveText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, sentIdocFileName: ValidIdocFileName, GetIdocPlusTwoNB());
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
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

			AssertProcessingResult(entryHeader, iccWithNbIrispEdiMessage, iccWithNbReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", irispWithNbPositiveText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBA", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBA", entryLine2.ZG_NBStatus);
			});
		}
	}

	public void TestProcessNegativeIrispWithEntryLinesPending()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		(var entryHeader, var ediIrispEdiMessage, var ediReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, eHubTrackingIDFromSentInterchange, sentIdocFileName: ValidIdocFileName, GetIdocPlusTwoNB());
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			entryLine1.ZG_NBStatus = "NBS";
			entryLine2.ZG_NBStatus = "NBS";

			processor.ProcessMessage(ediIrispEdiMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, ediIrispEdiMessage, ediReceivedInterchangeMessage, expectedEntryMessageStatus: "ERO", expectedEntryHeaderStatus: "", NegativeIrispSampleText, MessageNum, expectedEntryMessagesCount: 2);
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBR", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBR", entryLine2.ZG_NBStatus);
			});
		}
	}

	public void TestProcessPositiveIrispWithNbMessagesNegative()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		string irispWithNbNegativeNbText = GetPositiveIrispWithNegativeNbText();

		(var entryHeader, var awoWithNbIrispEdiMessage, var awoWithNbReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, irispWithNbNegativeNbText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, sentIdocFileName: ValidIdocFileName, GetIdocPlusTwoNB());
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			entryLine1.ZG_NBStatus = "NBS";
			entryLine2.ZG_NBStatus = "NBS";

			processor.ProcessMessage(awoWithNbIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			AssertProcessingResult(entryHeader, awoWithNbIrispEdiMessage, awoWithNbReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "NBR", irispWithNbNegativeNbText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBR", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBR", entryLine2.ZG_NBStatus);
			});
		}
	}

	public void TestAwoIrispProcessWithPartialNbMessagePositiveAndNextStandaloneNb()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";

		string positiveIrispWithPartialPositiveNbText = GetPositiveIrispWithPartialPositiveNbText();

		(var entryHeader, var awoWithNbIrispEdiMessage, var awoWithNbReceivedInterchangeMessage, var declaration) = SetupDeclarationAndGetDataWithJobForTest(MessageNum, positiveIrispWithPartialPositiveNbText, PositiveIrispFileName, eHubTrackingIDFromSentInterchange, sentIdocFileName: ValidIdocFileName, GetIdocPlusTwoNB());
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			entryLine1.ZG_NBStatus = "NBS";
			entryLine2.ZG_NBStatus = "NBS";

			processor.ProcessMessage(awoWithNbIrispEdiMessage);
			Factory.Save();

			AssertEntryNumbersAfterIrispOk(entryHeader);

			AssertProcessingResult(entryHeader, awoWithNbIrispEdiMessage, awoWithNbReceivedInterchangeMessage, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "NBR", positiveIrispWithPartialPositiveNbText.Trim(), MessageNum, expectedEntryMessagesCount: 2);
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBR", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBA", entryLine2.ZG_NBStatus);
			});

			//SENDING NB STANDALONE TO FIX NB MESSAGE
			var nbStandaloneMessageNum = GetMessageNumberForNbStandalone(offset: 1);
			string standaloneNbIdocText = $"TNB           {nbStandaloneMessageNum}004 T	61689	D	19912017	1	OTHER DATA";
			var eHubTrackingIDFromSentNBInterchange = ZGuid.NewZGuid().ToString();

			var sentNbIdocEdiMessage = entryHeader.Messages.AddNew();
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
			sentEdiInterchange.EI_HeaderText = GetSentHeaderText("00200119.RLR");
			sentEdiInterchange.ContainedMessages.Add(sentNbIdocEdiMessage);
			Factory.Save();

			var standaloneNbIrispText = GetPositiveStandaloneNbIrispText(nbStandaloneMessageNum);

			(var receivedStandaloneNbInterchange, var receivedStandaloneIrispMessage) = GetReceivedInterchangeAndMessage(standaloneNbIrispText, PositiveStandaloneNbIrispFileName, eHubTrackingIDFromSentNBInterchange, nbStandaloneMessageNum);
			Factory.Save();

			processor.ProcessMessage(receivedStandaloneIrispMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, receivedStandaloneIrispMessage, receivedStandaloneNbInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "REG", standaloneNbIrispText.Trim(), nbStandaloneMessageNum, expectedEntryMessagesCount: 4, expectedMessageSubType: "NB");
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBA", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBA", entryLine2.ZG_NBStatus);
			});
		}
	}

	public void TestNegativeStandaloneNbMessage()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		var declarationReference = "B0001";
		var entryHeaderReference = "BGM00001";
		var firstNbMessageNum = GetMessageNumberForNbStandalone(1);
		var secondNbMessageNum = GetMessageNumberForNbStandalone(offset: 2);

		var idocNbStandalone = GetStandaloneIdoc(firstNbMessageNum, secondNbMessageNum);

		var irispNbStandaloneNegative = GetNegativeStandaloneNbIrispText(firstNbMessageNum, secondNbMessageNum);
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			var sentNbIdocEdiMessage = entryHeader.Messages.AddNew();
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

			(var receivedStandaloneNbInterchange, var receivedStandaloneIrispMessage) = GetReceivedInterchangeAndMessage(irispNbStandaloneNegative, NegativeStandaloneNbIrispFileName, eHubTrackingIDFromSentInterchange, firstNbMessageNum);
			declaration.JE_DeclarationReference = declarationReference;
			entryHeader.CH_BGMReference = entryHeaderReference;
			entryHeader.CH_Status = "ACO";
			entryHeader.CH_EntryStatus = "NBR";

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			entryLine1.ZG_NBStatus = "NBS";
			entryLine2.ZG_NBStatus = "NBS";

			Factory.Save();

			processor.ProcessMessage(receivedStandaloneIrispMessage);
			Factory.Save();

			AssertProcessingResult(entryHeader, receivedStandaloneIrispMessage, receivedStandaloneNbInterchange, expectedEntryMessageStatus: "ACO", expectedEntryHeaderStatus: "NBR", irispNbStandaloneNegative, firstNbMessageNum, expectedEntryMessagesCount: 2, expectedMessageSubType: "NB");
			CombineAssertions("Entry Lines status", () =>
			{
				AssertEquals("ZG_NBStatus Entry Line 1", "NBR", entryLine1.ZG_NBStatus);
				AssertEquals("ZG_NBStatus Entry Line 2", "NBR", entryLine2.ZG_NBStatus);
			});
		}
	}

	#region Implementation

	ZString GetRegisteredPositiveIrispWithPositiveNbText() => CombineIrispWithPositiveNbIrispText(PositiveIrispSampleText);

	protected ZString CombineIrispWithPositiveNbIrispText(ZString irispText)
	{
		return $@"{irispText}
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}01279100P
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}02279100P ";
	}

	ZString GetPositiveIrispWithNegativeNbText()
	{
		return $@"{PositiveIrispSampleText}
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}01279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}02279100N
 013302 00L 00 Errore 0 : [Codice addizionale in input mancante]";
	}

	ZString GetPositiveIrispWithPartialPositiveNbText()
	{
		return $@"{PositiveIrispSampleText}
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}01279100N
 013703 00F 00 Errore 637 : [Regime comunitario incoerente con tipo dichiarazione]
ESEGUITO   15/04/20  14:13
RNB          {MessageNum}02279100P ";
	}

	ZString GetNegativeStandaloneNbIrispText(ZString firstNbMessageNum, ZString secondNbMessageNum)
	{
		return $@"845A            845A0521.X07200000124458279100    13149600150     003 00009    INVIO IN AMBIENTE DI PROVA    
 RICEVUTO  21/05/20 12:42,845A0521.R07
 RL=002,RS=000,ME=002,MS=000,PE=000,PS=000
ESEGUITO   21/05/20  12:42
RNB          {firstNbMessageNum}00279100N                                        
 000J00 00L 00 Errore 364 : [Colli incoerenti con archivio]           
ESEGUITO   21/05/20  12:42
RNB          {secondNbMessageNum}00279100N                                        
 000J00 00L 00 Errore 364 : [Colli incoerenti con archivio]           
";
	}

	ZString NegativeStandaloneNbIrispFileName => "845A0521.X07";

	protected ZString GetIdocPlusTwoNB()
	{
		return $@"T{MessageSubType}           {MessageNum}00012345	A	0123456	01	02072019	HERE SOME OTHER DATA
?{MessageSubType}1          {MessageNum}00IM1 DATA
?{MessageSubType}1          {MessageNum}00IM1 DATA
TNB           {MessageNum}01{MessageSubType}	{MessageNum}			1	OTHER DATA
TNB           {MessageNum}02{MessageSubType}	{MessageNum}			2	OTHER DATA";
	}

	ZString GetMessageNumberForNbStandalone(int offset) => ConvertToMessageNumForMessage(GetMessageNum() + offset);

	ZString GetPositiveStandaloneNbIrispText(ZString messageNum)
	{
		return $@"0020            00200119.XLR170000786171279100    10715680152     001 00005    INVIO IN AMBIENTE REALE       
 RICEVUTO  19/01/17 09:39,00200119.RLR
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   15/04/20  14:13
RNB          {messageNum}00279100P ";
	}

	ZString GetStandaloneIdoc(ZString firstNbMessageNum, ZString secondNbMessageNum)
	{
		return $@"TNB           {firstNbMessageNum}00{MessageSubType}	{MessageNum}			1	OTHER DATA
TNB           {secondNbMessageNum}00{MessageSubType}	{MessageNum}			2	OTHER DATA";
	}

	ZString PositiveStandaloneNbIrispFileName => "00200119.XLR";
	#endregion

}
