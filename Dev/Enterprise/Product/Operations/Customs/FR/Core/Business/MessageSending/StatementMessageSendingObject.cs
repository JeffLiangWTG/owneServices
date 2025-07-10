using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusStatement;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class StatementMessageSendingObject : IMessageSendingObject
	{
		public StatementMessageSendingObject(CusStatementHeader statement)
		{
			this.statement = Argument.NotNull(statement, nameof(statement));
		}

		public CusStatementHeader Statement => statement;

		IFRMessagesOwner IMessageSendingObject.MessagesOwner => statement;

		object IMessageSendingObject.DataSource => statement;

		ZString IMessageSendingObject.MessageType => MessageSubTypeList.Codes.DCG;

		readonly CusStatementHeader statement;
	}
}
