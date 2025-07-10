using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DESNOTSender : NctsHeaderSender
	{
		public DESNOTSender(NctsHeaderMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected override string MessageSubType => NctsMessageSubTypeList.Codes.DestinationMessage;

		protected override string LocalReferenceNumber => nctsHeader.LocalReferenceNumber;

		protected override string LogbookRegistrationNumber => nctsHeader.MovementReferenceNumber;

		protected override bool ShouldSetPhaseStatus(out ZString status)
		{
			status = NctsMovementHeaderTransactionStatusList.Codes.Arrival;
			return true;
		}
	}
}
