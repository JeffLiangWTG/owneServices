using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.SAL;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public class ExpAmendmentG5MessagePrettyFormatter : G5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ExpAmendmentG5MessagePrettyFormatter(G5ExpAmendV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly G5ExpAmendV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => SetG5MessageDetailsAcceptedCommon(response.Accepted, response.EnvelopeG5);

		public ZString CreateMessageDetailsRejected() => SetG5MessageDetailsRejectedCommon(response);
	}
}
