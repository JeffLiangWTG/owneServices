using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01V1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class RequestAndReceptionT2LPOUSMessagePrettyFormatter : T2LPOUSCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public RequestAndReceptionT2LPOUSMessagePrettyFormatter(Iep01SalType response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Iep01SalType response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => GetMessageDetailsAccepted(response.Message.PreparationDateAndTime,
			response.Mrnt2L, response.RiskAnalysisResultCode, response.Csvt2L, response.CsVeDeclaration);

		public ZString CreateMessageDetailsRejected() => GetT2LMessageDetailsRejected(response);
	}
}
