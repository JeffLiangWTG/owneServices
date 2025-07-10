using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentTwelveLevelAuthorisationSettingsCollection))]
	sealed class PaymentTwelveLevelAuthorisationSettingsCollectionTest : AmountBasedAuthorisationRequirementCollectionTest<PaymentTwelveLevelAuthorisationSettingsCollection>
	{
		#region Implementation

		protected override PaymentTwelveLevelAuthorisationSettingsCollection GetCollectionToTest()
		{
			return new PaymentTwelveLevelAuthorisationSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PaymentTwelveLevelAuthorisationSettings();
		}

		#endregion
	}
}
