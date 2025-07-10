using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AWBOriginalMessageSender : BaseMessageSender
	{
		public AWBOriginalMessageSender(AsycudaBill bill, IHouseWaybill dataProvider)
			: base(bill)
		{
			this.dataProvider = dataProvider;
			oldMessageStatus = bill.ABL_MessageStatus;
			oldBillStatus = bill.ABL_BillStatus;
		}
		readonly IHouseWaybill dataProvider;
		readonly ZString oldMessageStatus;
		readonly ZString oldBillStatus;

		protected override ZString MessageType => MessageTypes.Codes.ARD;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Original;

		protected override ZString MessageText => BuildMessageText(new AirRequestMessageBuilder(dataProvider));

		protected override void SetStatus()
		{
			bill.ABL_BillStatus = CustomsStatusList.Codes.SNT;
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
		}

		protected override void RollBackStatus()
		{
			bill.ABL_BillStatus = oldBillStatus;
			bill.ABL_MessageStatus = oldMessageStatus;
		}
	}
}
