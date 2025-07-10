using System.Text;
using CargoWise.Common;
using CargoWise.Customs.IN.MessageContracts;
using CargoWise.Customs.IN.MessageDefinitions.AirCgmAckCHCMI02;
using CargoWise.Types;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IN.Business;

sealed class ReceiveMessageInterpreter : BaseMessageInterpreter
{
	public ReceiveMessageInterpreter(EDIMessage message) : base(message)
	{
	}

	protected override ZString FormatMessageText()
	{
		using var email = Utils.CreateMessageFromEml(Message);
		var builder = new StringBuilder();
		builder.AppendLine(HtmlEncode(email.Subject));
		builder.Append("<hr><br>");
		builder.AppendLine(HtmlEncode(email.TextBody));

		HtmlTableCreator tableCreator;
		foreach (var attachment in email.Attachments)
		{
			var attachmentText = Encoding.UTF8.GetString(attachment.GetData());
			builder.Append("<hr><br>");
			builder.Append("Attachment<br><br>");
			builder.Append(HtmlEncode(attachmentText));
			builder.Append("<hr><br>");
			tableCreator = new HtmlTableCreator();

			var messageId = FlatFileMessage.ExtractMessageId(attachmentText);
			if (messageId == Constants.MessageID.AirCgmAcknowledgement)
			{
				var consacks = FlatFileMessage.DeserializeObject<AirCgmAckChcmi02>(attachmentText).Consacks;
				if (!consacks.IsNullOrEmpty())
				{
					tableCreator.WriteRow(new CellWithFormatting("MAWB", isTitle: true), new CellWithFormatting("HAWB", isTitle: true), new CellWithFormatting("Response", isTitle: true));
					foreach (var consack in consacks)
					{
						var errorCodes = consack.ErrorCode.Split(',', ';', ' ', '|');
						foreach (var errorCode in errorCodes)
						{
							tableCreator.WriteRow(consack.MasterAirwayBillNo, consack.HawbNo, errorCode + " - " + MessageInterpreterFactory.GetResponseDescription(Message.Factory, errorCode));
						}
					}

					builder.Append(tableCreator.ToHtml());
				}
			}
		}

		return builder.ToString();
	}
}
