using System.Globalization;
using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class H7CommonMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		protected ZString CreateMessageDetailsRejectedCommon(IH7CommonErrors response)
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(RejectedDeclarationText);
			messageDetails.Append(ListOfErrorsText);

			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorCodeColumnText, ErrorPointerColumnText, ErrorPlaceColumnText, ErrorReasonColumnText, ItemColumnText, ErrorOriginalValueColumnText);

			foreach (var error in response.Errors)
			{
				tableCreator.WriteRow(error.Code, error.Pointer, error.Place, error.Reason, error.Item, error.OriginalValue);
			}

			messageDetails.Append(tableCreator.ToHtml());

			return messageDetails.ToString();
		}

		protected void AppendPresentationAndAcceptance(StringBuilder messageDetails, IH7DeclarationInfo declarationInfo)
		{
			ZDateTime.TryParseExact(declarationInfo.PresentationDate + declarationInfo.PresentationTime, out var presentationDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
			ZDateTime.TryParseExact(declarationInfo.DeclarationAcceptanceDate + declarationInfo.DeclarationAcceptanceTime, out var acceptionDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);

			if (!presentationDate.IsEmpty || !acceptionDate.IsEmpty)
			{
				messageDetails.Append(blankLine);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, presentationDate, PresentationText);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, acceptionDate, AcceptanceText);
			}
		}

		protected void AppendDeclarationDetails(StringBuilder messageDetails, IH7DeclarationInfo declarationInfo, string messageId = "")
		{
			if (!string.IsNullOrEmpty(messageId)
				|| !string.IsNullOrEmpty(declarationInfo.Mrn)
				|| !string.IsNullOrEmpty(declarationInfo.EdeclarationCsvId))
			{
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, MessageIdText, messageId);
				AppendDataInNewTableIfNotEmpty(messageDetails, MrnText, declarationInfo.Mrn);
				AppendDataInNewTableIfNotEmpty(messageDetails, CsvIdText, declarationInfo.EdeclarationCsvId);
			}
		}

		protected void AppendResponseDetails(StringBuilder messageDetails, IH7DeclarationInfo declarationInfo, string responseCode = "")
		{
			if (!string.IsNullOrEmpty(responseCode)
				|| declarationInfo.DocumentationRequired == DocumentationRequiredS
				|| !string.IsNullOrEmpty(declarationInfo.RiskAnalysisResultCode))
			{
				messageDetails.Append(blankLine);

				AppendDataInNewTableIfNotEmpty(messageDetails, ResponseCodeText, responseCode);

				if (declarationInfo.DocumentationRequired == DocumentationRequiredS)
				{
					AppendDataInNewTableIfNotEmpty(messageDetails, DocumentationRequiredText, declarationInfo.DocumentationRequired);
				}

				AppendDataInNewTableIfNotEmpty(messageDetails, RiskAnalysisResultCodeText, GetRiskAnalysisResultCodeDescription(declarationInfo.RiskAnalysisResultCode));
			}
		}

		ZString GetRiskAnalysisResultCodeDescription(string riskAnalysisResultCode)
		{
			string riskText = riskAnalysisResultCode switch
			{
				RiskAnalysisResultCodeL => EnsInLatencyPeriodText,
				_ => GetCircuitFromText(riskAnalysisResultCode),
			};

			return string.IsNullOrEmpty(riskText) ? string.Empty : $"{riskAnalysisResultCode}: {riskText}";
		}

		protected void AppendReleaseDetails(StringBuilder messageDetails, IH7DeclarationInfo declarationInfo)
		{
			ZDateTime.TryParseExact(declarationInfo.ReleaseDate + declarationInfo.ReleaseTime, out var releaseDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);

			if (!releaseDate.IsEmpty || !string.IsNullOrEmpty(declarationInfo.ReleaseCsvId))
			{
				messageDetails.Append(blankLine);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, releaseDate, ReleaseText);
				AppendDataInNewTableIfNotEmpty(messageDetails, ReleaseCSVIDText, declarationInfo.ReleaseCsvId);
			}
		}

		protected void AppendTaxes(StringBuilder messageDetails, IH7DeclarationInfo declarationInfo)
		{
			if (declarationInfo.CalculationOfTaxes != null)
			{
				messageDetails.Append(blankLine);
				messageDetails.Append(TaxesAndFeesDataText);

				foreach (var tax in declarationInfo.CalculationOfTaxes.Tax)
				{
					messageDetails.Append(blankLine);
					AppendDataInNewTableIfNotEmpty(messageDetails, CurrencyText, declarationInfo.CalculationOfTaxes.CurrencyCode);
					AppendDataInNewTableIfNotEmpty(messageDetails, TaxTypeText, tax.TaxType);
					AppendDataInNewTableIfNotEmpty(messageDetails, TaxAmountPayableText, tax.PayableTaxAmount.ToString(CultureInfo.InvariantCulture));
					AppendDataInNewTableIfNotEmpty(messageDetails, TaxBaseText, tax.TaxBase.ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		const string DocumentationRequiredS = "S";
		const string RiskAnalysisResultCodeL = "L";
		protected string PresentationText => ResString.GetMultilingualString("a0a96006-bad2-48e0-8af1-d8fa96a05372", "Presentation:");
		protected string DocumentationRequiredText => ResString.GetMultilingualString("4090224a-d56e-49e2-85d2-b884a427e03c", "Documentation Required:");
		protected string RiskAnalysisResultCodeText => ResString.GetMultilingualString("17f9918b-4563-49f2-b408-18341b0fbdb0", "Risk Analysis Result Code:");
		protected string ReleaseText => ResString.GetMultilingualString("d9b27e48-d7d4-4275-a5ab-3f3a97a00f6b", "Release:");
		protected string ReleaseCSVIDText => ResString.GetMultilingualString("816ba8e0-2199-4f25-bebb-52657499bc86", "Release CSV ID:");
		protected string CurrencyText => ResString.GetMultilingualString("e34942ce-eff4-4179-9a4b-dc7795a6027c", "Currency:");
		protected string TaxTypeText => ResString.GetMultilingualString("110632a9-9811-4d69-be68-05a2cf1e2e35", "Tax Type:");
		protected string TaxAmountPayableText => ResString.GetMultilingualString("1574bc88-a8c4-4b54-a6af-0e03c5394f31", "Tax Amount Payable:");
		protected string TaxBaseText => ResString.GetMultilingualString("362da3c2-56cf-4558-90f9-13914e605c4d", "Tax Base:");
		protected string H7AdmissionText => ResString.GetMultilingualString("44265dc3-c61e-48c7-bb80-cd6fce751e8d", "H7 admission");
		protected string PreH7TextPresentationText => ResString.GetMultilingualString("c0448451-cfc5-4f94-8d2d-3bc15c63d36f", "Pre-H7 presentation");
		protected string EnsInLatencyPeriodText => ResString.GetMultilingualString("6eb5f541-a2b0-496c-a9a5-5fe0142cd287", "ENS in latency period");
		protected string ResponseDescriptionText => ResString.GetMultilingualString("6ea81e1e-46bb-441f-a026-03756563ba92", "Response Description:");
		protected string AdditionalInformationText => GetH4Text(ResString.GetMultilingualString("b9fb29fd-ec96-4636-b35a-f1003fe25b67", "Additional Information"));
		protected string DocumentCsvIdText => ResString.GetMultilingualString("1fed4c0c-91e3-4995-b389-0b3baff21723", "Document CSV ID:");
		protected string ClearanceRequestText => ResString.GetMultilingualString("5e12d696-35a6-4ef4-9ff6-85298fad8901", "Clearance Request:");
		protected string AcceptedReexportText => GetH3Text(ResString.GetMultilingualString("915802BB-6FF4-4CA0-B3D3-202CD6B30BC5", "Accepted Reexport"));
		protected string DeclarationInformationText => GetH3Text(ResString.GetMultilingualString("73f414f7-0d4b-4619-9e9d-2c2592464f2c", "Declaration Information"));
		protected string MessageIdText => ResString.GetMultilingualString("51c85cc2-2029-49a7-b06e-6de1ea18b823", "Message ID:");
	}
}
