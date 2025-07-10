using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AutomaticProcedureMessageSender_JobDeclarationTest : ITMessageSender_JobDeclarationTest
{
	protected override ZString ExpectedMessageStatus => EDIMessageStatusList.Codes.Queued;

	protected override ZString ExpectedMessageType => SADConstants.CustomsInterchangeType.IdocR;

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject)
		=> new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject);
}

sealed class FallbackProcedureMessageSender_JobDeclarationTest : ITMessageSender_JobDeclarationTest
{
	protected override ZString ExpectedMessageStatus => EDIMessageStatusList.Codes.Manual;

	protected override ZString ExpectedMessageType => Fallback;

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject)
		=> new FallbackProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject);

	const string Fallback = "FBK";
}

sealed class ManualProcedureMessageSender_JobDeclarationTest : ITMessageSender_JobDeclarationTest
{
	protected override ZString ExpectedMessageStatus => EDIMessageStatusList.Codes.Manual;

	protected override ZString ExpectedMessageType => SADConstants.CustomsInterchangeType.IdocR;

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject)
		=> new ManualProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject);
}

abstract class ITMessageSender_JobDeclarationTest : TestCaseWithFactory
{
	[TestDate(2019, 11, 25)]
	public void TestSendImportMessageSuccessfully_WithoutM2Lines()
	{
		var declaration = SetupDeclaration("IMP");
		declaration.ResetApportionedPreviousDocuments();
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_BGMReference = "A0001";
		var entryLine = entryHeader.MergedLines[0];
		Assert("PRE-CONDITION", !entryLine.GroupedPreviousDocuments.Any());

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var declarantTaxNumber = "11111111111";
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, declarantTaxNumber, currentValue: 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		Factory.Save();

		var messageResult = SendMessage(entryHeader);
		Factory.Save();

		AssertNotNull("Message sent result", messageResult);
		AssertEquals("Entry Header status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		AssertEquals("Entry Line status", "", entryLine.ZG_NBStatus);
		CheckEntryHeaderMessagesCollectionUsingStandardProcedure(entryHeader);
		Assert("JobDeclaration should not have status override log", !declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO"));
	}

	public void TestSendImportMessageWhenStatusIsAwo()
	{
		var declaration = SetupDeclaration("IMP");
		declaration.ResetApportionedPreviousDocuments();
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_BGMReference = "A0001";
		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "ICC";
		var entryLine = entryHeader.MergedLines[0];
		Assert("PRE-CONDITION", !entryLine.GroupedPreviousDocuments.Any());

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var declarantTaxNumber = "11111111111";
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, declarantTaxNumber, currentValue: 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		Factory.Save();

		var messageResult = SendMessage(entryHeader);
		Factory.Save();

		AssertNotNull("Message sent result", messageResult);
		AssertEquals("Entry Header status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		AssertEquals("Entry Line status", "", entryLine.ZG_NBStatus);
		CheckEntryHeaderMessagesCollectionUsingStandardProcedure(entryHeader);
		Assert("JobDeclaration should Have status override log", declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference == "Entry A0001 Sent in status: ACO, ICC"));
	}

	[TestDate(2020, 01, 01)]
	public void TestSendImportMessageSuccessfully_WithM2Lines()
	{
		var declaration = SetupDeclaration("IMP");
		declaration.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		declaration.PreviousDocuments.AddNew().CSI_Procedure = "A44";
		declaration.ResetApportionedPreviousDocuments();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine1 = entryHeader.MergedLines[0];
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices[0];
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		declaration.ResetApportionedPreviousDocuments();

		CombineAssertions("[PRE-CONDITION] M2 Lines", () =>
		{
			Assert("M2 Lines in Entry Line1", entryLine1.GroupedPreviousDocuments.Any());
			Assert("M2 Lines in Entry Line2", entryLine2.GroupedPreviousDocuments.Any());
		});

		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "NBR";
		entryLine1.ZG_NBStatus = "NBA";
		entryLine2.ZG_NBStatus = "NBR";

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var declarantTaxNumber = "11111111111";
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, declarantTaxNumber, currentValue: 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		Factory.Save();

		var messageResult = SendMessage(entryHeader);
		Factory.Save();

		AssertNotNull("Message sent result", messageResult);
		AssertEquals("Entry Header status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		CombineAssertions("Check ZG_Customs status", () =>
		{
			AssertEquals("Entry Line 1 status", EntryLineCustomsStatusList.Codes.Approved, entryLine1.ZG_NBStatus);
			AssertEquals("Entry Line 1 status", EntryLineCustomsStatusList.Codes.Sent, entryLine2.ZG_NBStatus);
		});
		CheckEntryHeaderMessagesCollectionUsingStandardProcedure(entryHeader);
		Assert("JobDeclaration should not have status override log", !declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO"));
	}

	[TestDate(2020, 01, 01)]
	public void TestSendImportMessageSuccessfullyCloningLastYearNumberRange()
	{
		var declaration = SetupDeclaration("IMP");
		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine = entryHeader.MergedLines[0];

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var tempFactory = new BusinessObjectFactory();
		var tempCompany = tempFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(tempCompany, 2019, "11111111111", currentValue: 10000, minimumValue: 1, maximumValue: 10000);
		tempFactory.Save();
		company.Reload();

		var messageResult = SendMessage(entryHeader);
		Factory.Save();

		AssertNotNull("Message sent result", messageResult);
		AssertEquals("Entry Header status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
		AssertEquals("Entry Line status", "", entryLine.ZG_NBStatus);
		CheckEntryHeaderMessagesCollectionUsingStandardProcedure(entryHeader);
	}

	protected JobDeclaration SetupDeclaration(ZString messageType)
	{
		var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		jobDeclaration.JE_MessageType = messageType;
		jobDeclaration.JE_CustomsProfile = "1234-DEC1";
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
		return jobDeclaration;
	}

	ITEDIMessage SendMessage(CusEntryHeader entryHeader)
	{
		var sendingObject = new IMMessageSendingObject(entryHeader, new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration));
		var messageSender = new ITMessageSender(Factory, GetMessageGenerator(sendingObject), entryHeader);
		return messageSender.Send();
	}

	protected abstract IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject);

	protected abstract ZString ExpectedMessageStatus { get; }

	protected abstract ZString ExpectedMessageType { get; }

	void CheckEntryHeaderMessagesCollectionUsingStandardProcedure(CusEntryHeader entryHeader) => CheckEntryHeaderMessagesCollection(entryHeader, ExpectedMessageStatus, ExpectedMessageType);

	void CheckEntryHeaderMessagesCollection(CusEntryHeader entryHeader, ZString messageStatus, ZString messageType)
	{
		var messages = entryHeader.Messages;
		AssertNotNull("Messages not null", messages);
		AssertEquals("One message expected", 1, messages.Count);

		CombineAssertions(() =>
		{
			var message = messages[0];
			AssertNotEquals("Message Text", "", message.EM_MessageText);
			AssertEquals("Application Code", "ITM", message.EM_ApplicationCode);
			AssertEquals("Status", messageStatus, message.EM_Status);
			AssertEquals("Receive", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("Sub Type", "IM", message.EM_MessageSubType);
			AssertEquals("Type", messageType, message.EM_MessageType);
			AssertEquals("Application Reference", "1234:BBB:IT000000", message.EM_ApplicationReference);
			AssertEquals("Message Num", "000001", message.EM_MessageNum);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
	}
}
