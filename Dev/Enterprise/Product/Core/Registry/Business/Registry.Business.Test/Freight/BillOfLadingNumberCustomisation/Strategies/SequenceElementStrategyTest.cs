using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SequenceElementStrategyTest : TestCase
	{
		public void TestGetRegExForDataType()
		{
			var strategy = new SequenceElementStrategy();
			var customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			element.Detail = "8";
			AssertEquals("[0-9]{8}", strategy.GetRegExForDataType(element));

			element.Detail = "4";
			AssertEquals("[0-9]{4}", strategy.GetRegExForDataType(element));
		}
	}
}
