using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC007CSender : NCTSMessageSender<ICC007C>
	{
		public CC007CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		protected override ICC007C GetDataProvider(BusinessObject messageObject) => new CC007CProvider((NctsHeader)messageObject);

		protected override IXmlMessageBuilder GetProducer(ICC007C dataProvider) => new CC007CMessageBuilder(dataProvider);

		protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Arrival;
	}
}
