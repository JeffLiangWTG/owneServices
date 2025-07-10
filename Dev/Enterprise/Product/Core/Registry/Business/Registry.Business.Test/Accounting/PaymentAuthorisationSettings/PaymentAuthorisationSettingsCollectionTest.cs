using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentAuthorisationSettingsCollection))]
	sealed class PaymentAuthorisationSettingsCollectionTest : AmountBasedAuthorisationRequirementCollectionTest<PaymentAuthorisationSettingsCollection>
	{
		#region Implementation

		protected override PaymentAuthorisationSettingsCollection GetCollectionToTest()
		{
			return new PaymentAuthorisationSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PaymentAuthorisationSettings();
		}

		#endregion
	}
}
