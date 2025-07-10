using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AutoIM515MessageProcessor : IEAutoSendCustomsMessageProcessor
	{
		public AutoIM515MessageProcessor(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString MessageDescription => AESOutgoingMessageTypeListForDisplay.Descriptions.ExportOriginal;

		protected override string MessageType => AESOutgoingMessageTypeList.Codes.ExportOriginal;

		protected override CusEntryHeaderMessageSendingAction GetSendingAction(Declaration.CusEntryHeader entryHeader) => new AESMessageSendingAction(entryHeader);
	}
}
