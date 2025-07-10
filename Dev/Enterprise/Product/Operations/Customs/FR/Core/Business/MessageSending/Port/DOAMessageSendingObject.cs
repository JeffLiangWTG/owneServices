using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DOAMessageSendingObject : IMessageSendingObject
	{
		public DOAMessageSendingObject(object parent, DOADataObject doaDataObject)
		{
			this.messageOwner = Argument.NotNull(parent as IFRMessagesOwner, "MessageOwner");
			this.stmALogParent = Argument.NotNull(parent as IStmALogParent, "StmALogParent");
			this.doaDataObject = Argument.NotNull(doaDataObject, nameof(doaDataObject));
		}
		readonly IFRMessagesOwner messageOwner;
		readonly DOADataObject doaDataObject;
		readonly IStmALogParent stmALogParent;

		public IFRMessagesOwner MessagesOwner => messageOwner;

		public IStmALogParent StmALogParent => stmALogParent;

		public object DataSource => doaDataObject;

		public ZString MessageType => MessageSubTypeList.Codes.DOA;
	}
}
