using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEJECV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class PresentationT2LPOUSMessagePrettyFormatter : T2LPOUSCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public PresentationT2LPOUSMessagePrettyFormatter(IejecSalType response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly IejecSalType response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => GetMessageDetailsAccepted(response.Message.PreparationDateAndTime,
			response.Mrnjec, response.RiskAnalysisResultCode, response.Csvjec, response.CsVeDeclaration);

		public ZString CreateMessageDetailsRejected() => GetT2LMessageDetailsRejected(response);
	}
}
