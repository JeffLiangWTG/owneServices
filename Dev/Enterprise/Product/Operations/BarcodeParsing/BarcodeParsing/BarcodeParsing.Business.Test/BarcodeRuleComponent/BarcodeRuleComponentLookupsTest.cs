using System.Linq;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeRuleComponentLookupsTest : BarcodeParsingLookupsTestCase
	{
		#region TestApplicationIdentifiers

		public void TestApplicationIdentifiers()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals(0, component.Lookups.ApplicationIdentifiers.Count);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertContainsExactElementsInAnyOrder("GS1 Rule Components should show the complete GS1 App. ID list from the registry.",
				WarehouseDataRegistry.Instance.ApplicationIdentifiers.Value.Cast<ApplicationIdentifier>()
				.Select(a => new CodeDescriptionPair(a.ApplicationID.ToString(), a.FullTitle)),
				component.Lookups.ApplicationIdentifiers);
		}

		#endregion

		#region TestFormatTypes

		public void TestFormatTypes()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			var allFormats = new OtherDataFormatTypes();
			allFormats.AddRange(new GS1DataFormatTypes());
			AssertContainsExactElementsInAnyOrder(allFormats, component.Lookups.FormatTypes);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertContainsExactElementsInAnyOrder("GS1 Rule Components should only have one Date format.",
				new GS1DataFormatTypes(), component.Lookups.FormatTypes);
		}

		#endregion

		#region TestLengthTypes

		public void TestLengthTypes()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertContainsExactElementsInAnyOrder(new LengthTypes(), component.Lookups.LengthTypes);
		}

		#endregion

		#region TestTargetFields

		public void TestTargetFields()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			rule.RuleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var targetFields = new DummyTargetFields();
			AssertContainsExactElementsInAnyOrder(
				targetFields
					.Cast<CodeDescriptionPair>()
					.Concat(new[] { new CodeDescriptionPair("IGN", "Ignore") }), component.Lookups.TargetFields);

			rule.RuleSet.BRS_Module = "";
			AssertContainsExactElementsInAnyOrder(new[] { new CodeDescriptionPair("IGN", "Ignore") }, component.Lookups.TargetFields);
		}

		#endregion
	}
}
