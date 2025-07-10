using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentTwelveLevelAuthorisationSettings))]
	sealed class PaymentTwelveLevelAuthorisationSettingsTest : AmountBasedTwelveLevelAuthorisationRequirementTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			PaymentTwelveLevelAuthorisationSettings result = new PaymentTwelveLevelAuthorisationSettings();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new PaymentTwelveLevelAuthorisationSettingsCollection();
		}

		#endregion
	}
}
