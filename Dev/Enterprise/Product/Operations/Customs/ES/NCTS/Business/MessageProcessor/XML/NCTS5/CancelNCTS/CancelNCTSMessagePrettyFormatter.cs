using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC014C_v515.CC014CV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class CancelNCTSMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CancelNCTSMessagePrettyFormatter(Cc014Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc014Cv1Sal response;

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") =>
			CreateMessageDetailsAcceptedCancelCommon(response.DatosRespuestaCorrecta != null, response.DatosRespuestaCorrecta.FechaInvalidacion, response.DatosRespuestaCorrecta.CsvDeclaracionElectronica, response.DatosRespuestaCorrecta.Estado);
	}
}
