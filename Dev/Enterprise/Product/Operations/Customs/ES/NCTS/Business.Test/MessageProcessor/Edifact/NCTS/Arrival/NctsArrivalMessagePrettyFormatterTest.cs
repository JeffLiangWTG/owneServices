using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsArrivalMessagePrettyFormatterTest : EdiFactMessagePrettyFormatterTest
	{
		public void TestCreateMessageDetailsAcceptedArrival()
		{
			var mockTestProvider = new Mock<INctsArrivalResponseMessageProvider>();
			mockTestProvider.Setup(m => m.DocumentMessageName).Returns("AVI");
			mockTestProvider.Setup(m => m.AdmissionDate).Returns(new ZDateTime(2020, 11, 20, 18, 56, 00));
			mockTestProvider.Setup(m => m.MessageFunction).Returns("4");
			mockTestProvider.Setup(m => m.PreviousSummaryDiscrepancy).Returns("1");
			mockTestProvider.Setup(m => m.SummaryReferenceNumber).Returns("99980000521");
			mockTestProvider.Setup(m => m.TransitReferenceNumber).Returns("20ES009998500102");

			var messagePrettyFormatter = new NctsArrivalMessagePrettyFormatter(mockTestProvider.Object);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals("<H3>Accepted Declaration AVI</H3>" +
					"<H4>Acceptance: 20-11-2020 18:56</H4>" +
					"<H4>Circuit: <strong><font color=\"#64AF00\">GREEN</font></strong></H4>" +
					"<H4>** No deviations between Transit and Previous Summary **</H4>" +
					"<H4>Summary Decl: 99980000521</H4>" +
					"<H4>MRN AVI: 20ES009998500102</H4>", messageInterpretationText);
		}
	}
}
