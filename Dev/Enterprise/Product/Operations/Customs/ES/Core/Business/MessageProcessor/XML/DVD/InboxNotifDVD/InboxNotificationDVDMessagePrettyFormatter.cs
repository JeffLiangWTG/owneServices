using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ActivaPDCVinculacionV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationDVDMessagePrettyFormatter : DVDCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationDVDMessagePrettyFormatter(ActivaPdcVinculacionV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ActivaPdcVinculacionV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.Respuesta;
			if (correctResponseData != null)
			{
				AppendDeclarationDataIfNotEmpty(messageDetails, correctResponseData.TipoDeDeclaracion, correctResponseData.CodigoOperacion);
				messageDetails.Append(blankLine);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				AppendGroupCircuit(messageDetails, correctResponseData.Circuito, correctResponseData.CircuitoAtc);
				messageDetails.Append(blankLine);
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, correctResponseData.CsvLevante, correctResponseData.FechaLevante + correctResponseData.HoraLevante);
				AppendTaxesAndFeesDataIfNotEmpty(messageDetails, correctResponseData.TotalAgarantizar, correctResponseData.TotalAgarantizarAtc);
				AppendGuaranteesDataIfNotEmpty(messageDetails, correctResponseData.GarantiaGrNutilizada, correctResponseData.GarantiaGrNutilizadaAtc);
				AppendGoodsItemsDataIfNotEmpty(messageDetails, response);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedComplete(response);
	}
}
