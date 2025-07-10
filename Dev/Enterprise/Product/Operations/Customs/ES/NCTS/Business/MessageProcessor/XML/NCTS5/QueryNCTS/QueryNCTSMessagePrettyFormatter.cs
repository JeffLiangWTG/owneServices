using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTRAC_v515.CCTRACV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class QueryNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public QueryNCTSMessagePrettyFormatter(Cctracv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cctracv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				var managementData = correctResponseData.Ncts5DatosGestion;
				var statusCode = managementData.Estado;
				messageDetails.Append(ManagementDataText);
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, VersionText, GetVersionDescription(managementData.FaseOrigen));
				AppendStatusData(messageDetails, statusCode);
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CancellationDateText, managementData.FechaInvalidacion != null ? ((ZDateTime)managementData.FechaInvalidacion).ToCustomsFormatDateStringddMMyyyyWithDash() : ZString.Empty);
				messageDetails.Append(blankLine);

				var statusNotInvalidOrCancelled = !invalidOrCancelledStatus.Contains(statusCode);
				if (statusNotInvalidOrCancelled)
				{
					AppendAcceptanceDataIfNotEmpty(messageDetails, correctResponseData.TransitOperation.Mrn, managementData.FechaAdmision ?? ZDateTime.Empty);
					messageDetails.Append(blankLine);
					messageDetails.Append(blankLine);
					messageDetails.Append(DepartureText);
					AppendLimitDateOfArrivalIfNotEmpty(messageDetails, managementData.FechaLimiteLlegada);
					AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(managementData.CircuitoExpedicion));
					AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, managementData.CsVdeDat, (ZDateTime?)managementData.FechaLevante);
				}
				else
				{
					AppendAcceptanceDataIfNotEmpty(messageDetails, correctResponseData.TransitOperation.Mrn, ZDateTime.Empty);
				}

				AppendExtraDataToGuarantees(messageDetails, extraDataFromProcessing);

				if (managementData.FechaHoraRecepcion != null || managementData.CircuitoRecepcion != null || managementData.UbicacionRecepcion != null)
				{
					messageDetails.Append(blankLine);
					messageDetails.Append(blankLine);
					messageDetails.Append(ArrivalText);
					AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, ArrivalDateText, managementData.FechaHoraRecepcion != null ? ((ZDateTime)managementData.FechaHoraRecepcion).ToCustomsFormatDateStringddMMyyyyHHmmssWithDash() : ZString.Empty);
					AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(managementData.CircuitoRecepcion));
					AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, LocationText, managementData.UbicacionRecepcion);
				}
				if (managementData.FechaLimiteLlegada != null || managementData.FechaUltimacionCompleta != null || managementData.CodigoResultadoDescarga != null)
				{
					messageDetails.Append(blankLine);
					messageDetails.Append(blankLine);
					messageDetails.Append(UnloadingText);
					AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, UnloadingResultDateText, managementData.FechaLimiteLlegada != null ? ((ZDateTime)managementData.FechaLimiteLlegada).ToCustomsFormatDateStringddMMyyyyWithDash() : ZString.Empty);
					AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, UltimateDateText, managementData.FechaUltimacionCompleta != null ? ((ZDateTime)managementData.FechaUltimacionCompleta).ToCustomsFormatDateStringddMMyyyyWithDash() : ZString.Empty);
					AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, ResultText, GetResultDescription(managementData.CodigoResultadoDescarga));
				}
			}
			return messageDetails.ToString();
		}

		void AppendExtraDataToGuarantees(StringBuilder messageDetails, ZString extraData)
		{
			if (extraData.Contains(ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter))
			{
				messageDetails.Append(blankLine + GuaranteesText + blankLine);
				var tableCreator = GetNewNonVisibleTableCreator();
				var guaranteeStatusData = extraData.Split(ExtraDataFromProcessing.SymbolToSeparateExtraDataForPrettyFormatter).First(x => x.Contains(ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter));

				WriteRowIfNotEmpty(tableCreator, GuaranteeStatusText, guaranteeStatusData.RemoveSafe(0, ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter.Length));
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		ZString GetVersionDescription(string versionCode) => string.IsNullOrEmpty(versionCode) ? string.Empty : versionCode == NCTSVersion4Code ? NCTSEDIVersionDescription : NCTSXMLVersionDescription;

		ZString GetResultDescription(string resultCode) => resultCode == NCTSUnloadingResultB11 ? NCTSUnloadingResultDescriptionB11 : resultCode == NCTSUnloadingResultB12 ? NCTSUnloadingResultDescriptionB12 : string.Empty;

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);

		const string NCTSVersion4Code = "P4";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string NCTSXMLVersionDescription = "NCTS5 (XML)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string NCTSEDIVersionDescription = "NCTS4 (EDI)";
		const string NCTSUnloadingResultB11 = "B11";
		const string NCTSUnloadingResultB12 = "B12";
		string NCTSUnloadingResultDescriptionB11 => ResString.GetMultilingualString("45339630-4EA6-4A64-AFC5-FF385FD75A6E", "Major discrepancies in unloading but unlocked guarantee (B11)");
		string NCTSUnloadingResultDescriptionB12 => ResString.GetMultilingualString("5F34297D-AAC4-4E3B-A7F1-64E785670D4A", "Major discrepancies in unloading and without unlocking guarantee (B12)");

		protected string ResultText => ResString.GetMultilingualString("CA80C721-D00C-454F-81E9-F01077F9B4FE", "Result:");
		protected string LocationText => ResString.GetMultilingualString("0EE3CAC2-1BC7-4C81-8DEF-88EB911A7C40", "Location:");
		protected string UltimateDateText => ResString.GetMultilingualString("A938CE01-597D-40FA-B922-CDC56662702D", "Ultimate Date:");
		protected string UnloadingResultDateText => ResString.GetMultilingualString("75003FB0-234D-46AA-B897-0217E397C081", "Unloading Result Date:");

		readonly string[] invalidOrCancelledStatus = { Ncts5TransitStatusList.Codes.Invalidated, Ncts5TransitStatusList.Codes.DeclarationCancelled, Ncts5TransitStatusList.Codes.PreDeclarationInvalidated, Ncts5TransitStatusList.Codes.InvalidatedByGuarantee };
	}
}
