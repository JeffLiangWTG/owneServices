//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiLicenceSettingValidation
//
//    This class should be used for overriding validation in AutoEdiLicenceSettingValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class EdiLicenceSettingValidation : AutoEdiLicenceSettingValidation
	{
		public EdiLicenceSettingValidation(AutoEdiLicenceSetting parent) : base(parent)
		{
		}

		public new EdiLicenceSetting Parent { get { return (EdiLicenceSetting)(base.Parent);  } }

		protected override void CheckLS9_Type()
		{
			base.CheckLS9_Type();
			MandatoryValidation.CheckEntered(Parent.LS9_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LS9_TypeInfo);

			if (!Parent.LS9_TypeInfo.HasErrors())
			{
				var parent = Parent;
				var db = parent.Database;
				if (db != null)
				{
					foreach (var another in db.LicenceSettings)
					{
						if (another.PK != parent.PK &&
							(another.LS9_Type == parent.LS9_Type || (another is PriceLicenceSetting && parent is PriceLicenceSetting)) &&
							(another.LS9_Type == BillingConstants.LicenceSetting.BuyingGroup || another.LS9_Name.EqualsIgnoringCase(parent.LS9_Name)) &&
							HaveOverlap(parent.LS9_ValidFrom, parent.LS9_ValidTo, another.LS9_ValidFrom, another.LS9_ValidTo))
						{
							parent.LS9_TypeInfo.AddError("Another setting with overlapping dates exists. There can only be one setting valid at a time.");
						}
					}
				}
			}
		}

		protected override void CheckLS9_Percent()
		{
			base.CheckLS9_Percent();
			if (Parent.LS9_Percent == 0 &&
				Parent.LS9_Type == BillingConstants.LicenceSetting.Discount &&
				Parent.LS9_IsActive)
			{
				Parent.LS9_PercentInfo.AddWarning("Zero means use default percentage off pricelist. Untick Active if you really mean to have no discount.");
			}
		}

		protected static bool HaveOverlap(ZDateTime start1, ZDateTime end1, ZDateTime start2, ZDateTime end2)
		{
			return (start2.IsEmpty || end1.IsEmpty || start2 <= end1)
				&& (start1.IsEmpty || end2.IsEmpty || start1 <= end2);
		}

		protected override void CheckLS9_ValidFrom()
		{
			base.CheckLS9_ValidFrom();

			if (!Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.LS9_ValidFromInfo);
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Parent.LS9_ValidFromInfo);
			}
		}

		protected override void CheckLS9_ValidFromIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.LS9_ValidFromInfo, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 10,
				FutureYearsBeforeWarning = 5,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});

			if (Parent.LS9_ValidFrom.IsValid && Parent.LS9_ValidFrom.Day != 1
				&& (!Parent.IsInDatabase || Parent.LS9_ValidFromInfo.HasChanges))
			{
				Parent.LS9_ValidFromInfo.AddError("Date must be the first day of the month.");
			}
		}

		protected override void CheckLS9_ValidToIsValidZDateTimeRange()
		{
			var info = Parent.LS9_ValidToInfo;

			TypeValidation.CheckValidZDateTimeRange(info, new TypeValidationLimits()
			{
				FutureYearsBeforeError = 50,
				FutureYearsBeforeWarning = 15,
				PastYearsBeforeError = 50,
				PastYearsBeforeWarning = 15
			});

			if (Parent.LS9_ValidTo.IsValid && Parent.LS9_ValidTo.Month == Parent.LS9_ValidTo.AddDays(1).Month
				&& (!Parent.IsInDatabase || info.HasChanges))
			{
				info.AddError("Date must be the last day of the month.");
			}
			else if (Parent.LS9_ValidTo.IsValid && !Parent.LS9_ValidTo.IsEmpty && !Parent.LS9_ValidFrom.IsEmpty)
			{
				CompareValidation.CheckDateIsAfterAnotherDate(info, Parent.LS9_ValidFromInfo);
			}
		}
	}
}

