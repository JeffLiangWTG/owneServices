using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class GlobalOrLocalElementStrategyTest : TestCase
	{
		public void TestGetRegExForDataType()
		{
			var strategy = new GlobalOrLocalElementStrategy();
			var customisation = new BillOfLadingNumberCustomisation();

			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			AssertEquals("[OL]{1}", strategy.GetRegExForDataType(element));
		}
	}
}
