using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CSender : NCTSMessageSender<ICC044C>
	{
		public CC044CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		protected override void PreSendCore()
		{
			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(messageSendingAction.Header);
		}

		protected override ICC044C GetDataProvider(BusinessObject messageObject) => new CC044CProvider((NctsHeader)messageObject);

		protected override IXmlMessageBuilder GetProducer(ICC044C dataProvider) => new CC044CMessageBuilder(dataProvider);

		protected override ZString NewCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;

		protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
	}
}
