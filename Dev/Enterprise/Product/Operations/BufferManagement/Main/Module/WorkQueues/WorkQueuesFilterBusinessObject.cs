using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class WorkQueuesFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Code", TagMagnitudeSchema.TGM_Code).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|WorkQueuesFilterBusinessObject|Code", "Code");
			filters.AddTextFilter("Description", TagMagnitudeSchema.TGM_Description).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|WorkQueuesFilterBusinessObject|Description", "Description");

			filters.AddGuidFilter("Owner Group", ModuleIDs.GlbGroup, TagMagnitudeSchema.TGM_GG_OwnerGroup, () => new GlbGroupActiveBusinessObjectCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|WorkQueuesFilterBusinessObject|OwnerGroup", "Owner Group");

			return filters;
		}

		#region FilterStripBusinessObject Overrides

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				var workQueuesGroup = TagProvider.GetWorkQueuesTagGroup(Factory);
				filter.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, workQueuesGroup.PK);

				return filter;
			}
		}

		#endregion
	}
}
