using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportLicenseMessageManagerTest : DeclarationMessageManagerTest
	{
		protected override ZString ExpectedMessageFriendlyName => MessageTypeList.Descriptions.LIC;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.ImportLicense;

		protected override ZString ExpectedEM_MessageType => MessageTypeList.Codes.LIC;

		protected override ZString ExpectedEM_ApplicationReference => ZString.Empty;

		protected override ZGuid ExpectedEM_GP => ZGuid.Empty;

		protected override string OriginalMessageType => ImportLicenseActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;

		protected override DeclarationMessageSendingObject CreateMessageSendingObject(CusEntryHeader entryHeader) => new ImportLicenseMessageSendingObject(entryHeader);
	}
}
