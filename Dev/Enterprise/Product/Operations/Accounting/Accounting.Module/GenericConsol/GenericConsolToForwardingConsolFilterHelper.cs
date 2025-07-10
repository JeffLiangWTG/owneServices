using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.GUI.FilterStrips;

namespace Enterprise.Accounting.Module
{
	class GenericConsolToForwardingConsolFilterHelper : FilterStripBOMappingHelper
	{
		public GenericConsolToForwardingConsolFilterHelper(GenericConsolFilterBusinessObject genericConsolFilterBO, FilterStripBusinessObject forwardingConsolFilterBO)
			: base(genericConsolFilterBO, forwardingConsolFilterBO)
		{
			Conversions = new Dictionary<string, string>();
			Exemptions = new List<string>();
		}

		public GenericConsolToForwardingConsolFilterHelper(GenericConsolFilterBusinessObject genericConsolFilterBO, FilterStripBusinessObject forwardingConsolFilterBO, Dictionary<string, string> conversions, List<string> exceptions)
			: base(genericConsolFilterBO, forwardingConsolFilterBO)
		{
			Conversions = conversions ?? new Dictionary<string, string>();
			Exemptions = exceptions ?? new List<string>();
		}

		public void MapFilters()
		{
			if (sourceFilterBO != null)
			{
				destinationFilterBO.ReturnNoResultsQuery = false;

				foreach (ModuleFilter sourceFilter in sourceFilterBO)
				{
					if (sourceFilter.IsActive && !sourceFilter.IsEmpty)
					{
						ModuleFilter destinationFilter = destinationFilterBO[sourceFilter.Description];
						if (destinationFilter != null)
						{
							destinationFilter.CopyTransientProperties(sourceFilter);
						}
						else
						{
							string destinationDescription;
							if (Conversions.TryGetValue(sourceFilter.Description, out destinationDescription))
							{
								destinationFilter = destinationFilterBO[destinationDescription];
								if (destinationFilter != null)
								{
									destinationFilter.CopyTransientProperties(sourceFilter);
								}
							}
							else
							{
								if (Exemptions.Contains(sourceFilter.Description) && (destinationFilter == null || destinationFilter.OrCategory == FilterOrCategory.None))
								{
									destinationFilterBO.ReturnNoResultsQuery = true;
									break;
								}
							}
						}
					}
				}
			}
		}

		readonly List<string> Exemptions;
		readonly Dictionary<string, string> Conversions;
	}
}
