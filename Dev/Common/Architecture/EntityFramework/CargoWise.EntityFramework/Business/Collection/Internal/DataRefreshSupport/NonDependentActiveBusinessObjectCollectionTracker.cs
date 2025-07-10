using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	internal class NonDependentActiveBusinessObjectCollectionTracker : IActiveBusinessObjectCollectionTracker, IService
	{
		public void NotifyCollectionIndexCreated(IActiveBusinessObjectCollectionIndex nonDependentCollectionIndex)
		{
			NonDependentCollections.Add(nonDependentCollectionIndex);
		}

		public void NotifyCollectionIndexDisposed(IActiveBusinessObjectCollectionIndex nonDependentCollectionIndex)
		{
			NonDependentCollections.Remove(nonDependentCollectionIndex);
		}

		public IEnumerable<BusinessObject> GetBizosMatchingInAnyCollectionFilter(IEnumerable<BusinessObject> bizos)
		{
			if (bizos != null && bizos.Any())
			{
				var bizosList = bizos.ToList();
				foreach (IActiveBusinessObjectCollectionIndex collection in NonDependentCollections)
				{
					var matchingBizos = collection.GetMatchingBusinessObjects(bizos);
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

		public bool MatchesInAnyCollectionFilter(BusinessObject businessObject)
		{
			var result = false;
			foreach (IActiveBusinessObjectCollectionIndex collection in NonDependentCollections)
			{
				if (collection.MatchesFilter(businessObject, false))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		internal List<IActiveBusinessObjectCollectionIndex> NonDependentCollections { get; } = new List<IActiveBusinessObjectCollectionIndex>();
	}
}
