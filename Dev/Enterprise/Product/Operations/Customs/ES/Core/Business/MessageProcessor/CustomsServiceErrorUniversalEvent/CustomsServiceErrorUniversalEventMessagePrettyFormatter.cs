using CargoWise.Common;
using CargoWise.Types;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.ES.Business
{
	public class CustomsServiceErrorUniversalEventMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CustomsServiceErrorUniversalEventMessagePrettyFormatter(UniversalEventWrapper responseMessage)
		{
			response = Argument.NotNull(responseMessage, nameof(responseMessage));
		}
		readonly UniversalEventWrapper response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => ZString.Empty;

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = GetH3Text(ResString.GetMultilingualString("8F8C85B3-C749-427E-A328-CAF90E8CB68A", "Universal Event Service Error"));
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(EventTypeColumnText, MessageTypeColumnText, ErrorReasonColumnText);
			tableCreator.WriteRow(response.EventType, response.MessageType, response.Reason);
			messageDetails += tableCreator.ToHtml();

			return messageDetails;
		}

		string EventTypeColumnText => GetStrongText(ResString.GetMultilingualString("F0D03B4D-ABC9-4277-8900-6D28363F1556", "Event Type"));
		string MessageTypeColumnText => GetStrongText(ResString.GetMultilingualString("1A56FF9A-3E52-45B2-A61F-47F9A4C8BE7C", "Message Type"));
	}
}
