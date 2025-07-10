using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DEPDATSender : NctsHeaderSender
	{
		public DEPDATSender(NctsHeaderMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override string MessageSubType => NctsMessageSubTypeList.Codes.DepartureMessage;

		protected override string LocalReferenceNumber => nctsHeader.MovementHeader.BM_PaperlessInbondNum;

		protected override string LogbookRegistrationNumber => nctsHeader.MovementReferenceNumber;

		protected override bool ShouldSetPhaseStatus(out ZString status)
		{
			status = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			return true;
		}

		protected override bool ShouldSetCustomsStatus(out ZString status)
		{
			status = ZString.Empty;
			return true;
		}

		protected override bool PreSend()
		{
			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(nctsHeader);

			NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, false);
			nctsHeader.MovementHeader.GuaranteeTransactionCoordinator.UpdatePendingTransactions(LocalReferenceNumber);

			return base.PreSend() && WarehouseNctsHeaderSender.PreSend(nctsHeader);
		}
	}
}
