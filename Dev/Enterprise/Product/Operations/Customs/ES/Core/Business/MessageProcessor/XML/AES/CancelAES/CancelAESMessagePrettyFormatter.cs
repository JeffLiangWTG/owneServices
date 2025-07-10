using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC514C_v514.CC514CV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class CancelAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CancelAESMessagePrettyFormatter(Cc514Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc514Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") =>
				CreateMessageDetailsAcceptedCancelCommon(response.DatosRespuestaCorrecta != null, response.DatosRespuestaCorrecta.FechaInvalidacion, response.DatosRespuestaCorrecta.CsvDeclaracionElectronica, response.DatosRespuestaCorrecta.EstadoAes);

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
