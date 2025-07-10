using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.MessagesWrappers.DOA;
using Enterprise.Customs.FR.Messaging.Interfaces.DOA;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Messaging.MessageBuilders.DOA;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DOAMessageBuilderManager : MessageBuilderManager<DOAMessageSendingObject>
	{
		public DOAMessageBuilderManager(ErrorCollector errorCollector)
		{
			this.errorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
		}
		readonly ErrorCollector errorCollector;

		public override ZString BuilderType => MessageTypeList.Codes.POR;

		public override IMessageBuilderBase NewMessageBuilder(DOAMessageSendingObject objectToSend)
		{
			return new DOASendMessageBuilder(GetDOAWrapper(objectToSend), errorCollector, TransactionTypes.Original);
		}

		IDOA GetDOAWrapper(DOAMessageSendingObject objectToSend)
		{
			return new DOAWrapper((DOADataObject)objectToSend.DataSource);
		}
	}
}
