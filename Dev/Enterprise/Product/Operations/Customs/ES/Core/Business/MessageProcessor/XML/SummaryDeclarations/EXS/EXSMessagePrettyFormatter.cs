using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE616V4Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE628V4Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE919V4Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Business
{
	public class EXSMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public EXSMessagePrettyFormatter(EXSResponse response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		protected readonly EXSResponse response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			var acceptedResponse = response as Cc628A;
			if (acceptedResponse != null)
			{
				AppendHeaderData(messageDetails, acceptedResponse.Heahea);
			}

			return messageDetails.ToString();
		}

		void AppendHeaderData(StringBuilder messageDetails, CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE628V4Sal.Heahea acceptedResponseHeader)
		{
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CSVElectronicDeclarationText, acceptedResponseHeader.DecCsvHea);
			AppendTypeData(messageDetails, acceptedResponseHeader.DecTypeHea);

			ZDateTime.TryParseExact(acceptedResponseHeader.DecRegDatTimHea115, out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatLong);
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, AcceptanceText, acceptanceDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());

			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, MrnText, acceptedResponseHeader.DocNumHea5);
			AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(acceptedResponseHeader.CusChanHea));
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CSVClearanceText, acceptedResponseHeader.RelCsvHea);
		}

		void AppendTypeData(StringBuilder messageDetails, string typeDataFromResponse)
		{
			var typeData = typeDataFromResponse switch
			{
				"A1" => ResString.GetMultilingualString("D0D16AEF-6442-4712-96E1-1324515C0307", "Exit Summary Declaration"),
				"A2" => ResString.GetMultilingualString("7BDFEEED-2E3C-4233-A9D3-FB683A74B411", "Exit Summary Declaration Express Consignment"),
				"A3" => ResString.GetMultilingualString("4C5C35C1-2FDD-47BE-B0B1-ECD2E9892259", "Re-Export Notification"),
				"NR" => ResString.GetMultilingualString("2A8F94FE-7428-4BEB-8644-33AB219D88F5", "Reshipment Notification"),
				_ => string.Empty,
			};
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, TypeText, typeData);
		}

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = new StringBuilder();
			var tableCreatorErrors = GetNewTableCreator();
			if (response is Cc616AType rejectedResponse)
			{
				messageDetails.Append(RejectedDeclarationText);
				var tableCreatorReason = GetNewNonVisibleTableCreator();
				WriteRowIfNotEmpty(tableCreatorReason, ReasonText, rejectedResponse.Heahea.RefNumHea4 + " " + rejectedResponse.Heahea.DecRejReaHea252);
				messageDetails.Append(tableCreatorReason.ToHtml());

				tableCreatorErrors.WriteRow(ErrorCodeColumnText, ErrorPlaceColumnText, ErrorReasonColumnText, ErrorOriginalValueColumnText);
				foreach (var error in rejectedResponse.Funerrer1)
				{
					tableCreatorErrors.WriteRow(error.ErrTypEr11, error.ErrPoiEr12, error.ErrReaEr13, error.OriAttValEr14);
				}
			}
			else if (response is Cd919B rejectedWithErrorResponse)
			{
				messageDetails.Append(RejectedDeclarationHeaderText);
				tableCreatorErrors.WriteRow(ErrorCodeColumnText, ErrorPlaceColumnText, ErrorReasonColumnText, ErrorOriginalValueColumnText, ErrorLocationColumnText);
				foreach (var error in rejectedWithErrorResponse.Xmlerr805)
				{
					tableCreatorErrors.WriteRow(error.ErrCodXmler806, error.ErrLinNumXmler800, error.ErrReaXmler802, error.OriAttValXmler804, error.ErrLocXmler803);
				}
			}

			messageDetails.Append(ListOfErrorsText);
			messageDetails.Append(tableCreatorErrors.ToHtml());

			return messageDetails.ToString();
		}

		string RejectedDeclarationHeaderText => GetH3Text(ResString.GetMultilingualString("7B51A82B-F215-44AE-986C-BE6E497A2D85", "Rejected Declaration (XML Error)"));
		string ReasonText => ResString.GetMultilingualString("4B98C36D-3A11-4CBE-A534-33B968D51847", "Reason:");
		string TypeText => ResString.GetMultilingualString("2440B189-3353-4A55-83C9-7DEDF5CF00DF", "Type:");
	}
}
