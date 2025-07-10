using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AutoIM415MessageProcessor : IEAutoSendCustomsMessageProcessor
	{
		public AutoIM415MessageProcessor(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString MessageDescription => AISOutgoingMessageTypeListForDisplay.Descriptions.CustomsDeclaration;

		protected override string MessageType => AISOutgoingMessageTypeList.Codes.CustomsDeclaration;

		protected override CusEntryHeaderMessageSendingAction GetSendingAction(Declaration.CusEntryHeader entryHeader) => new AISUCC5MessageSendingAction(entryHeader);
	}
}
