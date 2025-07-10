using System.Globalization;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsArrivalMessagePrettyFormatter : EdiFactMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public NctsArrivalMessagePrettyFormatter(INctsArrivalResponseMessageProvider messageHelperProvider) : base(messageHelperProvider)
		{
			messageProvider = messageHelperProvider;
		}
		readonly INctsArrivalResponseMessageProvider messageProvider;

		protected override ZString CreateMessageDetailsAcceptedCore()
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedArrivalDeclarationText(messageProvider.DocumentMessageName));

			AppendAddmissionDateIfNotEmpty(messageDetails);
			AppendArrivalCircuitIfNotEmpty(messageDetails);
			AppendPreviousSummaryDiscrepancyIfNotEmpty(messageDetails);
			AppendSummaryReferenceNumberIfNotEmpty(messageDetails);
			AppendTransitReferenceNumberIfNotEmpty(messageDetails);

			return messageDetails.ToString();
		}

		void AppendAddmissionDateIfNotEmpty(StringBuilder messageDetails)
		{
			var admissionDate = messageProvider.AdmissionDate;

			if (!admissionDate.IsEmpty)
			{
				messageDetails.Append(AcceptanceDateArrivalText(admissionDate.ToCustomsFormatDateStringddMMyyyyHHmmWithDash()));
			}
		}

		void AppendArrivalCircuitIfNotEmpty(StringBuilder messageDetails)
		{
			ZString circuit = GetCircuit();

			if (!circuit.IsEmpty)
			{
				messageDetails.Append(CircuitArrivalText(circuit));
			}
		}

		void AppendPreviousSummaryDiscrepancyIfNotEmpty(StringBuilder messageDetails)
		{
			var previousSummaryDiscrepancy = (string)messageProvider.PreviousSummaryDiscrepancy switch
			{
				PreviousSummaryDiscrepanctCodeList.Codes._1 => PreviousSummaryDiscrepanctCodeList.Descriptions._1,
				PreviousSummaryDiscrepanctCodeList.Codes._2 => PreviousSummaryDiscrepanctCodeList.Descriptions._2,
				PreviousSummaryDiscrepanctCodeList.Codes._3 => PreviousSummaryDiscrepanctCodeList.Descriptions._3,
				PreviousSummaryDiscrepanctCodeList.Codes._4 => PreviousSummaryDiscrepanctCodeList.Descriptions._4,
				_ => ZString.Empty,
			};
			if (!previousSummaryDiscrepancy.IsEmpty)
			{
				messageDetails.Append(string.Format(CultureInfo.InvariantCulture, GetH4Text("** {0} **"), previousSummaryDiscrepancy));
			}
		}

		void AppendSummaryReferenceNumberIfNotEmpty(StringBuilder messageDetails)
		{
			var summaryRefNumber = messageProvider.SummaryReferenceNumber;

			if (!summaryRefNumber.IsEmpty)
			{
				messageDetails.Append(SummaryArrivalText(summaryRefNumber));
			}
		}

		void AppendTransitReferenceNumberIfNotEmpty(StringBuilder messageDetails)
		{
			var transitRefNumber = messageProvider.TransitReferenceNumber;

			if ((!transitRefNumber.IsEmpty) && (messageProvider.MessageFunction != MessageFunctionCodeList.Codes.Rejected) && (DeclarationMessageTypeList.IsArrivalWithAVI(messageProvider.DocumentMessageName)))
			{
				messageDetails.Append(string.Format(CultureInfo.InvariantCulture, GetH4Text((NoResString)"MRN AVI: {0}"), transitRefNumber));
			}
		}

		string AcceptedArrivalDeclarationText(ZString documentMessageName) => GetH3Text(ResString.GetMultilingualString("0DDE6232-EC41-4317-A9D6-0B8D6223426C", "Accepted Declaration {0}", documentMessageName));
		string AcceptanceDateArrivalText(ZString admissionDate) => GetH4Text(ResString.GetMultilingualString("5F86A5A3-EBA1-44A3-9C94-8B89BC143BC9", "Acceptance: {0}", admissionDate));
		string CircuitArrivalText(ZString circuit) => GetH4Text(ResString.GetMultilingualString("9F2F9651-D3A4-4FA8-8620-948F90BE148C", "Circuit: {0}", circuit));
		string SummaryArrivalText(ZString summaryRefNumber) => GetH4Text(ResString.GetMultilingualString("D8486E2D-87B2-4B18-93A6-E2C1F155D82B", "Summary Decl: {0}", summaryRefNumber));
	}
}
