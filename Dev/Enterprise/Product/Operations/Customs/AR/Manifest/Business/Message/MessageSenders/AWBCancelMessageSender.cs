using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AWBCancelMessageSender : BaseMessageSender
	{
		public AWBCancelMessageSender(AsycudaBill bill, IHouseWaybill dataProvider)
			: base(bill)
		{
			this.dataProvider = dataProvider;
			oldMessageStatus = bill.ABL_MessageStatus;
		}
		readonly IHouseWaybill dataProvider;
		readonly ZString oldMessageStatus;

		protected override ZString MessageType => MessageTypes.Codes.ARD;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Cancellation;

		protected override ZString MessageText => BuildMessageText(new AirRequestMessageBuilder(dataProvider));

		protected override void SetStatus() => bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

		protected override void RollBackStatus() => bill.ABL_MessageStatus = oldMessageStatus;
	}
}
