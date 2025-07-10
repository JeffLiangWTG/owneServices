using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Export.NotifPreDUAV1Sal;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class InboxNotificationExportMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationExportMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var declarationResponse = new NotifPreDuav1Sal();

			declarationResponse.FechaOperacion = "20190522";

			var messagePrettyFormatter = new InboxNotificationExportMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals("<H4>Transaction Date = 22-05-2019</H4><br>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejectedForRejectedMessage()
		{
			var declarationResponse = new NotifPreDuav1Sal();

			declarationResponse.FechaOperacion = "20200402";

			var error1 = new NotifPreDuav1SalRechazoError()
			{
				Codigo = "14992",
				Descripcion = "NIF NO IDENTIFICADO CON LOS PARAMETROS DE CONSULTA ENVIADOS.",
				Localizacion = "CABECERA.EXPORTADOR/EXPEDIDOR CABECERA (CAS 2)"
			};

			var error2 = new NotifPreDuav1SalRechazoError()
			{
				Codigo = "25922",
				Descripcion = "AUTDESP NIU DEL EXPORTADOR SIN CUMPLIMENTAR. -VALOR 0-",
				Localizacion = "CABECERA.AUTORIZACION DESPACHO/TIPO AUT.(CAS14)"
			};

			declarationResponse.Rechazo = new Collection<NotifPreDuav1SalRechazoError>(new NotifPreDuav1SalRechazoError[] { error1, error2 });

			var messagePrettyFormatter = new InboxNotificationExportMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H4>Transaction Date = 02-04-2020</H4><br>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
					"<tr><td>14992</td><td>CABECERA.EXPORTADOR/EXPEDIDOR CABECERA (CAS 2). NIF NO IDENTIFICADO CON LOS PARAMETROS DE CONSULTA ENVIADOS.</td></tr>" +
					"<tr><td>25922</td><td>CABECERA.AUTORIZACION DESPACHO/TIPO AUT.(CAS14). AUTDESP NIU DEL EXPORTADOR SIN CUMPLIMENTAR. -VALOR 0-</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejectedForCancelledMessage()
		{
			var declarationResponse = new NotifPreDuav1Sal();

			declarationResponse.FechaOperacion = "20200212";

			declarationResponse.Anulacion = new NotifPreDuav1SalAnulacion()
			{
				FechaAnulacion = "20200212",
				MotivoAnulacion = "Anulacion Pre-DUA-Exportacion por caducidad"
			};

			var messagePrettyFormatter = new InboxNotificationExportMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H4>Transaction Date = 12-02-2020</H4><br>" +
					"<H3>Pre-SAD cancellation notification</H3>" +
					"<H3>Date: 12-02-2020</H3>" +
					"<H3>Motive: Anulacion Pre-DUA-Exportacion por caducidad</H3>", messageInterpretationText);
		}
	}
}
