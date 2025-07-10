using System;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AESMessageSendingAction : CusEntryHeaderMessageSendingAction
	{
		public AESMessageSendingAction(CusEntryHeader cusEntryHeader) : base(cusEntryHeader) { }

		protected override bool Annotation_ReadOnly => MessageType != AESOutgoingMessageTypeList.Codes.ExportCancellation && MessageType != AESOutgoingMessageTypeList.Codes.ExitCancellation;

		protected override Type SenderType => typeof(AESMessageSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups()
		{
			return new AESMessageSendingActionLookups(this);
		}

		public new AESMessageSendingActionValidation Validation => (AESMessageSendingActionValidation)base.Validation;

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation()
		{
			return new AESMessageSendingActionValidation(this);
		}
	}
}
