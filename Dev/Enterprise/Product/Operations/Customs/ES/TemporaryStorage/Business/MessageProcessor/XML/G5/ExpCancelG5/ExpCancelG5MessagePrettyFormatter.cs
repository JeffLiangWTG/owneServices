using System;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpCancelV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public class ExpCancelG5MessagePrettyFormatter : G5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ExpCancelG5MessagePrettyFormatter(G5ExpCancelV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly G5ExpCancelV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			StringBuilder messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedCancellationText);

			if (response.Accepted != null)
			{
				var admissionDate = DateTime.ParseExact(response.EnvelopeG5.PreparationDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds, CultureInfo.InvariantCulture);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, admissionDate, CancellationDateText);
				messageDetails.Append(blankLine);
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, response.Accepted.NotificationCsv);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetG5MessageDetailsRejectedCommon(response);
	}
}
