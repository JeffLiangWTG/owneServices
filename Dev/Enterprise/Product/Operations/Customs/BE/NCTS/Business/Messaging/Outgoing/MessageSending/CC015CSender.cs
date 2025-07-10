using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC015CSender : NCTSMessageSender<ICC015C>
	{
		public CC015CSender(MessageSendingAction messageSendingAction) : base(messageSendingAction)
		{
		}

		protected override ICC015C GetDataProvider(BusinessObject messageObject) => new CC015CHeaderProvider((NctsHeader)messageObject);

		protected override IXmlMessageBuilder GetProducer(ICC015C dataProvider) => new CC015CMessageBuilder(dataProvider);

		protected override ZString NewPhase => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override void PreSendCore()
		{
			var nctsHeader = (NctsHeader)MessageObject;
			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(nctsHeader);
			NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, false);
		}

		protected override void PostSendCore(BEMessage newMessage)
		{
			var nctsHeader = (NctsHeader)MessageObject;

			CreateGuaranteeTransaction(newMessage);

			if (nctsHeader.IsPhase5Departure && messageSendingAction.HasRepresentative)
			{
				CreateAdditionalReferenceForCBRNumer(nctsHeader, messageSendingAction.RepresentativeCBRNumber);
			}
		}

		void CreateGuaranteeTransaction(BEMessage message)
		{
			var movementHeader = ((NctsHeader)MessageObject).MovementHeader;
			movementHeader.GuaranteeTransactionCoordinator.CreatePendingTransactions(message.EM_MessageNum);
		}

		void CreateAdditionalReferenceForCBRNumer(NctsHeader nctsHeader, ZString reference)
		{
			var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = Constants.CusSupportingInfoSubTypes.AdditionalReference;
			additionalDocument.CSI_Code = Constants.AdditionalDocumentTypes.CBRNumber;
			additionalDocument.CSI_ReferenceNumber = reference;
		}
	}
}
