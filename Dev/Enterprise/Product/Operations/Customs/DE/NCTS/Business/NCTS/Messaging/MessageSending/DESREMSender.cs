using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DESREMSender : NctsHeaderSender
	{
		public DESREMSender(NctsHeaderMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override string MessageSubType => NctsMessageSubTypeList.Codes.DestinationMessage;

		protected override string LocalReferenceNumber => nctsHeader.LocalReferenceNumber;

		protected override string LogbookRegistrationNumber => nctsHeader.MovementReferenceNumber;

		protected override bool ShouldSetPhaseStatus(out ZString status)
		{
			status = NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
			return true;
		}

		protected override bool ShouldSetCustomsStatus(out ZString status)
		{
			status = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
			return true;
		}
	}
}
