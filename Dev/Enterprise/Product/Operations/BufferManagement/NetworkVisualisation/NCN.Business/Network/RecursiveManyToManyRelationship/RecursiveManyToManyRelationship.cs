using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public delegate void PivotsReassigned<T>(ActiveBusinessObjectCollection<T> pivots)
	where T : BusinessObject;

	public class RecursiveManyToManyRelationship<TBizo, TPivot> : CollectionRelationship
		where TBizo : BusinessObject
		where TPivot : BusinessObject
	{
		public RecursiveManyToManyRelationship(TBizo master, GetQueryFromBizos<TPivot> getBizoQueryFromPivots, GetQueryFromBizos<TBizo> getPivotQueryFromBizos, PivotsReassigned<TPivot> pivotsReassigned = null)
			: base(typeof(TBizo), new ZQuery())
		{
			Argument.NotNull(master, "master");
			Argument.NotNull(master.Factory, "master.Factory");
			Argument.NotNull(getBizoQueryFromPivots, "getBizoQueryFromPivots");
			Argument.NotNull(getPivotQueryFromBizos, "getPivotQueryFromBizos");

			this.master = master;
			this.getBizoQueryFromPivots = getBizoQueryFromPivots;
			this.getPivotQueryFromBizos = getPivotQueryFromBizos;
			this.pivotsReassigned = pivotsReassigned;
		}

		readonly TBizo master;
		readonly PivotsReassigned<TPivot> pivotsReassigned;
		readonly GetQueryFromBizos<TBizo> getPivotQueryFromBizos;
		readonly GetQueryFromBizos<TPivot> getBizoQueryFromPivots;
		ActiveBusinessObjectCollection<TPivot> pivots;

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var relationship = obj as RecursiveManyToManyRelationship<TBizo, TPivot>;

			if (relationship != null)
			{
				return relationship.master == master
					&& relationship.getPivotQueryFromBizos == getPivotQueryFromBizos
					&& relationship.getBizoQueryFromPivots == getBizoQueryFromPivots
					&& relationship.RelationshipFilter == RelationshipFilter;
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public override int GetHashCode()
		{
			return master.PK.GetHashCode() ^ getBizoQueryFromPivots.GetHashCode() ^ getPivotQueryFromBizos.GetHashCode();
		}

		#endregion

		#region Overrides

		protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			var query = pivots == null ? ZQuery.NoResultQuery : getBizoQueryFromPivots(pivots);
			query.FetchOnlyFromLocalCache |= fetchOnlyFromLocalCache;
			return businessObject.MatchesFilter(query);
		}

		protected override ZQuery RelationshipFilterCore
		{
			get { return pivots == null ? ZQuery.NoResultQuery : relationshipFilter ?? (relationshipFilter = getBizoQueryFromPivots(pivots)); }
		}

		ZQuery relationshipFilter;

		RelationshipSet<TBizo, TPivot> LoadAll()
		{
			return RecursiveManyToManyRelationshipHelpers.LoadRecursive(master.Factory, new[]
			{
				master
			}, getBizoQueryFromPivots, getPivotQueryFromBizos);
		}

		protected override BusinessObject[] LoadBusinessObjectsCore(BusinessObjectFactory factory, ZQuery filter)
		{
			var allBizos = LoadAll();
			pivots = InitialisePivots(allBizos.Nodes);
			return allBizos.Nodes.ToArray();
		}

		protected override bool SupportsAddToRelationshipCore()
		{
			return false;
		}

		#endregion

		#region Pivots

		ActiveBusinessObjectCollection<TPivot> InitialisePivots(IEnumerable<TBizo> bizos)
		{
			var objectPivots = new ActiveBusinessObjectCollection<TPivot>(master.Factory, getPivotQueryFromBizos(bizos));
			objectPivots.CountChanged += pivots_CountChanged;
			if (pivotsReassigned != null)
			{
				pivotsReassigned(objectPivots);
			}
			return objectPivots;
		}

		void pivots_CountChanged(object sender, EventArgs e)
		{
			pivots.CountChanged -= pivots_CountChanged;
			pivots = InitialisePivots(LoadAll().Nodes);
			relationshipFilter = null;
			OnRelationshipFilterChanged(e);
		}

		#endregion
	}
}
