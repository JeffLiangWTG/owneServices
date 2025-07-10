using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public abstract class GldDateRangeSetting : NonPersistentBusinessObject<GldDateRangeSettingValidation>, IObsoleteValidation
	{
		public GldDateRangeSetting()
		{
			this.StartDate = ZDateTime.Empty;
			this.EndDate = ZDateTime.Empty;
		}

		#region Schema

		public abstract class Schema
		{
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
		}

		#endregion

		#region StartDate

		public ZDateTime StartDate
		{
			get { return fStartDate; }
			set
			{
				var oldValue = fStartDate;
				SetNonPersistentPropertyValue(StartDateInfo, ref fStartDate, value);
				if ((oldValue != value || value.IsEmpty) && !IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}
		ZDateTime fStartDate;

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(Schema.StartDate); }
		}

		#endregion

		#region EndDate

		public ZDateTime EndDate
		{
			get { return fEndDate; }
			set
			{
				var oldValue = fEndDate;
				SetNonPersistentPropertyValue(EndDateInfo, ref fEndDate, value);
				if ((oldValue != value || value.IsEmpty) && !IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		ZDateTime fEndDate;

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(Schema.EndDate); }
		}

		#endregion
	}
}
