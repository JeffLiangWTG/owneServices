using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SalesRelationRuleNodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateType()
		{
			var collection = new SalesRelationRuleNodeCollection();
			var node1 = collection.AddNew();

			node1.Type = ZString.Empty;
			AssertMandatoryValidationError(node1.TypeInfo, true);

			node1.Type = "XXX";
			AssertMandatoryValidationError(node1.TypeInfo, false);
			AssertListValidationInvalidCodeError(node1.TypeInfo, true);

			node1.Type = SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity;
			AssertListValidationInvalidCodeError(node1.TypeInfo, false);

			node1.Type = SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities;
			AssertListValidationInvalidCodeError(node1.TypeInfo, false);

			var node2 = collection.AddNew();
			node2.Type = RelatableActivityTypeList.Codes.OpportunityManager;
			AssertListValidationInvalidCodeError(node2.TypeInfo, false);

			node1.ValidateType();
			AssertHasError(node1.TypeInfo, "A type of * can only be at the end of a sequence.");
		}
	}
}
