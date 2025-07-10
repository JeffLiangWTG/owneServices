using System.Net;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.Subscription.Incoming;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class SubscriptionErrorMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		public SubscriptionErrorMessagePrettyFormatter(ErrorNotification errorJson)
		{
			this.errorJson = Argument.NotNull(errorJson, nameof(errorJson));
		}

		readonly ErrorNotification errorJson;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html formatting, Row names for error table")]
		public ZString GetFormattedMessageText()
		{
			var messageDetails = "<H3>Customs Error</H3>";
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow("<strong>Info</strong>", "<strong>Description</strong>");
			tableCreator.WriteRow(nameof(errorJson.code), WebUtility.HtmlEncode(errorJson.code));
			tableCreator.WriteRow(nameof(errorJson.message), WebUtility.HtmlEncode(errorJson.message));
			tableCreator.WriteRow(nameof(errorJson.info.ambiente), WebUtility.HtmlEncode(errorJson.info?.ambiente));
			tableCreator.WriteRow(nameof(errorJson.info.usuario), WebUtility.HtmlEncode(errorJson.info?.usuario));
			messageDetails += tableCreator.ToHtml();

			return messageDetails;
		}
	}
}
