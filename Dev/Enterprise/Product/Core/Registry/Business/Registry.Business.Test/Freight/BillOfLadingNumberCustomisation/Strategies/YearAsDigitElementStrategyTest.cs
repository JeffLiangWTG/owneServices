using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class YearAsDigitElementStrategyTest : TestCase
	{
		public void TestGetRegExForDataType()
		{
			var strategy = new YearAsDigitElementStrategy();
			var customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			element.Detail = "2";
			AssertEquals("[0-9]{2}", strategy.GetRegExForDataType(element));

			element.Detail = "4";
			AssertEquals("[0-9]{4}", strategy.GetRegExForDataType(element));
		}
	}
}
