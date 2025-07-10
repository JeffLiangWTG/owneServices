using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class BLAmendMessageSender : BaseMessageSender
	{
		public BLAmendMessageSender(AsycudaBill bill, ISeaManifest dataProvider)
			: base(bill)
		{
			this.dataProvider = dataProvider;
			oldMessageStatus = bill.ABL_MessageStatus;
		}
		readonly ISeaManifest dataProvider;
		readonly ZString oldMessageStatus;

		protected override ZString MessageType => MessageTypes.Codes.ARA;

		protected override ZString MessageSubType => MessageSubTypeCodes.Codes.Change;

		protected override ZString MessageText => BuildMessageText(new SeaAmendmentMessageBuilder(dataProvider));

		protected override void SetStatus() => bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

		protected override void RollBackStatus() => bill.ABL_MessageStatus = oldMessageStatus;
	}
}
