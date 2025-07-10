using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271.D_NG_2715_MSG22002_AddAGlobalScannedAttachmentToEntity;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.IL.Business.Message.PrettiedCaptions;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC271RequestMessagePrettier : ILEDIMessagePrettierBase<DNg2715Msg22002AddAGlobalScannedAttachmentToEntity>
	{
		public ILDOC271RequestMessagePrettier(MessageDataObject<DNg2715Msg22002AddAGlobalScannedAttachmentToEntity> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData,
				builder =>
				builder.WithRequestSection((attachment, sb) =>
				{
					sb.Append(GetTable(attachment));
				})
			)
			.Build();

		public ZString GetTable(DNg2715Msg22002AddAGlobalScannedAttachmentToEntity attachment)
		{
			var creator = new HtmlTableCreator();

			var columnWidthTitleCell = 300;
			var columnWidthValueCell = 500;

			creator.WriteRowWithFormatting(
				CreateCell(PrettiedCaptions.Common.Date, true, columnWidthTitleCell),
				CreateCell(ConvertNullableDateTimeToString(attachment.RequestContentHeader?.TransmitionDateTime), false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(Attachment.FileName, true, columnWidthTitleCell),
				CreateCell(attachment.Attachment?.FileName, false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(Attachment.DocumentType, true, columnWidthTitleCell),
				CreateCell(attachment.Attachment?.DocumentType, false, columnWidthValueCell));

			creator.WriteRowWithFormatting(
				CreateCell(Attachment.ExternalAttachmentID, true, columnWidthTitleCell),
				CreateCell(attachment.Attachment?.ExternalAttachmentId, false, columnWidthValueCell));

			if (attachment.Attachment?.AdditionalData != null)
			{
				foreach (var additionalData in attachment.Attachment?.AdditionalData)
				{
					if (!string.IsNullOrEmpty(additionalData.FieldData))
					{
						creator.WriteRowWithFormatting(
						CreateCell(Attachment.FieldID, true, columnWidthTitleCell),
						CreateCell(ConvertNullableIntToString(additionalData.FieldId), false, columnWidthValueCell));

						creator.WriteRowWithFormatting(
							CreateCell(Attachment.FieldData, true, columnWidthTitleCell),
							CreateCell(additionalData.FieldData, false, columnWidthValueCell));
					}
				}
			}

			return creator.ToHtml();
		}
	}
}

