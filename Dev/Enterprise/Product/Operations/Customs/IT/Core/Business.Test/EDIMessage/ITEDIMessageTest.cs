using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(ITEDIMessage))]
sealed class ITEDIMessageTest : EDIMessageTest
{
	public void TestConstructor_Null()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ITEDIMessage(null, null));
	}

	public void TestApplicationCode()
	{
		var message = (ITEDIMessage)GetNewBusinessObject();
		AssertEquals("Application Code", "ITM", message.EM_ApplicationCode);
	}

	public void TestFillInPlaceHolders()
	{
		SetupFountainAndDeclarantAndAccountCollection();
		Factory.Save();

		var headerFixedPart = "TIM           <<MSGNO PLACEHOLDER>>00";
		var lineFixedPart = "?IM1          <<MSGNO PLACEHOLDER>>00";
		var headerFixedPartAfterSave = "TIM           00000100";
		var lineFixedPartAfterSave = "?IM1          00000100";

		var declaration = SetupDeclaration("IMP");
		var entryHeader = declaration.CustomsEntryHeaders[0];

		var imMessageSendingObject = new IMMessageSendingObject(entryHeader, new JobDeclarationMessageSendingObjectParent(declaration));
		var generatedEdiMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, imMessageSendingObject)).GenerateMessage();
		var serializedMessageText = generatedEdiMessage.EM_MessageText;
		generatedEdiMessage.Delete();
		AssertNotNullOrEmpty("Serialized message not null or empty", serializedMessageText);
		AssertEquals("Placeholder existing for header", true, serializedMessageText.Contains(headerFixedPart));
		AssertEquals("Placeholder existing for line", true, serializedMessageText.Contains(lineFixedPart));

		new ITMessageSender(Factory, new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, imMessageSendingObject), entryHeader).Send();
		Factory.Save();

		var ediMessages = entryHeader.Messages;
		AssertNotNull("Messages should not be null", ediMessages);
		AssertEquals("Messages count should be", 1, ediMessages.Count);
		var savedMessage = ediMessages[0];
		var savedMessageText = savedMessage.EM_MessageText;
		AssertNotNullOrEmpty("Serialized message not null or empty", savedMessageText);
		AssertEquals("Placeholder NOT existing for header", false, savedMessageText.Contains(headerFixedPart));
		AssertEquals("Placeholder NOT existing for line", false, savedMessageText.Contains(lineFixedPart));
		AssertEquals("Correct PAN existing for header", true, savedMessageText.Contains(headerFixedPartAfterSave));
		AssertEquals("Correct PAN existing for line", true, savedMessageText.Contains(lineFixedPartAfterSave));
	}

	public void TestReplaceInterchangeBodyTextMessageNoPlaceHolderForManualProcedure()
	{
		var interchange = Factory.New<ITEDIInterchange>();
		var message = Factory.New<ITEDIMessage>();
		interchange.ContainedMessages.Add(message);

		interchange.EI_From = "F";
		interchange.EI_To = "T";
		interchange.EI_Status = EDIInterchangeStatusList.Codes.Manual;
		interchange.EI_BodyText = "What follows ':' to be replaced on saving: <<MSGNO PLACEHOLDER>>";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");
		message.EM_Status = EDIMessageStatusList.Codes.Manual;
		Assert("Message is not in database yet", !message.IsInDatabase);
		Factory.Save();
		CombineAssertions("PlaceHolder has been replaced on Factory.Save()", () =>
		{
			AssertContains("123456", interchange.EI_BodyText);
			AssertNotContains("<<MSGNO PLACEHOLDER>>", interchange.EI_BodyText);
		});

		Assert("Message is already in database", message.IsInDatabase);
		interchange.EI_BodyText = "What follows ':' NOT to be replaced on saving: <<MSGNO PLACEHOLDER>>";
		Factory.Save();
		CombineAssertions("PlaceHolder is replaced only on first Factory.Save()", () =>
		{
			AssertNotContains("123456", interchange.EI_BodyText);
			AssertContains("<<MSGNO PLACEHOLDER>>", interchange.EI_BodyText);
		});
	}

	public void TestReplaceInterchangeBodyTextMessageNoPlaceHolderForNotManualProcedure()
	{
		var interchange = Factory.New<ITEDIInterchange>();
		var message = Factory.New<ITEDIMessage>();
		interchange.ContainedMessages.Add(message);

		interchange.EI_From = "F";
		interchange.EI_To = "T";
		interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
		interchange.EI_BodyText = "What follows ':' NOT to be replaced on saving: <<MSGNO PLACEHOLDER>>";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		Factory.Save();
		CombineAssertions("If not manual procedure, replacement doesn't happen", () =>
		{
			AssertNotContains("123456", interchange.EI_BodyText);
			AssertContains("<<MSGNO PLACEHOLDER>>", interchange.EI_BodyText);
		});
	}

	public void TestFillInPlaceHoldersWithNBMessages()
	{
		SetupFountainAndDeclarantAndAccountCollection();
		Factory.Save();

		var jobDeclaration = SetupDeclarationWithMultiplePreviousDocuments("IMP", numberOfEntryLines: 2);

		var nbMessage1FixedPart = "TNB           <<MSGNO PLACEHOLDER NB 1>>00";
		var nbMessageContinuation1FixedPart = "?NB1          <<MSGNO PLACEHOLDER NB 1>>00";
		var nbMessage2FixedPart = "TNB           <<MSGNO PLACEHOLDER NB 2>>00";
		var nbMessage2Continuation2FixedPart = "?NB1          <<MSGNO PLACEHOLDER NB 2>>00";

		var nbMessage1FixedPartAfterSave = "TNB           00000100";
		var nbMessageContinuation1FixedPartAfterSave = "?NB1          00000100";
		var nbMessage2FixedPartAfterSave = "TNB           00000200";
		var nbMessage2Continuation2FixedPartAfterSave = "?NB1          00000200";

		var entryHeader = jobDeclaration.CustomsEntryHeaders[0];

		var nbMessageSendingObject = new NBStandaloneMessageSendingObject(entryHeader, new JobDeclarationMessageSendingObjectParent(jobDeclaration));
		var generatedEdiMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, nbMessageSendingObject)).GenerateMessage();
		var serializedMessageText = generatedEdiMessage.EM_MessageText;
		generatedEdiMessage.Delete();
		AssertNotNullOrEmpty("Serialized message not null or empty", serializedMessageText);
		AssertEquals("Placeholder existing for header NB message 1", true, serializedMessageText.Contains(nbMessage1FixedPart));
		AssertEquals("Placeholder existing for continuation NB message 1", true, serializedMessageText.Contains(nbMessageContinuation1FixedPart));
		AssertEquals("Placeholder existing for header NB message 2", true, serializedMessageText.Contains(nbMessage2FixedPart));
		AssertEquals("Placeholder existing for continuation NB message 1", true, serializedMessageText.Contains(nbMessage2Continuation2FixedPart));

		new ITMessageSender(Factory, new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, nbMessageSendingObject), jobDeclaration.CustomsEntryHeaders[0]).Send();
		Factory.Save();

		var ediMessages = entryHeader.Messages;
		AssertNotNull("Messages should not be null", ediMessages);
		AssertEquals("Messages count should be", 1, ediMessages.Count);
		var savedMessage = ediMessages[0];
		var savedMessageText = savedMessage.EM_MessageText;
		AssertNotNullOrEmpty("Serialized message not null or empty", savedMessageText);
		AssertEquals("Placeholder NOT existing for header NB message 1", false, savedMessageText.Contains(nbMessage1FixedPart));
		AssertEquals("Placeholder NOT existing for continuation NB message 1", false, savedMessageText.Contains(nbMessageContinuation1FixedPart));
		AssertEquals("Placeholder NOT existing for header NB message 2", false, savedMessageText.Contains(nbMessage2FixedPart));
		AssertEquals("Placeholder NOT existing for continuation NB message 2", false, savedMessageText.Contains(nbMessage2Continuation2FixedPart));

		AssertEquals("Correct PAN existing for header NB message 1", true, savedMessageText.Contains(nbMessage1FixedPartAfterSave));
		AssertEquals("Correct PAN existing for line NB message 1", true, savedMessageText.Contains(nbMessageContinuation1FixedPartAfterSave));
		AssertEquals("Correct PAN existing for header NB message 2", true, savedMessageText.Contains(nbMessage2FixedPartAfterSave));
		AssertEquals("Correct PAN existing for line NB message 2", true, savedMessageText.Contains(nbMessage2Continuation2FixedPartAfterSave));
	}

	public void TestEM_MessageNum()
	{
		SetupFountainAndDeclarantAndAccountCollection();
		Factory.Save();

		var declaration = SetupDeclaration("IMP");
		var entryHeader = declaration.CustomsEntryHeaders[0];

		var imMessageSendingObject = new IMMessageSendingObject(entryHeader, new JobDeclarationMessageSendingObjectParent(declaration));
		new ITMessageSender(Factory, new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, imMessageSendingObject), entryHeader).Send();
		Factory.Save();

		var ediMessages = entryHeader.Messages;
		AssertNotNull("Messages should not be null", ediMessages);
		AssertEquals("Messages count should be", 1, ediMessages.Count);
		var savedMessage = ediMessages[0];

		AssertEquals("EM_MessageNum", "000001", savedMessage.EM_MessageNum);
	}

	public void TestEM_MessageNumWhenEntryHasMoreNBMessages()
	{
		SetupFountainAndDeclarantAndAccountCollection();
		Factory.Save();

		var jobDeclaration = SetupDeclarationWithMultiplePreviousDocuments("IMP", numberOfEntryLines: 1);
		var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
		var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(jobDeclaration);

		var nbStandaloneMessageSendingObject = new NBStandaloneMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent);
		new ITMessageSender(Factory, new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, nbStandaloneMessageSendingObject), jobDeclaration.CustomsEntryHeaders[0]).Send();
		Factory.Save();

		var ediMessages = entryHeader.Messages;
		AssertNotNull("Messages should not be null", ediMessages);
		AssertEquals("Messages count should be", 1, ediMessages.Count);
		var savedMessage = ediMessages[0];
		AssertEquals("EM_MessageNum", "000001", savedMessage.EM_MessageNum);

		jobDeclaration = SetupDeclarationWithMultiplePreviousDocuments("IMP", numberOfEntryLines: 5);
		entryHeader = jobDeclaration.CustomsEntryHeaders[0];

		nbStandaloneMessageSendingObject = new NBStandaloneMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent);
		new ITMessageSender(Factory, new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, nbStandaloneMessageSendingObject), jobDeclaration.CustomsEntryHeaders[0]).Send();
		Factory.Save();

		ediMessages = entryHeader.Messages;
		AssertNotNull("Messages should not be null", ediMessages);
		AssertEquals("Messages count should be", 1, ediMessages.Count);
		savedMessage = ediMessages[0];
		AssertEquals("EM_MessageNum", "000002:000006", savedMessage.EM_MessageNum);
	}

	public void TestEM_MessageInterpretation()
	{
		var ediMessage = Factory.New<ITEDIMessage>();
		ediMessage.EM_MessageText = "IDOC CONTENT";
		AssertEquals("Message Text has no manipulation", "IDOC CONTENT", ediMessage.EM_MessageInterpretation);
	}

	public void TestEM_Status()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var ediMessage = entryHeader.Messages.AddNew();

		ediMessage.EM_MessageType = "R";
		ediMessage.EM_Status = "FAL";
		AssertEquals("When EM_MessageType = 'R'. Setting EM_Status to 'FAL', CH_Status on parent EntryHeader", "FFT", entryHeader.CH_Status);

		ediMessage.EM_MessageType = "X";
		entryHeader.CH_Status = "ACO";
		ediMessage.EM_Status = "FAL";
		AssertEquals("When EM_MessageType = 'X'.Setting EM_Status to 'FAL', CH_Status on parent EntryHeader", "ACO", entryHeader.CH_Status);

		ediMessage.EM_MessageType = "R";
		entryHeader.CH_Status = "AWO";
		ediMessage.EM_Status = "QUE";
		AssertEquals("Setting EM_Status to 'QUE', CH_Status on parent EntryHeader", "AWO", entryHeader.CH_Status);

		var orphanEdiMessage = Factory.New<ITEDIMessage>();
		AssertNoExceptionThrown("Setting EM_Status in a EDIMessage with no parent", () => orphanEdiMessage.EM_Status = "FFT");
	}

	public void TestIsCancellation()
	{
		var message = Factory.New<ITEDIMessage>();

		CombineAssertions(() =>
		{
			AssertEquals("EM_Type = Empty, IsCancellation", false, message.IsCancellation);

			message.EM_MessageType = "CAN";
			AssertEquals("EM_Type = CAN, IsCancellation", true, message.IsCancellation);

			message.EM_MessageType = "NEW";
			AssertEquals("EM_Type = NEW, IsCancellation", false, message.IsCancellation);
		});
	}

	void SetupFountainAndDeclarantAndAccountCollection()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "11111111111", currentValue: 1, minimumValue: 1, maximumValue: 999999);

		Factory.NewWithValidTestData<OrgHeader>().OH_Code = "CODE1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-CODE1", "CODE1")
			.Build();
	}

	JobDeclaration SetupDeclaration(ZString messageType)
	{
		var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		jobDeclaration.JE_MessageType = messageType;
		jobDeclaration.JE_CustomsProfile = "1234-CODE1";
		jobDeclaration.JE_GS_NKCusAgent = "BBB";
		jobDeclaration.JE_CustomsOffice = "IT000000";
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = cusEntryHeader.MergedLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.PreviousDocuments.AddNew();
		invoiceLine.SupportingDocuments.AddNew();
		return jobDeclaration;
	}

	JobDeclaration SetupDeclarationWithMultiplePreviousDocuments(ZString messageType, int numberOfEntryLines)
	{
		var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		jobDeclaration.JE_MessageType = messageType;
		jobDeclaration.JE_CustomsProfile = "1234-CODE1";
		jobDeclaration.JE_GS_NKCusAgent = "BBB";
		jobDeclaration.JE_CustomsOffice = "IT000000";

		void AddPreviousDocuments(int referenceNumber)
		{
			var paDocument1 = jobDeclaration.PreviousDocuments.AddNew();
			paDocument1.CSI_Procedure = "A3";
			paDocument1.CSI_ReferenceNumber = $"{referenceNumber}PA-A";
			paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
			paDocument1.CSI_Status = "X";
			paDocument1.CSI_CustomsOffice = "IT137100";
			paDocument1.CSI_LineNo = 1;

			var paDocument2 = jobDeclaration.PreviousDocuments.AddNew();
			paDocument2.CSI_Procedure = "A3";
			paDocument2.CSI_ReferenceNumber = $"{referenceNumber}PA-B";

			var rpDocument1 = jobDeclaration.PreviousDocuments.AddNew();
			rpDocument1.CSI_Procedure = "2";
			rpDocument1.CSI_ReferenceNumber = $"{referenceNumber}RP";

			jobDeclaration.ResetApportionedPreviousDocuments();
		}

		for (int i = 0; i < 5; i++)
		{
			AddPreviousDocuments(i);
		}

		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		for (int i = 0; i < numberOfEntryLines; i++)
		{
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.ZG_NBStatus = "NBR";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.PreviousDocuments.AddNew();
			invoiceLine.SupportingDocuments.AddNew();
		}

		return jobDeclaration;
	}

	public void TestWrite()
	{
		const string headerTextContent = "Header text test";
		const string fileContent = "File content test";
		string headerText = $"<Header>{headerTextContent}</Header>";
		var filepath = Path.Combine(EnvProxy.Instance.TempPath, "11110000.R00");

		var interchange = Factory.New<ITEDIInterchange>();
		var message = Factory.New<ITEDIMessage>();
		message.EM_MessageText = fileContent;
		interchange.ContainedMessages.Add(message);
		interchange.EI_HeaderText = headerText;

		//Stream for IRisp
		using (var stream = new FileStream(filepath, FileMode.Create))
		{
			interchange.EI_InterchangeType = SADConstants.CustomsInterchangeType.IrispX;
			message.Write(stream);

			AssertEquals("File has been created", true, File.Exists(filepath));
			var result = File.ReadAllText(filepath);
			AssertEquals("File content for IRisp is correct", fileContent, result);
		}

		//Remove file
		File.Delete(filepath);
		AssertEquals("File has been deleted", false, File.Exists(filepath));

		//Stream for IDoc
		using (var stream = new FileStream(filepath, FileMode.Create))
		{
			interchange.EI_InterchangeType = SADConstants.CustomsInterchangeType.IdocR;
			message.Write(stream);

			AssertEquals("File has been created", true, File.Exists(filepath));
			var result = File.ReadAllText(filepath);
			AssertEquals("File content for IDoc is correct", headerTextContent + "\r\n" + fileContent, result);
		}

		File.Delete(filepath);
	}

	public void TestEM_MessageInterpretation_WhenHavePrettyFormatter()
	{
		const string expectedInterpretation = "<h3 style='display:inline'>Filename: </h3><p style='display:inline;color:'>02VM1004.R01</<p><br/><br/>" +
			"<h3 style='color:red;margin-left: 10pt'>Error 1</h3><br/>" +
			"<table style='margin-left: 20pt'>" +
			"<tr><td><p>Entry Line:</p></td><td>1</td></tr>" +
			"<tr><td><p>Field:</p></td><td>33.2 Additional Codes - Number of Occurrences</td></tr>" +
			"<tr><td><p>Occurrence:</p></td><td>0</td></tr>" +
			"<tr><td><p>Error Type:</p></td><td>L</td></tr>" +
			"<tr><td><p>Error Message:</p></td><td>0 - [Codice addizionale in input mancante]</td></tr>" +
			"</table><br/>";
		var irispMessage = Factory.New<ITEDIMessage>();
		irispMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IrispX;

		irispMessage.EM_MessageText = @"02VM            02VM1004.X01190015592475136101    007775D         001 00006    INVIO IN AMBIENTE REALE       
 RICEVUTO  04/10/19 08:07,02VM1004.R01
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   04/10/19  08:07
RIM          37507700136101N                                        
 013302 00L 00 Errore 0 : [Codice addizionale in input mancante]      ";

		AssertMultilineASCIIEquals(expectedInterpretation, irispMessage.EM_MessageInterpretation);
	}

	public void TestGetFileName()
	{
		var ediMessage = Factory.New<ITEDIMessage>();
		ediMessage.EM_ApplicationCode = "ITH";
		ediMessage.EM_MessageNum = "000001";

		var fileName = ediMessage.GetFileName();
		AssertEquals("File name", "EdiMessage_ITH_000001.txt", fileName);
	}

	public void TestGetFileName_WhenITEDIInterchange()
	{
		var ediMessage = Factory.New<ITEDIMessage>();
		ediMessage.EM_ApplicationCode = "ITH";
		ediMessage.EM_MessageNum = "000001";
		var interchange = Factory.New<ITEDIInterchange>();
		interchange.EI_HeaderText = "<ITMessage><Staff>BOB</Staff><Node>1234</Node><MessageType>R</MessageType><AccountNumber>11111111111-001</AccountNumber><Header>1234            845A1201.R02            137100    13149600150     003 00003</Header></ITMessage>";
		interchange.EI_ReceiveTransmit = "TRX";
		ediMessage.EM_EI = interchange.PK;

		var fileName = ediMessage.GetFileName();
		AssertEquals("File name", "845A1201.R02", fileName);
	}
}
