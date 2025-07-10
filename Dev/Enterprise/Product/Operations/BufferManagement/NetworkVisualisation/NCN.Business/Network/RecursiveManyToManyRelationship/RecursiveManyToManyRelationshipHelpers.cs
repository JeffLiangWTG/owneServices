using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public delegate ZQuery GetQueryFromBizos<T>(IEnumerable<T> bizos);

	public static class RecursiveManyToManyRelationshipHelpers
	{
		public static RelationshipSet<TBizo, TPivot> LoadRecursive<TBizo, TPivot>(BusinessObjectFactory factory, IEnumerable<TBizo> rootElements, GetQueryFromBizos<TPivot> getBizoQueryFromPivots, GetQueryFromBizos<TBizo> getPivotQueryFromBizos)
			where TBizo : BusinessObject
			where TPivot : BusinessObject
		{
			var aggregatedPivots = new HashSet<TPivot>();
			var aggregatedBizos = new HashSet<TBizo>();

			var lastPivots = new HashSet<TPivot>();
			var lastBizos = new HashSet<TBizo>(rootElements);

			var newBizosAdded = true;
			while (newBizosAdded)
			{
				var lastAggrShapeCount = aggregatedBizos.Count;
				aggregatedBizos.UnionWith(lastBizos);
				aggregatedPivots.UnionWith(lastPivots);
				newBizosAdded = lastAggrShapeCount != aggregatedBizos.Count;

				if (newBizosAdded)
				{
					var pivotQuery = getPivotQueryFromBizos(lastBizos);
					lastPivots = new HashSet<TPivot>(factory.Load<TPivot>(pivotQuery));

					var bizoQuery = getBizoQueryFromPivots(lastPivots);
					lastBizos = new HashSet<TBizo>(factory.Load<TBizo>(bizoQuery));
				}
			}

			return new RelationshipSet<TBizo, TPivot>(aggregatedBizos, aggregatedPivots);
		}
	}
}
