using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class MessageSendingEntryLineObjectValidation : AutoMessageSendingEntryLineObjectValidation
	{
		public MessageSendingEntryLineObjectValidation(AutoMessageSendingEntryLineObject parent) : base(parent)
		{
		}
		new MessageSendingEntryLineObject Parent => (MessageSendingEntryLineObject)base.Parent;

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();

			if (Parent.ShouldSend && Parent.Entry != null)
			{
				if (Parent.MessageType == ElectronicDocumentTypeList.Codes._5FN)
				{
					Parent.ShouldSendInfo.CheckShouldSendCore(Parent.Entry, Parent.MessageType, x => x.CE_EntryLineReference == Parent.EntryLineNo.ToString());

					if (!Parent.BizObjValidationMessageErrors.IsEmpty)
					{
						Parent.ShouldSendInfo.CheckShouldSendWithMessageErrors(Res.GetString("d1006e98-98a8-49ef-84c4-f1e579c6f0af", "entry line"));
					}
				}
			}
		}
	}
}
