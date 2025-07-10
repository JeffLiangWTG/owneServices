using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PaymentTwoLevelsAuthorisationSettingsCollection))]
	sealed class PaymentTwoLevelsAuthorisationSettingsCollectionTest : AmountBasedAuthorisationRequirementCollectionTest<PaymentTwoLevelsAuthorisationSettingsCollection>
	{
		#region Implementation

		protected override PaymentTwoLevelsAuthorisationSettingsCollection GetCollectionToTest()
		{
			return new PaymentTwoLevelsAuthorisationSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PaymentTwoLevelsAuthorisationSettings();
		}

		#endregion
	}
}
