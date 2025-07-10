using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class T2LCommonMessagePrettyFormatterTest<TResponse> : TestCaseWithFactory
		where TResponse : IT2LCommon, ICommonServiceSegment
	{
		public void TestCreateMessageDetailsAcceptedWithoutCSV()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			AssertEquals("<H3>Accepted Declaration</H3>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsAcceptedWithCSV()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "HHAXSXPQA6NT963Y", ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			AssertEquals("<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = HHAXSXPQA6NT963Y</H3>", messageInterpretationText);
		}

		public void TestAcceptanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-10-2021, 10:30:45</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included Acceptance data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData("20211004", "103045", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Acceptance data", expectedText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<H3>CSV Electronic Declaration = HHAXSXPQA6NT963Y</H3>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included CSVClearance data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "HHAXSXPQA6NT963Y", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSVClearance data", expectedText, messageInterpretationText);
			});
		}

		public void TestDescriptionData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento T2L de Expedición.</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included Description data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "Documento T2L de Expedición.");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Description data", expectedText, messageInterpretationText);
			});
		}

		public void TestMRNData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>20ES009999L0023207</td></tr></table>";
			var expectedContainsText = ShouldMRNDataBeTested ? expectedText : (string)ZString.Empty;

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included MRN data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "20ES009999L0023207", ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test MRN data", expectedContainsText, messageInterpretationText);
			});
		}

		public void TestCSVdelPDFdelT2LData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>C.S.V.:</td><td>&nbsp;&nbsp;</td><td>9KJLVLPCWNKUV7N8</td></tr></table>";
			var expectedContainsText = ShoulddelPDFdelT2LDataBeTested ? expectedText : (string)ZString.Empty;

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included CSVdelPDFdelT2L data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "9KJLVLPCWNKUV7N8", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSVdelPDFdelT2L data", expectedContainsText, messageInterpretationText);
			});
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			var expectedContainsText = ShouldCircuitDataBeTested ? expectedGreenCircuitText : (string)ZString.Empty;

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, hasCircuitAssigment: true);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedContainsText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, CircuitoTipo.N, true);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				expectedContainsText = ShouldCircuitDataBeTested ? expectedOrangeCircuitText : (string)ZString.Empty;
				AssertContains("Test Orange Circuit", expectedContainsText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, CircuitoTipo.R, true);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				expectedContainsText = ShouldCircuitDataBeTested ? expectedRedCircuitText : (string)ZString.Empty;
				AssertContains("Test Red Circuit", expectedContainsText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsRejected()
		{
			var response = GetResponse("7002", "El fichero que se anexa no tiene una extensión válida según se indica en tabla REGDFORM.");

			var formatter = GetFormatter(response);
			var messageInterpretationText = formatter.CreateMessageDetailsRejected();

			AssertEquals("<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>7002</td><td>El fichero que se anexa no tiene una extensión válida según se indica en tabla REGDFORM.</td></tr>" +
						"</table>", messageInterpretationText);
		}

		protected virtual bool ShouldMRNDataBeTested => false;
		protected virtual bool ShouldCircuitDataBeTested => false;
		protected virtual bool ShoulddelPDFdelT2LDataBeTested => false;

		protected ZString GetAcceptedInterpretationText(TResponse response) => GetFormatter(response).CreateMessageDetailsAccepted();

		protected abstract TResponse GetResponse(string responseCode = "", string responseDesc = "");

		protected abstract T2LCommonMessagePrettyFormatter<TResponse> GetFormatter(TResponse response);

		protected abstract TResponse SetResponseData(ZString date, ZString hour, ZString reference, ZString csvClearance, ZString csvPdfT2l, ZString description, CircuitoTipo circuito = CircuitoTipo.V, bool hasCircuitAssigment = false);
	}
}
