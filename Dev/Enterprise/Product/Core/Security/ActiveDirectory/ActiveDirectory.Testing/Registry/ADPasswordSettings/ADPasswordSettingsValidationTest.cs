using CargoWise.EntityFramework.Testing;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class ADPasswordSettingsValidationTest : BusinessObjectValidationTestCase
	{
		#region MaximumPasswordAge

		public void TestValidateMaximumPasswordAge()
		{
			var validationErrorMessage = "Must enter a number between Minimum Password Age (1 if not set) and 10675199. 42 is the recommended default.";
			bizo.MaximumPasswordAge = 0;

			// Not overridden, not enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforceMaximumPasswordAge = false;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertNoErrors(bizo.MaximumPasswordAgeInfo);

			// overridden but not enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMaximumPasswordAge = false;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertNoErrors(bizo.MaximumPasswordAgeInfo);

			// Not overridden, but enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforceMaximumPasswordAge = true;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertNoErrors(bizo.MaximumPasswordAgeInfo);

			// overridden and enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMaximumPasswordAge = true;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertHasErrors(bizo.MaximumPasswordAgeInfo);

			// Out of range: < min
			bizo.MaximumPasswordAge = -1;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertHasErrors(bizo.MaximumPasswordAgeInfo);
			AssertHasError(bizo.MaximumPasswordAgeInfo, validationErrorMessage);

			// Out of range: > max
			bizo.MaximumPasswordAge = 10_675_200;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertHasErrors(bizo.MaximumPasswordAgeInfo);
			AssertHasError(bizo.MaximumPasswordAgeInfo, validationErrorMessage);

			// Out of range: <= MinimumPasswordAge
			bizo.MinimumPasswordAge = 28;
			bizo.EnforceMinimumPasswordAge = false;
			bizo.MaximumPasswordAge = (int)bizo.MinimumPasswordAge - 1;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertNoErrors(bizo.MaximumPasswordAgeInfo);

			bizo.EnforceMinimumPasswordAge = true;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertHasErrors(bizo.MaximumPasswordAgeInfo);
			AssertHasError(bizo.MaximumPasswordAgeInfo, validationErrorMessage);

			bizo.MaximumPasswordAge = (int)bizo.MinimumPasswordAge;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertHasErrors(bizo.MaximumPasswordAgeInfo);
			AssertHasError(bizo.MaximumPasswordAgeInfo, validationErrorMessage);

			// Within range
			bizo.MaximumPasswordAge = (int)bizo.MinimumPasswordAge + 1;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertNoErrors(bizo.MaximumPasswordAgeInfo);

			bizo.EnforceMinimumPasswordAge = false;
			bizo.MaximumPasswordAge = 10_675_199;
			bizo.Validation.ValidateMaximumPasswordAge();
			AssertNoErrors(bizo.MaximumPasswordAgeInfo);
		}

		#endregion

		#region MinimumPasswordAge

		public void TestValidateMinimumPasswordAge()
		{
			var validationErrorMessage = "Must enter a number between 1 and Maximum Password Age (10675199 if not set). 1 is the recommended default.";
			bizo.MinimumPasswordAge = 0;

			// Not overridden, not enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforceMinimumPasswordAge = false;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertNoErrors(bizo.MinimumPasswordAgeInfo);

			// overridden but not enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMinimumPasswordAge = false;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertNoErrors(bizo.MinimumPasswordAgeInfo);

			// Not overridden, but enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforceMinimumPasswordAge = true;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertNoErrors(bizo.MinimumPasswordAgeInfo);

			// overridden and enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMinimumPasswordAge = true;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertHasErrors(bizo.MinimumPasswordAgeInfo);

			// Out of range: < min
			bizo.MinimumPasswordAge = -1;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertHasErrors(bizo.MinimumPasswordAgeInfo);
			AssertHasError(bizo.MinimumPasswordAgeInfo, validationErrorMessage);

			// Out of range: > max
			bizo.MinimumPasswordAge = 10_675_200;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertHasErrors(bizo.MinimumPasswordAgeInfo);
			AssertHasError(bizo.MinimumPasswordAgeInfo, validationErrorMessage);

			// Out of range: >= MinimumPasswordAge
			bizo.MaximumPasswordAge = 200;
			bizo.EnforceMaximumPasswordAge = false;
			bizo.MinimumPasswordAge = bizo.MaximumPasswordAge + 1;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertNoErrors(bizo.MinimumPasswordAgeInfo);

			bizo.EnforceMaximumPasswordAge = true;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertHasErrors(bizo.MinimumPasswordAgeInfo);
			AssertHasError(bizo.MinimumPasswordAgeInfo, validationErrorMessage);

			bizo.MinimumPasswordAge = (int)bizo.MaximumPasswordAge;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertHasErrors(bizo.MinimumPasswordAgeInfo);
			AssertHasError(bizo.MinimumPasswordAgeInfo, validationErrorMessage);

			// Within range
			bizo.MinimumPasswordAge = bizo.MaximumPasswordAge - 1;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertNoErrors(bizo.MinimumPasswordAgeInfo);

			bizo.EnforceMaximumPasswordAge = false;
			bizo.MinimumPasswordAge = 10_675_199;
			bizo.Validation.ValidateMinimumPasswordAge();
			AssertNoErrors(bizo.MinimumPasswordAgeInfo);
		}

		#endregion

		#region MinimumPasswordLength

		public void TestValidateMinimumPasswordLength()
		{
			var validationErrorMessage = "Must enter a number between 1 and 255. 7 is the recommended default.";
			bizo.MinimumPasswordLength = 0;

			// Not overridden, not enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforceMinimumPasswordLength = false;
			bizo.Validation.ValidateMinimumPasswordLength();
			AssertNoErrors(bizo.MinimumPasswordLengthInfo);

			// overridden but not enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMinimumPasswordLength = false;
			bizo.Validation.ValidateMinimumPasswordLength();
			AssertNoErrors(bizo.MinimumPasswordLengthInfo);

			// Not overridden, but enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforceMinimumPasswordLength = true;
			bizo.Validation.ValidateMinimumPasswordLength();
			AssertNoErrors(bizo.MinimumPasswordLengthInfo);

			// overridden and enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMinimumPasswordLength = true;
			bizo.Validation.ValidateMinimumPasswordLength();
			AssertHasErrors(bizo.MinimumPasswordLengthInfo);

			// Out of range: < min
			bizo.MinimumPasswordLength = -1;
			bizo.Validation.ValidateMinimumPasswordLength();
			AssertHasErrors(bizo.MinimumPasswordLengthInfo);
			AssertHasError(bizo.MinimumPasswordLengthInfo, validationErrorMessage);

			// Out of range: > max
			bizo.MinimumPasswordLength = 256;
			bizo.Validation.ValidateMinimumPasswordLength();
			AssertHasErrors(bizo.MinimumPasswordLengthInfo);
			AssertHasError(bizo.MinimumPasswordLengthInfo, validationErrorMessage);

			// Within range
			bizo.MinimumPasswordLength = 255;
			bizo.Validation.ValidateMinimumPasswordLength();
			AssertNoErrors(bizo.MinimumPasswordLengthInfo);
		}

		#endregion

		#region PasswordHistoryLength

		public void TestValidatePasswordHistoryLength()
		{
			var validationErrorMessage = "Must enter a number between 1 and 1024. 24 is the recommended default.";
			bizo.PasswordHistoryLength = 0;

			// Not overridden, not enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordHistoryLength = false;
			bizo.Validation.ValidatePasswordHistoryLength();
			AssertNoErrors(bizo.PasswordHistoryLengthInfo);

			// overridden but not enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordHistoryLength = false;
			bizo.Validation.ValidatePasswordHistoryLength();
			AssertNoErrors(bizo.PasswordHistoryLengthInfo);

			// Not overridden, but enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordHistoryLength = true;
			bizo.Validation.ValidatePasswordHistoryLength();
			AssertNoErrors(bizo.PasswordHistoryLengthInfo);

			// overridden and enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordHistoryLength = true;
			bizo.Validation.ValidatePasswordHistoryLength();
			AssertHasErrors(bizo.PasswordHistoryLengthInfo);

			// Out of range: < min
			bizo.PasswordHistoryLength = -1;
			bizo.Validation.ValidatePasswordHistoryLength();
			AssertHasErrors(bizo.PasswordHistoryLengthInfo);
			AssertHasError(bizo.PasswordHistoryLengthInfo, validationErrorMessage);

			// Out of range: > max
			bizo.PasswordHistoryLength = 1_025;
			bizo.Validation.ValidatePasswordHistoryLength();
			AssertHasErrors(bizo.PasswordHistoryLengthInfo);
			AssertHasError(bizo.PasswordHistoryLengthInfo, validationErrorMessage);

			// Within range
			bizo.PasswordHistoryLength = 1_024;
			bizo.Validation.ValidatePasswordHistoryLength();
			AssertNoErrors(bizo.PasswordHistoryLengthInfo);
		}

		#endregion

		#region LockoutThreshold

		public void TestValidateLockoutThreshold()
		{
			var validationErrorMessage = "Please enter a 'Number of failed logon attempts allowed' within the range 1 to 65535.";
			bizo.LockoutThreshold = 0;

			// Not overridden, not enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordLockoutPolicy = false;
			bizo.Validation.ValidateLockoutThreshold();
			AssertNoErrors(bizo.LockoutThresholdInfo);

			// overridden but not enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordLockoutPolicy = false;
			bizo.Validation.ValidateLockoutThreshold();
			AssertNoErrors(bizo.LockoutThresholdInfo);

			// Not overridden, but enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordLockoutPolicy = true;
			bizo.Validation.ValidateLockoutThreshold();
			AssertNoErrors(bizo.LockoutThresholdInfo);

			// overridden and enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordLockoutPolicy = true;
			bizo.Validation.ValidateLockoutThreshold();
			AssertHasErrors(bizo.LockoutThresholdInfo);

			// Out of range: < min
			bizo.LockoutThreshold = -1;
			bizo.Validation.ValidateLockoutThreshold();
			AssertHasErrors(bizo.LockoutThresholdInfo);
			AssertHasError(bizo.LockoutThresholdInfo, validationErrorMessage);

			// Out of range: > max
			bizo.LockoutThreshold = 65_536;
			bizo.Validation.ValidateLockoutThreshold();
			AssertHasErrors(bizo.LockoutThresholdInfo);
			AssertHasError(bizo.LockoutThresholdInfo, validationErrorMessage);

			// Within range
			bizo.LockoutThreshold = 65_535;
			bizo.Validation.ValidateLockoutThreshold();
			AssertNoErrors(bizo.LockoutThresholdInfo);
		}

		#endregion

		#region LockoutObservationWindow

		public void TestValidateLockoutObservationWindow()
		{
			var validationErrorMessage = "Must enter a number between 1 and the lockout duration. 30 is the recommended default.";
			bizo.LockoutObservationWindow = 0;
			bizo.RequireAdminUnlock = true;

			// Not overridden, not enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordLockoutPolicy = false;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertNoErrors(bizo.LockoutObservationWindowInfo);

			// overridden but not enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordLockoutPolicy = false;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertNoErrors(bizo.LockoutObservationWindowInfo);

			// Not overridden, but enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordLockoutPolicy = true;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertNoErrors(bizo.LockoutObservationWindowInfo);

			// overridden and enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordLockoutPolicy = true;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertHasErrors(bizo.LockoutObservationWindowInfo);

			// Out of range: < min
			bizo.LockoutObservationWindow = -1;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertHasErrors(bizo.LockoutObservationWindowInfo);
			AssertHasError(bizo.LockoutObservationWindowInfo, validationErrorMessage);

			// Within range
			bizo.LockoutObservationWindow = 66;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertNoErrors(bizo.LockoutObservationWindowInfo);

			// Not RequireAdminUnlock
			bizo.RequireAdminUnlock = false;
			bizo.LockoutDuration = 60;

			// Out of range: < min
			bizo.LockoutObservationWindow = -1;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertHasErrors(bizo.LockoutObservationWindowInfo);
			AssertHasError(bizo.LockoutObservationWindowInfo, validationErrorMessage);

			// Out of range: > max
			bizo.LockoutObservationWindow = bizo.LockoutDuration + 1;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertHasErrors(bizo.LockoutObservationWindowInfo);
			AssertHasError(bizo.LockoutObservationWindowInfo, validationErrorMessage);

			// Within range
			bizo.LockoutObservationWindow = bizo.LockoutDuration - 1;
			bizo.Validation.ValidateLockoutObservationWindow();
			AssertNoErrors(bizo.LockoutObservationWindowInfo);
		}

		#endregion

		#region LockoutDuration

		public void TestValidateLockoutDuration()
		{
			var validationErrorMessage = "Must enter a number greater than or equal to the higher value of 1 and reset failed logon attempts count. 30 is the recommended default.";
			bizo.LockoutDuration = 0;

			// Not overridden, not enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordLockoutPolicy = false;
			bizo.Validation.ValidateLockoutDuration();
			AssertNoErrors(bizo.LockoutDurationInfo);

			// overridden but not enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordLockoutPolicy = false;
			bizo.Validation.ValidateLockoutDuration();
			AssertNoErrors(bizo.LockoutDurationInfo);

			// Not overridden, but enforced
			bizo.OverrideDomainPasswordPolicy = false;
			bizo.EnforcePasswordLockoutPolicy = true;
			bizo.Validation.ValidateLockoutDuration();
			AssertNoErrors(bizo.LockoutDurationInfo);

			// overridden and enforced
			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordLockoutPolicy = true;
			bizo.Validation.ValidateLockoutDuration();
			AssertHasErrors(bizo.LockoutDurationInfo);

			// Not RequireAdminUnlock
			bizo.RequireAdminUnlock = false;

			// Out of range: < min
			bizo.LockoutDuration = -1;
			bizo.Validation.ValidateLockoutDuration();
			AssertHasErrors(bizo.LockoutDurationInfo);
			AssertHasError(bizo.LockoutDurationInfo, validationErrorMessage);

			// Out of range: < MinimumPasswordAge
			bizo.LockoutObservationWindow = 38;
			bizo.LockoutDuration = bizo.LockoutObservationWindow - 1;
			bizo.Validation.ValidateLockoutDuration();
			AssertHasErrors(bizo.LockoutDurationInfo);
			AssertHasError(bizo.LockoutDurationInfo, validationErrorMessage);

			// Within range
			bizo.LockoutDuration = 40;
			bizo.Validation.ValidateLockoutDuration();
			AssertNoErrors(bizo.LockoutDurationInfo);

			// RequireAdminUnlock
			bizo.RequireAdminUnlock = true;

			// Out of range: < min - does not matter
			bizo.LockoutDuration = -1;
			bizo.Validation.ValidateLockoutDuration();
			AssertNoErrors(bizo.LockoutDurationInfo);

			// Out of range: < MinimumPasswordAge - does not matter
			bizo.LockoutObservationWindow = 38;
			bizo.LockoutDuration = 20;
			bizo.Validation.ValidateLockoutDuration();
			AssertNoErrors(bizo.LockoutDurationInfo);
		}

		#endregion

		#region Implementation

		ADPasswordSettings bizo;

		protected override void SetUp()
		{
			base.SetUp();
			bizo = new ADPasswordSettings();
		}

		#endregion;
	}
}
