using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ArrivalAtExitMessagePrettyFormatterTest : EdiFactV921ESMessagePrettyFormatterTest<ICUSRESV921ESMessageProvider>
	{
		public override void TestRegisterData()
		{
			var mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText(mockTestHelper);

			var expectedAccpetanceText = "<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:04</td></tr>";
			var expectedRegisterText = "<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Acceptance if it's not in the response", expectedAccpetanceText, messageInterpretationText);

				mockTestHelper = SetResponseData(new ZDateTime(2021, 08, 01, 11, 05, 04), ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test include Acceptance if it's in the response", expectedAccpetanceText, messageInterpretationText);

				AssertNotContains("Test No include Register if it's not in the response", expectedRegisterText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, "21ES00999912345678", ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test include Register if it's in the response", expectedRegisterText, messageInterpretationText);
			});
		}

		public override void TestCircuitData()
		{
			var mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText(mockTestHelper);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, "4", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, "6", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, "5", ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public override void TestClearanceData()
		{
			var mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
			var messageInterpretationText = GetInterpretationText(mockTestHelper);

			var expectedClearanceText = "<tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>B9026422646FE2AB</td></tr>";
			var expectedClearanceDateText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>01-08-2021, 11:05:05</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Clearance Number if it's not in the response", expectedClearanceText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, "B9026422646FE2AB", ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test include Clearance Number if it's in the response", expectedClearanceText, messageInterpretationText);

				AssertNotContains("Test No include Clearance Date if it's not in the response", expectedClearanceDateText, messageInterpretationText);

				mockTestHelper = SetResponseData(ZDateTime.Empty, ZString.Empty, ZString.Empty, ZString.Empty, new ZDateTime(2021, 08, 01, 11, 05, 05), ZString.Empty, ZString.Empty, ZDateTime.Empty);
				messageInterpretationText = GetInterpretationText(mockTestHelper);
				AssertContains("Test include Clearance Date if it's in the response", expectedClearanceDateText, messageInterpretationText);
			});
		}

		protected override ZString GetInterpretationText(Mock<ICUSRESV921ESMessageProvider> mockTestHelper)
		{
			var messagePrettyFormatter = new ArrivalAtExitMessagePrettyFormatter(mockTestHelper.Object);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		protected override ZString GetInterpretationText_IExportResponseMessageProvider(Mock<IExportResponseMessageProvider> mockTestHelper)
		{
			var messagePrettyFormatter = new ArrivalAtExitMessagePrettyFormatter(mockTestHelper.Object);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}
	}
}
