using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class StmActivityLogFilterProvider : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StmActivityLogFilterProvider(StmActivityLogCollection collection)
			: base(collection.Factory)
		{
			this.collection = collection;
			this.collection.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged);
		}

		public StmActivityLogCollection Collection
		{
			get { return collection; }
		}
		public StmActivityLogCollection collection;

		#region Filter Properties

		[MaxLength(3)]
		public ZString ActivityLogFilterStaff
		{
			get { return Collection.ActivityLogFilterStaff; }
			set
			{
				CheckMaximumLength(ActivityLogFilterStaffInfo, value);
				Collection.ActivityLogFilterStaff = value;
				ActivityLogFilterStaffInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ActivityLogFilterStaffInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogFilterStaff)); }
		}

		public ZDateTime ActivityLogFilterDateFrom
		{
			get { return Collection.ActivityLogFilterDateFromLocal; }
			set
			{
				ActivityLogFilterDateFromInfo.ClearAllNotifications();
				Collection.ActivityLogFilterDateFromLocal = value;
				if (!value.IsValid)
				{ ActivityLogFilterDateFromInfo.AddError(Res.GetString("2828E434-A3EC-4047-A32D-834926F50CC4", "The 'From' date field is invalid!")); }
				TypeValidation.CheckValidZDateTimeRange(ActivityLogFilterDateFromInfo, GetTypeValidationLimits());
				ActivityLogFilterDateFromInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ActivityLogFilterDateFromInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogFilterDateFrom)); }
		}

		public ZDateTime ActivityLogFilterDateTo
		{
			get { return Collection.ActivityLogFilterDateToLocal; }
			set
			{
				ActivityLogFilterDateToInfo.ClearAllNotifications();
				Collection.ActivityLogFilterDateToLocal = value;
				if (!value.IsValid)
				{ ActivityLogFilterDateToInfo.AddError(Res.GetString("b981cb2e-af7d-4125-ab92-c0ddc160ac66", "The 'To' date field is invalid!")); }
				if (ActivityLogFilterDateFrom > ActivityLogFilterDateTo)
				{
					ActivityLogFilterDateToInfo.AddWarning(Res.GetString("344C7674-2B67-4751-8A6F-F1233AE87B28", "To Date and Time should be greater than the From date and Time."));
				}
				TypeValidation.CheckValidZDateTimeRange(ActivityLogFilterDateToInfo, GetTypeValidationLimits());
				ActivityLogFilterDateToInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ActivityLogFilterDateToInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogFilterDateTo)); }
		}

		[List("ActivityLogFilterTypes")]
		[MaxLength(19)]
		public ZString ActivityLogFilterType
		{
			get { return Collection.ActivityLogFilterType; }
			set
			{
				CheckMaximumLength(ActivityLogFilterTypeInfo, value);
				Collection.ActivityLogFilterType = value;
				ActivityLogFilterTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ActivityLogFilterTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogFilterType)); }
		}

		[MaxLength(150)]
		public ZString ActivityLogFilterFormCaption
		{
			get { return Collection.ActivityLogFilterFormCaption; }
			set
			{
				CheckMaximumLength(ActivityLogFilterFormCaptionInfo, value);
				Collection.ActivityLogFilterFormCaption = value;
				ActivityLogFilterFormCaptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ActivityLogFilterFormCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogFilterFormCaption)); }
		}

		TypeValidationLimits GetTypeValidationLimits()
		{
			var maximumPastYear = ObjectFactory.Get<ISystemDataRegistry>().ActivityLogMaximumPastYears;
			if (maximumPastYear == 0)
			{
				maximumPastYear = DateRangeValidation.MaximumPastYears;
			}

			return new TypeValidationLimits()
			{
				PastYearsBeforeError = maximumPastYear
			};
		}

		#endregion

		#region Clear Filters

		public void ClearActivityLogFilters()
		{
			ActivityLogFilterDateFrom = ZDateTime.Today;
			ActivityLogFilterDateTo = ZDateTime.Today.AddDays(1);
			ActivityLogFilterType = StmActivityLogCollection.ActivityTypeAll;
			ActivityLogFilterFormCaption = ZString.Empty;
			ActivityLogFilterStaff = ZString.Empty;
		}

		#endregion

		#region Total Properties

		void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			ActivityLogTotalActiveMinutesInfo.RefreshBinding();
			ActivityLogTotalInactiveMinutesInfo.RefreshBinding();
			ActivityLogTotalControlChangesInfo.RefreshBinding();
			ActivityLogTotalKeyStrokesInfo.RefreshBinding();
			ActivityLogTotalMouseClicksInfo.RefreshBinding();
		}

		public ZDecimal ActivityLogTotalActiveMinutes
		{
			get
			{
				ZDecimal result = 0;
				foreach (StmActivityLog log in Collection)
				{
					result += log.ActiveDurationMinutes;
				}
				return result;
			}
		}

		public ZPropertyInfo ActivityLogTotalActiveMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogTotalActiveMinutes)); }
		}

		public ZDecimal ActivityLogTotalInactiveMinutes
		{
			get
			{
				ZDecimal result = 0;
				foreach (StmActivityLog log in Collection)
				{
					result += log.InactiveDurationMinutes;
				}
				return result;
			}
		}

		public ZPropertyInfo ActivityLogTotalInactiveMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogTotalInactiveMinutes)); }
		}

		public ZInt ActivityLogTotalKeyStrokes
		{
			get
			{
				ZInt result = 0;
				foreach (StmActivityLog log in Collection)
				{
					result += log.S7_KeyStrokes;
				}
				return result;
			}
		}

		public ZPropertyInfo ActivityLogTotalKeyStrokesInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogTotalKeyStrokes)); }
		}

		public ZInt ActivityLogTotalMouseClicks
		{
			get
			{
				ZInt result = 0;
				foreach (StmActivityLog log in Collection)
				{
					result += log.S7_MouseClicks;
				}
				return result;
			}
		}

		public ZPropertyInfo ActivityLogTotalMouseClicksInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogTotalMouseClicks)); }
		}

		public ZInt ActivityLogTotalControlChanges
		{
			get
			{
				ZInt result = 0;
				foreach (StmActivityLog log in Collection)
				{
					result += log.S7_ControlChanges;
				}
				return result;
			}
		}

		public ZPropertyInfo ActivityLogTotalControlChangesInfo
		{
			get { return GetZPropertyInfo(nameof(ActivityLogTotalControlChanges)); }
		}

		#endregion

		#region Lookups

		#region Activity Log Type Filter List

		public CodeDescriptionPairList ActivityLogFilterTypes
		{
			get
			{
				if (activityLogFilterTypes == null)
				{
					activityLogFilterTypes = new CodeDescriptionPairList();
					activityLogFilterTypes.AddPair(ResString.GetMultilingualString("0643bffb-49c7-47ae-a2c2-9451eca7ab86", "All Activity"));
					activityLogFilterTypes.AddPair(ResString.GetMultilingualString("EA5374DB-94AB-41F6-BE10-498E728C1A40", "Current Application"));
					activityLogFilterTypes.AddPair(ResString.GetMultilingualString("8dcffc3c-e191-4996-93b7-d37c98fa706b", "External"));
				}
				return activityLogFilterTypes;
			}
		}

		CodeDescriptionPairList activityLogFilterTypes;

		#endregion

		#region Staff

		public IActiveBusinessObjectCollection Staff
		{
			get
			{
				if (staff == null)
				{
					staff = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IGlbStaffCollection>(), Factory);
				}
				return staff;
			}
		}

		IActiveBusinessObjectCollection staff;

		#endregion

		#endregion
	}
}
