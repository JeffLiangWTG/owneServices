using Enterprise.Registry.Business.BillCustomisationStrategies;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	public class StrategyRegExFormattingHelperTest : TestCase
	{
		public void TestGetMaxToLengthRegExValue()
		{
			var mockCalcMaxGeneratedLength = new Mock<ICalcMaxGeneratedLength>(MockBehavior.Strict);
			mockCalcMaxGeneratedLength.Setup(x => x.CalcMaxGeneratedLength).Returns(3);

			AssertEquals("[0-9a-zA-Z]{,3}", StrategyRegExFormattingHelper.GetMaxToLengthRegExValue(mockCalcMaxGeneratedLength.Object));
			mockCalcMaxGeneratedLength.Verify(x => x.CalcMaxGeneratedLength, Times.Exactly(1));
		}

		public void TestGetExactlyLengthRegExValue()
		{
			var mockCalcMaxGeneratedLength = new Mock<ICalcMaxGeneratedLength>(MockBehavior.Strict);
			mockCalcMaxGeneratedLength.Setup(x => x.CalcMaxGeneratedLength).Returns(5);

			AssertEquals("[0-9a-zA-Z]{5}", StrategyRegExFormattingHelper.GetExactlyLengthRegExValue(mockCalcMaxGeneratedLength.Object));
			mockCalcMaxGeneratedLength.Verify(x => x.CalcMaxGeneratedLength, Times.Exactly(1));
		}
	}
}