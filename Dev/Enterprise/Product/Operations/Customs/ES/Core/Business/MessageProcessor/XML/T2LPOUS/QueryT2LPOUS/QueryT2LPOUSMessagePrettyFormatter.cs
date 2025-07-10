using System.Collections.ObjectModel;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01CONSV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.ES.Business
{
	public class QueryT2LPOUSMessagePrettyFormatter : T2LPOUSCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public QueryT2LPOUSMessagePrettyFormatter(Iep01Cons response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Iep01Cons response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);
			messageDetails.Append(blankLine);

			var tableCreator = GetNewNonVisibleTableCreator();
			WriteRowIfNotEmpty(tableCreator, MrnT2lText, response.ProofData?.Mrnt2L);
			WriteRowIfNotEmpty(tableCreator, MrnJecText, response.ProofData?.Mrnjec);
			messageDetails.Append(tableCreator.ToHtml());

			AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(response.ProofData?.RiskAnalysisResultCode));
			messageDetails.Append(blankLine);

			ZDateTime.TryParseExact(response.ProofData?.RequestDate, out var requestDate, CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss);
			ZDateTime.TryParseExact(response.ProofData?.RegistrationDate, out var registrationDate, CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss);

			tableCreator = GetNewNonVisibleTableCreator();
			WriteRowIfNotEmpty(tableCreator, RequestDateText, requestDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			WriteRowIfNotEmpty(tableCreator, RegistrationDateText, registrationDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			messageDetails.Append(tableCreator.ToHtml());
			messageDetails.Append(blankLine);

			AppendProofStatusIfNotEmpty(messageDetails, response.ProofData?.ProofStatus);
			
			return messageDetails.ToString();
		}

		protected void AppendRequestDateIfNotEmpty(StringBuilder messageDetails, string requestDate) => AppendDataInNewTableIfNotEmpty(messageDetails, RequestDateText, requestDate);

		protected void AppendProofStatusIfNotEmpty(StringBuilder messageDetails, string proofStatus) => AppendDataInNewTableIfNotEmpty(messageDetails, ProofStatusText, proofStatus);

		protected void AppendRegistrationDateIfNotEmpty(StringBuilder messageDetails, string registrationDate) => AppendDataInNewTableIfNotEmpty(messageDetails, RegistrationDateText, registrationDate);

		string MrnT2lText => ResString.GetMultilingualString("512305BE-5E0A-4F17-8C16-D8461B74A992", "Register T2L (MRN):");

		string MrnJecText => ResString.GetMultilingualString("D42F5A98-46E4-453F-9D0E-01F985554DD7", "Register JEC (MRN):");

		string RequestDateText => ResString.GetMultilingualString("067C963E-8D4E-4F72-96B0-E30FE88C9921", "Request Date:");

		string ProofStatusText => ResString.GetMultilingualString("18F7F6ED-013F-4BBD-9E7B-3C69E9232696", "Proof Status:");

		string RegistrationDateText => ResString.GetMultilingualString("2DCAB395-3434-41F8-BE68-A238DB84CFCB", "Registration Date:");

		public ZString CreateMessageDetailsRejected()
		{
			var listErrorCon = new Collection<IT2LPOUSError>();
			foreach (IT2LPOUSErrors error in response.ErrorData)
			{
				listErrorCon.AddRange(error.T2LPOUSErrors);
			}

			return GetT2LMessageDetailsRejectedCommon(listErrorCon);
		}
	}
}
