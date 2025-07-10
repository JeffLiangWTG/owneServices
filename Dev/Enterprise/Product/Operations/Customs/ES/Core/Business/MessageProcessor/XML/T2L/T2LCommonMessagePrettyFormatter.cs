using System.Globalization;
using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public abstract class T2LCommonMessagePrettyFormatter<TResponse> : CommonMessagePrettyFormatter, IMessagePrettyFormatter
		where TResponse : IT2LCommon, ICommonServiceSegment
	{
		public T2LCommonMessagePrettyFormatter(TResponse response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		protected readonly TResponse response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => CreateMessageDetailsAcceptedCore();

		protected virtual ZString CreateMessageDetailsAcceptedCore()
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(GetAcceptedDeclarationText());

			AppendAcceptanceDataIfNotEmpty(messageDetails, response.ServiceSegmentId.LeftOrNull(14));
			AppendExtraData(messageDetails);
			AppendResponseDescriptionDataIfNotEmpty(messageDetails);

			return messageDetails.ToString();
		}

		protected ZString GetAcceptedDeclarationText()
		{
			var messageDetails = AcceptedDeclarationText;
			var csvCode = response.CSVCode;
			if (!string.IsNullOrEmpty(csvCode))
			{
				messageDetails += string.Format(CultureInfo.InvariantCulture, csvElectronicDeclaration(csvCode));
			}

			return messageDetails;
		}

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = RejectedDeclarationText;
			var responseCode = response.ResponseCode;
			var responseDescription = response.ResponseDescription;

			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorErrorColumnText, DescriptionColumnText);
			tableCreator.WriteRow(responseCode, responseDescription);
			messageDetails += tableCreator.ToHtml();

			return messageDetails;
		}

		protected virtual void AppendExtraData(StringBuilder messageDetails) { }

		protected void AppendMRNDataIfNotEmpty(StringBuilder messageDetails, ZString mrnCode) => AppendDataInNewTableIfNotEmpty(messageDetails, MrnText, mrnCode);

		protected void AppendCSVdelPDFdelT2LDataIfNotEmpty(StringBuilder messageDetails, ZString csvCode, bool hadBlankLine = true)
		{
			if (!string.IsNullOrEmpty(csvCode))
			{
				if (hadBlankLine)
				{
					messageDetails.Append(blankLine);
				}
				AppendDataInNewTableIfNotEmpty(messageDetails, CSVText, csvCode);
			}
		}

		protected void AppendResponseDescriptionDataIfNotEmpty(StringBuilder messageDetails) => AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, DescriptionText, response.ResponseDescription);

		protected ZString GetCircuit(CircuitoTipo circuit, bool circuitSpecified) => circuitSpecified ? GetCircuitFromText(circuit.ToString()) : ZString.Empty;

		protected const string CSVText = "C.S.V.:";

		string csvElectronicDeclaration(ZString csvCode) => GetH3Text(ResString.GetMultilingualString("F7844B27-C563-4971-BCDE-EFADC7328478", "CSV Electronic Declaration = {0}", csvCode));
	}
}
