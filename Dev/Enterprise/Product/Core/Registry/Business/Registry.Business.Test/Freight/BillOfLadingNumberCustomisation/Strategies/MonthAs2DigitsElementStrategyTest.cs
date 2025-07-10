using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class MonthAs2DigitsElementStrategyTest : TestCase
	{
		public void TestGetRegExForDataType()
		{
			var strategy = new MonthAs2DigitsElementStrategy();
			var customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			AssertEquals("(0[1-9]|1[0-2])", strategy.GetRegExForDataType(element));
		}
	}
}
