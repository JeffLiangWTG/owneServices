using System.Collections.ObjectModel;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ConsultaDVDH2V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class QueryDVDMessagePrettyFormatter : DVDCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public QueryDVDMessagePrettyFormatter(ConsultaDvdh2V1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ConsultaDvdh2V1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.Respuesta;
			if (correctResponseData != null)
			{
				var administration = correctResponseData.Administracion == null ? TdAdministracion.Aeat : correctResponseData.Administracion;
				AppendDeclarationDataIfNotEmpty(messageDetails, correctResponseData.TipoDeDeclaracion, string.Empty);
				messageDetails.Append(blankLine);
				AppendMrnDataIfNotEmpty(messageDetails, response.Mensaje.MrnOperacion);
				AppendGroupCircuitWithAdministration(messageDetails, correctResponseData.Circuito, administration);
				messageDetails.Append(blankLine);
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, correctResponseData.CsvLevante, correctResponseData.FechaLevante + correctResponseData.HoraLevante);
				AppendTaxesAndFeesDataIfNotEmptyWithAdministration(messageDetails, correctResponseData.TotalAgarantizar, administration);
				AppendGuaranteesDataIfNotEmptyWithAdministration(messageDetails, correctResponseData.GarantiaGrNutilizada, administration);
				AppendGoodsItemsDataIfNotEmpty(messageDetails, response);
			}

			return messageDetails.ToString();
		}

		void AppendGroupCircuitWithAdministration(StringBuilder messageDetails, TdCircuito? responseCircuitCode, TdAdministracion? administration)
		{
			if (administration == TdAdministracion.Aeat)
			{
				AppendGroupCircuit(messageDetails, responseCircuitCode, null);
			}
			else
			{
				AppendGroupCircuit(messageDetails, null, responseCircuitCode);
			}
		}

		void AppendGuaranteesDataIfNotEmptyWithAdministration(StringBuilder messageDetails, Collection<TdGarantiaGrNutilizada> guarantees, TdAdministracion? administration)
		{
			if (!guarantees.IsNullOrEmpty())
			{
				if (administration == TdAdministracion.Aeat)
				{
					AppendGuaranteesDataIfNotEmpty(messageDetails, guarantees, null);
				}
				else
				{
					AppendGuaranteesDataIfNotEmpty(messageDetails, null, guarantees);
				}
			}
		}

		void AppendTaxesAndFeesDataIfNotEmptyWithAdministration(StringBuilder messageDetails, decimal? guaranteedTotal, TdAdministracion? administration)
		{
			if (guaranteedTotal != null)
			{
				if (administration == TdAdministracion.Aeat)
				{
					AppendTaxesAndFeesDataIfNotEmpty(messageDetails, guaranteedTotal, null);
				}
				else
				{
					AppendTaxesAndFeesDataIfNotEmpty(messageDetails, null, guaranteedTotal);
				}
			}
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedComplete(response);
	}
}
