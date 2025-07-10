using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3RevokeV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3RevokeMessagePrettyFormatter : G3CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public G3RevokeMessagePrettyFormatter(G3RevokeV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}

		readonly G3RevokeV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			AppendAcceptedDeclarationDetails(messageDetails);

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(RejectedText);

			AppendRejectedDeclarationDetails(messageDetails);
			AppendListOfErrors(response, messageDetails);

			return messageDetails.ToString();
		}

		void AppendAcceptedDeclarationDetails(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response?.Accepted.Lrn)
				|| !string.IsNullOrEmpty(response?.Accepted.Mrn)
				|| !string.IsNullOrEmpty(response?.Accepted.Csv))
			{
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, LrnText, response.Accepted.Lrn);
				AppendDataInNewTableIfNotEmpty(messageDetails, MrnText, response.Accepted.Mrn);
				AppendDataInNewTableIfNotEmpty(messageDetails, CsvIdText, response.Accepted.Csv);
			}
		}

		void AppendRejectedDeclarationDetails(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response?.Rejected.Lrn)
				|| !string.IsNullOrEmpty(response?.Rejected.Mrn))
			{
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, LrnText, response.Rejected.Lrn);
				AppendDataInNewTableIfNotEmpty(messageDetails, MrnText, response.Rejected.Mrn);
			}
		}
	}
}
