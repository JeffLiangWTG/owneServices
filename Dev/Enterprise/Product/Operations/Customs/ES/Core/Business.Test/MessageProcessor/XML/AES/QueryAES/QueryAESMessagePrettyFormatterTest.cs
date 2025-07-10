using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCAESC_v514.CCAESCV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class QueryAESMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new QueryAESMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var declarationResponse = SetResponseData("22ES000101100825A6", "A1", "PA", "2022-10-20", new DateTime(2022, 10, 11), "V", "V", "CD89SVWH9K852Q6M", new DateTime(2022, 10, 12),
														"I", "A2", new DateTime(2022, 10, 13), "64LQAGQ4Z4FYK8L7", new DateTime(2022, 10, 16), new DateTime(2022, 10, 14), "Place",
														"V", "AAAAAAAAAAAAAAAA", new DateTime(2022, 10, 15), new DateTime(2022, 10, 10), null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<H4>Management Data</H4>" +
				"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100825A6</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>CD89SVWH9K852Q6M</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>64LQAGQ4Z4FYK8L7</td></tr>" +
				"<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>13-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>10-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Exit Stop Date:</td><td>&nbsp;&nbsp;</td><td>16-10-2022</td></tr></table>" +
				"<H4>Exit Control</H4>" +
				"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>14-10-2022</td></tr>" +
				"<tr><td>Arrival Location:</td><td>&nbsp;&nbsp;</td><td>Place</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>15-10-2022</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Ccaescv1Sal();
			declarationResponse.ControlRespuesta = new Ccaescv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CCAESC/ExportOperation/LRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CCAESC/ExportOperation/LRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new QueryAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CCAESC/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>14</td><td>/CCAESC/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new Ccaescv1Sal();
			declarationResponse.ControlRespuesta = new Ccaescv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError("18", "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCAESCXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCAESCXV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(ZString.Empty, "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new QueryAESMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCAESCXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCAESCXV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestVersionData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDirectExitTypeText = "<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>AES (XML)</td></tr></table>";
			var expectedIndirectExitTypeText = "<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>ECS (EDI)</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Version NOT included if it's not in the response", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "A1", ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AES Version", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "E2", ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test EDI Version", expectedIndirectExitTypeText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDEStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Indirect Dispatch</td></tr></table>";
			var expectedDSStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DS - Direct Dispatch</td></tr></table>";
			var expectedPLStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PL - Pending Clearance</td></tr></table>";
			var expectedAWStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";
			var expectedSAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>SA - Effective Exit</td></tr></table>";
			var expectedPAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pending Presentation of Goods</td></tr></table>";
			var expectedCAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>CA - Declaration Canceled</td></tr></table>";
			var expectedPIStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";
			var expectedIVStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>";
			var expectedNLStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not cleared</td></tr></table>";
			var expectedGTStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>GT - Managed by Transit</td></tr></table>";
			var expectedGOStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>GO - Managed in other place</td></tr></table>";
			var expectedTPStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>TP - Pending Transit evidence</td></tr></table>";
			var expectedRQStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RQ - Pending to UE</td></tr></table>";
			var expectedREStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received and Under Control</td></tr></table>";
			var expectedRZStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RZ - Rejected deviation</td></tr></table>";
			var expectedROStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RO - Received in another country</td></tr></table>";
			var expectedPSStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PS - Pending Departure</td></tr></table>";
			var expectedSTStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>ST - Stop at Exit</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Status NOT included if it's not in the response", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "DE", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "DS", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DS Status", expectedDSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PL", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PL Status", expectedPLStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "AW", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test AW Status", expectedAWStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "SA", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test SA Status", expectedSAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PA", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PA Status", expectedPAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "CA", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CA Status", expectedCAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PI", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PI Status", expectedPIStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "IV", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test IV Status", expectedIVStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "NL", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test NL Status", expectedNLStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "GT", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test GT Status", expectedGTStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "GO", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test GO Status", expectedGOStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "TP", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test TP Status", expectedTPStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "RQ", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RQ Status", expectedRQStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "RE", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RE Status", expectedREStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "RZ", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RZ Status", expectedRZStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "RO", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RO Status", expectedROStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PS", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PS Status", expectedPSStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "ST", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test ST Status", expectedSTStatusText, messageInterpretationText);
			});
		}

		public void TestInvalidationDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Invalidation date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "2021-02-20", null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Invalidation date", expectedText, messageInterpretationText);
			});
		}

		public void TestAcceptanceDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Acceptance date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Acceptance date", expectedText, messageInterpretationText);
			});
		}

		public void TestCircuitAEATData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, "V", ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, "N", ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, "R", ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCircuitATCData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit ATC NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, "V", ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit ATC", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, "N", ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit ATC", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, "R", ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit ATC", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance data NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, "3AG5G6SSCJJ93NML", null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance data", expectedText, messageInterpretationText);
			});
		}

		public void TestReleaseDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Release date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, "3AG5G6SSCJJ93NML", new DateTime(2021, 02, 20, 05, 50, 30),
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Release date when date and csvCode are in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30),
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Release date NOT included when date is in the response but csvCode is not", expectedText, messageInterpretationText);
			});
		}

		public void TestExitTypeData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDirectExitTypeText = "<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[D] Direct</td></tr></table>";
			var expectedIndirectExitTypeText = "<br><table border=\"0\"><tr><td>Exit Type:</td><td>&nbsp;&nbsp;</td><td>[I] Indirect</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit Type NOT included if it's not in the response", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														"D", ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Direct Exit Type", expectedDirectExitTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														"I", ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Indirect Exit Type", expectedIndirectExitTypeText, messageInterpretationText);
			});
		}

		public void TestExitResultData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedA1ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A1 - Satisfied</td></tr></table>";
			var expectedA2ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A2 - Satisfied</td></tr></table>";
			var expectedA4ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>A4 - Satisfied</td></tr></table>";
			var expectedB1ExitResultText = "<br><table border=\"0\"><tr><td>Exit Result:</td><td>&nbsp;&nbsp;</td><td>B1 - Dissatisfied</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit Result NOT included if it's not in the response", expectedA1ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, "A1", null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A1 Exit Result", expectedA1ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, "A2", null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A2 Exit Result", expectedA2ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, "A4", null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A4 Exit Result", expectedA4ExitResultText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, "B1", null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test B1 Exit Result", expectedB1ExitResultText, messageInterpretationText);
			});
		}

		public void TestEffectiveDepartureClearanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Effective Departure Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Effective Departure Clearance data NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, "3AG5G6SSCJJ93NML", null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Effective Departure Clearance data", expectedText, messageInterpretationText);
			});
		}

		public void TestEffectiveDepartureDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<tr><td>Effective Departure Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Effective Departure date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), "3AG5G6SSCJJ93NML", null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Effective Departure date when date and csvCode are in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Effective Departure date NOT included when date is in the response but csvCode is not", expectedText, messageInterpretationText);
			});
		}

		public void TestExitGoodsDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Exit Goods Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit Goods date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, new DateTime(2021, 02, 20, 05, 50, 30), null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Exit Goods date", expectedText, messageInterpretationText);
			});
		}

		public void TestExitStopDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Exit Stop Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit Stop date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, new DateTime(2021, 02, 20, 05, 50, 30));
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Exit Stop date when ExitStoppedDate is filled", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, new DateTime(2021, 04, 15, 05, 50, 30));
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Exit Stop date when FechaParadaAduanaSalida is filled (even when ExitStoppedDate is filled)", expectedText, messageInterpretationText);
			});
		}

		public void TestArrivalDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Arrival date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Arrival date", expectedText, messageInterpretationText);
			});
		}

		public void TestArrivalLocationData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Arrival Location:</td><td>&nbsp;&nbsp;</td><td>Place</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Arrival Location data NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, "Place",
														ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Arrival Location data", expectedText, messageInterpretationText);
			});
		}

		public void TestCircuitExitData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														"V", ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														"N", ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														"R", ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceExitData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance Exit data NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, "3AG5G6SSCJJ93NML", null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance Exit data", expectedText, messageInterpretationText);
			});
		}

		public void TestReleaseExitDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Exit date NOT included if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, "3AG5G6SSCJJ93NML", new DateTime(2021, 02, 20, 05, 50, 30), null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Exit date when date and csvCode are in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, null,
														ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, ZString.Empty,
														ZString.Empty, ZString.Empty, new DateTime(2021, 02, 20, 05, 50, 30), null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Exit date NOT included when date is in the response but csvCode is not", expectedText, messageInterpretationText);
			});
		}

		Ccaescv1Sal SetResponseData(ZString mrn, ZString phase, ZString status, ZString invalidationDate, DateTime? acceptanceDate, ZString circuitAEAT, ZString circuitATC, ZString csvCode, DateTime? csvClearanceDate,
			ZString directFlag, ZString exitResult, DateTime? effectiveDepartureDate, ZString effectiveDepartureClearance, DateTime? exitStopDateInGestion, DateTime? arrivalDate, ZString arrivalLocation,
			ZString arrivalCircuit, ZString exitClearance, DateTime? exitClearanceDate, DateTime? exitGoodsDate, DateTime? exitStopDateInExitControl)
		{
			var response = new Ccaescv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType70()
			{
				ExportOperation = new ExportOperationType70()
				{
					Mrn = mrn,
				},
				AesDatosGestion = new AesDatosGestion70()
				{
					FaseAes = phase,
					EstadoAes = status,
					FechaInvalidacion = invalidationDate,
					FechaAdmision = acceptanceDate,
					CircuitoAeat = circuitAEAT,
					CircuitoAtc = circuitATC,
					CsvLevanteExportacion = csvCode,
					FechaLevante = csvClearanceDate,
					FlagDirectaIndirecta = directFlag,
					ResultadoSalida = exitResult,
					FechaSalidaEfectiva = effectiveDepartureDate,
					CsvCertificadoSalida = effectiveDepartureClearance,
					FechaParadaAduanaSalida = exitStopDateInGestion,
					FechaLlegada = arrivalDate,
					UbicacionLlegada = arrivalLocation,
					CircuitoLlegada = arrivalCircuit,
					CsvLevanteSalida = exitClearance,
					FechaLevanteSalida = exitClearanceDate,
				},
				ExitControlResult = new ExitControlResultType70()
				{
					ExitDate = exitGoodsDate,
					ExitStoppedDate = exitStopDateInExitControl
				}
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Ccaescv1Sal response)
		{
			var messagePrettyFormatter = new QueryAESMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		FunctionalErrorType SetFunctionalError(ZString errorCode, ZString errorDescription, ZString errorLocation, ZString wrongValue, ZString reason)
		{
			var error = new FunctionalErrorType();
			error.ErrorCode = errorCode;
			error.ErrorDescription = errorDescription;
			error.ErrorPointer = errorLocation;
			error.OriginalAttributeValue = wrongValue;
			error.ErrorReason = reason;
			return error;
		}

		XmlErrorType SetXMLError(ZString errorCode, ZString errorLineNumber, ZString errorColumnNumber, ZString errorText, ZString wrongValue, ZString location)
		{
			var error = new XmlErrorType();
			error.ErrorCode = errorCode;
			error.ErrorLineNumber = errorLineNumber;
			error.ErrorColumnNumber = errorColumnNumber;
			error.ErrorText = errorText;
			error.OriginalAttributeValue = wrongValue;
			error.ErrorPointer = location;
			return error;
		}
	}
}
