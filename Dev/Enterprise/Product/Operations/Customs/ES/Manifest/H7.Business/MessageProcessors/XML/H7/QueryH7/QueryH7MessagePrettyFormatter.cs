using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ConsultaH7V1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class QueryH7MessagePrettyFormatter : H7CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public QueryH7MessagePrettyFormatter(ConsultaH7V1Sal response, MrnInfoTd mrnInfoForBill)
		{
			this.response = Argument.NotNull(response, nameof(response));
			this.mrnInfoForBill = mrnInfoForBill;
		}

		readonly ConsultaH7V1Sal response;
		readonly MrnInfoTd mrnInfoForBill;

		public ZString CreateMessageDetailsRejected() => CreateMessageDetailsRejectedCommon(response);

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			AppendDataInNewTableIfNotEmpty(messageDetails, MessageIdText, response.Message.CorrelationIdentifier);
			AppendDataInNewTableIfNotEmpty(messageDetails, ResponseCodeText, response.Response.ResponseCode);

			if (mrnInfoForBill != null)
			{
				messageDetails.Append(DeclarationInformationText);
				AppendDataInNewTableIfNotEmpty(messageDetails, StatusText, GetStatusDeclarationDescription(mrnInfoForBill.StatusDeclaration));
				AppendPresentationAndAcceptance(messageDetails, mrnInfoForBill);
				AppendDeclarationDetails(messageDetails, mrnInfoForBill);
				AppendResponseDetails(messageDetails, mrnInfoForBill);
				AppendReleaseDetails(messageDetails, mrnInfoForBill);
				AppendTaxes(messageDetails, mrnInfoForBill);
			}

			return messageDetails.ToString();
		}

		ZString GetStatusDeclarationDescription(string statusCode)
		{
			ZString description = statusCode;
			if (!description.IsEmpty)
			{
				var statusList = new ESH7CustomsStatusList();
				var statusDescriptionFromCode = statusList.GetDescriptionFromCode(statusCode);
				if (!string.IsNullOrEmpty(statusDescriptionFromCode))
				{
					description = $"{statusCode}: {statusDescriptionFromCode}";
				}
			}
			return description;
		}
	}
}
