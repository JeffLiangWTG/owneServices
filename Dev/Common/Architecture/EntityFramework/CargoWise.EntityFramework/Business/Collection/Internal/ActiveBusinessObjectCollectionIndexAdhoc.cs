using System;
using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal sealed class ActiveBusinessObjectCollectionIndexAdhoc<T> : ActiveBusinessObjectCollectionIndex<T> where T : BusinessObject
	{
		public ActiveBusinessObjectCollectionIndexAdhoc(BusinessObjectFactory factory, Type collectionType, Type elementType, ICollectionRelationship relationship, ZQuery additionalFilter, IComparer sortComparer, object[] collectionState,
			ActiveBusinessObjectCollectionIndexCache<T>.CacheKey cacheKey) : base(factory, collectionType, elementType, relationship, additionalFilter, sortComparer, collectionState, cacheKey)
		{
		}

		protected override BusinessObjectList PopulateCacheCore()
		{
			BusinessObjectList newList = new BusinessObjectList(this);
			newList.AddRange(Relationship.LoadBusinessObjects(Factory, AdditionalFilter).Where(element => (object)element != null));
			if (SortComparer != null && inPopulateCacheCounter <= 3)
			{
				newList.Sort(SortComparer);
			}

			newList.AddRange(UncommittedObjects);

			return newList;
		}

		protected override void Table_RowDeleting(object sender, DataRowChangeEventArgs e)
		{
			AdhocCollectionRelationship adhocRelationship = Relationship as AdhocCollectionRelationship;
			if (adhocRelationship != null)
			{
				foreach (ZGuid pk in adhocRelationship.PKList)
				{
					if ((Guid)e.Row[0] == pk)
					{
						HasChangesFromDelete = true;
						break;
					}
				}
			}
		}

		protected override IDisposable SuppressDataLoad()
		{
			return null;
		}

		#region ICancelAddNew

		protected override void DeleteCore(T element)
		{
			element.Delete();
		}

		#endregion

		#region BusinessObject events

		protected override void BusinessObject_NotificationsChangedCore(NotificationsChangedEventArgs e)
		{
			OnNotificationsChanged(e);
		}

		#endregion

	}
}
