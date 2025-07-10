using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.AnulaPDCVinculacionV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class CancelDVDMessagePrettyFormatter : DVDCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CancelDVDMessagePrettyFormatter(AnulaPdcVinculacionV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly AnulaPdcVinculacionV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedCancellationText);

			var correctResponseData = response.Respuesta;
			if (correctResponseData != null)
			{
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, correctResponseData.CsvDeclaracionElectronica);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedComplete(response);
	}
}
