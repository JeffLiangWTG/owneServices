using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class BLOriginalMessageSender : BaseMessageSender
	{
		public BLOriginalMessageSender(AsycudaBill bill, ISeaManifest dataProvider)
			: base(bill)
		{
			this.dataProvider = dataProvider;
			oldMessageStatus = bill.ABL_MessageStatus;
			oldBillStatus = bill.ABL_BillStatus;
		}
		readonly ISeaManifest dataProvider;
		readonly ZString oldMessageStatus;
		readonly ZString oldBillStatus;

		protected override ZString MessageType => MessageTypes.Codes.ARA;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Original;

		protected override ZString MessageText => BuildMessageText(new SeaRequestMessageBuilder(dataProvider));

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
