using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE528MessageProcessorTest : IE428MessageProcessorTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC528C;

	protected override string BGMReference => "TestReference528";

	protected override string DeclarationMessageType => Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

	protected override string WCOTypeCode => WCoTypeCodes.ExportAcceptance;

	protected override string ExpectedEntryHeaderStatus => StatusNew.Accepted;

	protected override string ExpectedEntryHeaderEntryStatus => EntryStatusNew.MRNAllocated;

	public void TestProcessMessage_PhaseStatus511()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = DeclarationMessageType;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();	
		entryHeader.CH_BGMReference = BGMReference;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", EntryStatusNew.MRNAllocated, entryHeader.CH_EntryStatus);
			AssertEquals("CH_Status", StatusNew.Accepted, entryHeader.CH_Status);
		});
	}
}
