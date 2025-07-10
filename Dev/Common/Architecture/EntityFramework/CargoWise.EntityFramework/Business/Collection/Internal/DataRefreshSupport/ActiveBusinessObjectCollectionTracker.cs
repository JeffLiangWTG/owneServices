using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Helper class to track ActiveBusinessObjectCollectionIndexes to support the
	/// MatchesInAnyCollectionFilter(BusinessObject) method.
	/// </summary>
	internal class ActiveBusinessObjectCollectionTracker
	{
		public void NotifyCollectionIndexCreated(IActiveBusinessObjectCollectionIndex collection)
		{
			if (collection.Relationship is DependentRelationship)
			{
				DependentCollections.NotifyCollectionIndexCreated(collection);
			}
			else if (collection.Relationship is ManyToManyRelationship)
			{
				ManyToManyCollections.NotifyCollectionIndexCreated(collection);
			}
			else
			{
				NonDependentCollections.NotifyCollectionIndexCreated(collection);
			}
		}

		public void NotifyCollectionIndexDisposed(IActiveBusinessObjectCollectionIndex collection)
		{
			if (collection.Relationship is DependentRelationship)
			{
				DependentCollections.NotifyCollectionIndexDisposed(collection);
			}
			else if (collection.Relationship is ManyToManyRelationship)
			{
				ManyToManyCollections.NotifyCollectionIndexDisposed(collection);
			}
			else
			{
				NonDependentCollections.NotifyCollectionIndexDisposed(collection);
			}
		}

		List<IActiveBusinessObjectCollectionTracker> trackers;
		public List<IActiveBusinessObjectCollectionTracker> AllTrackers
		{
			get
			{
				if (trackers == null)
				{
					trackers = new List<IActiveBusinessObjectCollectionTracker> { NonDependentCollections, DependentCollections, ManyToManyCollections };
				}
				return trackers;
			}
		}

		public IEnumerable<BusinessObject> GetBizosMatchingInAnyCollectionFilter(IEnumerable<BusinessObject> bizos)
		{
			var bizosList = bizos?.ToList();
			if (bizosList.Count > 0)
			{
				foreach (IActiveBusinessObjectCollectionTracker tracker in AllTrackers)
				{
					var matchingBizos = tracker.GetBizosMatchingInAnyCollectionFilter(bizosList);
					foreach (var bizo in matchingBizos)
					{
						yield return bizo;
					}
					bizosList = bizosList.Except(matchingBizos).ToList();
					if (bizosList.Count == 0)
					{
						yield break;
					}
				}
			}
		}

		internal NonDependentActiveBusinessObjectCollectionTracker NonDependentCollections { get; } = new NonDependentActiveBusinessObjectCollectionTracker();
		internal DependentActiveBusinessObjectCollectionTracker DependentCollections { get; } = new DependentActiveBusinessObjectCollectionTracker();
		internal ManyToManyActiveBusinessObjectCollectionTracker ManyToManyCollections { get; } = new ManyToManyActiveBusinessObjectCollectionTracker();
	}
}
