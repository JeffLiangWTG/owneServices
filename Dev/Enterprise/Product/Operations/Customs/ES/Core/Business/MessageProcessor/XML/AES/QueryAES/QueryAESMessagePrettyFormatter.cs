using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCAESC_v514.CCAESCV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class QueryAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public QueryAESMessagePrettyFormatter(Ccaescv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Ccaescv1Sal response;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string AESVersionCode = "A1";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string AESVersionDescription = "AES (XML)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string EDIVersionDescription = "ECS (EDI)";

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				var managementData = correctResponseData.AesDatosGestion;
				var statusCode = managementData.EstadoAes;
				messageDetails.Append(ManagementDataText);
				AppendVersionData(messageDetails, managementData);
				AppendStatusData(messageDetails, statusCode);
				AppendInvalidationDateIfNotEmpty(messageDetails, managementData);

				var statusNotInvalidOrCancelled = !invalidOrCancelledStatus.Contains(statusCode);
				if (statusNotInvalidOrCancelled)
				{
					messageDetails.Append(blankLine);
					AppendAcceptanceDateIfNotEmpty(messageDetails, managementData.FechaAdmision ?? ZDateTime.Empty);
					AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.ExportOperation.Mrn);
					AppendGroupCircuit(messageDetails, managementData.CircuitoAeat, managementData.CircuitoAtc);
					AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, managementData.CsvLevanteExportacion, (ZDateTime?)managementData.FechaLevante);
					AppendExitTypeDataIfNotEmpty(messageDetails, managementData.FlagDirectaIndirecta);
					AppendExitResultData(messageDetails, managementData.ResultadoSalida);
					AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, managementData.CsvCertificadoSalida, (ZDateTime?)managementData.FechaSalidaEfectiva, EffectiveDepartureClearanceText, EffectiveDepartureDateText);
					AppendExitGoodsDate(messageDetails, correctResponseData);
					AppendExitStopDate(messageDetails, correctResponseData);
					AppendExitControlData(messageDetails, managementData);
				}
				else
				{
					AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.ExportOperation.Mrn);
					AppendExitStopDate(messageDetails, correctResponseData);
				}
			}

			return messageDetails.ToString();
		}

		void AppendVersionData(StringBuilder messageDetails, AesDatosGestion70 managementData)
		{
			var versionCode = managementData.FaseAes;
			if (!string.IsNullOrEmpty(versionCode))
			{
				var versionDescription = versionCode == AESVersionCode ? AESVersionDescription : EDIVersionDescription;
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, VersionText, versionDescription);
			}
		}

		void AppendInvalidationDateIfNotEmpty(StringBuilder messageDetails, AesDatosGestion70 managementData)
		{
			var invalidaionDatetext = managementData.FechaInvalidacion;
			if (!string.IsNullOrEmpty(invalidaionDatetext))
			{
				ZDateTime.TryParseExact(invalidaionDatetext, out var invalidationDate, CustomsDateTimeExtension.DateFormatWithDash);
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, InvalidationDateText, invalidationDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		void AppendExitGoodsDate(StringBuilder messageDetails, DatosRespuestaCorrectaType70 responseData)
		{
			var exitGoodsDate = responseData.ExitControlResult?.ExitDate ?? ZDateTime.Empty;
			if (!exitGoodsDate.IsEmpty)
			{
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, ExitGoodsDateText, exitGoodsDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		void AppendExitStopDate(StringBuilder messageDetails, DatosRespuestaCorrectaType70 responseData)
		{
			var exitStopDateFromManagement = responseData.AesDatosGestion.FechaParadaAduanaSalida ?? ZDateTime.Empty;
			var exitStopDateFromExitResult = responseData.ExitControlResult?.ExitStoppedDate ?? ZDateTime.Empty;

			var exitStopDate = exitStopDateFromManagement.IsEmpty ? exitStopDateFromExitResult : exitStopDateFromManagement;
			if (!exitStopDate.IsEmpty)
			{
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, ExitStopDateText, exitStopDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		void AppendExitControlData(StringBuilder messageDetails, AesDatosGestion70 managementData)
		{
			var arrivalDate = managementData.FechaLlegada ?? ZDateTime.Empty;
			var arrivalLocation = managementData.UbicacionLlegada;
			var circuit = GetCircuitFromText(managementData.CircuitoLlegada);
			var clearance = managementData.CsvLevanteSalida;
			var clearanceDate = managementData.FechaLevanteSalida;

			if (!arrivalDate.IsEmpty || !string.IsNullOrEmpty(arrivalLocation) || !circuit.IsEmpty || !string.IsNullOrEmpty(clearance) || clearanceDate != null)
			{
				messageDetails.Append(ExitControlText);
				AppendArrivalData(messageDetails, arrivalDate, arrivalLocation);
				AppendExitControlCircuit(messageDetails, circuit);
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, clearance, (ZDateTime?)clearanceDate);
			}
		}

		void AppendArrivalData(StringBuilder messageDetails, ZDateTime arrivalDate, string arrivalLocation)
		{
			if (!arrivalDate.IsEmpty || !string.IsNullOrEmpty(arrivalLocation))
			{
				messageDetails.Append(blankLine);
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, ArrivalDateText, arrivalDate.ToCustomsFormatDateStringddMMyyyyWithDash());
				WriteRowIfNotEmpty(tableCreator, ArrivalLocationText, arrivalLocation);

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		void AppendExitControlCircuit(StringBuilder messageDetails, ZString circuit)
		{
			if (!circuit.IsEmpty)
			{
				var tableCreator = GetNewNonVisibleTableCreator();
				AppendCircuitIfNotEmpty(messageDetails, circuit, tableCreator);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);

		protected string ExitControlText => GetH4Text(ResString.GetMultilingualString("2C6FB1BB-9921-4881-B3E6-6EC88112940C", "Exit Control"));
		protected string ExitGoodsDateText => ResString.GetMultilingualString("D77EEACB-2E1C-42FE-AC8F-26598CE1D690", "Exit Goods Date:");
		protected string ExitStopDateText => ResString.GetMultilingualString("A4A51C10-6315-4053-AED7-4AC6BCCDB537", "Exit Stop Date:");
		protected string ArrivalLocationText => ResString.GetMultilingualString("FF804575-EBA8-4E76-BCC7-9A8520814414", "Arrival Location:");

		readonly string[] invalidOrCancelledStatus = { AESStatusCodeList.Codes.Invalidated, AESStatusCodeList.Codes.DeclarationCancelled, AESStatusCodeList.Codes.PreDeclarationInvalidated };
	}
}
