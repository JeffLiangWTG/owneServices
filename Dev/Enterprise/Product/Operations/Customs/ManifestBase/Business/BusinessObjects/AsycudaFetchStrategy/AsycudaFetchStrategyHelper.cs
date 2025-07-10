using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public static class AsycudaFetchStrategyHelper
	{
		public static void FetchForDelete(BusinessObject businessObject)
		{
			AsycudaBreadthFirstTraversalFetcher(businessObject, AsycudaFetchStrategy.FetchForDeleteKey, strategy => strategy.AddFetchHintsForDelete(), strategy => strategy.FetchForDelete(), strategy => strategy.FetchForDelete());
		}

		public static void FetchForLoadChildEditableObjects(BusinessObject businessObject)
		{
			AsycudaBreadthFirstTraversalFetcher(businessObject, AsycudaFetchStrategy.FetchForLoadChildEditableObjectsKey, strategy => strategy.AddFetchHintsForLoadChildEditableObjects(), strategy => strategy.FetchForLoadChildEditableObjects(), strategy => strategy.FetchForLoadChildEditableObjects());
		}

		public static void FetchForValidate(BusinessObject businessObject)
		{
			AsycudaBreadthFirstTraversalFetcher(businessObject, AsycudaFetchStrategy.FetchForValidateKey, strategy => strategy.AddFetchHintsForValidate(), strategy => strategy.FetchForValidate(), strategy => strategy.FetchForValidate());
		}

		public static void AsycudaBreadthFirstTraversalFetcher(BusinessObject businessObject, string cacheKey, Action<IAsycudaFetchStrategy> addFetchHints, FetchActionForAscycudaStrategies fetchAction, FetchActionForNonAsycudaStrategies fetchActionForNonAsycudaStrategies)
		{
			if (businessObject.IsInDatabase && businessObject.FetchStrategy is IAsycudaFetchStrategy strategy)
			{
				var alreadyFetched = businessObject.Factory.GetCachedValue(cacheKey, () => new HashSet<ZGuid>(), CacheStalenessPolicy.NeverStale);
				if (!alreadyFetched.Contains(businessObject.PK))
				{
					var discovered = new HashSet<ZGuid>();
					var queue = new Queue<BusinessObject>();
					queue.Enqueue(businessObject);
					addFetchHints(strategy);
					AddAdditonalStrategies(businessObject, fetchActionForNonAsycudaStrategies);

					while (queue.Any())
					{
						var current = queue.Dequeue();
						if (current.FetchStrategy is IAsycudaFetchStrategy currentFetchStrategy)
						{
							var children = fetchAction(currentFetchStrategy).ToArray();
							if (children.Any())
							{
								foreach (var child in children)
								{
									if (child != null && !discovered.Contains(child.PK))
									{
										if (child.FetchStrategy is IAsycudaFetchStrategy childFetchStrategy)
										{
											discovered.Add(child.PK);
											queue.Enqueue(child);
											addFetchHints(childFetchStrategy);
											AddAdditonalStrategies(child, fetchActionForNonAsycudaStrategies);
										}
										else
										{
											fetchActionForNonAsycudaStrategies(child.FetchStrategy);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public delegate IEnumerable<BusinessObject> FetchActionForAscycudaStrategies(IAsycudaFetchStrategy fetchStrategy);
		public delegate void FetchActionForNonAsycudaStrategies(IBusinessObjectFetchStrategy fetchStrategy);

		static void AddAdditonalStrategies(BusinessObject businessObject, FetchActionForNonAsycudaStrategies fetchActionForNonAsycudaStrategies)
		{
			if (businessObject is IAdditionalBusinessObjectFetchStrategyProvider additionalStrategyProvider)
			{
				additionalStrategyProvider.GetFetchStrategies().ForEach(x => fetchActionForNonAsycudaStrategies(x));
			}
		}
	}
}
