using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.AnulaImportacionV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class CANPreDUAMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CANPreDUAMessagePrettyFormatter(AnulaImportacionV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly AnulaImportacionV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => AcceptedCancellationText; 
		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = RejectedCancellationText;
			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorErrorColumnText, DescriptionColumnText);
			tableCreator.WriteRow(response.CodigoRespuesta, response.DescripcionRespuesta);
			messageDetails += tableCreator.ToHtml();

			return messageDetails;
		}

		string RejectedCancellationText => GetH3Text(ResString.GetMultilingualString("9509CF5C-C5D5-49A9-9847-A0C87706CEEB", "Rejected Cancellation"));
	}
}
