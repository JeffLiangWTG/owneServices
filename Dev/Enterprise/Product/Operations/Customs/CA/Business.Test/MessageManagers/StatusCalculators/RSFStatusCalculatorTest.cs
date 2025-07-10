using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Edifact.D99B.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(RSFStatusCalculator))]
	sealed class RSFStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public override void TestCalculatedJobStatus()
		{
			Assert(calculator.CalculatedJobStatus(null).IsEmpty);
		}

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.CSARevenueSummaryForm, calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new RSFStatusCalculator();
		}

		public void TestCalculateRSFStatus()
		{
			AssertEquals(MessageStatusList.Codes.AcknowledgedOriginal, RSFStatusCalculator.CalculateRSFStatus(MessageStatusList.Codes.AwaitingOriginal, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted));
			AssertEquals(MessageStatusList.Codes.AcknowledgedChange, RSFStatusCalculator.CalculateRSFStatus(MessageStatusList.Codes.AwaitingChange, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted));

			AssertEquals(MessageStatusList.Codes.ErrorOriginal, RSFStatusCalculator.CalculateRSFStatus(MessageStatusList.Codes.AwaitingOriginal, ProcessingIndicatorDescriptionCodeList.ErrorMessage));
			AssertEquals(MessageStatusList.Codes.ErrorChange, RSFStatusCalculator.CalculateRSFStatus(MessageStatusList.Codes.AwaitingChange, ProcessingIndicatorDescriptionCodeList.ErrorMessage));
		}
	}
}
