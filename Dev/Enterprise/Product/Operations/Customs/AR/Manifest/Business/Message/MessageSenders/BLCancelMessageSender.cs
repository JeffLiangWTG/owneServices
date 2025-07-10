using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class BLCancelMessageSender : BaseMessageSender
	{
		public BLCancelMessageSender(AsycudaBill bill, ICancellation dataProvider)
			: base(bill)
		{
			this.dataProvider = dataProvider;
			oldMessageStatus = bill.ABL_MessageStatus;
		}
		readonly ICancellation dataProvider;
		readonly ZString oldMessageStatus;

		protected override ZString MessageType => MessageTypes.Codes.ARA;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Cancellation;

		protected override ZString MessageText => BuildMessageText(new SeaCancellationMessageBuilder(dataProvider));

		protected override void SetStatus() => bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

		protected override void RollBackStatus()
		{
			bill.ABL_MessageStatus = oldMessageStatus;
		}
	}
}
