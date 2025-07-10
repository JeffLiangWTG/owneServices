using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.UniversalCopy.Module
{
	public class UniversalCopyScheduleFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();
			var templateNameAndModuleSubGroup = new TemplateNameAndModuleSubGroup();
			var activeStatusAndTaskDescriptionSubgroup = new ActiveStatusAndTaskDescriptionSubGroup();

			var activeStatusFilter = collection.AddTextFilter("Active Status", GetActiveStatusQuery, CancelledStatusList);
			activeStatusFilter.Visibility = FilterVisibility.AlwaysApplied;
			activeStatusFilter.DefaultProperty = CancelledStatusList[StatusActive].Code;
			activeStatusFilter.Category = FilterCategories.StatusAndFlags;
			activeStatusFilter.MultilingualDescription = ScheduleTaskIsActiveFilterDescription;
			activeStatusFilter.SubGroup = activeStatusAndTaskDescriptionSubgroup;

			var taskDescriptionFilter = collection.AddTextFilter("TaskDescription", GetTaskDescriptionQuery);
			taskDescriptionFilter.MultilingualDescription = TaskDescriptionFilterDescription;
			taskDescriptionFilter.SubGroup = activeStatusAndTaskDescriptionSubgroup;
			taskDescriptionFilter.MaxLength = StmScheduleTaskSchema.S5_ScheduleDescription.MaxLength;

			var templateNameFilter = collection.AddTextFilter("TemplateName", GetTemplateNameQuery);
			templateNameFilter.MaxLength = StmModuleFilterSchema.S9_FilterName.MaxLength;
			templateNameFilter.MultilingualDescription = TemplateNameFilterDescription;
			templateNameFilter.SubGroup = templateNameAndModuleSubGroup;

			collection.AddFilter(new ParentJobModuleFilter("Parent Job", StmUniversalCopySchema.SUC_CopyObjectId, Factory) { SupportsFiltersMatchComparisonOperator = false });

			return collection;
		}

		ZQuery GetTemplateNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQuery(StmModuleFilterSchema.S9_FilterName, comparisonOperator, value);
		}

		ZQuery GetTaskDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQuery(StmScheduleTaskSchema.S5_ScheduleDescription, comparisonOperator, value);
		}

		ZQuery GetActiveStatusQuery(ZString value)
		{
			if (StatusAll.EqualsUnresolvedOrLocalized(value, ignoreCase: false))
			{
				return new ZQuery();
			}

			var isStatusInactive = StatusInactive.EqualsUnresolvedOrLocalized(value, ignoreCase: false);

			return GetQuery(StmScheduleTaskSchema.S5_IsActive, SQLComparisonOperator.Equal, !isStatusInactive);
		}

		ZQuery GetQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZQuery();
			query.AddToFilter(column, comparisonOperator, value);
			return query;
		}

		class ActiveStatusAndTaskDescriptionSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(StmUniversalCopy));
				var subQuery = new ZDBOnlySubQuery(typeof(StmUniversalCopyScheduleTask), StmScheduleTaskSchema.S5_ParentID);
				subQuery.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmUniversalCopySchema.Constants.Prefix);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		class TemplateNameAndModuleSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(StmUniversalCopy));
				var subQuery = new ZDBOnlySubQuery(typeof(UniversalCopyTemplate), StmUniversalCopySchema.SUC_S9_CopyTemplate);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		ResourceString TaskDescriptionFilterDescription => ResString.GetMultilingualString("004ec058-53b7-4827-9c7e-dac4466c3c89", "Task Description");

		ResourceString TemplateNameFilterDescription => ResString.GetMultilingualString("14ad2a0b-39a9-4b96-ab37-6510e97fa55d", "Template Name");

		ResourceString ScheduleTaskIsActiveFilterDescription => ResString.GetMultilingualString("b700ab15-1ff4-44ae-a873-03e7731f77b6", "Active Status");

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.TaskDescription:
					var taskDescriptionFilter = new IndexSearchModuleTextFilter(searchField);
					taskDescriptionFilter.MultilingualDescription = TaskDescriptionFilterDescription;
					taskDescriptionFilter.MaxLength = StmScheduleTaskSchema.S5_ScheduleDescription.MaxLength;
					return new SearchFieldOverride(searchField, taskDescriptionFilter);
				case SearchFieldConstants.TemplateName:
					var templateNameFilter = new IndexSearchModuleTextFilter(searchField);
					templateNameFilter.MultilingualDescription = TemplateNameFilterDescription;
					templateNameFilter.MaxLength = StmModuleFilterSchema.S9_FilterName.MaxLength;
					return new SearchFieldOverride(searchField, templateNameFilter);
				case SearchFieldConstants.ParentJob:
					var parentJobFilter = new IndexSearchParentJobModuleFilter(searchField, StmUniversalCopySchema.SUC_CopyObjectId, Factory) { SupportsFiltersMatchComparisonOperator = false };
					return new SearchFieldOverride(searchField, parentJobFilter);
				case SearchFieldConstants.ScheduleTaskIsActive:
					var scheduleTaskIsActiveFilter = new IndexSearchModuleTextFilter(FilterDescriptions.ActiveStatus, searchField, GetActiveStatusGlowQuery, CancelledStatusList, FilterCategories.StatusAndFlags);
					scheduleTaskIsActiveFilter.Visibility = FilterVisibility.AlwaysApplied;
					scheduleTaskIsActiveFilter.DefaultProperty = CancelledStatusList[StatusActive].Code;
					scheduleTaskIsActiveFilter.MultilingualDescription = ScheduleTaskIsActiveFilterDescription;
					return new SearchFieldOverride(searchField, scheduleTaskIsActiveFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		IGlowQuery GetActiveStatusGlowQuery(SearchField searchField, ZString status)
		{
			status = status.Trim();
			if (StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				return new EmptyQuery();
			}

			if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, true);
			}

			return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, false);
		}

		static class SearchFieldConstants
		{
			public const string TaskDescription = "TASKDESCRIPTION";

			public const string TemplateName = "TEMPLATENAME";

			public const string ParentJob = "PARENTJOB";

			public const string ScheduleTaskIsActive = "SCHEDULETASKISACTIVE";
		}
	}
}
