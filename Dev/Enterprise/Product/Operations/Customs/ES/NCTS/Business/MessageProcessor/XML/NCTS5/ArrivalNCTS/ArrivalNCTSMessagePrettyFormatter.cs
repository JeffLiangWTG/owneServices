using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC007C_v515.CC007CV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class ArrivalNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ArrivalNCTSMessagePrettyFormatter(Cc007Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc007Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);
			messageDetails.Append(blankLine);
			AppendResponseCodeIfS(messageDetails, code: response.ControlRespuesta.CodigoRespuesta);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				AppendAcceptanceDataIfNotEmpty(messageDetails, correctResponseData.Mrn, (ZDateTime)correctResponseData.FechaHoraRecepcion, referenceTextMrn: ArrivalReferenceText);
				AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(correctResponseData.CircuitoRecepcion));
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, SummaryText, correctResponseData.NumeroSumariaRecepcionG4Ultimacion);
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, DiscrepanciesPreviousSummaryText, GetStatusDescriptionDiscrepancies(correctResponseData.IndicadorDescuadreConPrevia));
				AppendStatusData(messageDetails, correctResponseData.Estado);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);

		ZString GetStatusDescriptionDiscrepancies(string discrepancies)
		{
			return discrepancies switch
			{
				Discrepancies1 => Res.GetData("EA07CCCF-3A35-43A9-B2A9-1B47EE7F060B", "No deviations between Transit and Previous Summary").Caption,
				Discrepancies2 => Res.GetData("605EFFCB-AC4F-413C-B134-C8F2E39A07CC", "Deviation in packages quantity between Transit and Previous Summary").Caption,
				Discrepancies3 => Res.GetData("D30B84C7-95F8-4F48-B33B-0396727E9CCF", "Deviation in gross weight  between Transit and Previous Summary").Caption,
				Discrepancies4 => Res.GetData("3961FCD3-3717-4FB3-8310-1447AC4288C3", "Deviation in gross weight  and packages quantity between Transit and Previous Summary").Caption,
				_ => ZString.Empty,
			};
		}

		protected void AppendResponseCodeIfS(StringBuilder messageDetails, string code = null)
		{
			if (code != null && code == ResponseCodeRequestingDataFromCustoms)
			{
				AppendDataInNewTableIfNotEmpty(messageDetails, ResponseCodeText, ResponseCodeS);
			}
		}

		string ResponseCodeS => ResString.GetMultilingualString("61463BC7-F2A0-4999-9DDE-17FBA2F7A4ED", "Requesting data to Customs Departure. ") + ResponseCodeSExtraText;

		const string Discrepancies1 = "1";
		const string Discrepancies2 = "2";
		const string Discrepancies3 = "3";
		const string Discrepancies4 = "4";
	}
}
