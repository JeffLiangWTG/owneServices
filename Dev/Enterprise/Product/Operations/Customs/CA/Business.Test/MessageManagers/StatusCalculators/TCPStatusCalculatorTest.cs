using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Edifact.D99B.Elements;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(TCPStatusCalculator))]
	sealed class TCPStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public override void TestCalculatedJobStatus()
		{
			Assert(calculator.CalculatedJobStatus(null).IsEmpty);
		}

		public void TestCalculateTCPStatus()
		{
			AssertEquals(CSAStatusList.Codes.Added, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.AwaitingAdd, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted));
			AssertEquals(CSAStatusList.Codes.Deleted, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.AwaitingDelete, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted));

			AssertEquals(CSAStatusList.Codes.ErrorAdded, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.AwaitingAdd, ProcessingIndicatorDescriptionCodeList.ErrorMessage));
			AssertEquals(CSAStatusList.Codes.ErrorDeleted, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.AwaitingDelete, ProcessingIndicatorDescriptionCodeList.ErrorMessage));

			AssertEquals(CSAStatusList.Codes.AwaitingAdd, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.AwaitingAdd, ProcessingIndicatorDescriptionCodeList.MessageContentAcceptedWithComments));
			AssertEquals(CSAStatusList.Codes.AwaitingDelete, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.AwaitingDelete, ProcessingIndicatorDescriptionCodeList.MessageContentAcceptedWithComments));

			AssertEquals(CSAStatusList.Codes.ErrorAdded, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.ErrorAdded, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted));
			AssertEquals(CSAStatusList.Codes.ErrorDeleted, TCPStatusCalculator.CalculateTCPStatus(CSAStatusList.Codes.ErrorDeleted, ProcessingIndicatorDescriptionCodeList.MessageContentAccepted));
		}

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.TradeChainPartner, calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new TCPStatusCalculator();
		}
	}
}
