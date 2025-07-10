using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AnulaPreH7V1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CancellationH7MessagePrettyFormatter : H7CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CancellationH7MessagePrettyFormatter(AnulaPreH7V1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}

		protected readonly AnulaPreH7V1Sal response;

		public ZString CreateMessageDetailsRejected() => CreateMessageDetailsRejectedCommon(response);

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedCancellationText);

			AppendStatus(messageDetails);
			AppendCancellationDetails(messageDetails);

			return messageDetails.ToString();
		}

		void AppendStatus(StringBuilder messageDetails)
		{
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, StatusText, response.Response.OperationCode + " " + H7CancelledText);
		}

		void AppendCancellationDetails(StringBuilder messageDetails)
		{
			ZDateTime.TryParseExact(response.Message.PreparationDate + response.Message.PreparationTime, out var preparationDateTime, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);

			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, MessageIdText, response.Message.CorrelationIdentifier);
			AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, preparationDateTime, CancellationDateText);
			AppendDataInNewTableIfNotEmpty(messageDetails, CsvIdText, response.EdeclarationCsvId);
		}

		string H7CancelledText => ResString.GetMultilingualString("338cb6b2-046d-4a00-9883-ce365d1cd9cb", "H7 Canceled");
	}
}
