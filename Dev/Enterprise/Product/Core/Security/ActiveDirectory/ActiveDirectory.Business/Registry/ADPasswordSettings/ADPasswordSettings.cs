using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Licensing;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADPasswordSettings : NonPersistentBusinessObject
	{
		public ADPasswordSettings()
			: base()
		{
		}

		#region Properties

		#region Override Domain Password Policy

		[ResourceStringData("PasswordSettings.OverrideDomainPasswordPolicy", Caption = "Override Domain Password Policy")]
		public ZBool OverrideDomainPasswordPolicy
		{
			get => overrideDomainPasswordPolicy;
			set
			{
				SetNonPersistentPropertyValue(OverrideDomainPasswordPolicyInfo, ref overrideDomainPasswordPolicy, value);
			}
		}

		ZBool overrideDomainPasswordPolicy;

		public ZPropertyInfo OverrideDomainPasswordPolicyInfo => GetZPropertyInfo(nameof(OverrideDomainPasswordPolicy));

		#endregion

		#region MaximumPasswordAge

		[ResourceStringData("PasswordSettings.MaximumPasswordAge", Caption = "User must change password after (days)", ShortCaption = "Maximum Password Age")]
		public ZInt MaximumPasswordAge
		{
			get => maximumPasswordAge;
			set
			{
				SetNonPersistentPropertyValue(MaximumPasswordAgeInfo, ref maximumPasswordAge, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMaximumPasswordAge();
				}
			}
		}
		ZInt maximumPasswordAge;

		public ZPropertyInfo MaximumPasswordAgeInfo => GetZPropertyInfo(nameof(MaximumPasswordAge));

		public bool MaximumPasswordAge_ReadOnly => !OverrideDomainPasswordPolicy || !EnforceMaximumPasswordAge;

		#endregion

		#region MinimumPasswordAge

		[ResourceStringData("PasswordSettings.MinimumPasswordAge", Caption = "User cannot change password within (days)", ShortCaption = "Minimum Password Age")]
		public ZInt MinimumPasswordAge
		{
			get => minimumPasswordAge;
			set
			{
				SetNonPersistentPropertyValue(MinimumPasswordAgeInfo, ref minimumPasswordAge, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMinimumPasswordAge();
				}
			}
		}
		ZInt minimumPasswordAge;

		public ZPropertyInfo MinimumPasswordAgeInfo => GetZPropertyInfo(nameof(MinimumPasswordAge));

		public bool MinimumPasswordAge_ReadOnly => !OverrideDomainPasswordPolicy || !EnforceMinimumPasswordAge;

		#endregion

		#region MinimumPasswordLength

		[ResourceStringData("PasswordSettings.MinimumPasswordLength", Caption = "Minimum password length (characters)")]
		public ZInt MinimumPasswordLength
		{
			get => minimumPasswordLength;
			set
			{
				SetNonPersistentPropertyValue(MinimumPasswordLengthInfo, ref minimumPasswordLength, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMinimumPasswordLength();
				}
			}
		}

		ZInt minimumPasswordLength;

		public ZPropertyInfo MinimumPasswordLengthInfo => GetZPropertyInfo(nameof(MinimumPasswordLength));

		public bool MinimumPasswordLength_ReadOnly => !OverrideDomainPasswordPolicy || !EnforceMinimumPasswordLength;

		#endregion

		#region PasswordHistoryLength

		[ResourceStringData("PasswordSettings.PasswordHistoryLength", Caption = "Number of passwords remembered")]
		public ZInt PasswordHistoryLength
		{
			get => passwordHistoryLength;
			set
			{
				SetNonPersistentPropertyValue(PasswordHistoryLengthInfo, ref passwordHistoryLength, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePasswordHistoryLength();
				}
			}
		}

		ZInt passwordHistoryLength;

		public ZPropertyInfo PasswordHistoryLengthInfo => GetZPropertyInfo(nameof(PasswordHistoryLength));

		public bool PasswordHistoryLength_ReadOnly => !OverrideDomainPasswordPolicy || !EnforcePasswordHistoryLength;

		#endregion

		#region PasswordComplexityEnabled

		[ResourceStringData("PasswordSettings.PasswordComplexityEnabled", Caption = "Password must meet complexity requirements")]
		public ZBool PasswordComplexityEnabled
		{
			get => passwordComplexityEnabled;
			set
			{
				SetNonPersistentPropertyValue(PasswordComplexityEnabledInfo, ref passwordComplexityEnabled, value);
			}
		}

		ZBool passwordComplexityEnabled;

		public ZPropertyInfo PasswordComplexityEnabledInfo => GetZPropertyInfo(nameof(PasswordComplexityEnabled));

		public bool PasswordComplexityEnabled_ReadOnly => !OverrideDomainPasswordPolicy;

		#endregion

		#region LockoutThreshold

		[ResourceStringData("PasswordSettings.LockoutThreshold", Caption = "Number of failed logon attempts allowed")]
		public ZInt LockoutThreshold
		{
			get => lockoutThreshold;
			set
			{
				SetNonPersistentPropertyValue(LockoutThresholdInfo, ref lockoutThreshold, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateLockoutThreshold();
				}
			}
		}

		ZInt lockoutThreshold;

		public ZPropertyInfo LockoutThresholdInfo => GetZPropertyInfo(nameof(LockoutThreshold));

		public bool LockoutThreshold_ReadOnly => !OverrideDomainPasswordPolicy || !EnforcePasswordLockoutPolicy;

		#endregion

		#region LockoutObservationWindow

		[ResourceStringData("PasswordSettings.LockoutObservationWindow", Caption = "Reset failed logon attempts count after (minutes)", ShortCaption = "reset failed logon attempts count")]
		public ZInt LockoutObservationWindow
		{
			get => lockoutObservationWindow;
			set
			{
				SetNonPersistentPropertyValue(LockoutObservationWindowInfo, ref lockoutObservationWindow, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateLockoutObservationWindow();
				}
			}
		}

		ZInt lockoutObservationWindow;

		public ZPropertyInfo LockoutObservationWindowInfo => GetZPropertyInfo(nameof(LockoutObservationWindow));

		public bool LockoutObservationWindow_ReadOnly => !OverrideDomainPasswordPolicy || !EnforcePasswordLockoutPolicy;

		#endregion

		#region LockoutDuration

		[ResourceStringData("PasswordSettings.LockoutDuration", Caption = "Account will be locked out for a duration of (minutes)", ShortCaption = "lockout duration")]
		public ZInt LockoutDuration
		{
			get => lockoutDuration;
			set
			{
				SetNonPersistentPropertyValue(LockoutDurationInfo, ref lockoutDuration, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateLockoutDuration();
				}
			}
		}

		ZInt lockoutDuration;

		public ZPropertyInfo LockoutDurationInfo => GetZPropertyInfo(nameof(LockoutDuration));

		public bool LockoutDuration_ReadOnly => !OverrideDomainPasswordPolicy || !EnforcePasswordLockoutPolicy || RequireAdminUnlock;

		#endregion

		#region RequireAdminUnlock

		[ResourceStringData("PasswordSettings.RequireAdminUnlock", Caption = "Lockout Until an administrator manually unlocks the account")]

		public ZBool RequireAdminUnlock
		{
			get => requireAdminUnlock;
			set
			{
				SetNonPersistentPropertyValue(RequireAminUnlockInfo, ref requireAdminUnlock, value);
			}
		}

		ZBool requireAdminUnlock;

		public ZPropertyInfo RequireAminUnlockInfo => GetZPropertyInfo(nameof(RequireAdminUnlock));

		public bool RequireAdminUnlock_ReadOnly => !OverrideDomainPasswordPolicy || !EnforcePasswordLockoutPolicy;

		#endregion

		#region EnforceMinimumPasswordLength

		[ResourceStringData("PasswordSettings.EnforceMinimumPasswordLength", Caption = "Enforce minimum password length")]
		public ZBool EnforceMinimumPasswordLength
		{
			get => enforceMinimumPasswordLength;
			set
			{
				SetNonPersistentPropertyValue(EnforceMinimumPasswordLengthInfo, ref enforceMinimumPasswordLength, value);
			}
		}

		ZBool enforceMinimumPasswordLength;

		public ZPropertyInfo EnforceMinimumPasswordLengthInfo => GetZPropertyInfo(nameof(EnforceMinimumPasswordLength));

		public bool EnforceMinimumPasswordLength_ReadOnly => !OverrideDomainPasswordPolicy;

		#endregion

		#region EnforcePasswordHistory

		[ResourceStringData("PasswordSettings.EnforcePasswordHistoryLength", Caption = "Enforce password history")]
		public ZBool EnforcePasswordHistoryLength
		{
			get => enforcePasswordHistoryLength;
			set
			{
				SetNonPersistentPropertyValue(EnforcePasswordHistoryLengthInfo, ref enforcePasswordHistoryLength, value);
			}
		}

		ZBool enforcePasswordHistoryLength;

		public ZPropertyInfo EnforcePasswordHistoryLengthInfo => GetZPropertyInfo(nameof(EnforcePasswordHistoryLength));

		public bool EnforcePasswordHistoryLength_ReadOnly => !OverrideDomainPasswordPolicy;

		#endregion

		#region EnforceMinimumPasswordAge

		[ResourceStringData("PasswordSettings.EnforceMinimumPasswordAge", Caption = "Enforce minimum password age")]
		public ZBool EnforceMinimumPasswordAge
		{
			get => enforceMinimumPasswordAge;
			set
			{
				SetNonPersistentPropertyValue(EnforceMinimumPasswordAgeInfo, ref enforceMinimumPasswordAge, value);
			}
		}

		ZBool enforceMinimumPasswordAge;

		public ZPropertyInfo EnforceMinimumPasswordAgeInfo => GetZPropertyInfo(nameof(EnforceMinimumPasswordAge));

		public bool EnforceMinimumPasswordAge_ReadOnly => !OverrideDomainPasswordPolicy;

		#endregion

		#region EnforceMaximumPasswordAge

		[ResourceStringData("PasswordSettings.EnforceMaximumPasswordAge", Caption = "Enforce maximum password age")]
		public ZBool EnforceMaximumPasswordAge
		{
			get => enforceMaximumPasswordAge;
			set
			{
				SetNonPersistentPropertyValue(EnforceMaximumPasswordAgeInfo, ref enforceMaximumPasswordAge, value);
			}
		}

		ZBool enforceMaximumPasswordAge;

		public ZPropertyInfo EnforceMaximumPasswordAgeInfo => GetZPropertyInfo(nameof(EnforceMaximumPasswordAge));

		public bool EnforceMaximumPasswordAge_ReadOnly => !OverrideDomainPasswordPolicy;

		#endregion

		#region EnforcePasswordLockoutPolicy

		[ResourceStringData("PasswordSettings.EnforcePasswordLockoutPolicy", Caption = "Enforce password lockout policy")]
		public ZBool EnforcePasswordLockoutPolicy
		{
			get => enforcePasswordLockoutPolicy;
			set
			{
				SetNonPersistentPropertyValue(EnforcePasswordLockoutPolicyInfo, ref enforcePasswordLockoutPolicy, value);
			}
		}

		ZBool enforcePasswordLockoutPolicy;

		public ZPropertyInfo EnforcePasswordLockoutPolicyInfo => GetZPropertyInfo(nameof(EnforcePasswordLockoutPolicy));

		public bool EnforcePasswordLockoutPolicy_ReadOnly => !OverrideDomainPasswordPolicy;

		#endregion

		#region AD Password Settings Object Name

		[ResourceStringData("PasswordSettings.ADPasswordSettingsObjectName", Caption = "AD Password Settings Object Name")]
		public ZString ADPasswordSettingsObjectName => GetDefaultADPasswordSettingsObjectName();

		public ZPropertyInfo ADPasswordSettingsObjectNameInfo => GetZPropertyInfo(nameof(ADPasswordSettingsObjectName));

		public bool ADPasswordSettingsObjectName_ReadOnly => true;

		protected string GetDefaultADPasswordSettingsObjectName()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return registrationKey.EnterpriseCode + registrationKey.ServerCode + "_PasswordPolicy";
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ADPasswordSettingsValidation Validation => new ADPasswordSettingsValidation(this);

		#endregion
	}
}
