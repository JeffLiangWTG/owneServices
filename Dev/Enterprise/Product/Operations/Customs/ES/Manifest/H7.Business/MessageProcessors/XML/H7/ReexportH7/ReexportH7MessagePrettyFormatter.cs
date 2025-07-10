using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ReexportacionH7V1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class ReexportH7MessagePrettyFormatter : H7CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ReexportH7MessagePrettyFormatter(ReexportacionH7V1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}

		protected readonly ReexportacionH7V1Sal response;

		public ZString CreateMessageDetailsRejected() => CreateMessageDetailsRejectedCommon(response);

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedReexportText);

			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, DeclarationTypeText, GetOperationalCodeDescription(response.Response.OperationCode));
			AppendDataInNewTableIfNotEmpty(messageDetails, ResponseCodeText, response.Response.ResponseCode);
			AppendDeclarationDetails(messageDetails);

			return messageDetails.ToString();
		}

		ZString GetOperationalCodeDescription(string operationCode)
		{
			var operationText = operationCode switch
			{
				OperationCodeList.Codes.InvalidateH7WithExsEtd => OperationCodeList.Descriptions.InvalidateH7WithExsEtd,
				OperationCodeList.Codes.AnnullReExportExsEtd => OperationCodeList.Descriptions.AnnullReExportExsEtd,
				OperationCodeList.Codes.InvalidateH7WithTransit => OperationCodeList.Descriptions.InvalidateH7WithTransit,
				OperationCodeList.Codes.AnnulReExportWithTransit => OperationCodeList.Descriptions.AnnulReExportWithTransit,
				_ => string.Empty,
			};

			return string.IsNullOrEmpty(operationCode) ? string.Empty : $"{operationCode}: {operationText}";
		}

		void AppendDeclarationDetails(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response.Message.CorrelationIdentifier)
				|| !string.IsNullOrEmpty(response.EdeclarationCsvId))
			{
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, MessageIdText, response.Message.CorrelationIdentifier);
				AppendDataInNewTableIfNotEmpty(messageDetails, CsvIdText, response.EdeclarationCsvId);
			}
		}
	}
}
