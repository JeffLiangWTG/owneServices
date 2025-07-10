using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC013CSender : NCTSMessageSender<ICC013C>
	{
		public CC013CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		protected override ICC013C GetDataProvider(BusinessObject messageObject) => new CC013CHeaderProvider((NctsHeader)messageObject);

		protected override IXmlMessageBuilder GetProducer(ICC013C dataProvider) => new CC013CMessageBuilder(dataProvider);

		protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Amendment;

		protected override void PreSendCore()
		{
			var nctsHeader = (NctsHeader)MessageObject;
			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(nctsHeader);
			NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, true);
		}

		protected override void PostSendCore(BEMessage newMessage)
		{
			CreateGuaranteeTransactions(newMessage);
		}

		void CreateGuaranteeTransactions(BEMessage message)
		{
			var movementHeader = ((NctsHeader)MessageObject).MovementHeader;
			movementHeader.GuaranteeTransactionCoordinator.UpdatePendingTransactions(message.EM_MessageNum);
		}
	}
}
