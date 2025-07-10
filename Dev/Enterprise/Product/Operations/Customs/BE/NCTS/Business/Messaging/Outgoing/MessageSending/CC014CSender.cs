using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC014CSender : NCTSMessageSender<ICC014C>
	{
		public CC014CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		protected override ICC014C GetDataProvider(BusinessObject messageObject) => new CC014CProvider(messageSendingAction);

		protected override IXmlMessageBuilder GetProducer(ICC014C dataProvider) => new CC014CMessageBuilder(dataProvider);

		protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
	}
}
