using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection))]
	sealed class AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollectionTest : AmountBasedAuthorisationRequirementCollectionTest<AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection>
	{
		#region Implementation

		protected override AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection GetCollectionToTest()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
		}

		#endregion
	}
}
