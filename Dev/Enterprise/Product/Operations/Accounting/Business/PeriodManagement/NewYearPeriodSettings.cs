using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class NewYearPeriodSettings : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
			public const string PeriodFormat = "PeriodFormat";
			public const string WeekDay = "WeekDay";
			public const string AccountingYearBasedType = "AccountingYearBasedType";
		}

		public static class AccountingYearBaseTypes
		{
			public const string EndDateCalendarYear = "EDY";
			public const string StartDateCalendarYear = "STY";
		}

		#endregion

		public NewYearPeriodSettings(bool isResetingPeriods = false, bool isBasedOnStartDate = false)
		{
			using (SuspendSettingHasChanges())
			{
				PeriodFormat = Core.Constants.ACPeriodFormat.Month;
				WeekDay = DayOfWeekCodeList.Codes.Friday;
				IsResetingPeriods = isResetingPeriods;
				AccountingYearBasedType = isBasedOnStartDate ? AccountingYearBaseTypes.StartDateCalendarYear : AccountingYearBaseTypes.EndDateCalendarYear;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePeriodFormat();
			ValidateWeekDay();
			ValidateStartDate();
			ValidateEndDate();
			ValidateAccountingYearBasedType();
		}

		internal ZInt MaximumNumberOfDaysInFinancialYear
		{
			get { return 400; }
		}

		internal ZInt MinimumNumberOfDaysInFinancialYear
		{
			get { return 330; }
		}

		#region StartDate

		public ZDateTime StartDate
		{
			get { return fStartDate; }
			set
			{
				fStartDate = value;
				ResetStartDate();
				StartDateInfo.RefreshBinding();
			}
		}

		void ResetStartDate()
		{
			if (PeriodFormat == Core.Constants.ACPeriodFormat.Month)
			{
				EndDate = ZDateTime.Empty;
			}

			ValidateStartDate();

			if (PeriodFormat == Core.Constants.ACPeriodFormat.Month)
			{
				EndDate = !StartDateInfo.HasErrors() ? StartDate.AddYears(1).AddDays(-1) : ZDateTime.Empty;
				EndDateInfo.RefreshBinding();
			}

			ValidateAccountingYearBasedType();
		}

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(Schema.StartDate); }
		}

		protected bool StartDate_ReadOnly
		{
			get { return periodManagementFormEvent; }
		}

		public void PeriodManagementFormEvent(bool readOnly)
		{
			periodManagementFormEvent = readOnly;
		}

		bool periodManagementFormEvent;
		#endregion

		#region EndDate

		public ZDateTime EndDate
		{
			get { return fEndDate; }
			set
			{
				fEndDate = value;
				if (PeriodFormat != Core.Constants.ACPeriodFormat.Month)
				{
					ValidateEndDate();
					ValidateAccountingYearBasedType();
				}
				else
				{
					EndDateInfo.ClearAllNotifications();
				}
				EndDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(Schema.EndDate); }
		}

		protected bool EndDate_ReadOnly
		{
			get { return PeriodFormat == Core.Constants.ACPeriodFormat.Month; }
		}

		#endregion

		#region PeriodFormat

		[MaxLength(3)]
		[List("PeriodType")]
		public ZString PeriodFormat
		{
			get { return periodFormat; }
			set
			{
				if (periodFormat != value)
				{
					CheckMaximumLength(PeriodFormatInfo, value);
					SetNonPersistentPropertyValue(PeriodFormatInfo, ref periodFormat, value);
					WeekDayInfo.RefreshBinding();
					if (PeriodFormat == Core.Constants.ACPeriodFormat.Month)
					{
						ResetStartDate();
					}
				}
			}
		}

		public ZPropertyInfo PeriodFormatInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodFormat); }
		}

		#endregion

		#region WeekDay

		[MaxLength(3)]
		[List("WeekDayType")]
		public ZString WeekDay
		{
			get { return weekDay; }
			set
			{
				if (weekDay != value)
				{
					CheckMaximumLength(WeekDayInfo, value);
					SetNonPersistentPropertyValue(WeekDayInfo, ref weekDay, value);
				}
			}
		}

		public ZPropertyInfo WeekDayInfo
		{
			get { return GetZPropertyInfo(Schema.WeekDay); }
		}

		protected bool WeekDay_ReadOnly
		{
			get { return PeriodFormat == Core.Constants.ACPeriodFormat.Month; }
		}

		#endregion

		#region IsPeriodsSetBefore

		public ZBool IsPeriodsSetBefore
		{
			get { return isPeriodsSetBefore; }
			set
			{
				isPeriodsSetBefore = value;
			}
		}

		#endregion

		public ZPropertyInfo AccountingYearBasedTypeInfo
		{
			get { return GetZPropertyInfo(Schema.AccountingYearBasedType); }
		}

		#region AccountingYearBasedType

		[MaxLength(3)]
		[List("AccountingYearBasedTypes")]
		public ZString AccountingYearBasedType
		{
			get { return accountingYearBasedType; }
			set
			{
				if (accountingYearBasedType != value)
				{
					CheckMaximumLength(AccountingYearBasedTypeInfo, value);
					SetNonPersistentPropertyValue(AccountingYearBasedTypeInfo, ref accountingYearBasedType, value);
					ValidateAccountingYearBasedType();
				}
			}
		}

		#endregion

		public int DayOfWeek
		{
			get
			{
				switch (WeekDay)
				{
					case DayOfWeekCodeList.Codes.Sunday: return 0;
					case DayOfWeekCodeList.Codes.Monday: return 1;
					case DayOfWeekCodeList.Codes.Tuesday: return 2;
					case DayOfWeekCodeList.Codes.Wednesday: return 3;
					case DayOfWeekCodeList.Codes.Thursday: return 4;
					case DayOfWeekCodeList.Codes.Friday: return 5;
					case DayOfWeekCodeList.Codes.Saturday: return 6;
				}
				return -1;
			}
		}

		public CodeDescriptionPairList PeriodType
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.ACPeriodCountType); }
		}

		public CodeDescriptionPairList WeekDayType
		{
			get { return new DayOfWeekCodeList(); }
		}

		public CodeDescriptionPairList AccountingYearBasedTypes
		{
			get
			{
				if (accountingYearBasedTypes == null)
				{
					accountingYearBasedTypes = new CodeDescriptionPairList();
					accountingYearBasedTypes.AddPair(AccountingYearBaseTypes.EndDateCalendarYear, Res.GetString("1605546C-A113-4B4E-BD61-5FBA5C4AE434", "End Date Calendar Year"));
					accountingYearBasedTypes.AddPair(AccountingYearBaseTypes.StartDateCalendarYear, Res.GetString("385D7DF4-A146-4319-9108-17B09FF78A35", "Start Date Calendar Year"));
				}

				return accountingYearBasedTypes;
			}
		}
		CodeDescriptionPairList accountingYearBasedTypes;

		#region Validation

		public void ValidateWeekDay()
		{
			if (!IsValidationSuspended)
			{
				WeekDayInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(WeekDayInfo);
				ListValidation.ErrorIfInvalidCode(WeekDayInfo, WeekDayType);
			}
		}

		public void ValidatePeriodFormat()
		{
			if (!IsValidationSuspended)
			{
				PeriodFormatInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(PeriodFormatInfo);
				ListValidation.ErrorIfInvalidCode(PeriodFormatInfo, PeriodType);
			}
		}

		public void ValidateAccountingYearBasedType()
		{
			if (!IsValidationSuspended)
			{
				AccountingYearBasedTypeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AccountingYearBasedTypeInfo);
				ListValidation.ErrorIfInvalidCode(AccountingYearBasedTypeInfo, AccountingYearBasedTypes);
				if (StartDate.IsValid && EndDate.IsValid && StartDate.Year == EndDate.Year && AccountingYearBasedType == AccountingYearBaseTypes.StartDateCalendarYear)
				{
					AccountingYearBasedTypeInfo.AddError(Res.GetString("7EF239EA-4E80-4D51-9DB9-FD97BD7C5C85", "The Accounting Year Based On option should remain as EDY if the Start Date and End Date fall into the same year."));
				}
			}
		}

		public void ValidateStartDate()
		{
			if (!IsValidationSuspended)
			{
				StartDateInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(StartDateInfo);

				if (!StartDateInfo.ReadOnly && StartDate.IsValid)
				{
					if (StartDate.Year < (ZDateTime.Now.Year - 1) && !IsResetingPeriods)
					{
						StartDateInfo.AddError(Res.GetString("5c7b5f7c-2c3a-41a1-a037-9582f9dd6c99", "Start date must be equal to or later than 1/Jan/") + (ZDateTime.Now.Year - 1));
					}
					if (StartDate.Date > ZDateTime.Now.Date)
					{
						StartDateInfo.AddError(Res.GetString("a8e7529c-773c-445f-92a4-30dce0b8cf1f", "Start date must be less than or equal to today"));
					}
					if (!StartDateInfo.HasErrors() && IsMoreThanMaximumNumberOfDaysInFinancialYear)
					{
						StartDateInfo.AddError(Res.GetString("c9e71771-db76-4d2b-b554-04d38d559c03", "The maximum number of days in a financial year is {0} days", MaximumNumberOfDaysInFinancialYear));
					}
					if (!StartDateInfo.HasErrors() && IsLessThanMinimumNumberOfDaysInFinancialYear)
					{
						StartDateInfo.AddError(Res.GetString("2e937cfa-9488-4cdb-b183-031d006079b9", "The minimum number of days in a financial year is {0} days", MinimumNumberOfDaysInFinancialYear));
					}
					if (!StartDateInfo.HasErrors() && PeriodFormat == Core.Constants.ACPeriodFormat.Month && StartDate.Day != 1)
					{
						StartDateInfo.AddError(Res.GetString("7af8ebe4-7385-4093-8411-127c9f696e23", "For calendar months period format, financial year must start on 1st day of a month"));
					}
					if (!StartDateInfo.HasErrors() && StartDate.Day != 1)
					{
						StartDateInfo.AddWarning(Res.GetString("ba852057-8d68-480b-9e17-9f61ae43930e", "Financial year normally starts on 1st day of a month"));
					}
				}
				else if (!StartDate.IsValid)
				{
					StartDateInfo.AddError(Res.GetString("4894c647-2520-4663-a08d-3cbb5d41694b", "Please enter a valid date"));
				}
			}
		}

		public void ValidateEndDate()
		{
			if (!IsValidationSuspended)
			{
				EndDateInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(EndDateInfo);

				if (!EndDateInfo.ReadOnly && EndDate.IsValid)
				{
					if (EndDate.Date <= StartDate.Date)
					{
						EndDateInfo.AddError(Res.GetString("73e09d48-5508-428f-8399-956756f0a3a4", "End date must be later than start date"));
					}
					if (!EndDateInfo.HasErrors() && IsMoreThanMaximumNumberOfDaysInFinancialYear)
					{
						EndDateInfo.AddError(Res.GetString("c9e71771-db76-4d2b-b554-04d38d559c03", "The maximum number of days in a financial year is {0} days", MaximumNumberOfDaysInFinancialYear));
					}
					if (!EndDateInfo.HasErrors() && IsLessThanMinimumNumberOfDaysInFinancialYear)
					{
						EndDateInfo.AddError(Res.GetString("2e937cfa-9488-4cdb-b183-031d006079b9", "The minimum number of days in a financial year is {0} days", MinimumNumberOfDaysInFinancialYear));
					}
					ZInt nextMonth = EndDate.AddDays(1).Month;
					ZInt year = EndDate.AddDays(1).Year;
					if (!EndDateInfo.HasErrors() && year == EndDate.Year && nextMonth <= EndDate.Month)
					{
						EndDateInfo.AddWarning(Res.GetString("61980fe1-0870-47d1-ba1f-2b82b7d565f7", "Financial year normally ends on the last day of a month"));
					}
				}
				else if (!EndDate.IsValid)
				{
					EndDateInfo.AddError(Res.GetString("4894c647-2520-4663-a08d-3cbb5d41694b", "Please enter a valid date"));
				}
			}
		}

		bool IsMoreThanMaximumNumberOfDaysInFinancialYear
		{
			get
			{
				if (StartDate.IsValid && EndDate.IsValid)
				{
					ZDateTime zEndDate = EndDate.AddDays(-(MaximumNumberOfDaysInFinancialYear));
					return (zEndDate.Date > StartDate.Date);
				}
				else
				{
					return false;
				}
			}
		}

		bool IsLessThanMinimumNumberOfDaysInFinancialYear
		{
			get
			{
				if (StartDate.IsValid && EndDate.IsValid)
				{
					ZDateTime zEndDate = EndDate.AddDays(-(MinimumNumberOfDaysInFinancialYear));
					return (zEndDate.Date < StartDate.Date);
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Implementation

		ZDateTime fStartDate;
		ZDateTime fEndDate;
		ZString periodFormat;
		ZString weekDay;
		readonly ZBool IsResetingPeriods;
		ZBool isPeriodsSetBefore;
		ZString accountingYearBasedType;

		#endregion
	}
}
