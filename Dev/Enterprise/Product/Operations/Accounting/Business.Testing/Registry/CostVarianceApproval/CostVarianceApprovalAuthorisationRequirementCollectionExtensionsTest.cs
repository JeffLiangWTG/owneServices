using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class CostVarianceApprovalAuthorisationRequirementCollectionExtensionsTest : AmountBasedMultiLevelAuthorisationRequirementExtensionsTest
	{
		protected override AmountBasedMultiLevelAuthorisationRequirement GetNewElement()
		{
			return collection.AddNew();
		}

		CostVarianceApprovalAuthorisationRequirementCollection collection;

		protected override void SetUp()
		{
			base.SetUp();
			collection = new CostVarianceApproval().AuthorisationRequirements;
		}
	}
}