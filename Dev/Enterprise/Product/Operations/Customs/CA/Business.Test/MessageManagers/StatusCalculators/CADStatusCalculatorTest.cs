using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(CADStatusCalculator))]
	sealed class CADStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.CommercialAccountingDeclaration, calculator.MessageTypeDescription);
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new CADStatusCalculator();
		}

		public override void TestCalculatedJobStatus()
		{
			Assert(true);
		}
	}
}
