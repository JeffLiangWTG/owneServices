using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusStatement;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class StatementMessageSender : MessageSender<StatementMessageSendingObject>
	{
		public StatementMessageSender(StatementMessageSendingObject objectToSend, ErrorCollector errorCollector) : base(objectToSend, errorCollector)
		{
			statement = Argument.NotNull(objectToSend.Statement, nameof(statement));
		}

		protected override MessageBuilderManager<StatementMessageSendingObject> GetBuilderManager()
		{
			return new StatementMessageBuilderManager(errorCollector);
		}

		protected override bool PreSend()
		{
			originalStatus = statement.B2_Status;
			return true;
		}

		protected override void PreCreateEdiMessage()
		{
			statement.B2_Status = StatementStatusList.Codes.PendingResponse;
		}

		protected override void PostSend(ZBool result)
		{
			if (!result)
			{
				statement.B2_Status = originalStatus;
			}
		}

		readonly CusStatementHeader statement;
		ZString originalStatus;
	}
}
