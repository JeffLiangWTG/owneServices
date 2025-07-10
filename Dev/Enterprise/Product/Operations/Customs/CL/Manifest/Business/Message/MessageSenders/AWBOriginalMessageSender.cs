using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AWBOriginalMessageSender : BaseMessageSender
	{
		public AWBOriginalMessageSender(AsycudaBill bill, IAWBRequest dataProvider) : base(bill)
		{
			this.dataProvider = dataProvider;
			oldMessageStatus = bill.ABL_MessageStatus;
			oldBillStatus = bill.ABL_BillStatus;
		}
		readonly IAWBRequest dataProvider;
		readonly ZString oldMessageStatus;
		readonly ZString oldBillStatus;

		protected override ZString MessageType => MessageTypes.Codes.CHE;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Original;

		protected override ZString MessageText => BuildMessageText(new AWBRequestMessageBuilder(dataProvider));

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

