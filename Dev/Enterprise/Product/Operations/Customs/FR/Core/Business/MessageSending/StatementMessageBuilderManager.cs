using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessagesWrappers.DCG;
using Enterprise.Customs.FR.Messaging.Interfaces.DCG;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DCG;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class StatementMessageBuilderManager : MessageBuilderManager<StatementMessageSendingObject>
	{
		public StatementMessageBuilderManager(ErrorCollector errorCollector)
		{
			Argument.NotNull(errorCollector, nameof(errorCollector));
			this.errorCollector = errorCollector;
		}

		public override IMessageBuilderBase NewMessageBuilder(StatementMessageSendingObject objectToSend)
		{
			return new DCGSendMessageBuilder(GetDCGHeaderWrapper(objectToSend), errorCollector, TransactionTypes.Original);
		}

		public override ZString BuilderType => MessageTypeList.Codes.DCG;

		IDCG GetDCGHeaderWrapper(StatementMessageSendingObject objectToSend)
		{
			return new DCGStatementWrapper(objectToSend);
		}

		readonly ErrorCollector errorCollector;
	}
}
