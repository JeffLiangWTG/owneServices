using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentThreeLevelAuthorisationSettings))]
	sealed class PaymentThreeLevelAuthorisationSettingsTest : AmountBasedThreeLevelAuthorisationRequirementTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			PaymentThreeLevelAuthorisationSettings result = new PaymentThreeLevelAuthorisationSettings();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new PaymentThreeLevelAuthorisationSettingsCollection();
		}

		#endregion
	}
}
