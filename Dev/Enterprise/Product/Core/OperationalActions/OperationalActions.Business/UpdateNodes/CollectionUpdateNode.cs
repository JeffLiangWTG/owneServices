using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public class CollectionUpdateNode : StepUpdateNode
	{
		internal CollectionUpdateNode(PropertyInfo info)
			: base(info) { }

		public override IEnumerable<BusinessObject> Apply(IEnumerable<BusinessObject> targets)
		{
			if (targets.IsNullOrEmpty())
			{
				return targets;
			}

			var children = GetChildren();
			var viewCollections = IntermediateCollections(targets);

			var viewChildren = children.OfType<ViewUpdateNode>();
			if (viewChildren.Any() && viewChildren.All(child => !child.ElementsHasPropertyInfoForCollection(viewCollections.FirstOrDefault())))
			{
				var viewResults = Enumerable.Empty<BusinessObject>();

				// A view of the collection has been specified
				foreach (var child in viewChildren)
				{
					viewResults = viewResults.Concat(child.ApplyToCollections(viewCollections));
				}

				return viewResults;
			}

			foreach (var leaf in GetFields())
			{
				HandleLeafForBizOs(leaf, targets);
			}

			return HandleAllChildren(targets);
		}

		IEnumerable<BusinessObject> HandleAllChildren(IEnumerable<BusinessObject> targets)
		{
			var children = GetChildren();
			var collectionTargets = CollectionTargets(targets);

			if (children.IsNullOrEmpty())
			{
				if (ReflectionHelper.GetPropertyInfo(targets.FirstOrDefault()?.GetType(), Info.Name) != null)
				{
					return collectionTargets;
				}
				else
				{
					return targets;
				}
			}

			var results = Enumerable.Empty<BusinessObject>();
			IEnumerable<UpdateNode> remainingChildren;

			var collectionUpdateNodeChildren = children.OfType<CollectionUpdateNode>();
			remainingChildren = children.Except(collectionUpdateNodeChildren);
			var propertyUpdateNodeChildren = children.OfType<PropertyUpdateNode>();
			remainingChildren = remainingChildren.Except(propertyUpdateNodeChildren);

			var firstCollection = (IBusinessObjectCollection)Info.GetValue(targets.FirstOrDefault(), null);

			foreach (var child in collectionUpdateNodeChildren)
			{
				if (ReflectionHelper.GetPropertyInfo(firstCollection.GetType(), child.Info.Name) != null && ReflectionHelper.GetPropertyInfo(firstCollection.TypeOfElements, child.Info.Name) == null)
				{
					results = results.Concat(child.ApplyToCollections(IntermediateCollections(targets)));
				}
				else
				{
					results = results.Concat(child.Apply(collectionTargets));
				}
			}

			foreach (var child in propertyUpdateNodeChildren)
			{
				var childProperty = ReflectionHelper.GetPropertyInfo(firstCollection.GetType(), (child.Info.Name));
				if (childProperty != null && ReflectionHelper.GetPropertyInfo(firstCollection.TypeOfElements, child.Info.Name) == null && typeof(BusinessObject).IsAssignableFrom(childProperty.PropertyType))
				{
					results = results.Concat(child.ApplyToBizOsOnCollections(IntermediateCollections(targets)));
				}
				else
				{
					results = results.Concat(child.Apply(collectionTargets));
				}
			}

			foreach (var otherChild in remainingChildren)
			{
				results = results.Concat(otherChild.Apply(collectionTargets));
			}

			var resultsDict = new Dictionary<ZGuid, BusinessObject>();
			foreach (var result in results)
			{
				if (!resultsDict.ContainsKey(result.PK))
				{
					resultsDict.Add(result.PK, result);
				}
			}

			return resultsDict.Values;
		}

		protected override void HandleLeafForBizOs(InfoValuePair leaf, IEnumerable<BusinessObject> targets)
		{
			if (ReflectionHelper.GetPropertyInfo(targets.FirstOrDefault()?.GetType(), Info.Name) != null)
			{
				base.HandleLeafForBizOs(leaf, CollectionTargets(targets));
				return;
			}

			base.HandleLeafForBizOs(leaf, targets);
		}

		IEnumerable<BusinessObject> ApplyToCollections(IEnumerable<IBusinessObjectCollection> collections)
		{
			var children = GetChildren();
			var results = Enumerable.Empty<BusinessObject>();

			if (collections.IsNullOrEmpty())
			{
				return results;
			}

			var childCollection = (IBusinessObjectCollection)Info.GetValue(collections.FirstOrDefault());
			var unpackedCollections = UnpackCollections(collections);

			if (children.Any(child => child is CollectionUpdateNode))
			{
				var collectionNodes = children.OfType<CollectionUpdateNode>();
				var otherNodes = children.Except(collectionNodes);

				foreach (var child in children.OfType<CollectionUpdateNode>())
				{
					if (ReflectionHelper.GetPropertyInfo(childCollection.GetType(), child.Info.Name) != null && ReflectionHelper.GetPropertyInfo(childCollection.TypeOfElements, child.Info.Name) == null)
					{
						var nextLayerOfCollections = new List<IBusinessObjectCollection>();

						foreach (var collection in collections)
						{
							nextLayerOfCollections.Add((IBusinessObjectCollection)Info.GetValue(collection, null));
						}

						results = results.Concat(child.ApplyToCollections(nextLayerOfCollections));
					}
					else
					{
						results = results.Concat(child.Apply(unpackedCollections));
					}
				}

				foreach (var otherChild in otherNodes)
				{
					results = results.Concat(otherChild.Apply(unpackedCollections));
				}

				var resultsDict = new Dictionary<ZGuid, BusinessObject>();
				foreach (var result in results)
				{
					if (!resultsDict.ContainsKey(result.PK))
					{
						resultsDict.Add(result.PK, result);
					}
				}

				return resultsDict.Values;
			}

			return base.Apply(unpackedCollections);
		}

		IEnumerable<IBusinessObjectCollection> IntermediateCollections(IEnumerable<BusinessObject> fromTargets)
		{
			foreach (BusinessObject target in fromTargets)
			{
				var collection = (IBusinessObjectCollection)Info.GetValue(target, null);

				if (collection != null)
				{
					yield return collection;
				}
			}
		}

		IEnumerable<BusinessObject> UnpackCollections(IEnumerable<IBusinessObjectCollection> collections)
		{
			var result = Enumerable.Empty<BusinessObject>();

			foreach (var collection in collections)
			{
				var childCollection = (IBusinessObjectCollection)Info.GetValue(collection, null);
				if (childCollection != null)
				{
					result = result.Union(childCollection as IEnumerable<BusinessObject>);
				}
			}

			return result;
		}

		IEnumerable<BusinessObject> CollectionTargets(IEnumerable<BusinessObject> fromTargets)
		{
			Dictionary<ZGuid, BusinessObject> result = new Dictionary<ZGuid, BusinessObject>();

			foreach (BusinessObject target in fromTargets)
			{
				IBusinessObjectCollection collection = null;

				try
				{
					collection = (IBusinessObjectCollection)Info.GetValue(target, null);
				}
				catch (TargetException ex)
				{
					var message = $@"target:{target?.HumanReadableName}, PK:{target?.PK}, Type:{target?.GetType().FullName}, target:{target[Info.Name]} Property:{target[Info.Name]}
Info:{Info.Name}, DeclaringType:{Info.DeclaringType}, PropertyType.FullName:{Info.PropertyType?.FullName}";

					ErrorReporter.ReportOnce("TargetException in CollectionTargets", message, ex);
					throw;
				}

				if (collection != null)
				{
					foreach (BusinessObject element in collection)
					{
						if (!result.ContainsKey(element.PK))
						{
							result.Add(element.PK, element);
						}
					}
				}
			}

			return result.Values;
		}
	}
}

#region Test
#if DEBUG

#region DisplayProxy

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(StepUpdateNode.StepDisplayProxy<StepUpdateNode>))]
	partial class StepUpdateNode
	{
	}
}

#endregion

#endif
#endregion
