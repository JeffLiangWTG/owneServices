using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	/// <summary>
	/// Represents a single period.
	/// </summary>
	public class Period : AccPeriodManagement
	{
		public Period(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public PeriodManager PeriodManager
		{
			get { return fPeriodManager; }
			set { fPeriodManager = value; }
		}
		PeriodManager fPeriodManager;

		bool AM_Year_Readonly => !Env.CurrentUser.IsSupportUser;
		bool AM_Period_Readonly => !Env.CurrentUser.IsSupportUser;

		[ReadOnlyMember(nameof(AM_Year_Readonly))]
		[MaxLength(4)]
		public override ZShort AM_Year
		{
			get { return base.AM_Year; }
			set
			{
				if (!base.IsValidationSuspended)
				{
					Validation.ValidateAM_Period();
				}

				base.AM_Year = value;
			}
		}

		[ReadOnlyMember(nameof(AM_Period_Readonly))]
		[MaxLength(6)]
		public override ZInt AM_Period
		{
			get { return base.AM_Period; }
			set
			{
				base.AM_Period = value;
				UpdateRestPeriodsInSameYear();
			}
		}

		public override ZDateTime AM_EndDate
		{
			get { return base.AM_EndDate; }
			set
			{
				if (value.IsValid)
				{
					ZDateTime value1 = new ZDateTime(value.Year, value.Month, value.Day, 23, 59, 00);
					base.AM_EndDate = value1;

					if (!AM_EndDateInfo.HasErrors())
					{
						if (NextPeriod != null)
						{
							NextPeriod.AM_StartDate = value1.AddMinutes(1);
						}
						else if (LastPeriodOfCurrentYear != null && LastPeriodOfCurrentYear.PK == this.PK
							&& FirstPeriodOfNextYear != null)
						{
							FirstPeriodOfNextYear.AM_StartDate = value1.AddMinutes(1);
						}
					}
				}
				else
				{
					base.AM_EndDate = value;
				}
			}
		}

		#region PreviousPeriod
		public Period PreviousPeriod
		{
			get
			{
				if (fPreviousPeriodCached == null)
				{
					fPreviousPeriodCached = new CachedProperty<Period>(Factory, GetPreviousPeriodCached);
				}
				return fPreviousPeriodCached.Value;
			}
		}
		CachedProperty<Period> fPreviousPeriodCached;

		Period GetPreviousPeriodCached()
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, AM_GC_Company);
			filter.AddToFilter(AccPeriodManagementSchema.AM_Period, AM_Period - 1);
			return Factory.LoadTop1<Period>(filter);
		}

		public Period GetLastPeriodOfPreviousFinancialYear()
		{
			if (PeriodManager != null)
			{
				var query = new ZDBOnlyQuery(typeof(AccPeriodManagement));
				query.AddToFilter(AccPeriodManagementSchema.AM_Year, PeriodManager.FinancialYear - 1);
				query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, AM_GC_Company);
				query.OrderBy = AccPeriodManagementSchema.Constants.AM_Period + " DESC";
				return Factory.LoadTop1<Period>(query);
			}
			return null;
		}

		#endregion

		#region NextPeriod
		public Period NextPeriod
		{
			get
			{
				if (fNextPeriodCached == null)
				{
					fNextPeriodCached = new CachedProperty<Period>(Factory, GetNextPeriodCached);
				}
				return fNextPeriodCached.Value;
			}
		}

		CachedProperty<Period> fNextPeriodCached;

		Period GetNextPeriodCached()
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.GreaterThan, AM_Period);
			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());
			filter.OrderBy = AccPeriodManagementSchema.AM_Period.Name;
			return Factory.LoadTop1<Period>(filter);
		}

		void UpdateRestPeriodsInSameYear()
		{
			if (PeriodManager?.FinancialYear >= AM_Year)
			{
				return;
			}

			var periods = PeriodManager?.Periods;
			if (periods == null)
			{
				return;
			}

			var periodIndex = periods.ToList().IndexOf(this);
			if (periodIndex < 0)
			{
				return;
			}
			else if (periodIndex == periods.Count - 1)
			{
				IncreasePeriodForNextFY(AM_Period);
			}
			else
			{
				periods[periodIndex + 1].AM_Year = AM_Year;
				periods[periodIndex + 1].AM_Period = AM_Period + 1;
			}
		}

		void IncreasePeriodForNextFY(int baseValue)
		{
			if (PeriodManager != null)
			{
				var query = new ZDBOnlyQuery(typeof(AccPeriodManagement));
				query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, AM_GC_Company);
				query.AddToFilter(AccPeriodManagementSchema.AM_Year, PeriodManager?.FinancialYear + 1);
				query.OrderBy = AccPeriodManagementSchema.Constants.AM_Period;
				var periods = Factory.Load<Period>(query);

				for (int i = 0; i < periods?.Length; i += 1)
				{
					periods[i].AM_Period = baseValue + i + 1;
				}
			}
		}

		#endregion

		#region LastPeriodOfCurrentYear
		public Period LastPeriodOfCurrentYear
		{
			get
			{
				if (fLastPeriodOfCurrentYearCached == null)
				{
					fLastPeriodOfCurrentYearCached = new CachedProperty<Period>(Factory, GetLastPeriodOfCurrentYearCached);
				}
				return fLastPeriodOfCurrentYearCached.Value;
			}
		}
		CachedProperty<Period> fLastPeriodOfCurrentYearCached;

		Period GetLastPeriodOfCurrentYearCached()
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_Year, SQLComparisonOperator.Equal, AM_Year);
			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());
			filter.OrderBy = AccPeriodManagementSchema.AM_EndDate.Name + " DESC";
			return Factory.LoadTop1<Period>(filter);
		}

		#endregion

		#region FirstPeriodOfCurrentYear
		public Period FirstPeriodOfCurrentYear
		{
			get
			{
				if (fFirstPeriodOfCurrentYearCached == null)
				{
					fFirstPeriodOfCurrentYearCached = new CachedProperty<Period>(Factory, GetFirstPeriodOfCurrentYearCached);
				}
				return fFirstPeriodOfCurrentYearCached.Value;
			}
		}
		CachedProperty<Period> fFirstPeriodOfCurrentYearCached;

		Period GetFirstPeriodOfCurrentYearCached()
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_Year, SQLComparisonOperator.Equal, AM_Year);
			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());
			filter.OrderBy = AccPeriodManagementSchema.AM_Period.Name;
			return Factory.LoadTop1<Period>(filter);
		}

		#endregion

		#region FirstPeriodOfNextYear
		public Period FirstPeriodOfNextYear
		{
			get
			{
				if (fFirstPeriodOfNextYearCached == null)
				{
					fFirstPeriodOfNextYearCached = new CachedProperty<Period>(Factory, GetFirstPeriodOfNextYear);
				}
				return fFirstPeriodOfNextYearCached.Value;
			}
		}
		CachedProperty<Period> fFirstPeriodOfNextYearCached;

		Period GetFirstPeriodOfNextYear()
		{
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_Year, SQLComparisonOperator.Equal, AM_Year + 1);
			filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());
			filter.OrderBy = AccPeriodManagementSchema.AM_Period.Name;
			return Factory.LoadTop1<Period>(filter);
		}

		#endregion

		#region Overriden Info

		protected bool AM_EndDate_ReadOnly
		{
			get
			{
				bool result;

				if (AM_EndDateInfo.HasErrors() && AM_IsGeneralLedgerClosed && !AM_IsSubLedgerClosed)
				{
					result = false;
				}
				else
				{
					bool isLastYearPeriodNotInDB = !IsInDatabase && LastPeriodOfCurrentYear != null && LastPeriodOfCurrentYear.PK == PK;
					result = AM_IsGeneralLedgerClosed || AM_IsSubLedgerClosed || isLastYearPeriodNotInDB;
				}

				return result;
			}
		}

		protected bool AM_StartDate_ReadOnly
		{
			get { return true; }
		}

		protected bool AM_IsGeneralLedgerClosed_ReadOnly
		{
			get { return true; }
		}

		protected bool AM_IsSubLedgerClosed_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AM_GC_Company = GlbCompany.CurrentCompany.PK;
		}

		protected override AccPeriodManagementValidation GetNewValidation()
		{
			return new PeriodValidation(this);
		}
		#endregion

		#region IsAutoLogged

		public ZString PeriodAddEventDetails
		{
			get { return periodAddEventDetails; }
			set
			{
				periodAddEventDetails = value;
			}
		}
		ZString periodAddEventDetails;

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();

			var autoCreatedLog = Logs.AutoCreatedLog;
			if (autoCreatedLog != null)
			{
				using (((IUpdateFieldsLock)autoCreatedLog).LockForUpdatingKeyFields())
				{
					autoCreatedLog.SL_Reference = autoCreatedLog.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystem.Code ?
						PeriodAddEventDetails : (ZString)"";
				}
			}
		}

		#endregion
	}
}
