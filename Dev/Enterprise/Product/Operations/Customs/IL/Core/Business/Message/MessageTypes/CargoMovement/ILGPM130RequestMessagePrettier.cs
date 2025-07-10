using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.GPM.REQ_130.GP_NG_1030_MSG1_GatepassRequestMessage;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.IL.Business.Message.PrettiedCaptions;

namespace Enterprise.Customs.IL.Business
{
	sealed class ILGPM130RequestMessagePrettier : ILEDIMessagePrettierBase<GpNg1030Msg1GatepassRequestMessage>
	{
		public ILGPM130RequestMessagePrettier(MessageDataObject<GpNg1030Msg1GatepassRequestMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData.GatepassRequestMessage.Single(),
				builder =>
				builder.WithRequestSection((gatepassRequestMessage, sb) =>
				{
					sb.Append(GetTable(gatepassRequestMessage));
				})
			)
			.Build();

		public ZString GetTable(GpNg1030Msg1GatepassRequestMessageGatepassRequestMessage gatepassRequestMessage)
		{
			var creator = new HtmlTableCreator();
			var factory = Factory;

			const int columnWidthTitleCell = 300;
			const int columnWidthValueCell = 500;

			creator.WriteRowWithFormatting(
				CreateCell(GatePassMovement.GatePassNumber, true, columnWidthTitleCell),
				CreateCell(gatepassRequestMessage.GatepassNumber.ToString(), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(GatePassMovement.RequestDate, true, columnWidthTitleCell),
				CreateCell(ConvertNullableDateTimeToString(gatepassRequestMessage.RequestDate), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(GatePassMovement.UpdateCode, true, columnWidthTitleCell),
				CreateCell(gatepassRequestMessage.UpdateCode == 1 ? GatePassMovement.ActionNew : GatePassMovement.ActionCancel, false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(GatePassMovement.OriginSiteCode, true, columnWidthTitleCell),
				CreateCell(factory.GetRefUnloco(gatepassRequestMessage.OriginSiteCode), false, columnWidthValueCell));

			if (gatepassRequestMessage.GatepassDestinationSiteSpecified)
			{
				creator.WriteRowWithFormatting(
					CreateCell(GatePassMovement.DesignatedSiteCode, true, columnWidthTitleCell),
					CreateCell(factory.GetRefUnloco(gatepassRequestMessage.GatepassDestinationSite[0].DesignateSiteCode), false, columnWidthValueCell));

				creator.WriteRowWithFormatting(
					CreateCell(GatePassMovement.TransportationTypeCode, true, columnWidthTitleCell),
					CreateCell(factory.GetCodeDescriptionFromRefCusCodeListCombinedCode(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILTransportMethod, gatepassRequestMessage.GatepassDestinationSite[0].TransportationTypeCode.ToString()), false, columnWidthValueCell));
			}
			return creator.ToHtml();
		}
	}
}
