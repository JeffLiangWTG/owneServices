using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(IIDStatusCalculator))]
	sealed class IIDStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public override void TestCalculatedJobStatus()
		{
			AssertEquals("GetMessageAwaitingStatus", ZString.Empty, calculator.CalculatedJobStatus(null));
		}

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.IntegratedImportDeclaration, calculator.MessageTypeDescription);
		}

		public override void TestIsLodged()
		{
			Assert("IsLodged", !calculator.IsLodged(ZString.Empty));
			Assert("IsLodged", !calculator.IsLodged(MessageStatusList.Codes.AwaitingOriginal));
			Assert("IsLodged", !calculator.IsLodged(MessageStatusList.Codes.ErrorOriginal));
			Assert("IsLodged", calculator.IsLodged(MessageStatusList.Codes.AwaitingReplace));
			Assert("IsLodged", calculator.IsLodged("??"));
		}

		public void TestGetMessageAwaitingStatus()
		{
			AssertEquals("GetMessageAwaitingStatus", MessageStatusList.Codes.AwaitingOriginal, calculator.GetMessageAwaitingStatus(IIDMessageSubTypeList.Codes.Original));
			AssertEquals("GetMessageAwaitingStatus", MessageStatusList.Codes.AwaitingChange, calculator.GetMessageAwaitingStatus(IIDMessageSubTypeList.Codes.Change));
			AssertEquals("GetMessageAwaitingStatus", MessageStatusList.Codes.AwaitingReplace, calculator.GetMessageAwaitingStatus(IIDMessageSubTypeList.Codes.Amendment));
			AssertEquals("GetMessageAwaitingStatus", MessageStatusList.Codes.AwaitingDelete, calculator.GetMessageAwaitingStatus(IIDMessageSubTypeList.Codes.Cancellation));
			AssertEquals("GetMessageAwaitingStatus", ZString.Empty, calculator.GetMessageAwaitingStatus("??"));
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new IIDStatusCalculator();
		}
	}
}
