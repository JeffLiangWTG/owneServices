using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTRAC_v515.CCTRACV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class QueryNCTSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new QueryNCTSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedPA()
		{
			var declarationResponse = SetResponseData(MRN, VersionP5, "PA", new DateTime(2022, 10, 12), new DateTime(2022, 10, 11, 10, 10, 10), "V", Clearance, new DateTime(2022, 10, 12), "V",
						new DateTime(2022, 10, 20), ResultB11, new DateTime(2022, 10, 09, 10, 10, 10), Location, new DateTime(2023, 07, 02));

			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H4>Management Data</H4>" +
				"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>NCTS5 (XML)</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pre-Declaration</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>11-10-2022, 10:10:10</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRN + "</td></tr></table>" +
				"<br><br><H4>Departure</H4>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>20-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>" + Clearance + "</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-10-2022</td></tr></table>" +
				"<br><H2>Guarantees</H2>" +
				"<br><table border=\"0\"><tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not written Off (status not UL)</td></tr></table>" +
				"<br><br><H4>Arrival</H4>" +
				"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>09-10-2022, 10:10:10</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Location:</td><td>&nbsp;&nbsp;</td><td>" + Location + "</td></tr></table>" +
				"<br><br><H4>Unloading</H4>" +
				"<br><table border=\"0\"><tr><td>Unloading Result Date:</td><td>&nbsp;&nbsp;</td><td>20-10-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Ultimate Date:</td><td>&nbsp;&nbsp;</td><td>02-07-2023</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Result:</td><td>&nbsp;&nbsp;</td><td>Major discrepancies in unloading but unlocked guarantee (B11)</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Cctracv1Sal();
			declarationResponse.ControlRespuesta = new Cctracv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CCTRAC/ExportOperation/LRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CCTRAC/ExportOperation/LRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new QueryNCTSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CCTRAC/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>14</td><td>/CCTRAC/ExportOperation/LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new Cctracv1Sal();
			declarationResponse.ControlRespuesta = new Cctracv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError(XmlErrorCodes.Item18, "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCTRACXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCTRACXV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(XmlErrorCodes.Item40, "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new QueryNCTSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>Item18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCTRACXV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCTRACXV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>Item40</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestVersionData()
		{
			var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());

			var expectedVersionXML5Text = "<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>NCTS5 (XML)</td></tr></table>";
			var expectedVersionXML4Text = "<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>NCTS4 (EDI)</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Version NOT included if it's not in the response", expectedVersionXML5Text, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, VersionP5, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);

				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test NCTS5 Version", expectedVersionXML5Text, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "P4", ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);

				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test NCTS4 EDI Version", expectedVersionXML4Text, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{
			var expectedPAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pre-Declaration</td></tr></table>";
			var expectedPGStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PG - Pending Guarantee</td></tr></table>";
			var expectedPDStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";
			var expectedDEStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";
			var expectedCAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>CA - Declaration Canceled</td></tr></table>";
			var expectedPIStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";
			var expectedIGStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IG - Invalidated by Guarantee</td></tr></table>";
			var expectedIVStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>";
			var expectedNLStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not Cleared</td></tr></table>";
			var expectedRQStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RQ - Requesting to the country of departure</td></tr></table>";
			var expectedRZStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RZ - Deviation Rejected By UE</td></tr></table>";
			var expectedREStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received</td></tr></table>";
			var expectedLIStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>LI - Liquidation Initiated</td></tr></table>";
			var expectedUCStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>UC - Ultimated For Payment</td></tr></table>";
			var expectedRDStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RD - Pending Resolution Of Discrepancy</td></tr></table>";
			var expectedULStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>UL - Completed</td></tr></table>";

			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Status NOT included if it's not in the response", expectedDEStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("PA"));
				AssertContains("Test PA Status", expectedPAStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("PG"));
				AssertContains("Test PG Status", expectedPGStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("PD"));
				AssertContains("Test PD Status", expectedPDStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("DE"));
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("CA"));
				AssertContains("Test CA Status", expectedCAStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("PI"));
				AssertContains("Test PI Status", expectedPIStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("IG"));
				AssertContains("Test IG Status", expectedIGStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("IV"));
				AssertContains("Test IV Status", expectedIVStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("NL"));
				AssertContains("Test NL Status", expectedNLStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("RQ"));
				AssertContains("Test RQ Status", expectedRQStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("RZ"));
				AssertContains("Test RZ Status", expectedRZStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("RE"));
				AssertContains("Test RE Status", expectedREStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("LI"));
				AssertContains("Test LI Status", expectedLIStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("UC"));
				AssertContains("Test UC Status", expectedUCStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("RD"));
				AssertContains("Test RD Status", expectedRDStatusText, messageInterpretationText);

				messageInterpretationText = GetAcceptedInterpretationText(definesStatus("UL"));
				AssertContains("Test UL Status", expectedULStatusText, messageInterpretationText);
			});
		}

		Cctracv1Sal definesStatus(ZString status)
		{
			return SetResponseData(ZString.Empty, ZString.Empty, status, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
				null, ZString.Empty, null, ZString.Empty, null);
		}

		Cctracv1Sal definesEmptyCCTRACV1Sal()
		{
			return SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
				null, ZString.Empty, null, ZString.Empty, null);
		}

		public void TestCancellationDate()
		{
			var expectedText = "<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>21-02-2021</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Cancellation date NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, new DateTime(2021, 02, 21), null, ZString.Empty, ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Cancellation date", expectedText, messageInterpretationText);
			});
		}

		public void TestLimitDate()
		{
			var expectedText = "<table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>20-10-2022</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Limit date NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
										new DateTime(2022, 10, 20), ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Limit date", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, Ncts5TransitStatusList.Codes.DeclarationCancelled, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
						new DateTime(2022, 10, 20), ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Limit date when status = CA", expectedText, messageInterpretationText);
			});
		}

		public void TestAcceptanceDate()
		{
			var expectedText = "<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-02-2021, 05:50:30</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Acceptance date NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Acceptance date", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, Ncts5TransitStatusList.Codes.DeclarationCancelled, null, new DateTime(2021, 02, 20, 05, 50, 30), ZString.Empty, ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Acceptance date when status = CA", expectedText, messageInterpretationText);
			});
		}

		public void TestUltimateDate()
		{
			var expectedText = "<table border=\"0\"><tr><td>Ultimate Date:</td><td>&nbsp;&nbsp;</td><td>02-07-2023</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Ultimate date NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, new DateTime(2023, 07, 02));
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Ultimate date", expectedText, messageInterpretationText);
			});
		}

		public void TestArrivalDate()
		{
			var expectedText = "<table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>19-02-2021, 15:51:32</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Arrival date NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, new DateTime(2021, 02, 19, 15, 51, 32), ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Arrival date", expectedText, messageInterpretationText);
			});
		}

		public void TestLocation()
		{
			var expectedText = "<table border=\"0\"><tr><td>Location:</td><td>&nbsp;&nbsp;</td><td>" + Location + "</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Location NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(MRN, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, Location, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Location", expectedText, messageInterpretationText);
			});
		}

		public void TestExpeditionCircuitData()
		{
			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "V", ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "N", ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "R", ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, Ncts5TransitStatusList.Codes.DeclarationCancelled, null, null, "V", ZString.Empty, null, ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Green Circuit when status = CA", expectedGreenCircuitText, messageInterpretationText);
			});
		}

		public void TestArrivalCircuitData()
		{
			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "V", ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "N", ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "R", ZString.Empty, null, ZString.Empty,
										null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestClearanceData()
		{
			var expectedText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>" + Clearance + "</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Clearance data NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, Clearance, null, ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Clearance data", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, Ncts5TransitStatusList.Codes.DeclarationCancelled, null, null, ZString.Empty, Clearance, null, ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Clearance data when status = CA", expectedText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceDate()
		{
			var expectedText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>24-02-2021</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test CSV Clearance date NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, new DateTime(2021, 02, 24), ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test CSV Clearance date but without Clearance data", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, Clearance, new DateTime(2021, 02, 24), ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance date", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, Ncts5TransitStatusList.Codes.DeclarationCancelled, null, null, ZString.Empty, Clearance, new DateTime(2021, 02, 24), ZString.Empty,
						null, ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test CSV Clearance date when status = CA", expectedText, messageInterpretationText);
			});
		}

		public void TestUnloadingDate()
		{
			var expectedText = "<table border=\"0\"><tr><td>Unloading Result Date:</td><td>&nbsp;&nbsp;</td><td>25-02-2021</td></tr></table>";
			CombineAssertions(() =>
			{
				var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());
				AssertNotContains("Test Unloading date of arrival NOT included if it's not in the response", expectedText, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
										new DateTime(2021, 02, 25), ZString.Empty, null, ZString.Empty, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Unloading date of arrival", expectedText, messageInterpretationText);
			});
		}

		public void TestUnloadingResult()
		{
			var messageInterpretationText = GetAcceptedInterpretationText(definesEmptyCCTRACV1Sal());

			var expectedResultB11Text = "<br><table border=\"0\"><tr><td>Result:</td><td>&nbsp;&nbsp;</td><td>Major discrepancies in unloading but unlocked guarantee (B11)</td></tr></table>";
			var expectedResultB12Text = "<br><table border=\"0\"><tr><td>Result:</td><td>&nbsp;&nbsp;</td><td>Major discrepancies in unloading and without unlocking guarantee (B12)</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Result NOT included if it's not in the response", expectedResultB11Text, messageInterpretationText);

				var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
						null, ResultB11, null, ZString.Empty, null);

				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Result B11", expectedResultB11Text, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, null, ZString.Empty,
						null, "B12", null, ZString.Empty, null);

				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Result B12", expectedResultB12Text, messageInterpretationText);
			});
		}

		public void TestGuaranteeStatusData()
		{
			var messagePrettyFormatter = new QueryNCTSMessagePrettyFormatter(definesEmptyCCTRACV1Sal());
			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedGuaranteeStatusText = "<br><H2>Guarantees</H2>" +
				"<br><table border=\"0\"><tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Status from processor</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Guarantee Status NOT included if extraDataFromProcessing is empty", expectedGuaranteeStatusText, messageInterpretationText);

				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted("GuaranteeStatus:Status from processor*OtherData:extra data*");
				AssertContains("Test Guarantee Status", expectedGuaranteeStatusText, messageInterpretationText);

				messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted("Guarantee:Status from processor*");
				AssertNotContains("Test Guarantee Status NOT included if extraDataFromProcessing has no GuaranteeStatus text", expectedGuaranteeStatusText, messageInterpretationText);
			});
		}

		Cctracv1Sal SetResponseData(ZString mrn, ZString phase, ZString status, DateTime? cancellationDate, DateTime? acceptanceDate, ZString circuit, ZString csvCode, DateTime? csvClearanceDate, ZString arrivalCircuit,
			DateTime? dischargeDate, ZString unloadingResult, DateTime? arrivalDate, ZString location, DateTime? ultimateDate)
		{
			var response = new Cctracv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType70()
			{
				TransitOperation = new TransitOperationType70()
				{
					Mrn = mrn,
				},
				Ncts5DatosGestion = new Ncts5DatosGestion70()
				{
					FaseOrigen = phase,
					Estado = status,
					FechaInvalidacion = cancellationDate,
					FechaAdmision = acceptanceDate,
					FechaLevante = csvClearanceDate,
					CircuitoExpedicion = circuit,
					CsVdeDat = csvCode,
					FechaLimiteLlegada = dischargeDate,
					CircuitoRecepcion = arrivalCircuit,
					CodigoResultadoDescarga = unloadingResult,
					FechaHoraRecepcion = arrivalDate,
					UbicacionRecepcion = location,
					FechaUltimacionCompleta = ultimateDate,
				}
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Cctracv1Sal response)
		{
			var messagePrettyFormatter = new QueryNCTSMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted("GuaranteeStatus:Not written Off (status not UL)*");
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

		XmlErrorType SetXMLError(XmlErrorCodes errorCode, ZString errorLineNumber, ZString errorColumnNumber, ZString errorText, ZString wrongValue, ZString location)
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

		const string Location = "nacUbicac1";
		const string MRN = "MRNES23000000";
		const string Clearance = "3AG5G6SSCJJ93NML";
		const string ResultB11 = "B11";
		const string VersionP5 = "P5";
	}
}
