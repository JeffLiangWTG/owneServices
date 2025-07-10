using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class ExtendLastFinancialYearSettingsValidation : ZValidation
	{
		public ExtendLastFinancialYearSettingsValidation(ExtendLastFinancialYearSettings parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			ValidatePeriodFormat();
			ValidateEndDate();
		}

		public void ValidatePeriodFormat()
		{
			Parent.PeriodFormatInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.PeriodFormatInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PeriodFormatInfo, Parent.PeriodType);
		}

		public void ValidateEndDate()
		{
			Parent.EndDateInfo.ClearAllNotifications();
			var endDate = Parent.EndDate;
			var endDateInfo = Parent.EndDateInfo;
			MandatoryValidation.CheckEntered(endDateInfo, Res.GetString("47f84149-2070-4b03-8cd7-914671f8ce77", "Financial Year End Date"));
			TypeValidation.CheckValidSmallDateTime(endDateInfo);
			if (!endDateInfo.HasErrors() && endDate.IsValid)
			{
				var nextMonth = endDate.AddDays(1).Month;
				if (endDate.Date < Parent.LastPeriodEndDate.Date)
				{
					endDateInfo.AddError(Res.GetString("0EB180A6-8F46-4DC8-9DEE-2CAAA6B4E9A6", "End Date cannot be earlier than Last Day of Last Accounting Period"));
				}
				if (nextMonth == endDate.Month && !endDateInfo.HasErrors())
				{
					if (Parent.PeriodFormat == ACPeriodFormat.Month)
					{
						endDateInfo.AddError(Res.GetString("0E7EEFA3-24E1-43BA-A6C7-1E88584D2478", "Financial Year End Date must be equal to Last Day of Month"));
					}
					else
					{
						endDateInfo.AddWarning(Res.GetString("838D2DC1-0B31-4825-8B9C-4F1B722125F6", "Financial Year normally ends on the last day of a month"));
					}
				}
				if (!endDateInfo.HasErrors())
				{
					var startDate = Parent.StartDate;
					if ((endDate - startDate).Days > 365 * 2)
					{
						Parent.EndDateInfo.AddError(Res.GetString("3E146E6E-EBAD-43CA-B809-EC34FD7FA4CF", "Date Range between Financial Year Start Date and Financial Year End Date is more than 2 years"));
					}
				}
			}
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		public readonly ExtendLastFinancialYearSettings Parent;
	}
}
