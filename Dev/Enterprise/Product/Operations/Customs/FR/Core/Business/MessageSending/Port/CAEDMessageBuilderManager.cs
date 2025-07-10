using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Business.MessagesWrappers.CAED;
using Enterprise.Customs.FR.Messaging.Interfaces.CAED;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Messaging.MessageBuilders.CAED;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class CAEDMessageBuilderManager : MessageBuilderManager<CAEDMessageSendingObject>
	{
		public CAEDMessageBuilderManager(ErrorCollector errorCollector)
		{
			this.errorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
		}
		readonly ErrorCollector errorCollector;

		public override ZString BuilderType => MessageTypeList.Codes.POR;

		public override IMessageBuilderBase NewMessageBuilder(CAEDMessageSendingObject objectToSend)
		{
			return new CAEDSendMessageBuilder(GetCAEDWrapper(objectToSend), errorCollector, TransactionTypes.Original);
		}

		ICAED GetCAEDWrapper(CAEDMessageSendingObject objectToSend)
		{
			return new CAEDWrapper((CAEDDataObject)objectToSend.DataSource);
		}
	}
}
