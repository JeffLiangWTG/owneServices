using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SalesRelationDirectionRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationNodeSequenceAsString()
		{
			var collection = new SalesRelationDirectionRuleCollection();
			var rule1 = collection.AddNew();

			rule1.ValidationNodeSequenceAsString();
			AssertMandatoryValidationError(rule1.NodeSequenceAsStringInfo, true);

			rule1.Nodes.AddNew().Type = SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity;
			rule1.ValidationNodeSequenceAsString();
			AssertMandatoryValidationError(rule1.NodeSequenceAsStringInfo, false);
			AssertHasError(rule1.NodeSequenceAsStringInfo, "Must contain at least 2 types.");

			rule1.Nodes.AddNew().Type = SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities;
			rule1.ValidationNodeSequenceAsString();
			AssertNoErrors(rule1.NodeSequenceAsStringInfo);

			var rule2 = collection.AddNew();
			rule2.ValidationNodeSequenceAsString();
			AssertPropertyIsUniqueInCollectionValidationError(rule2.NodeSequenceAsStringInfo, false);

			foreach (SalesRelationRuleNode node in rule1.Nodes)
			{
				rule2.Nodes.AddNew().Type = node.Type;
			}
			rule2.ValidationNodeSequenceAsString();
			AssertPropertyIsUniqueInCollectionValidationError(rule2.NodeSequenceAsStringInfo, true);
		}
	}
}
