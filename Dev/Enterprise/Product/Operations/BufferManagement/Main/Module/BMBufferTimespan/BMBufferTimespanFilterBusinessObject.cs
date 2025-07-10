using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMBufferTimespanFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Name", BMBufferTimespanSchema.BMT_Name).MultilingualDescription =
				ResString.GetMultilingualString("MasterFiles|BMBufferTimespan|Name", "Name");

			var durationFilter = new ModuleDurationFilter(
				ProcessHeader.ModuleFilterConstants.BufferTimespan,
				Factory,
				BMBufferTimespanSchema.BMT_BufferTimespanInMinutes);

			durationFilter.MinDurationMinutes = 0;
			durationFilter.MaxDurationMinutes = 100000;
			durationFilter.MultilingualDescription = ResString.GetMultilingualString(
				"MasterFiles|BMBufferTimespan|Timespan",
				"Timespan (Hours)");
			durationFilter.Category = FilterCategories.NumbersAndReferences;
			durationFilter.Visibility = FilterVisibility.Visible;

			filters.AddFilter(durationFilter);
			return filters;
		}
	}
}
