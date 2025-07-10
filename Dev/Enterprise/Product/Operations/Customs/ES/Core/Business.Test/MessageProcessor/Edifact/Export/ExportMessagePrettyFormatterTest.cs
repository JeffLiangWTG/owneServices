using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ExportMessagePrettyFormatterTest : EdiFactV921ESMessagePrettyFormatterTest<IExportResponseMessageProvider>
	{
		public override void TestRegisterData()
		{
			var mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);

			var expectedAccpetanceText = "<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:04</td></tr>";
			var expectedRegisterText = "<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Acceptance if it's not in the response", expectedAccpetanceText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(new ZDateTime(2021, 08, 01, 11, 05, 04), ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Acceptance if it's in the response", expectedAccpetanceText, messageInterpretationText);

				AssertNotContains("Test No include Register if it's not in the response", expectedRegisterText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, "21ES00999912345678", ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Register if it's in the response", expectedRegisterText, messageInterpretationText);
			});
		}

		public override void TestCircuitData()
		{
			var mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "4", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "6", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "5", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public override void TestClearanceData()
		{
			var mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);

			var expectedClearanceText = "<tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>B9026422646FE2AB</td></tr>";
			var expectedClearanceDateText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:05</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Clearance Number if it's not in the response", expectedClearanceText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, "B9026422646FE2AB", ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Clearance Number if it's in the response", expectedClearanceText, messageInterpretationText);

				AssertNotContains("Test No include Clearance Date if it's not in the response", expectedClearanceDateText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, new ZDateTime(2021, 08, 01, 11, 05, 05), ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Clearance Date if it's in the response", expectedClearanceDateText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAcceptedExport()
		{
			var mockTestHelper = SetResponseData(new ZDateTime(2021, 08, 01, 11, 05, 04), "21ES00999912345678", "4", "B9026422646FE2AB", new ZDateTime(2021, 08, 01, 11, 05, 05), EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities, ClearanceResultCodeList.Codes.A1);
			var messageInterpretationText = GetInterpretationText(mockTestHelper);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\">" +
				"<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:04</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr>" +
				"</table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>B9026422646FE2AB</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:05</td></tr>" +
				"<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A1) Satisfactory</td></tr>" +
				"<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>EAD printed by Customs authorities or through the Virtual Office</td></tr>" +
				"</table>";

			AssertEquals("Test Accepted Message Details", expectedMessageInterpretationText, messageInterpretationText);
		}

		public void TestExtraCircuitData()
		{
			var mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "P", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);

			var expectedPreSADCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong>PreSAD</strong></td></tr></table>";
			var expectedComplementaryCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong>Complementary</strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertContains("Test PreSAD", expectedPreSADCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "3", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test Complementary", expectedComplementaryCircuitText, messageInterpretationText);
			});
		}

		public void TestClearanceCriteria()
		{
			var mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetInterpretationText(mockTestHelper);

			var expectedA1CriteriaText = "<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A1) Satisfactory</td></tr>";
			var expectedA2CriteriaText = "<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A2) Considered Satisfactory</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Clearance Criteria if it's not in the response", expectedA1CriteriaText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ClearanceResultCodeList.Codes.A1);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test include Clearance Criteria for Satisfactory", expectedA1CriteriaText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ClearanceResultCodeList.Codes.A2);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test include Clearance Criteria for Considered satisfactory", expectedA2CriteriaText, messageInterpretationText);
			});
		}

		public void TestPrintProcedure()
		{
			var mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);

			var expectedNoEADProcedureText = "<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>No EAD Print</td></tr>";
			var expectedCustomsProcedureText = "<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>EAD printed by Customs authorities or through the Virtual Office</td></tr>";
			var expectedDeclarantProcedureText = "<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>EAD can be printed by declarant or through the Virtual Office</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Print Procedure if it's not in the response", expectedNoEADProcedureText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, EADPrintProcedureCodeList.Codes._0NoEADPrint, ZString.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Print Procedure for No EAD print", expectedNoEADProcedureText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities, ZString.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Print Procedure for EAD printed by Customs authorities", expectedCustomsProcedureText, messageInterpretationText);

				mockTestHelper = SetResponseData_IExportResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, EADPrintProcedureCodeList.Codes._2EADCanBePrintedByDeclarant, ZString.Empty);
				messageInterpretationText = GetInterpretationText_IExportResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Print Procedure for EAD printed by declarant", expectedDeclarantProcedureText, messageInterpretationText);
			});
		}

		protected override Mock<IExportResponseMessageProvider> SetResponseData(ZDateTime admissionDate, ZString registrationNumber, ZString messageFunction, ZString csvReleaseCode, ZDateTime csvReleaseCreationDate, ZString printActionRequired, ZString? customsClearanceStatus = null, ZDateTime? transitMaxDate = null)
		{
			var mockTestHelper = base.SetResponseData(admissionDate, registrationNumber, messageFunction, csvReleaseCode, csvReleaseCreationDate, printActionRequired);
			//mockTestHelper.ExpectAndReturnAlways(nameof(IExportResponseMessageProvider.CustomsClearanceStatus), customsClearanceStatus);
			mockTestHelper.Setup(m => m.CustomsClearanceStatus).Returns(new ZString(customsClearanceStatus));

			return mockTestHelper;
		}

		protected override ZString GetInterpretationText_IExportResponseMessageProvider(Mock<IExportResponseMessageProvider> mockTestHelper)
		{
			var messagePrettyFormatter = new ExportMessagePrettyFormatter(mockTestHelper.Object);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		protected override ZString GetInterpretationText(Mock<IExportResponseMessageProvider> mockTestHelper)
		{
			var messagePrettyFormatter = new ExportMessagePrettyFormatter(mockTestHelper.Object);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
