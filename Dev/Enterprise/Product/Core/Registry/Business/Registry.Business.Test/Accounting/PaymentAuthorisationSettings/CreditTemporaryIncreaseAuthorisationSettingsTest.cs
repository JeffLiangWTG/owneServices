using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditTemporaryIncreaseAuthorisationSettings))]
	sealed class CreditTemporaryIncreaseAuthorisationSettingsTest : AmountOrPercentageBasedThreeLevelAuthorisationRequirementTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CreditTemporaryIncreaseAuthorisationSettings();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsCollection();
		}

		public void TestValidateDaysToExpiry()
		{
			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			var setting1 = collection.AddNew();
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.DaysToExpiryInfo, "Please enter a 'Days To Expiry' greater than 0.");
			setting1.DaysToExpiry = 1;
			setting1.RunPreSaveValidation();
			AssertNoErrors(setting1.DaysToExpiryInfo);
			setting1.DaysToExpiry = 0;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.DaysToExpiryInfo, "Please enter a 'Days To Expiry' greater than 0.");
			setting1.DaysToExpiry = -1;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.DaysToExpiryInfo, "Please enter a 'Days To Expiry' greater than 0.");
			setting1.DaysToExpiry = 366;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.DaysToExpiryInfo, "Please enter a value up to 365");
		}

		#endregion
	}
}
