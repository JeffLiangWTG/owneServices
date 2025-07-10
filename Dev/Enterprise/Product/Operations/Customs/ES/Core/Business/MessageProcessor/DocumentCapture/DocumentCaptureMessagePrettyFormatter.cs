using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class DocumentCaptureMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public DocumentCaptureMessagePrettyFormatter(AttachedDocument responseMessage)
		{
			response = Argument.NotNull(responseMessage, nameof(responseMessage));
		}
		readonly AttachedDocument response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => GetH3Text(ResString.GetMultilingualString("633399CF-D7B5-4A6B-8066-B98EA4A7BE60", "Document {0} received and saved on eDocs.", response.FileName));

		public ZString CreateMessageDetailsRejected() => GetH3Text(ResString.GetMultilingualString("78F8082F-2CAD-4A99-BB10-0EBB0E47BC18", "EDI Message response processing for requested document failed."));
	}
}
