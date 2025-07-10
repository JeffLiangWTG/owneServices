using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentAuthorisationSettings))]
	sealed class PaymentAuthorisationSettingsTest : AmountBasedMultiLevelAuthorisationRequirementTest
	{
		#region Implementation

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new PaymentAuthorisationSettingsCollection();
		}

		#endregion
	}
}
