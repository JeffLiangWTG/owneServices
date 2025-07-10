using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC141CSender : NCTSMessageSender<ICC141C>
	{
		public CC141CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		protected override ICC141C GetDataProvider(BusinessObject messageObject) => new CC141CProvider(messageSendingAction);

		protected override IXmlMessageBuilder GetProducer(ICC141C dataProvider) => new CC141CMessageBuilder(dataProvider);

		protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.NonArrivedMovement;
	}
}
