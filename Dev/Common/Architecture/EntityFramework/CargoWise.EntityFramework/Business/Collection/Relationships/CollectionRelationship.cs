using System;
using System.Collections;
using System.Reflection;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Represents the relationship of a collection.
	/// </summary>
	public interface ICollectionRelationship
	{
		ZQuery RelationshipFilter { get; }
		event EventHandler RelationshipFilterChanged;
		ICollectionRelationship AddFilter(ZQuery additionalFilter);
		bool MatchesRelationshipFilter(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache);

		BusinessObject[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter);
		BusinessObject Master { get; }
		bool HasChangesIncludingRelationship(BusinessObject businessObject);
		void ClearHasChangesIncludingRelationship(BusinessObject businessObject);

		bool SupportsAddToRelationship();
		void AddToRelationship(BusinessObject businessObject);
		void RemoveFromRelationship(BusinessObject businessObject);
		void Clear();
	}

	public interface ICollectionRelationshipWithCustomEnumerator : ICollectionRelationship
	{
		IEnumerator GetDataEnumerator(ZQuery additionalFilter);
	}

	/// <summary>
	/// A simple collection relationship that can take a filter.
	/// </summary>
	public class CollectionRelationship : ICollectionRelationshipWithCustomEnumerator
	{
		public CollectionRelationship(Type elementType)
			: this(elementType, null)
		{
		}

		public CollectionRelationship(Type elementType, ZQuery filter)
		{
			this.ElementType = elementType;
			this.AdditionalRelationshipFilter = filter;
		}

		protected readonly Type ElementType;

		#region IsMatchesRelationshipFilterOverridden

		[ThreadStatic]
		static bool lastIsMatchesRelationshipFilterOverridden;
		[ThreadStatic]
		static Type lastTypeForIsMatchesRelationshipFilterOverridden;
		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, bool?> isMatchesRelationshipFilterOverriddenCache = new LRUCache<Type, bool?>();

		/// <summary>
		/// Call this method to see if a call to MatchesFilter is really necessary, or if a call
		/// to Index.MatchesFilterBaseBehaviour will surfice. This is used by the index to determine if
		/// a WeakReference really needs to be dereferenced, which can degrade performance.
		/// </summary>
		internal static bool IsMatchesRelationshipFilterOverridden(Type relationshipType)
		{
			bool? result;
			if (relationshipType == lastTypeForIsMatchesRelationshipFilterOverridden)
			{
				result = lastIsMatchesRelationshipFilterOverridden;
			}
			else
			{
				result = isMatchesRelationshipFilterOverriddenCache[relationshipType];
				if (result == null)
				{
					result = !typeof(CollectionRelationship).IsAssignableFrom(relationshipType) || IsMatchesRelationshipFilterOverriddenCore(relationshipType);
					isMatchesRelationshipFilterOverriddenCache.Add(relationshipType, result);
				}
				lastTypeForIsMatchesRelationshipFilterOverridden = relationshipType;
				lastIsMatchesRelationshipFilterOverridden = (bool)result;
			}
			return (bool)result;
		}

		static bool IsMatchesRelationshipFilterOverriddenCore(Type relationshipType)
		{
			Type currentType = relationshipType;
			Type baseRelationshipType = FindBaseRelationshipType(relationshipType);
			while (baseRelationshipType != null && currentType != null)
			{
				MethodInfo method = relationshipType.GetMethod("MatchesRelationshipFilterCore", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(BusinessObject), typeof(bool), typeof(bool) }, null);
				if (method != null && method.DeclaringType != baseRelationshipType)
				{
					return true;
				}
				currentType = currentType.BaseType;
			}
			return false;
		}

		static Type FindBaseRelationshipType(Type relationshipType)
		{
			Type currentType = relationshipType;
			while (currentType != null && currentType != typeof(CollectionRelationship))
			{
				currentType = currentType.BaseType;
			}
			return currentType;
		}

		#endregion

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			CollectionRelationship rhs = obj as CollectionRelationship;
			bool result = rhs != null;

			result = result && GetType() == rhs.GetType();
			result = result && object.Equals(AdditionalRelationshipFilter, rhs.AdditionalRelationshipFilter);
			return result;
		}

		public override int GetHashCode()
		{
			return RelationshipFilter.GetHashCode();
		}

		#endregion

		#region RelationshipFilter

		public ZQuery RelationshipFilter
		{
			get
			{
				if (relationshipFilter == null)
				{
					if (Master != null && Master.IsDeleted)
					{
						return ZQuery.NoResultQuery;
					}

					ZQuery relationshipFilterCore = RelationshipFilterCore;
					relationshipFilter = new ZQuery();
					if (relationshipFilterCore != null)
					{
						relationshipFilter.AddToFilter(relationshipFilterCore);
					}
					if (AdditionalRelationshipFilter != null)
					{
						relationshipFilter.AddToFilter(AdditionalRelationshipFilter);
					}
					relationshipFilter.MaximumRows =
						(AdditionalRelationshipFilter == null ? null : AdditionalRelationshipFilter.MaximumRows) ??
						(relationshipFilterCore == null ? null : relationshipFilterCore.MaximumRows);
					relationshipFilter.ModificationsEnabled = false;
					relationshipFilterIdentifier = ZGuid.NewZGuid();
				}
				return relationshipFilter;
			}
		}
		ZQuery relationshipFilter;
		ZGuid relationshipFilterIdentifier;

		ZQuery RelationshipFilterIgnoreActiveFilter
		{
			get
			{
				if (relationshipFilterIgnoreActiveFilter == null)
				{
					relationshipFilterIgnoreActiveFilter = RelationshipFilter.DeepClone();
					relationshipFilterIgnoreActiveFilter.IgnoreActiveFilter = true;
					relationshipFilterIgnoreActiveFilter.ModificationsEnabled = false;
					relationshipFilterIgnoreActiveFilterIdentifier = ZGuid.NewZGuid();
				}
				return relationshipFilterIgnoreActiveFilter;
			}
		}
		ZQuery relationshipFilterIgnoreActiveFilter;
		ZGuid relationshipFilterIgnoreActiveFilterIdentifier;

		protected ZQuery AdditionalRelationshipFilter
		{
			get { return additionalRelationshipFilter; }
			private set
			{
				additionalRelationshipFilter = value;
				if (value != null)
				{
					value.ModificationsEnabled = false;
					relationshipFilter = null;
				}
			}
		}
		ZQuery additionalRelationshipFilter;

		protected virtual ZQuery RelationshipFilterCore
		{
			get { return null; }
		}

		event EventHandler ICollectionRelationship.RelationshipFilterChanged
		{
			add { relationshipFilterChanged += value; }
			remove { relationshipFilterChanged -= value; }
		}
		event EventHandler relationshipFilterChanged;

		protected void OnRelationshipFilterChanged(EventArgs e)
		{
			InvalidateRelationshipFilter();
			if (relationshipFilterChanged != null)
			{
				relationshipFilterChanged(this, e);
			}
		}

		void InvalidateRelationshipFilter()
		{
			relationshipFilter = null;
			relationshipFilterIgnoreActiveFilter = null;
		}

		#endregion

		#region ICollectionRelationship

		public bool MatchesRelationshipFilter(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			return MatchesRelationshipFilterCore(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache);
		}

		protected virtual bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			ZQuery query = new ZQuery(ignoreActiveFilter ? RelationshipFilterIgnoreActiveFilter : RelationshipFilter);
			query.FetchOnlyFromLocalCache |= fetchOnlyFromLocalCache;
			return businessObject.MatchesFilter(query, ignoreActiveFilter ? relationshipFilterIgnoreActiveFilterIdentifier.ToString() : relationshipFilterIdentifier.ToString());
		}

		public ICollectionRelationship AddFilter(ZQuery additionalFilter)
		{
			CollectionRelationship result = Clone();

			ZQuery newAdditionalRelationshipFilter = new ZQuery();
			newAdditionalRelationshipFilter.AddToFilter(additionalFilter);

			if (AdditionalRelationshipFilter != null)
			{
				newAdditionalRelationshipFilter.AddToFilter(AdditionalRelationshipFilter);
				newAdditionalRelationshipFilter.MaximumRows = additionalFilter.MaximumRows ?? result.AdditionalRelationshipFilter.MaximumRows;
			}
			result.AdditionalRelationshipFilter = newAdditionalRelationshipFilter;
			return result;
		}

		protected virtual CollectionRelationship Clone()
		{
			return (CollectionRelationship)MemberwiseClone();
		}

		public BusinessObject[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter)
		{
			return LoadBusinessObjectsCore(factory, filter);
		}

		protected virtual BusinessObject[] LoadBusinessObjectsCore(BusinessObjectFactory factory, ZQuery filter)
		{
			ZQuery completeFilter = new ZQuery();
			completeFilter.AddToFilter(filter);
			completeFilter.AddToFilter(RelationshipFilter);
			completeFilter.MaximumRows = filter.MaximumRows ?? RelationshipFilter.MaximumRows;
			completeFilter.FetchOnlyFromLocalCache |= (Master != null && !Master.IsInDatabase);
			//completeFilter.ReLoadExistingRows |= completeFilter.IsDBOnlyQuery && !completeFilter.FetchOnlyFromLocalCache;
			return factory.Load(ElementType, completeFilter);
		}

		public virtual BusinessObject Master
		{
			get { return null; }
		}

		public virtual bool HasChangesIncludingRelationship(BusinessObject businessObject)
		{
			return businessObject.HasChanges;
		}

		public virtual void ClearHasChangesIncludingRelationship(BusinessObject businessObject)
		{
			IBusinessObjectState businessObjectState = businessObject;
			businessObjectState.ClearHasChangesIncludingChildren();
		}

		public bool SupportsAddToRelationship()
		{
			return SupportsAddToRelationshipCore();
		}

		protected virtual bool SupportsAddToRelationshipCore()
		{
			return false;
		}

		IEnumerator ICollectionRelationshipWithCustomEnumerator.GetDataEnumerator(ZQuery additionalFilter)
		{
			return GetDataEnumeratorCore(additionalFilter);
		}

		protected virtual IEnumerator GetDataEnumeratorCore(ZQuery additionalFilter) => null;

		void ICollectionRelationship.AddToRelationship(BusinessObject businessObject)
		{
			AddToRelationship(businessObject);
		}

		protected virtual void AddToRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Cannot add to a collection with a " + (GetType() == typeof(CollectionRelationship) ? "non-dependent CollectionRelationship" : GetType().Name));
		}

		void ICollectionRelationship.RemoveFromRelationship(BusinessObject businessObject)
		{
			RemoveFromRelationship(businessObject);
		}

		protected virtual void RemoveFromRelationship(BusinessObject businessObject)
		{
			throw new InvalidOperationException("Cannot remove from a collection with a " + GetType().Name);
		}

		void ICollectionRelationship.Clear()
		{
			Clear();
		}

		protected virtual void Clear()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
