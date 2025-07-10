using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class CAEDMessageSendingObject : IMessageSendingObject
	{
		public CAEDMessageSendingObject(object parent, CAEDDataObject caedDataObject)
		{
			this.messageOwner = Argument.NotNull(parent as IFRMessagesOwner, "MessageOwner");
			this.stmALogParent = Argument.NotNull(parent as IStmALogParent, "StmALogParent");
			this.cAEDDataObject = Argument.NotNull(caedDataObject, nameof(caedDataObject));
		}
		readonly IFRMessagesOwner messageOwner;
		readonly CAEDDataObject cAEDDataObject;
		readonly IStmALogParent stmALogParent;

		public IFRMessagesOwner MessagesOwner => messageOwner;

		public IStmALogParent StmALogParent => stmALogParent;

		public object DataSource => cAEDDataObject;

		public ZString MessageType => MessageSubTypeList.Codes.CAED;
	}
}
