using System.Net;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class UniversalEventMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public UniversalEventMessagePrettyFormatter(UniversalEventWrapper universalEvent)
		{
			response = Argument.NotNull(universalEvent, nameof(universalEvent));
		}
		readonly UniversalEventWrapper response;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html formatting, Row names for error table")]
		public ZString GetFormattedMessageText()
		{
			var messageDetails = "<H3>Service Error</H3>";
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow("<strong>Error</strong>", "<strong>Description</strong>");
			tableCreator.WriteRow(response.ResponseType, WebUtility.HtmlEncode(response.Reason));
			messageDetails += tableCreator.ToHtml();

			return messageDetails;
		}
	}
}
