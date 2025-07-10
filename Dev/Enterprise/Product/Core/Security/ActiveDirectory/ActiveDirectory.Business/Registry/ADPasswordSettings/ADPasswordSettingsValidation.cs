using System;
using CargoWise.EntityFramework;
using static Enterprise.Security.ActiveDirectory.ADPasswordSettingsConstants;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADPasswordSettingsValidation : ZValidation
	{
		public ADPasswordSettingsValidation(ADPasswordSettings parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly ADPasswordSettings parent;

		public override Type AutoValidationType => typeof(ADPasswordSettingsValidation);

		public override void ValidateAll()
		{
			if (!parent.IsValidationSuspended)
			{
				ValidateMaximumPasswordAge();
				ValidateMinimumPasswordAge();
				ValidateMinimumPasswordLength();
				ValidatePasswordHistoryLength();
				ValidateLockoutThreshold();
				ValidateLockoutObservationWindow();
				ValidateLockoutDuration();
			}
		}

		public void ValidateMaximumPasswordAge()
		{
			ValidateCalculatedProperty(parent.MaximumPasswordAgeInfo);
		}

		protected void CheckMaximumPasswordAge()
		{
			if (parent.OverrideDomainPasswordPolicy && parent.EnforceMaximumPasswordAge)
			{
				MandatoryValidation.CheckEntered(parent.MaximumPasswordAgeInfo);
				if (parent.MaximumPasswordAge > PasswordAge.Maximum || parent.MaximumPasswordAge < (parent.EnforceMinimumPasswordAge ? Math.Max(PasswordAge.Minimun, parent.MinimumPasswordAge + 1) : PasswordAge.Minimun)) // MS AD's rule is parent.MinimumPasswordAge + 1 although its message said betweeen, let's just follow its rule and message
				{
					var minimumPasswordAgeResourceString = DataBoundResourceStrings.GetDataForProperty(parent.MinimumPasswordAgeInfo.PropertyDescriptor);
					parent.MaximumPasswordAgeInfo.AddError(Res.GetString("361A8B28-C6C4-4EBE-8922-B8B3D5F4ECDA", "Must enter a number between {0} ({1} if not set) and {2}. {3} is the recommended default.", minimumPasswordAgeResourceString.ShortCaption, PasswordAge.Minimun, PasswordAge.Maximum, PasswordAge.MaximumRecommended));
				}
			}
		}

		public void ValidateMinimumPasswordAge()
		{
			ValidateCalculatedProperty(parent.MinimumPasswordAgeInfo);
		}

		protected void CheckMinimumPasswordAge()
		{
			if (parent.OverrideDomainPasswordPolicy && parent.EnforceMinimumPasswordAge)
			{
				MandatoryValidation.CheckEntered(parent.MinimumPasswordAgeInfo);
				if (parent.MinimumPasswordAge < PasswordAge.Minimun || parent.MinimumPasswordAge > (parent.EnforceMaximumPasswordAge ? Math.Min(parent.MaximumPasswordAge - 1, PasswordAge.Maximum) : PasswordAge.Maximum)) // MS AD's rule is parent.MaximumPasswordAge - 1 although its message says between, let's just follow its rule and message
				{
					var maximumPasswordAgeResourceString = DataBoundResourceStrings.GetDataForProperty(parent.MaximumPasswordAgeInfo.PropertyDescriptor);
					parent.MinimumPasswordAgeInfo.AddError(Res.GetString("2EF37BCD-E93B-4973-A2A9-E5341827895F", "Must enter a number between {0} and {1} ({2} if not set). {3} is the recommended default.", PasswordAge.Minimun, maximumPasswordAgeResourceString.ShortCaption, PasswordAge.Maximum, PasswordAge.MinimunRecommended));
				}
			}
		}

		public void ValidateMinimumPasswordLength()
		{
			ValidateCalculatedProperty(parent.MinimumPasswordLengthInfo);
		}

		protected void CheckMinimumPasswordLength()
		{
			if (parent.OverrideDomainPasswordPolicy && parent.EnforceMinimumPasswordLength)
			{
				MandatoryValidation.CheckEntered(parent.MinimumPasswordLengthInfo);
				if (parent.MinimumPasswordLength < PasswordLength.Minimum || parent.MinimumPasswordLength > PasswordLength.Maximum)
				{
					parent.MinimumPasswordLengthInfo.AddError(Res.GetString("D13DFC4B-D947-447B-91F5-68EA8F274152", "Must enter a number between {0} and {1}. {2} is the recommended default.", PasswordLength.Minimum, PasswordLength.Maximum, PasswordLength.Recommended));
				}
			}
		}

		public void ValidatePasswordHistoryLength()
		{
			ValidateCalculatedProperty(parent.PasswordHistoryLengthInfo);
		}

		protected void CheckPasswordHistoryLength()
		{
			if (parent.OverrideDomainPasswordPolicy && parent.EnforcePasswordHistoryLength)
			{
				MandatoryValidation.CheckEntered(parent.PasswordHistoryLengthInfo);
				if (parent.PasswordHistoryLength < PasswordHistoryLength.Minimum || parent.PasswordHistoryLength > PasswordHistoryLength.Maximum)
				{
					parent.PasswordHistoryLengthInfo.AddError(Res.GetString("CB08D2AC-E9F3-4B7E-AF17-0DBC01FC0A28", "Must enter a number between {0} and {1}. {2} is the recommended default.", PasswordHistoryLength.Minimum, PasswordHistoryLength.Maximum, PasswordHistoryLength.Recommended));
				}
			}
		}

		public void ValidateLockoutThreshold()
		{
			ValidateCalculatedProperty(parent.LockoutThresholdInfo);
		}

		protected void CheckLockoutThreshold()
		{
			if (parent.OverrideDomainPasswordPolicy && parent.EnforcePasswordLockoutPolicy)
			{
				MandatoryValidation.CheckEntered(parent.LockoutThresholdInfo);
				CompareValidation.CheckWithinRange(parent.LockoutThresholdInfo, LockoutThreshold.Minimum, LockoutThreshold.Maximum);
			}
		}

		public void ValidateLockoutObservationWindow()
		{
			ValidateCalculatedProperty(parent.LockoutObservationWindowInfo);
		}

		protected void CheckLockoutObservationWindow()
		{
			if (parent.OverrideDomainPasswordPolicy && parent.EnforcePasswordLockoutPolicy)
			{
				MandatoryValidation.CheckEntered(parent.LockoutObservationWindowInfo);
				if (parent.LockoutObservationWindow < LockoutObservationWindow.Minimum || (!parent.RequireAdminUnlock && parent.LockoutObservationWindow > parent.LockoutDuration))
				{
					var lockOutDurationResourceString = DataBoundResourceStrings.GetDataForProperty(parent.LockoutDurationInfo.PropertyDescriptor);
					parent.LockoutObservationWindowInfo.AddError(Res.GetString("DB81895F-8B7B-4B88-9377-1ECFEC14F475", "Must enter a number between {0} and the {1}. {2} is the recommended default.", LockoutObservationWindow.Minimum, lockOutDurationResourceString.ShortCaption, LockoutObservationWindow.Recommended));
				}
			}
		}

		public void ValidateLockoutDuration()
		{
			ValidateCalculatedProperty(parent.LockoutDurationInfo);
		}

		protected void CheckLockoutDuration()
		{
			if (parent.OverrideDomainPasswordPolicy && parent.EnforcePasswordLockoutPolicy && !parent.RequireAdminUnlock)
			{
				MandatoryValidation.CheckEntered(parent.LockoutDurationInfo);
				if (!parent.RequireAdminUnlock)
				{
					if (parent.LockoutDuration < Math.Max(LockoutDuration.Minimum, parent.LockoutObservationWindow))
					{
						var lockoutObservationWindowResourceString = DataBoundResourceStrings.GetDataForProperty(parent.LockoutObservationWindowInfo.PropertyDescriptor);
						parent.LockoutDurationInfo.AddError(Res.GetString("26957320-7D4C-47A1-B0D3-E114C3D84C84", "Must enter a number greater than or equal to the higher value of {0} and {1}. {2} is the recommended default.", LockoutDuration.Minimum, lockoutObservationWindowResourceString.ShortCaption, LockoutDuration.Recommended));
					}
				}
			}
		}
	}
}
