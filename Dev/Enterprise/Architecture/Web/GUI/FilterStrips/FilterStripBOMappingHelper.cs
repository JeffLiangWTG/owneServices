using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class FilterStripBOMappingHelper
	{
		protected readonly FilterStripBusinessObject sourceFilterBO;
		protected readonly FilterStripBusinessObject destinationFilterBO;

		public FilterStripBOMappingHelper(FilterStripBusinessObject sourceFilterBO, FilterStripBusinessObject destinationFilterBO)
		{
			this.sourceFilterBO = sourceFilterBO;
			this.destinationFilterBO = destinationFilterBO;
		}

		public void MapModuleFilters(string sourceFilterName)
		{
			MapModuleFilters(sourceFilterName, sourceFilterName, false);
		}

		public void MapModuleFilters(string sourceFilterName, string destinationFilterName)
		{
			MapModuleFilters(sourceFilterName, destinationFilterName, false);
		}

		public void MapModuleGuidsFiltersSwapped(string sourceFilterName)
		{
			MapModuleGuidsFiltersSwapped(sourceFilterName, sourceFilterName);
		}

		public void MapModuleGuidsFiltersSwapped(string sourceFilterName, string destinationFilterName)
		{
			if (sourceFilterBO[sourceFilterName] as ModuleGuidsFilter != null)
			{
				MapModuleFilters(sourceFilterName, destinationFilterName, true);
			}
		}

		void MapModuleFilters(string sourceFilterName, string destinationFilterName, bool swapped)
		{
			ModuleFilter sourceFilter = sourceFilterBO[sourceFilterName];
			ModuleFilter destinationFilter = destinationFilterBO[destinationFilterName];
			if (destinationFilter != null &&
				sourceFilter != null &&
				sourceFilter.IsActive &&
				destinationFilter.GetType() == sourceFilter.GetType())
			{
				if (swapped)
				{
					if (destinationFilter as ModuleGuidsFilter != null)
					{
						destinationFilter.IsActive = true;
						((ModuleGuidsFilter)destinationFilter).Property1 = ((ModuleGuidsFilter)sourceFilter).Property2;
						((ModuleGuidsFilter)destinationFilter).Property2 = ((ModuleGuidsFilter)sourceFilter).Property1;
					}
					else
					{
						throw new InvalidOperationException("'Swapped' is not supported for this filter (" + sourceFilterName + ")");
					}
				}
				else
				{
					destinationFilter.CopyTransientProperties(sourceFilter);
				}
			}
		}

		public void MapModuleFilters<T>(string sourceFilterName) where T : ModuleFilter
		{
			MapModuleFilters<T>(sourceFilterName, sourceFilterName);
		}

		public void MapModuleFilters<T>(string sourceFilterName, string destinationFilterName) where T : ModuleFilter
		{
			var sourceFilter = (T)sourceFilterBO[sourceFilterName];
			var destinationFilter = (T)destinationFilterBO[destinationFilterName];
			if (destinationFilter != null && sourceFilter?.IsActive == true)
			{
				destinationFilter.CopyTransientProperties(sourceFilter);
			}
		}
	}
}
