using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentSixLevelAuthorisationSettingsCollection))]
	sealed class PaymentSixLevelAuthorisationSettingsCollectionTest : AmountBasedAuthorisationRequirementCollectionTest<PaymentSixLevelAuthorisationSettingsCollection>
	{
		#region Implementation

		protected override PaymentSixLevelAuthorisationSettingsCollection GetCollectionToTest()
		{
			return new PaymentSixLevelAuthorisationSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PaymentSixLevelAuthorisationSettings();
		}

		#endregion
	}
}
