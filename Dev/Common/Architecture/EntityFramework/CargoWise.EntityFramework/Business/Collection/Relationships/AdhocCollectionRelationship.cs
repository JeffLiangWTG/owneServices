using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class AdhocCollectionRelationship : CollectionRelationship
	{
		public AdhocCollectionRelationship(Type elementType)
			: base(elementType)
		{
		}

		public IEnumerable<ZGuid> PKList
		{
			get { return pkList.List; }
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return hashObject.GetHashCode();
		}

		readonly object hashObject = new object();

		#endregion

		#region Clone

		protected override CollectionRelationship Clone()
		{
			AdhocCollectionRelationship result = (AdhocCollectionRelationship)base.Clone();
			result.pkList = pkList.Clone();
			return result;
		}

		#endregion

		#region AddToRelationship / RemoveFromRelationship / Clear

		protected override bool SupportsAddToRelationshipCore()
		{
			return true;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			pkList.CopyOnWriteIfRequired();
			pkList.List.Add(businessObject.PK);
			businessObject.HasChangesChanged -= BusinessObjectChanged;
			businessObject.HasChangesChanged += BusinessObjectChanged;
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			pkList.CopyOnWriteIfRequired();
			businessObject.HasChangesChanged -= BusinessObjectChanged;
			pkList.List.Remove(businessObject.PK);
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override void Clear()
		{
			pkList.CopyOnWriteIfRequired();
			pkList.List.Clear();
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		#endregion

		#region RelationshipFilter

		// NOTE: If unsealing, MatchesRelationshipFilterCore will require changes
		protected override sealed ZQuery RelationshipFilterCore
		{
			get
			{
				ZQuery query = new ZQuery();
				if (pkList.List.Count == 0)
				{
					query = ZQuery.NoResultQuery;
				}
				else
				{
					query.AddToFilter(PkColumn, pkList.List.ToArray());
				}
				return query;
			}
		}

		SchemaPKColumn PkColumn => ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(BusinessObjectFactory.GetTableNameFromType(ElementType));

		#endregion

		#region CopyOnWriteList class

		class CopyOnWriteList<T>
		{
			public CopyOnWriteList<T> Clone()
			{
				CopyOnWriteList<T> result = (CopyOnWriteList<T>)MemberwiseClone();
				copyOnWrite = true;
				result.copyOnWrite = true;
				return result;
			}

			public List<T> List
			{
				get
				{
					if (list == null)
					{
						list = new List<T>();
					}
					return list;
				}
			}
			List<T> list;

			public void CopyOnWriteIfRequired()
			{
				if (copyOnWrite)
				{
					if (list != null)
					{
						list = new List<T>(list);
					}
					copyOnWrite = false;
				}
			}

			bool copyOnWrite;
		}

		#endregion

		#region Implementation

		CopyOnWriteList<ZGuid> pkList = new CopyOnWriteList<ZGuid>();

		protected override BusinessObject[] LoadBusinessObjectsCore(BusinessObjectFactory factory, ZQuery filter)
		{
			List<BusinessObject> result = new List<BusinessObject>();
			foreach (ZGuid pk in pkList.List)
			{
				BusinessObject businessObject = factory.Load(ElementType, pk);
				if (businessObject != null && !businessObject.IsDeleted && businessObject.MatchesFilter(filter))
				{
					result.Add(businessObject);
				}
			}
			return result.ToArray();
		}

		protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
		{
			return
				pkList.List.Contains(businessObject.PK) &&
				// Calling base may require evaluation of an expensive query for large collections - do the below to retain previous behaviour with a simpler query
				businessObject.MatchesFilter(new ZQuery(PkColumn, businessObject.PK) { IgnoreActiveFilter = ignoreActiveFilter, FetchOnlyFromLocalCache = true });
		}

		void BusinessObjectChanged(object o, EventArgs e)
		{
			BusinessObject businessObject = (BusinessObject)o;
			if (businessObject.IsDeleted)
			{
				RemoveFromRelationship(businessObject);
			}
		}

		#endregion
	}
}
