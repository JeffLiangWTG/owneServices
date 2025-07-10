using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Xhub.Products.Customs;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class CustomsServiceErrorMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CustomsServiceErrorMessagePrettyFormatter(CommonCustomsServiceError responseMessage)
		{
			response = Argument.NotNull(responseMessage, nameof(responseMessage));
		}
		readonly CommonCustomsServiceError response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => ZString.Empty;

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = GetH3Text(ResString.GetMultilingualString("4F0398C0-EE50-4F68-8B85-47C367EF9571", "Service Error"));
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorErrorColumnText, DescriptionColumnText);
			tableCreator.WriteRow(response.ErrorType, response.ErrorDescription);
			messageDetails += tableCreator.ToHtml();

			return messageDetails;
		}
	}
}
