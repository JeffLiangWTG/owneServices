using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmALogFilterBusinessObject : FilterStripBusinessObject
	{
		public ZStmALogFilterBusinessObject(IStmALogParent master) : this()
		{
			this.master = master;
		}

		public ZStmALogFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ControllerIDs.StmALog.Name;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (ZStmALogFilterBusinessObject)base.CloneInternal(args);
			result.master = master;
			return result;
		}

		public ZGuid LogsParentPK => master?.LogsParentPK ?? Guid.Empty;

		protected IStmALogParent master;

		#region Queries

		ZBool IncludeChangeEventsFilter => LogsToShow == LogsToShow.Operations;

		public static class Schema
		{
			public const string ShowChangeEvent = "Include Change Events";
			public const string ShowEstimates = "Include Estimates";
			public const string ShowCancelled = "Include Cancelled";
			public const string ShowFor = "Show for";
			public const string EventTime = "Event Time";
			public const string PostedTime = "Posted Time";
			public const string Reference = "Reference";
			public const string User = "User";
			public const string EventCode = "Event Code";
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;

				if (!IncludeChangeEventsFilter)
				{
					AddLogsTypeQuery(query, false);
				}
				return query;
			}
		}

		protected override void ApplyToFilterGroups(ZQuery query, IEnumerable<ModuleFilter> filtersInGroup)
		{
			if (NeedsShowForFilter(filtersInGroup))
			{
				query.AddToFilter(GetShowEventsForQuery(StmALogCollectionView.BizObjNameToShowAllCurrentLogsToShow));
			}
			base.ApplyToFilterGroups(query, filtersInGroup);
		}

		void AddLogsTypeQuery(ZQuery query, ZBool showChangeLogs)
		{
			ZQuery typesQuery = null;
			SQLComparisonOperator op = null;
			JoinCondition cond = null;
			if (IncludeChangeEventsFilter && !showChangeLogs)
			{
				op = SQLComparisonOperator.NotEqual;
				cond = JoinCondition.And;
			}
			else if (LogsToShow == LogsToShow.ChangeLogs)
			{
				op = SQLComparisonOperator.Equal;
				cond = JoinCondition.Or;
			}
			if (op != null && cond != null)
			{
				typesQuery = new ZQuery();
				foreach (var evt in Events.ChangeLogs)
				{
					typesQuery.AddToFilter(cond, StmALogSchema.SL_SE_NKEvent, op, evt.Code);
				}
			}
			if (typesQuery != null)
			{
				query.AddToFilter(typesQuery, JoinCondition.And);
			}
		}

		bool NeedsShowForFilter(IEnumerable<ModuleFilter> filtersInGroup)
		{
			ModuleTextFilter textFilter = filtersInGroup?.OfType<ModuleTextFilter>().FirstOrDefault(f => f.Description.StartsWith(Schema.ShowFor, StringComparison.OrdinalIgnoreCase));
			return textFilter == null || textFilter.Property.IsEmpty;
		}

		protected virtual bool ShowBaseFiltersWithoutMaster => false;
		public override bool ShouldAddActiveStatusFilter => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			if (master != null)
			{
				if (LogsToShow != LogsToShow.ChangeLogs)
				{
					ModuleFlagsFilter f1 = result.AddFlagsFilter(Schema.ShowEstimates, new string[] { Res.GetString("48be702c-cc14-422c-901e-e2b984154f83", "Show Estimates") }, new GetFlagsQuery[] { GetShowEstimatesQuery });
					f1.MultilingualDescription = ResString.GetMultilingualString("eb907c43-687c-4679-84c2-1678340744c1", "Include Estimates");
					f1.Property0 = ZBool.False;
					f1.Visibility = FilterVisibility.AlwaysApplied;

					ModuleFlagsFilter f2 = result.AddFlagsFilter(Schema.ShowCancelled, new string[] { Res.GetString("6cc08179-6220-480e-9af9-23f594c955d8", "Show Canceled") }, new GetFlagsQuery[] { GetShowCancelledQuery });
					f2.MultilingualDescription = ResString.GetMultilingualString("4f0bacb1-6089-4b53-89f8-80e6b2fd7aaa", "Include Canceled");
					f2.Property0 = ZBool.False;
					f2.Visibility = FilterVisibility.AlwaysApplied;
					if (IncludeChangeEventsFilter)
					{
						ModuleFlagsFilter f3 = result.AddFlagsFilter(Schema.ShowChangeEvent, new string[] { Res.GetString("50e053e2-e093-4562-a74b-1b40ffe13cb9", "Show Change Events") }, new GetFlagsQuery[] { GetShowChangeEventQuery });
						f3.MultilingualDescription = ResString.GetMultilingualString("8a9f52d8-dfdf-4d4b-b3d8-e81a5fe60b45", "Include Change Events");
						f3.Property0 = ZBool.False;
						f3.Visibility = FilterVisibility.AlwaysApplied;
					}
				}

				ModuleTextFilter f4 = result.AddTextFilter(Schema.ShowFor, GetShowEventsForQuery, GetEventsOriginList);
				f4.IsOrCategoryReadOnly = true;
				f4.IsGroupOrCategoryReadOnly = true;
				f4.MultilingualDescription = ResString.GetMultilingualString("9e1343e5-68a9-4076-a6a3-9a00486bc44c", "Show for");
				f4.PropertyValidation += info => ListValidation.ErrorIfInvalidCode(info, GetEventsOriginList());
				f4.Property = StmALogCollectionView.BizObjNameToShowAllCurrentLogsToShow.GetUnresolvedString();
				f4.Visibility = FilterVisibility.AlwaysApplied;
			}

			if (master != null || ShowBaseFiltersWithoutMaster)
			{
				result.AddTextFilter(Schema.Reference, StmALogSchema.SL_Reference).MultilingualDescription = ResString.GetMultilingualString("e47a224b-8e5f-4b88-9b06-e1ebd6ef946a", "Reference");
				result.AddNkFilter(Schema.User, StmALogSchema.SL_GS_NKUser, ModuleIDs.GlbStaff, (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbStaffCollection>(), Factory)).MultilingualDescription = ResString.GetMultilingualString("cc8177b9-8a8f-4850-be8b-c59fd5f08b31", "User");
				result.AddDateFilter(Schema.EventTime, StmALogSchema.SL_EventTime).MultilingualDescription = ResString.GetMultilingualString("2025fc68-e8fe-493b-b1d3-c45026e58003", "Event Time");
				var postedTime = result.AddDateFilter(Schema.PostedTime, StmALogSchema.SL_PostedTimeUtc, true);
				postedTime.MultilingualDescription = ResString.GetMultilingualString("21960425-98e5-4845-b9d0-7cec5ff589e4", "Posted Time");
				result.AddTextFilter(Schema.EventCode, StmALogSchema.SL_SE_NKEvent, EventCodesList).MultilingualDescription = ResString.GetMultilingualString("C62E476C-81FF-4DE6-B887-451137B67BDB", "Event Code");
			}

			return result;
		}

		ZQuery GetShowEstimatesQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value == ZBool.False)
			{
				query.AddToFilter(StmALogSchema.SL_IsEstimate, SQLComparisonOperator.Equal, value);
			}
			return query;
		}

		ZQuery GetShowCancelledQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value == ZBool.False)
			{
				query.AddToFilter(StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, value);
			}
			return query;
		}

		ZQuery GetShowChangeEventQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			AddLogsTypeQuery(query, value);
			return query;
		}

		ZQuery GetShowEventsForQuery(ZString value)
		{
			if (master == null)
			{
				return new ZQuery();
			}

			var showEventsFor = BizObjNameToShowEventsFor_List.GetDescriptionFromCode(value);

			if (!string.IsNullOrEmpty(showEventsFor) && showEventsFor != StmALogCollectionView.BizObjNameToShowAllCurrentLogsToShow)
			{
				if (showEventsFor == master.LogsParentPK.ToStringKey())
				{
					return new ZQuery(StmALogSchema.SL_Parent, master.LogsParentPK);
				}
				else
				{
					var obj = master.BusinessObjectsWithRelatedEvents.FirstOrDefault(x => x != null && x.PK.ToStringKey() == showEventsFor);

					if (obj != null)
					{
						return new ZQuery(StmALogSchema.SL_Parent, obj.PK);
					}
					else
					{
						return ZQuery.NoResultQuery;
					}
				}
			}
			else
			{
				return GetBusinessObjectsWithRelatedEventsQuery(value);
			}
		}

		protected virtual ZQuery GetBusinessObjectsWithRelatedEventsQuery(ZString showForCode)
		{
			var query = new ZQuery { AllowTableValuedParameters = true };

			var parentPrimaryKeys = master.BusinessObjectsWithRelatedEvents
				.Where(x => x != null)
				.Select(x => x.PK)
				.Append(master.LogsParentPK);

			if (parentPrimaryKeys.Any())
			{
				query.AddToFilter(StmALogSchema.SL_Parent, parentPrimaryKeys);
			}

			return query;
		}

		#endregion

		#region Lookups

		CodeDescriptionPairList GetEventsOriginList()
		{
			if (eventsOriginList == null)
			{
				eventsOriginList = new CodeDescriptionPairList();
				BizObjNameToShowEventsFor_List.OfType<CodeDescriptionPair>().ToList().ForEach(x => eventsOriginList.AddPair(x.MultilingualCode, x.MultilingualCode));
				eventsOriginList.AddRange(AdditionalEventsOrigins);
			}
			return eventsOriginList;
		}
		CodeDescriptionPairList eventsOriginList;

		protected virtual CodeDescriptionPairList AdditionalEventsOrigins { get; } = new CodeDescriptionPairList();

		protected internal virtual CodeDescriptionPairList BizObjNameToShowEventsFor_List
		{
			get
			{
				if (bizObjNameToShowEventsFor_List == null)
				{
					bizObjNameToShowEventsFor_List = StmALogCollectionView.GetBizObjNameToShowEventsFor_List(master, StmALogCollectionView.BizObjNameToShowAllCurrentLogsToShow);
				}
				return bizObjNameToShowEventsFor_List;
			}
		}
		CodeDescriptionPairList bizObjNameToShowEventsFor_List;

		protected internal virtual CodeDescriptionPairList EventCodesList
		{
			get
			{
				if (eventCodes == null)
				{
					eventCodes = new StmEventCodeDescriptionPairList();
				}
				return eventCodes;
			}
		}
		CodeDescriptionPairList eventCodes;

		#endregion

		public LogsToShow LogsToShow { get; set; }
	}
}
