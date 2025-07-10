using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(G7ExportStatusCalculator))]
	sealed class G7ExportDeclarationCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public override void TestCalculatedJobStatus()
		{
			Assert(true);//TODO put a test here
		}

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.G7Export, calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new G7ExportStatusCalculator();
		}
	}
}
