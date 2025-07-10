using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5RecNotifV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public class ReceptionG5MessagePrettyFormatter : G5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ReceptionG5MessagePrettyFormatter(G5RecNotifV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly G5RecNotifV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => SetG5MessageDetailsAcceptedCommon(response.Accepted, response.EnvelopeG5);

		public ZString CreateMessageDetailsRejected() => SetG5MessageDetailsRejectedCommon(response);
	}
}
