using CargoWise.Application;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module
{
	public class BMControlCustomisationFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Name", BMControlCustomisationSchema.FM_Name).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|BMControlCustomisationFilter|Name", "Name");
			result.AddTextFilter("ControlType", BMControlCustomisationSchema.FM_ControlType, () => new CustomisedControlTypeList()).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|BMControlCustomisationFilter|ControlType", "Control Type");
			result.AddFlagsFilter("SystemWide", new[] { Res.GetString("BufferManagement|BMControlCustomisationFilter|SystemWide", "System Wide") }, new[] { BMControlCustomisationSchema.FM_IsSystemWide }).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|BMControlCustomisationFilter|SystemWide", "System Wide");
			result.AddTextFilter("JobType", BMControlCustomisationSchema.FM_JobType, () => ObjectFactory.Get<IWorkflowDescriptorList>()).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|BMControlCustomisationFilter|JobType", "Job Type");
			return result;
		}

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.ControlType:
					var controlTypeFilter = new IndexSearchModuleTextFilter(searchField, new CustomisedControlTypeList());
					return new SearchFieldOverride(searchField, controlTypeFilter);
				case SearchFieldConstants.JobType:
					var jobTypeFilter = new IndexSearchModuleTextFilter(searchField, ObjectFactory.Get<IWorkflowDescriptorList>());
					return new SearchFieldOverride(searchField, jobTypeFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		static class SearchFieldConstants
		{
			public const string ControlType = "CONTROLTYPE";

			public const string JobType = "JOBTYPE";
		}
	}
}
