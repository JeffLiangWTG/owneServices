using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentTwoLevelsAuthorisationSettings))]
	sealed class PaymentTwoLevelsAuthorisationSettingsTest : AmountBasedTwoLevelAuthorisationRequirementTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			PaymentTwoLevelsAuthorisationSettings result = new PaymentTwoLevelsAuthorisationSettings();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new PaymentTwoLevelsAuthorisationSettingsCollection();
		}

		#endregion
	}
}
