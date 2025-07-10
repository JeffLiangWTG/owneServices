using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportSiscomexDeclarationMessageManagerTest : DeclarationMessageManagerTest
	{
		protected override ZString ExpectedMessageFriendlyName => MessageTypeList.Descriptions.LIS;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override ZString ExpectedEM_MessageType => MessageTypeList.Codes.CDI;

		protected override ZGuid ExpectedEM_GP => ZGuid.Empty;

		protected override string OriginalMessageType => ImportSiscomexActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;

		protected override DeclarationMessageSendingObject CreateMessageSendingObject(CusEntryHeader entryHeader) => new ImportSiscomexMessageSendingObject(entryHeader);
	}
}
