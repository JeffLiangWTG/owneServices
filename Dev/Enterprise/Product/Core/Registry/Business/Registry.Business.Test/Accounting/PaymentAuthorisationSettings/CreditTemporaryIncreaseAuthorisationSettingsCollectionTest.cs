using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditTemporaryIncreaseAuthorisationSettingsCollection))]
	sealed class CreditTemporaryIncreaseAuthorisationSettingsCollectionTest : AmountBasedAuthorisationRequirementCollectionTest<CreditTemporaryIncreaseAuthorisationSettingsCollection>
	{
		#region Implementation

		protected override CreditTemporaryIncreaseAuthorisationSettingsCollection GetCollectionToTest()
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CreditTemporaryIncreaseAuthorisationSettings();
		}

		#endregion
	}
}
