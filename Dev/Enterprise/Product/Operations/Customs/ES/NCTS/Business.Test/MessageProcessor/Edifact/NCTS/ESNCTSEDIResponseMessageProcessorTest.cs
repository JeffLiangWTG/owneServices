using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public abstract class ESNCTSEDIResponseMessageProcessorTest<TResponseMessageProcessor, TResponseProvider> : ESNCTSResponseMessageProcessorTest<TResponseMessageProcessor, TResponseProvider>
	where TResponseMessageProcessor : ESNCTSResponseMessageProcessor<TResponseProvider>
	where TResponseProvider : class, ICUSRESMessageProvider
	{
		public void TestProcessMessageRejected()
		{
			var error1 = SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text");
			var error2 = SetUpError("55243", "CABECERA.DECLARANTE", "Ubicacion debe de ser p·blica.");
			var error3 = SetUpError("55570", "CABECERA.TIR CARNET (CAS. 1).", "Cuaderno TIR incorrecto.");
			var error4 = SetUpError("55318", "CABECERA.COUNTRY DEPARTURE (CAS. 15)", "C¾digo paÝs origen err¾neo.");
			var error5 = SetUpError("55533", "PARTIDA(1).DOCUMENTO PRESENTADO (CAS 44)(1)", "Fecha de Validez no puede ser menor de la fecha en curso.");

			var mockMessageProcessorData = new MockResponseProvider()
			{
				MessageName = "963",
				AdmissionDate = ZDateTime.Empty,
				MessageFunction = "2",
				ErrorList = new List<ErrorMessage> { error1, error2, error3, error4, error5 }
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedCUSRESMessageProvider(mockMessageProcessorData).Object);

			processor.ProcessMessage(message);

			var expectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
					"<tr><td><strong>Error</strong></td><td><strong>Location</strong></td><td><strong>Description</strong></td></tr>" +
					"<tr><td>55110</td><td>CABECERA.Other Text</td><td>El estado del Tránsito impide la operación..More Text</td></tr>" +
					"<tr><td>55243</td><td>CABECERA.DECLARANTE</td><td>Ubicacion debe de ser p·blica.</td></tr>" +
					"<tr><td>55570</td><td>CABECERA.TIR CARNET (CAS. 1).</td><td>Cuaderno TIR incorrecto.</td></tr>" +
					"<tr><td>55318</td><td>CABECERA.COUNTRY DEPARTURE (CAS. 15)</td><td>C¾digo paÝs origen err¾neo.</td></tr>" +
					"<tr><td>55533</td><td>PARTIDA(1).DOCUMENTO PRESENTADO (CAS 44)(1)</td><td>Fecha de Validez no puede ser menor de la fecha en curso.</td></tr>" +
					"</table>";

			var expectedMessageStatus = nctsHeader.BH_HeaderType == NctsMovementType.Codes.Departure ? "RCV" : NctsMessageStatusList.Codes.ArrivalNotificationRejected;

			AssertNCTS(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "REJ", commonCustomsStatus: NctsTransitStatusList.Codes.DeclarationRejected, messageStatus: expectedMessageStatus);
			AssertEquals("MovementHeader.BM_CustomsStatus", NctsTransitStatusList.Codes.DeclarationRejected, GetBM_CustomsStatus());
		}

		public void TestErrorResponse()
		{
			SetSentInterchange(nctsHeader, InterchangeID);
			var responseMessage = CreateNewEDIMessage(ApplicationReference, GetEdifactErrorMessageFile(), InterchangeID, serializeMessageText: false);

			ProcessMessageForTest(responseMessage);

			var expectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>50052</td><td>Error Traducción:Segmento (TPL) Mensaje erroneo</td></tr>" +
						"</table>";
			AssertNCTS(responseMessage, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "REJ");
		}

		protected Mock<TResponseProvider> GetMockedCUSRESMessageProvider(MockResponseProvider providerData)
		{
			var mockTestHelper = new Mock<TResponseProvider> { CallBase = true };
			mockTestHelper.Setup(m => m.DocumentMessageName).Returns(providerData.MessageName);
			mockTestHelper.Setup(m => m.AdmissionDate).Returns(providerData.AdmissionDate);
			mockTestHelper.Setup(m => m.MessageFunction).Returns(providerData.MessageFunction);
			mockTestHelper.Setup(m => m.FreeTextErrors).Returns(providerData.ErrorList);
			return mockTestHelper;
		}

		protected abstract TResponseMessageProcessor GetMockedProcessor(ICUSRESMessageProvider messageProvider);

		protected ErrorMessage SetUpError(ZString code, ZString location, ZString description)
		{
			return new ErrorMessage
			{
				Code = code,
				Location = location,
				Description = description
			};
		}

		protected abstract ZString GetBM_CustomsStatus();

		string GetEdifactErrorMessageFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EdifactTestFilePath, "EdifactErrorMessage.txt");

		protected class MockResponseProvider
		{
			public ZString MessageName { get; set; }
			public ZDateTime AdmissionDate { get; set; }
			public ZString MessageFunction { get; set; }
			public List<ErrorMessage> ErrorList { get; set; }
		}
	}
}
