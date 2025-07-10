using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentSixLevelAuthorisationSettings))]
	sealed class PaymentSixLevelAuthorisationSettingsTest : AmountBasedSixLevelAuthorisationRequirementTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			PaymentSixLevelAuthorisationSettings result = new PaymentSixLevelAuthorisationSettings();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new PaymentSixLevelAuthorisationSettingsCollection();
		}

		#endregion
	}
}
