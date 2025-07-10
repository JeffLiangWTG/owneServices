using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureMessagePrettyFormatterTest : EdiFactV921ESMessagePrettyFormatterTest<INctsDepartureAndTIRResponseMessageProvider>
	{
		public override void TestRegisterData()
		{
			var mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);

			var expectedAccpetanceText = "<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:04</td></tr>";
			var expectedRegisterText = "<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Acceptance if it's not in the response", expectedAccpetanceText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(new ZDateTime(2021, 08, 01, 11, 05, 04), ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Acceptance if it's in the response", expectedAccpetanceText, messageInterpretationText);

				AssertNotContains("Test No include Register if it's not in the response", expectedRegisterText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, "21ES00999912345678", ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Register if it's in the response", expectedRegisterText, messageInterpretationText);
			});
		}

		public override void TestCircuitData()
		{
			var mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "4", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "6", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, "5", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public override void TestClearanceData()
		{
			var mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);

			var expectedClearanceText = "<tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>B9026422646FE2AB</td></tr>";
			var expectedClearanceDateText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:05</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Clearance Number if it's not in the response", expectedClearanceText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, "B9026422646FE2AB", ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Clearance Number if it's in the response", expectedClearanceText, messageInterpretationText);

				AssertNotContains("Test No include Clearance Date if it's not in the response", expectedClearanceDateText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, new ZDateTime(2021, 08, 01, 11, 05, 05), ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Clearance Date if it's in the response", expectedClearanceDateText, messageInterpretationText);
			});
		}

		public void TestCreateMessageDetailsAcceptedDeparture()
		{
			var mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(new ZDateTime(2021, 08, 01, 11, 05, 04), "21ES00999912345678", "4", "B9026422646FE2AB", new ZDateTime(2021, 08, 01, 11, 05, 05), TADPrintProcedureCodeList.Codes.OperatorMustPrintOutCopyAOfTad, ClearanceCriteriaCodeList.Codes.SimplifiedProcedure, new ZDateTime(2021, 08, 19));
			var messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\">" +
				"<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:04</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr>" +
				"</table>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>19-08-2021</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>B9026422646FE2AB</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:05</td></tr>" +
				"<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A3) Simplified Procedure</td></tr>" +
				"<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>Copy \"A\" of TAD</td></tr>" +
				"</table>";

			AssertEquals("Test Accepted Message Details", expectedMessageInterpretationText, messageInterpretationText);
		}

		public void TestLimitDateOfArrival()
		{
			var mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);

			var expectedLimitDateText = "<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>19-08-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Limit date of arrival if it's not in the response", expectedLimitDateText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, new ZDateTime(2021, 08, 19));
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Limit date of arrival if it's in the response", expectedLimitDateText, messageInterpretationText);
			});
		}

		public void TestClearanceCriteria()
		{
			var mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);

			var expectedSimplifiedProcedureText = "<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A3) Simplified Procedure</td></tr>";
			var expectedNormalProcedureText = "<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A2) Normal Procedure</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Clearance Criteria if it's not in the response", expectedSimplifiedProcedureText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ClearanceCriteriaCodeList.Codes.SimplifiedProcedure, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Clearance Criteria for Simplified Procedure", expectedSimplifiedProcedureText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ClearanceCriteriaCodeList.Codes.NormalProcedure, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Clearance Criteria for Normal Procedure", expectedNormalProcedureText, messageInterpretationText);
			});
		}

		public void TestPrintProcedure()
		{
			var mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);

			var expectedCopyAText = "<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>Copy \"A\" of TAD</td></tr>";
			var expectedCopyABText = "<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>Copies \"A\" and \"B\" of TAD</td></tr>";
			var expectedCustomsPrintText = "<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>Customs must print TAD</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Print Procedure if it's not in the response", expectedCopyAText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, TADPrintProcedureCodeList.Codes.OperatorMustPrintOutCopyAOfTad, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Print Procedure for Copy A", expectedCopyAText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, TADPrintProcedureCodeList.Codes.OperatorMustPrintOutCopiesAAndBOfTad, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Print Procedure for Copy A and B", expectedCopyABText, messageInterpretationText);

				mockTestHelper = SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, TADPrintProcedureCodeList.Codes.CustomsMustPrintOutTad, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(mockTestHelper);
				AssertContains("Test include Print Procedure for Customs print", expectedCustomsPrintText, messageInterpretationText);
			});
		}

		Mock<INctsDepartureAndTIRResponseMessageProvider> SetResponseData_INctsDepartureAndTIRResponseMessageProvider(ZDateTime admissionDate, ZString registrationNumber, ZString messageFunction, ZString csvReleaseCode, ZDateTime csvReleaseCreationDate, ZString printActionRequired, ZString? customsClearanceStatus = null, ZDateTime? transitMaxDate = null)
		{
			var mockTestHelper = new Mock<INctsDepartureAndTIRResponseMessageProvider>();
			mockTestHelper.Setup(m => m.DocumentMessageName).Returns(UniversalReferenceConstants.DeclarationResponseCode.Accepted);
			mockTestHelper.Setup(m => m.AdmissionDate).Returns(admissionDate);
			mockTestHelper.Setup(m => m.RegistrationNumber).Returns(registrationNumber);
			mockTestHelper.Setup(m => m.MessageFunction).Returns(messageFunction);
			mockTestHelper.Setup(m => m.CSVReleaseCode).Returns(csvReleaseCode);
			mockTestHelper.Setup(m => m.CSVReleaseCreationDate).Returns(csvReleaseCreationDate);
			mockTestHelper.Setup(m => m.PrintActionRequired).Returns(printActionRequired);
			mockTestHelper.Setup(m => m.CustomsClearanceCriteria).Returns((ZString)customsClearanceStatus);
			mockTestHelper.Setup(m => m.TransitMaxDate).Returns((ZDateTime)transitMaxDate);

			return mockTestHelper;
		}

		ZString GetInterpretationText_INctsDepartureAndTIRResponseMessageProvider(Mock<INctsDepartureAndTIRResponseMessageProvider> mockTestHelper)
		{
			var messagePrettyFormatter = new NctsDepartureMessagePrettyFormatter(mockTestHelper.Object);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		protected override ZString GetInterpretationText_IExportResponseMessageProvider(Mock<IExportResponseMessageProvider> mockTestHelper)
		{
			throw new System.NotImplementedException();
		}

		protected override ZString GetInterpretationText(Mock<INctsDepartureAndTIRResponseMessageProvider> mockTestHelper)
		{
			var messagePrettyFormatter = new NctsDepartureMessagePrettyFormatter(mockTestHelper.Object);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
