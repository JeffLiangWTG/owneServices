using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class JobDeclarationMessageSendingObject : EU.Business.JobDeclarationMessageSendingObject, IMessageSendingObject
	{
		public JobDeclarationMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public new Declaration.CusEntryHeader Header => (Declaration.CusEntryHeader)base.Header;

		public Declaration.JobDeclaration Declaration => Header.Declaration;

		protected bool IsOneAndOnlyEntry => Header.Declaration?.ActiveEntryHeaders.Count == 1;

		public virtual bool IsNew => MessageType == EntryActionCodeList.Codes.VAL || MessageType == EntryActionCodeList.Codes.ANT;

		#region
		IFRMessagesOwner IMessageSendingObject.MessagesOwner => Header;

		object IMessageSendingObject.DataSource => Header;

		#endregion
	}
}
