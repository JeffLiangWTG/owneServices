using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMReleaseSequenceFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(FilterDescriptions.Name, BMReleaseSequenceSchema.BMR_Name).MultilingualDescription = ResString.GetMultilingualString("BMReleaseSequenceFilterBusinessObject.Name", "Name");

			var groupFilter = result.AddGuidFilter(FilterDescriptions.ReleaseGroup, ModuleIDs.GlbGroup, BMReleaseSequenceSchema.BMR_GG_ReleaseGroup, () => new GlbGroupCollection(Factory));
			groupFilter.Category = FilterCategories.Other;
			groupFilter.MultilingualDescription = ResString.GetMultilingualString("BMReleaseSequenceFilterBusinessObject.ReleaseGroup", "Release Group");

			var capabilityFilter = result.AddGuidFilter(FilterDescriptions.Capability, ModuleIDs.GlbCapability, BMReleaseSequenceSchema.BMR_G4_Capability, () => new GlbCapabilityCollection(Factory));
			capabilityFilter.Category = FilterCategories.Other;
			capabilityFilter.MultilingualDescription = ResString.GetMultilingualString("BMReleaseSequenceFilterBusinessObject.Capability", "Capability");

			return result;
		}

		public static class FilterDescriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string Name = "Name";
			public const string ReleaseGroup = "ReleaseGroup";
			public const string Capability = "Capability";

			#endregion
		}
	}
}
