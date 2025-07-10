using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DLO.REQ_120.MN_NG_1200_MSG2_DeliveryOrder_Message;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.IL.Business.Message.PrettiedCaptions;

namespace Enterprise.Customs.IL.Business
{
	public class ILDLO120RequestMessagePrettier : ILEDIMessagePrettierBase<MnNg1200Msg2DeliveryOrderMessage>
	{
		public ILDLO120RequestMessagePrettier(MessageDataObject<MnNg1200Msg2DeliveryOrderMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData.DeliveryOrder.Single(),
				builder =>
				builder.WithRequestSection((deliveryOrder, sb) =>
				{
					sb.Append(GetTable(deliveryOrder));
				})
			)
			.Build();

		public ZString GetTable(MnNg1200Msg2DeliveryOrderMessageDeliveryOrder deliveryOrder)
		{
			var creator = new HtmlTableCreator();

			var columnWidthTitleCell = 300;
			var columnWidthValueCell = 500;

			creator.WriteRowWithFormatting(
				CreateCell(DeliveryOrder.DeliveryOrderNumber, true, columnWidthTitleCell),
				CreateCell(deliveryOrder.Header?.DeliveryOrderNumber.ToString(), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(DeliveryOrder.RequestDate, true, columnWidthTitleCell),
				CreateCell(ConvertNullableDateTimeToString(deliveryOrder.Header?.DeliveryOrderDate), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(DeliveryOrder.ActionCode, true, columnWidthTitleCell),
				CreateCell(deliveryOrder.Header?.ActionTypeCode == 2 ? DeliveryOrder.ActionNew : DeliveryOrder.ActionCancel, false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(DeliveryOrder.ReceiverVAT, true, columnWidthTitleCell),
				CreateCell(ConvertNullableIntToString(deliveryOrder.ReceiverDetails?.CustomerIdentification?.ExternalId), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(DeliveryOrder.ReceiverName, true, columnWidthTitleCell),
				CreateCell(deliveryOrder.ReceiverDetails?.ReceiverName, false, columnWidthValueCell));

			return creator.ToHtml();
		}
	}
}

