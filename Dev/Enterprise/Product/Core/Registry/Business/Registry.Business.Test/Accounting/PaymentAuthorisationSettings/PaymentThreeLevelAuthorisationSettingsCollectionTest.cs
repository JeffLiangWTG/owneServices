using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentThreeLevelAuthorisationSettingsCollection))]
	sealed class PaymentThreeLevelAuthorisationSettingsCollectionTest : AmountBasedAuthorisationRequirementCollectionTest<PaymentThreeLevelAuthorisationSettingsCollection>
	{
		#region Implementation

		protected override PaymentThreeLevelAuthorisationSettingsCollection GetCollectionToTest()
		{
			return new PaymentThreeLevelAuthorisationSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PaymentThreeLevelAuthorisationSettings();
		}

		#endregion
	}
}
