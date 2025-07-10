using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DG;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.Incoming;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public abstract class G5CommonMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		protected ZString SetG5MessageDetailsAcceptedCommon(AcceptedDg correctResponseData, EnvelopeG5SalDg envelopResponseData)
		{
			var messageDetails = new StringBuilder();
			
			messageDetails.Append(AcceptedDeclarationText);
			if (correctResponseData != null && envelopResponseData != null)
			{
				messageDetails.Append(blankLine);
				ZDateTime.TryParseExact(envelopResponseData.PreparationDate, out var admissionDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, admissionDate, AcceptanceText);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(correctResponseData.Channel.ToString()));
				AppendGroupDsdt(messageDetails, correctResponseData.TsAtDestination);
				AppendGroupCSV(messageDetails, correctResponseData.ReleaseCsv, correctResponseData.NotificationCsv);
			}

			return messageDetails.ToString();
		}

		void AppendGroupCSV(StringBuilder messageDetails, string csvClearance, string csvElectronicDeclaration)
		{
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CSVClearanceText, csvClearance);
			messageDetails.Append(blankLine);
			AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, csvElectronicDeclaration);
		}

		void AppendGroupDsdt(StringBuilder messageDetails, ZString dsdtMRN)
		{
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, DsdtMrnText, dsdtMRN);
			AppendDataInNewTableIfNotEmpty(messageDetails, DSDTSummaryDeclarationFormatText, DocumentHelper.GetDsdtMRNNumberFormat(dsdtMRN));
		}

		protected ZString SetG5MessageDetailsRejectedCommon(IG5CommonErrors response)
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(RejectedDeclarationText);
			messageDetails.Append(ListOfErrorsText);

			var tableCreator = GetNewTableCreator();
			tableCreator.WriteRow(ErrorCodeColumnText, ErrorReasonColumnText, ErrorTypeColumnText, ErrorPlaceColumnText, ErrorGoodsItemNumberColumnText);

			foreach (var error in response.Errors)
			{
				var goodsItemNumber = error.GoodsItemNumber?.ToString() ?? ZString.Empty;
				tableCreator.WriteRow(error.Code, error.Description, error.Type, error.Pointer, goodsItemNumber);
			}

			messageDetails.Append(tableCreator.ToHtml());

			return messageDetails.ToString();
		}

		string ErrorTypeColumnText => GetStrongText(ResString.GetMultilingualString("7A1CEF5A-35C8-4D5E-9633-6EC90E1CFF0D", "Error Type"));

		string ErrorGoodsItemNumberColumnText => GetStrongText(ResString.GetMultilingualString("454902C8-E95C-487D-954E-3C4260E42BCC", "Goods Item Number"));

		string DsdtMrnText => ResString.GetMultilingualString("CCE793C1-6481-4E88-A3A8-68076D816538", "DSDT MRN:");

		string DSDTSummaryDeclarationFormatText => ResString.GetMultilingualString("97FAEEE7-7BC9-4B54-BFBB-E8BFDCAE9AED", "DSDT (Summary Declaration format):");
	}
}
