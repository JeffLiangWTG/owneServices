using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryHeaderAsISendableCustomsEntryTest : TestCaseWithFactory
{
	public void TestPreProcessBeforeSending()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var sendableEntry = (ISendableCustomsEntry)entryHeader;
		AssertNoExceptionThrown("Doing nothing and should not throw any exception", () => sendableEntry.PreProcessBeforeSending());
	}

	public void TestConsumeGuarantee()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var sendableEntry = (ISendableCustomsEntry)entryHeader;
		AssertNoExceptionThrown("Doing nothing and should not throw any exception", () => sendableEntry.ConsumeGuarantee(entryHeader.Factory, Factory.NewWithValidTestData<ITEDIMessage>()));
	}

	public void TestMarksAsSentSimpleEntry()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		((ISendableCustomsEntry)entryHeader).MarkAsSent(new Mock<IMessageType>().Object);

		CombineAssertions("POST-CONDITION", () =>
		{
			AssertEquals("CH_Status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
			AssertEquals("Has Status Override Log", false, declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference == "Entry A0001 Sent in status: ACO, ICC"));
		});
	}

	public void TestMarksAsSentContainingSendableNbLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		declaration.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		declaration.PreviousDocuments.AddNew().CSI_Procedure = "A44";

		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.ZG_NBStatus = EntryLineCustomsStatusList.Codes.Approved;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.ZG_NBStatus = EntryLineCustomsStatusList.Codes.Rejected;

		((ISendableCustomsEntry)entryHeader).MarkAsSent(new Mock<IMessageType>().Object);
		CombineAssertions("POST-CONDITION", () =>
		{
			AssertEquals("CH_Status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
			AssertEquals("ZG_NBStatus on the approved entry line", EntryLineCustomsStatusList.Codes.Approved, entryLine1.ZG_NBStatus);
			AssertEquals("ZG_NBStatus of the sent entry line", EntryLineCustomsStatusList.Codes.Sent, entryLine2.ZG_NBStatus);
		});
	}

	public void TestMarksAsSentStatusOverrideEvent()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "A0001";
		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "ICC";
		var iSendableCustomsEntry = (ISendableCustomsEntry)entryHeader;

		iSendableCustomsEntry.MarkAsSent(new Mock<IMessageType>().Object);

		CombineAssertions("POST-CONDITION", () =>
		{
			AssertEquals("CH_Status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
			AssertEquals("Has Status Override Log", true, declaration.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference == "Entry A0001 Sent in status: ACO, ICC"));
		});
	}

	public void TestMarksAsSent_WhenMessageInCancellation()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var sendableCustomsEntry = entryHeader as ISendableCustomsEntry;
		var cancellationMessage = entryHeader.Messages.AddNew();
		cancellationMessage.EM_MessageType = EDIMessageTypeList.Codes.Cancellation;

		sendableCustomsEntry.MarkAsSent(cancellationMessage);

		CombineAssertions(() =>
		{
			AssertEquals("CH_Status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
			AssertEquals("CH_EntryStatus", ITEntryStatusList.Codes.Canceling, entryHeader.CH_EntryStatus);
		});
	}

	public void TestMarksAsSent_WhenArgumentIsNull()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var sendableCustomsEntry = entryHeader as ISendableCustomsEntry;

		sendableCustomsEntry.MarkAsSent(null);

		CombineAssertions(() =>
		{
			AssertEquals("CH_Status", Common.Shared.MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);
			AssertEquals("CH_EntryStatus", "", entryHeader.CH_EntryStatus);
		});
	}
}
