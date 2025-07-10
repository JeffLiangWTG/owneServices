using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeValidationRuleLookupsTest : BarcodeParsingLookupsTestCase
	{
		#region TestFormatTypes

		public void TestFormatTypes()
		{
			var rule = Helper.CreateValidationRule();
			var allFormats = new OtherDataFormatTypes();
			allFormats.AddRange(new GS1DataFormatTypes());
			AssertContainsExactElementsInAnyOrder(allFormats, rule.Lookups.FormatTypes);
		}

		#endregion

		#region TestLengthTypes

		public void TestLengthTypes()
		{
			var rule = Helper.CreateValidationRule();
			AssertContainsExactElementsInAnyOrder(new LengthTypes(), rule.Lookups.LengthTypes);
		}

		#endregion

		#region TestTargetFields

		public void TestTargetFields()
		{
			var rule = Helper.CreateValidationRule();
			rule.RuleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var targetFields = new DummyTargetFields();
			AssertContainsExactElementsInAnyOrder(targetFields.Cast<CodeDescriptionPair>(), rule.Lookups.TargetFields);

			rule.RuleSet.BRS_Module = "";
			AssertEquals(0, rule.Lookups.TargetFields.Count);
		}

		#endregion
	}
}
