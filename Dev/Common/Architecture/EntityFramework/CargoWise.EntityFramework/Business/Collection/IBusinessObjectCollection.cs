using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Async;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectCollection<out TBusinessObject> : IBusinessObjectCollection, IEnumerable<TBusinessObject>
	{
		void Load();
		new TBusinessObject AddNew();
		new TBusinessObject this[int index] { get; }
		void RemoveAll();
		void RemoveAndDelete(BusinessObject businessObject);
		void RemoveAndDeleteAll();
		void MarkAsNeedingValidation();
		void Sort(string propertyName);
		void Sort(string propertyName, ListSortDirection direction);
		Array ToArray(Type type);

		event CollectionCountChangedEventHandler CountChanged;
	}

	public interface IBusinessObjectCollection : IBindingList, IBusiness, ISortable
	{
		new BusinessObject AddNew();
		bool Contains(BusinessObject businessObject);
		bool Contains(ZGuid pk);

		PropertyDescriptor ListPropertyDescriptor { get; set; }
		object Parent { get; set; }

		void AddRange(IEnumerable businessObjects);
		Type TypeOfElements { get; }
		Type GetTypeOfElementsFromPK(ZGuid pk);
		BusinessObject[] ToArray();
		BusinessObject[] Find(ZQuery filter);
		ISortable Elements { get; }
		[Obsolete("For BusinessObjectCollection, Remove() (incorrecetly) calls RemoveFromRelationship(). For ActiveBusinessObjectCollection, Remove() calls Delete().", true)]
		void Remove(BusinessObject businessObject);
		void RemoveFromRelationship(BusinessObject businessObject);
		void Delete(BusinessObject businessObject);
		bool ReadOnly { get; }
		void AddGuidListMapping(string propertyName, string listName);
		void AddIsTime(string propertyName);
		bool IsLoaded { get; }
		BusinessObject FindByPK(ZGuid pk);
		void ApplySort(SortInfo sort);
		int IndexOf(IBusiness bizObj, int startIndex, int countToSearchFromStartIndex);

		IDisposable SuspendListChanged();
		IDisposable SuspendAdditionallyForImport();

		ZQuery CompleteFilter { get; }
		ZQuery RelationshipFilter { get; }
		IBusinessObjectCollectionFetchStrategy FetchStrategy { get; }
		IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction);
		SortInfo SortInformation { get; }
		event EventHandler SortChanged;
	}

	public interface IBindingTrackingBusinessObjectCollection
	{
		IDisposable Binding();
	}

	public interface IHaveAbstractElementType
	{
		Type NonAbstractTypeOfElements { get; }
	}

	public static class IBusinessObjectCollectionExtensions
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetupNewElementButDoNotAddIt(this IBusinessObjectCollection collection, BusinessObject newElement, bool setupCollectionRelationships)
		{
			BusinessObjectCollection legacyCollection = collection as BusinessObjectCollection;
			IActiveBusinessObjectCollection activeCollection = collection as IActiveBusinessObjectCollection;
			if (legacyCollection != null)
			{
				legacyCollection.SetupNewElementButDoNotAddIt(newElement, setupCollectionRelationships);
			}
			else if (activeCollection != null)
			{
				if (setupCollectionRelationships && activeCollection.Relationship.SupportsAddToRelationship())
				{
					collection.Add(newElement);
				}
				activeCollection.SetDefaultsForNewElement(newElement);
			}
			else
			{
				throw new InvalidOperationException("Collection type " + collection.GetType().FullName + " not supported");
			}
		}

		public static T[] ToArray<T>(this IBusinessObjectCollection collection) where T : BusinessObject
		{
			var result = new List<T>();
			if (collection != null)
			{
				result.AddRange(collection.OfType<T>());
			}
			return result.ToArray();
		}

		public static IZType[] GetFieldValues(this IBusinessObjectCollection collection, string fieldName)
		{
			IZType[] result = new IZType[collection.Count];
			for (int i = 0; i < collection.Count; i++)
			{
				result[i] = (IZType)((BusinessObject)collection[i])[fieldName];
			}
			return result;
		}

		public static IZType[] GetFieldValues(this IBusinessObjectCollection collection, SchemaColumn schemaColumn)
		{
			IZType[] result = new IZType[collection.Count];
			for (int i = 0; i < collection.Count; i++)
			{
				result[i] = (IZType)((BusinessObject)collection[i])[schemaColumn];
			}
			return result;
		}
	}

	public interface IBusinessObjectCollectionTestingMembers
	{
#if DEBUG
		ZQuery AdditionalFilter { get; }
		ZQuery GetAdditionalFilter();
		string GetGuidListMapping(string columnName);
#endif
	}

	public interface ISortable
	{
		void ApplySort(IComparer comparer);
	}

	public interface IBusinessObjectFilterFactory
	{
		IBusinessObjectFilter NewBusinessObjectFilter();
	}

	public delegate void BusinessObjectFilterIsMatchingCallback(ThreadSentryCallbackData<BusinessObject, bool> callbackData);

	public interface IBusinessObjectFilter
	{
		bool IsMatching(BusinessObject bizObj);
	}
}
