using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class StmActivityLogCollection : BusinessObjectCollection<StmActivityLog>
	{
		public StmActivityLogCollection(BusinessObject master)
			: base(master.Factory ?? new BusinessObjectFactory())
		{
			ActivityLogFilterDateFromLocal = ZDateTime.Today;
			activityLogFilterDateToUtc = activityLogFilterDateFromUtc.AddDays(1);
		}

		#region Filter Properties

		public ZString ActivityLogFilterStaff
		{
			get { return activityLogFilterStaff; }
			set { activityLogFilterStaff = value; }
		}

		ZString activityLogFilterStaff;

		public ZDateTime ActivityLogFilterDateFromUtc
		{
			get { return activityLogFilterDateFromUtc; }
			set { activityLogFilterDateFromUtc = value; }
		}

		ZDateTime activityLogFilterDateFromUtc;

		public ZDateTime ActivityLogFilterDateFromLocal
		{
			get { return activityLogFilterDateFromUtc.IsValid ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(activityLogFilterDateFromUtc.ToDateTime()) : activityLogFilterDateFromUtc; }
			set { activityLogFilterDateFromUtc = value.IsValid ? EnvProxy.Instance.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZDateTime ActivityLogFilterDateToUtc
		{
			get { return activityLogFilterDateToUtc; }
			set { activityLogFilterDateToUtc = value; }
		}

		ZDateTime activityLogFilterDateToUtc;

		public ZDateTime ActivityLogFilterDateToLocal
		{
			get { return activityLogFilterDateToUtc.IsValid ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(activityLogFilterDateToUtc.ToDateTime()) : activityLogFilterDateToUtc; }
			set { activityLogFilterDateToUtc = value.IsValid ? EnvProxy.Instance.Time.GetUtcFromLocalTime(value.ToDateTime()) : value; }
		}

		public ZString ActivityLogFilterType
		{
			get { return activityLogFilterType; }
			set { activityLogFilterType = value; }
		}

		ZString activityLogFilterType = ActivityTypeAll;

		public ZString ActivityLogFilterFormCaption
		{
			get { return activityLogFilterFormCaption; }
			set { activityLogFilterFormCaption = value; }
		}

		ZString activityLogFilterFormCaption;

		#endregion

		#region Filter

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = base.CreateAdditionalFilter();

			if (!ActivityLogFilterDateFromUtc.IsEmpty)
			{
				query.AddToFilter(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThan, ActivityLogFilterDateFromUtc);
			}

			if (!ActivityLogFilterDateToUtc.IsEmpty)
			{
				query.AddToFilter(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ActivityLogFilterDateToUtc);
			}

			if (ActivityLogFilterType == ActivityTypeThisApp)
			{
				query.AddToFilter(StmActivityLogSchema.S7_EnterpriseActivity, true);
			}
			else if (ActivityLogFilterType == ActivityTypeExternal)
			{
				query.AddToFilter(StmActivityLogSchema.S7_EnterpriseActivity, false);
			}
			if (!ActivityLogFilterFormCaption.IsEmpty)
			{
				query.AddToFilter(StmActivityLogSchema.S7_FormCaption, SQLComparisonOperator.Contains, ActivityLogFilterFormCaption);
			}

			if (!ActivityLogFilterStaff.IsEmpty)
			{
				query.AddToFilter(StmActivityLogSchema.S7_GS_NKUser, SQLComparisonOperator.Equal, ActivityLogFilterStaff);
			}

			query.AddToFilter(StmActivityLogSchema.S7_ControllerID, SQLComparisonOperator.NotEqual, LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString());

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public const string ActivityTypeAll = "All Activity";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public const string ActivityTypeThisApp = "Current Application";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public const string ActivityTypeExternal = "External";

		#endregion

		#region Allow New / Readonly

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		#endregion
	}

	public class StmActivityLogCollectionByStaff : StmActivityLogCollection
	{
		public StmActivityLogCollectionByStaff(IGlbStaff staff)
			: base((EnterpriseBusinessObject)staff)
		{
			this.Staff = staff;
		}

		readonly IGlbStaff Staff;

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			if (Staff != null)
			{
				query.AddToFilter(StmActivityLogSchema.S7_GS_NKUser, Staff.GS_Code);
			}
			return query;
		}

		#endregion
	}

	public class StmActivityLogCollectionByParent : StmActivityLogCollection
	{
		public StmActivityLogCollectionByParent(BusinessObject parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		readonly BusinessObject Parent;

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			if (Parent != null)
			{
				query.AddToFilter(StmActivityLogSchema.S7_ParentID, Parent.PK);
				string tableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(Parent.TableName);
				if (tableCode != null)
				{
					query.AddToFilter(StmActivityLogSchema.S7_ParentTableCode, tableCode);
				}
			}
			return query;
		}

		#endregion
	}
}
