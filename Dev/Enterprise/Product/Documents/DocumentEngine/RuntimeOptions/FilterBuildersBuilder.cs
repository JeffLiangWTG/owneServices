using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class FilterBuildersBuilder
	{
		List<IReportDocumenter> filterBuilderDocumenters;
		public List<IReportDocumenter> FilterBuilderDocumenters
		{
			get
			{
				if (filterBuilderDocumenters == null)
				{
					filterBuilderDocumenters = new List<IReportDocumenter>();
					var fFilterBuilders = new FilterCollectionBuilder(null, null, null, null, string.Empty, ReportRunningType.ReportReferenceGuide).FilterBuilders;
					foreach (var filterBuilder in fFilterBuilders.OfType<FilterBuilder>())
					{
						filterBuilderDocumenters.Add(filterBuilder.Documentation);
					}
				}
				return filterBuilderDocumenters;
			}
		}
	}
}
