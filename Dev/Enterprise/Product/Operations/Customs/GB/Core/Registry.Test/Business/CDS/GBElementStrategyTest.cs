using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(GBElementStrategy))]
	public class GBElementStrategyTest : TestCaseWithFactory
	{
		public void TestGetRegExForDataType()
		{
			var strategy = new GBElementStrategy(BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode, "Enterprise Code", 123);
			var customisation = new BillOfLadingNumberCustomisation();
			var element = customisation.Elements
				.Cast<BillOfLadingNumberCustomisationElement>()
				.Single(x => x.Key == strategy.Key);

			element.OverrideStrategy(strategy);
			AssertEquals("[0-9a-zA-Z]{123}", strategy.GetRegExForDataType(element));
		}
	}
}
