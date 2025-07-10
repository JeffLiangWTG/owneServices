using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class EdiFactMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new EdiFactMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var mockTestHelper = new Mock<ICUSRESMessageProvider>();

			var messagePrettyFormatter = new EdiFactMessagePrettyFormatter(mockTestHelper.Object);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals("Test Accepted Message Details", "<H3>Accepted Declaration</H3>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var mockTestHelper = new Mock<ICUSRESMessageProvider>();
			var error1 = SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text");
			var error2 = SetUpError("55243", "CABECERA.DECLARANTE", "Ubicacion debe de ser p·blica.");
			var error3 = SetUpError("55570", "CABECERA.TIR CARNET (CAS. 1).", "Cuaderno TIR incorrecto.");
			var error4 = SetUpError("55318", "CABECERA.COUNTRY DEPARTURE (CAS. 15)", "C¾digo paÝs origen err¾neo.");
			var error5 = SetUpError("55533", "PARTIDA(1).DOCUMENTO PRESENTADO (CAS 44)(1)", "Fecha de Validez no puede ser menor de la fecha en curso.");
			mockTestHelper.Setup(m => m.FreeTextErrors).Returns(new List<ErrorMessage> { error1, error2, error3, error4, error5 });

			var messagePrettyFormatter = new EdiFactMessagePrettyFormatter(mockTestHelper.Object);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H3>Rejected Declaration</H3>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Error</strong></td><td><strong>Location</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>55110</td><td>CABECERA.Other Text</td><td>El estado del Tránsito impide la operación..More Text</td></tr>" +
					"<tr><td>55243</td><td>CABECERA.DECLARANTE</td><td>Ubicacion debe de ser p·blica.</td></tr>" +
					"<tr><td>55570</td><td>CABECERA.TIR CARNET (CAS. 1).</td><td>Cuaderno TIR incorrecto.</td></tr>" +
					"<tr><td>55318</td><td>CABECERA.COUNTRY DEPARTURE (CAS. 15)</td><td>C¾digo paÝs origen err¾neo.</td></tr>" +
					"<tr><td>55533</td><td>PARTIDA(1).DOCUMENTO PRESENTADO (CAS 44)(1)</td><td>Fecha de Validez no puede ser menor de la fecha en curso.</td></tr>" +
					"</table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsErrorResponse()
		{
			var mockTestHelper = new Mock<ICUSRESMessageProvider>();
			var error1 = SetUpError("55110", ZString.Empty, "El estado del Tránsito impide la operación.");
			mockTestHelper.Setup(m => m.FreeTextErrors).Returns(new List<ErrorMessage> { error1 });

			var messagePrettyFormatter = new EdiFactMessagePrettyFormatter(mockTestHelper.Object);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H3>Rejected Declaration</H3>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>55110</td><td>El estado del Tránsito impide la operación.</td></tr>" +
					"</table>", messageInterpretationText);
		}

		ErrorMessage SetUpError(ZString code, ZString location, ZString description)
		{
			return new ErrorMessage
			{
				Code = code,
				Location = location,
				Description = description
			};
		}
	}
}
