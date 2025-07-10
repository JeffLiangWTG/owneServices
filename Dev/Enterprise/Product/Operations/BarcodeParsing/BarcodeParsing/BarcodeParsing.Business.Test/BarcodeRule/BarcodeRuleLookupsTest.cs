
namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeRuleLookupsTest : BarcodeParsingLookupsTestCase
	{
		#region TestTerminatorTypes

		public void TestTerminatorTypes()
		{
			AssertContainsExactElementsInAnyOrder(new TerminatorTypes(), Helper.CreateRule().Lookups.TerminatorTypes);
		}

		#endregion
	}
}
