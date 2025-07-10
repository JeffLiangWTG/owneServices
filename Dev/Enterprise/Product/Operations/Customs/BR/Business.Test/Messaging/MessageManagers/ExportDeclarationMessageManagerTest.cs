using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ExportDeclarationMessageManagerTest : DeclarationMessageManagerTest
	{
		protected override ZString ExpectedMessageFriendlyName => MessageTypeList.Descriptions.CDE;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.Export;

		protected override ZString ExpectedEM_MessageType => MessageTypeList.Codes.CDE;

		protected override string OriginalMessageType => ExportEntryActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => ExportEntryActionCodeList.Codes.RET;

		protected override string WithdrawalMessageType => null;

		protected override DeclarationMessageSendingObject CreateMessageSendingObject(CusEntryHeader entryHeader) => new ExportDeclarationMessageSendingObject(entryHeader);

		public new void TestHasActiveMessages()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entryHeader.EntryNumber = "123";
			entryHeader.CH_EntryStatus = Constants.EntryStatus.Registered;

			var messageObject = new ExportDeclarationMessageManager(new ExportDeclarationMessageSendingObject(entryHeader));

			Assert("HasActiveMessages should be True", messageObject.HasActiveMessages);

			entryHeader.EntryNumber = ZString.Empty;
			entryHeader.CH_EntryStatus = ZString.Empty;

			Assert("HasActiveMessages should be True", messageObject.HasActiveMessages);

			entryHeader.CH_Status = ZString.Empty;

			AssertEquals("HasActiveMessages should be false", false, messageObject.HasActiveMessages);

			entryHeader.EntryNumber = "123";
			entryHeader.CH_EntryStatus = Constants.EntryStatus.Registered;

			Assert("HasActiveMessages should be true", messageObject.HasActiveMessages);
		}

		protected override CusEntryHeader CreateEntryHeader()
		{
			var entryHeader = base.CreateEntryHeader();
			entryHeader.EntryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			return entryHeader;
		}
	}
}
