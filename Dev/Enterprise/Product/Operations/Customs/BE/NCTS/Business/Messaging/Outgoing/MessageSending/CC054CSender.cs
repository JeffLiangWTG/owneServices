using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC054CSender : NCTSMessageSender<ICC054C>
	{
		public CC054CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		protected override ICC054C GetDataProvider(BusinessObject messageObject) => new CC054CProvider((NctsHeader)messageObject, messageSendingAction.AgreeWithMinorDiscrepancies);

		protected override IXmlMessageBuilder GetProducer(ICC054C dataProvider) => new CC054CMessageBuilder(dataProvider);

		protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
