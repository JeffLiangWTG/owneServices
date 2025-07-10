using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AltaH7V1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class DeclarationH7MessagePrettyFormatter : H7CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public DeclarationH7MessagePrettyFormatter(AltaH7V1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}

		protected readonly AltaH7V1Sal response;

		const string H7AdmissionDeclarationType = "0";
		const string PreH7DeclarationType = "1";

		public ZString CreateMessageDetailsRejected() => CreateMessageDetailsRejectedCommon(response);

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, DeclarationTypeText, GetOperationalCodeDescription(response.Response.OperationCode));
			AppendPresentationAndAcceptance(messageDetails, response);
			AppendDeclarationDetails(messageDetails, response, response.Message.CorrelationIdentifier);
			AppendResponseDetails(messageDetails, response, response.Response?.ResponseCode);
			AppendReleaseDetails(messageDetails, response);
			AppendTaxes(messageDetails, response);

			return messageDetails.ToString();
		}

		ZString GetOperationalCodeDescription(string operationCode)
		{
			var operationText = operationCode switch
			{
				H7AdmissionDeclarationType => H7AdmissionText,
				PreH7DeclarationType => PreH7TextPresentationText,
				_ => string.Empty,
			};

			return string.IsNullOrEmpty(operationCode) ? string.Empty : $"{operationCode}: {operationText}";
		}
	}
}
