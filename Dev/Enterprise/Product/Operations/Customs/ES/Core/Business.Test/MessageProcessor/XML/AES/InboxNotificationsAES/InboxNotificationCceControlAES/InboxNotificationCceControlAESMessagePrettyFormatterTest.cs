using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaControlesCCEV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationCceControlAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new InboxNotificationCceControlAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var controlType1 = SetControlType(1, "40", "Mirar detenidamente todas las cajas por escaner.");
			var controlType2 = SetControlType(2, "30", ZString.Empty);
			var controlTypeList = new Collection<TipoControlType> { controlType1, controlType2 };

			var doc1 = SetDocument(1, "C055", "Declaracion de conformidad (Anexo IV del Reglamento (UE) No 10/2011)");
			var doc2 = SetDocument(2, "C077", ZString.Empty);
			var docList = new Collection<DocumentoSolicitadoType> { doc1, doc2 };

			var declarationResponse = SetResponseData(new DateTime(2020, 10, 20, 10, 50, 30), "21ES00999912345678", "0", new DateTime(2022, 09, 20, 10, 50, 30), "PL", controlTypeList, docList);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CONCCE - Control needed at CCE</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>20-10-2020, 10:50:30</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Control Notification Date:</td><td>&nbsp;&nbsp;</td><td>20-09-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Notification Type:</td><td>&nbsp;&nbsp;</td><td>0 - Decission to Control (and requested documents if needed)</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>" +
				"<H4>Type of Control</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Item</strong></td><td><strong>Type</strong></td><td><strong>Description</strong></td></tr>" +
				"<tr><td>1</td><td>40 - Physical controls</td><td>Mirar detenidamente todas las cajas por escaner.</td></tr>" +
				"<tr><td>2</td><td>30 - Non-intrusive inspection</td><td>&nbsp;</td></tr></table>" +
				"<H4>Required Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Item</strong></td><td><strong>Type</strong></td><td><strong>Description</strong></td></tr>" +
				"<tr><td>1</td><td>C055</td><td>Declaracion de conformidad (Anexo IV del Reglamento (UE) No 10/2011)</td></tr>" +
				"<tr><td>2</td><td>C077</td><td>&nbsp;</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var messagePrettyFormatter = new InboxNotificationCceControlAESMessagePrettyFormatter(new ComunicaControlesCcev1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();
			AssertEquals("Expected empty Rejected declaration message interpretation text", ZString.Empty, messageInterpretationText);
		}

		public void TestNotificationTypeData()
		{
			var controlType0 = SetControlType(1, "0", ZString.Empty);
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType0 }, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedNotificationType0Text = "<table border=\"0\"><tr><td>Notification Type:</td><td>&nbsp;&nbsp;</td><td>0 - Decission to Control (and requested documents if needed)</td></tr></table>";
			var expectedNotificationType1Text = "<table border=\"0\"><tr><td>Notification Type:</td><td>&nbsp;&nbsp;</td><td>1 - Additional documents request</td></tr></table>";
			var expectedNotificationType2Text = "<table border=\"0\"><tr><td>Notification Type:</td><td>&nbsp;&nbsp;</td><td>2 - Intention to Control</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Notification Type NOT included if it's not in the response", expectedNotificationType0Text, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "0", DateTime.Now, ZString.Empty, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Notification Type 0", expectedNotificationType0Text, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "1", DateTime.Now, ZString.Empty, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Notification Type 1", expectedNotificationType1Text, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, "2", DateTime.Now, ZString.Empty, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Notification Type 2", expectedNotificationType2Text, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var controlType0 = SetControlType(1, "0", ZString.Empty);
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType0 }, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedPLStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>";
			var expectedAWStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedPLStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, "PL", null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PL Status", expectedPLStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, "AW", null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AW Status", expectedAWStatusText, messageInterpretationText);
			});
		}

		public void TestControlTypeData()
		{
			var controlType0 = SetControlType(1, "0", ZString.Empty);
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType0 }, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedControlType10Text = "<tr><td>1</td><td>10 - Documentary Controls</td><td>&nbsp;</td></tr></table>";
			var expectedControlType20Text = "<tr><td>1</td><td>20 - Nuclear/radioactive material check</td><td>&nbsp;</td></tr></table>";
			var expectedControlType30Text = "<tr><td>1</td><td>30 - Non-intrusive inspection</td><td>&nbsp;</td></tr></table>";
			var expectedControlType40Text = "<tr><td>1</td><td>40 - Physical controls</td><td>&nbsp;</td></tr></table>";
			var expectedControlType41Text = "<tr><td>1</td><td>41 - Identification of consignment and seals</td><td>&nbsp;</td></tr></table>";
			var expectedControlType42Text = "<tr><td>1</td><td>42 - Intrusive inspection</td><td>&nbsp;</td></tr></table>";
			var expectedControlType43Text = "<tr><td>1</td><td>43 - Quantity control / Partial or total</td><td>&nbsp;</td></tr></table>";
			var expectedControlType44Text = "<tr><td>1</td><td>44 - Nature and characteristics of the goods</td><td>&nbsp;</td></tr></table>";
			var expectedControlType45Text = "<tr><td>1</td><td>45 - Sampling</td><td>&nbsp;</td></tr></table>";
			var expectedControlType50Text = "<tr><td>1</td><td>50 - Other</td><td>&nbsp;</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Control Type row NOT included if it's not in the response", expectedControlType10Text, messageInterpretationText);

				var controlType10 = SetControlType(1, "10", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType10 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 10", expectedControlType10Text, messageInterpretationText);

				var controlType20 = SetControlType(1, "20", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType20 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 20", expectedControlType20Text, messageInterpretationText);

				var controlType30 = SetControlType(1, "30", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType30 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 30", expectedControlType30Text, messageInterpretationText);

				var controlType40 = SetControlType(1, "40", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType40 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 40", expectedControlType40Text, messageInterpretationText);

				var controlType41 = SetControlType(1, "41", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType41 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 41", expectedControlType41Text, messageInterpretationText);

				var controlType42 = SetControlType(1, "42", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType42 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 42", expectedControlType42Text, messageInterpretationText);

				var controlType43 = SetControlType(1, "43", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType43 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 43", expectedControlType43Text, messageInterpretationText);

				var controlType44 = SetControlType(1, "44", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType44 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 44", expectedControlType44Text, messageInterpretationText);

				var controlType45 = SetControlType(1, "45", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType45 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 45", expectedControlType45Text, messageInterpretationText);

				var controlType50 = SetControlType(1, "50", ZString.Empty);
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType50 }, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Control Type row for 50", expectedControlType50Text, messageInterpretationText);
			});
		}

		public void TestRequiredDocumentsData()
		{
			var controlType0 = SetControlType(1, "0", ZString.Empty);
			var declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, new Collection<TipoControlType> { controlType0 }, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<H4>Required Documents</H4>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Required Documents NOT included if it's not in the response", expectedText, messageInterpretationText);

				var doc = SetDocument(2, "C077", ZString.Empty);
				var docList = new Collection<DocumentoSolicitadoType> { doc };
				declarationResponse = SetResponseData(DateTime.Now, ZString.Empty, ZString.Empty, DateTime.Now, ZString.Empty, null, docList);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Required Documents", expectedText, messageInterpretationText);
			});
		}

		ComunicaControlesCcev1Sal SetResponseData(DateTime acceptanceDate, ZString mrn, ZString notificationType, DateTime notificationDate, ZString status, Collection<TipoControlType> controlTypeList, Collection<DocumentoSolicitadoType> documentList = null)
		{
			var response = new ComunicaControlesCcev1Sal();
			response.PreparationDateAndTime = acceptanceDate;
			response.DatosComunicacion = new DatosComunicacionControlesCceType()
			{
				Mrn = mrn,
				TipoNotificacion = notificationType,
				FechaNotificacionControl = notificationDate,
				EstadoAes = status
			};
			response.TipoDeControl = controlTypeList;
			response.DocumentoSolicitado = documentList;
			return response;
		}

		TipoControlType SetControlType(int seqNum, ZString typeCode, ZString text)
		{
			var controlType = new TipoControlType()
			{
				NumeroSecuencia = (uint)seqNum,
				Tipo = typeCode,
				Texto = text
			};
			return controlType;
		}

		DocumentoSolicitadoType SetDocument(int seqNum, ZString typeCode, ZString description)
		{
			var doc = new DocumentoSolicitadoType()
			{
				NumeroSecuencia = (uint)seqNum,
				TipoDocumento = typeCode,
				Descripcion = description
			};
			return doc;
		}

		ZString GetAcceptedInterpretationText(ComunicaControlesCcev1Sal response)
		{
			var messagePrettyFormatter = new InboxNotificationCceControlAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
