using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AWBMessageSender : BaseMessageSender
	{
		public AWBMessageSender(AsycudaBill bill, IHouseWaybill dataProvider, ZString messageSubType)
			: base(bill)
		{
			this.dataProvider = dataProvider;
			oldMessageStatus = bill.ABL_MessageStatus;
			oldBillStatus = bill.ABL_BillStatus;
			this.messageSubType = messageSubType;
		}

		readonly IHouseWaybill dataProvider;
		readonly ZString oldMessageStatus;
		readonly ZString oldBillStatus;
		readonly ZString messageSubType;

		protected override ZString MessageSubType => messageSubType;

		protected override ZString MessageType => MessageTypes.Codes.MXE;

		protected override ZString MessageText => MXMessageFormatting.FormatWithXMLRepresentation(((IXmlMessageBuilder)new AWBRequestMessageBuilder(dataProvider)).GenerateXmlMessage().GetSerializedString());

		protected override void SetStatus()
		{
			if (messageSubType == MessageSubTypeCodes.Codes.Original)
			{
				bill.ABL_BillStatus = CustomsStatusList.Codes.SNT;
			}
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
		}
		protected override void RollBackStatus()
		{
			bill.ABL_BillStatus = oldBillStatus;
			bill.ABL_MessageStatus = oldMessageStatus;
		}
	}
}
