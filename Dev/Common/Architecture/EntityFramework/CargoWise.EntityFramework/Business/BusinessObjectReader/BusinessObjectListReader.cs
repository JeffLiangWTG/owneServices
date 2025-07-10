using System;
using System.Collections;

namespace CargoWise.EntityFramework
{
	public class BusinessObjectListReader : FilteredBusinessObjectReader, IList
	{
		public BusinessObjectListReader(ZQuery objectFilter, Type businessObjectType, BusinessObjectFactory factory = null) : base(objectFilter, businessObjectType, factory)
		{
		}

		public BusinessObjectListReader(BusinessObjectFactoryProvider factoryProvider, ZQuery objectFilter, Type businessObjectType) : base(factoryProvider, objectFilter, businessObjectType)
		{
		}

		protected BusinessObjectListReader(BusinessObjectFactoryProvider factoryProvider, Type businessObjectType) : base(factoryProvider, businessObjectType)
		{
		}

		protected BusinessObjectListReader(Type businessObjectType, BusinessObjectFactory factory = null)
			: base(businessObjectType, factory)
		{
		}

		#region Implementation of ICollection

		void ICollection.CopyTo(Array array, int index)
		{
			foreach (BusinessObject businessObject in this)
			{
				array.SetValue(businessObject, index++);
			}
		}

		int ICollection.Count
		{
			get { return Factory.GetDatabaseCount(BusinessObjectType, ObjectFilter); }
		}

		object ICollection.SyncRoot
		{
			get { return syncRoot; }
		}
		readonly static object syncRoot = new object();

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		#endregion

		#region Implementation of IList

		int IList.Add(object item)
		{
			throw new NotSupportedException("BusinessObjectListReader is readonly collection.");
		}

		void IList.Clear()
		{
			throw new NotSupportedException("BusinessObjectListReader is readonly collection.");
		}

		bool IList.Contains(object item)
		{
			throw new NotSupportedException("BusinessObjectListReader is one-direction sequentional reader.");
		}

		int IList.IndexOf(object item)
		{
			throw new NotSupportedException("BusinessObjectListReader is one-direction sequentional reader.");
		}

		void IList.Insert(int index, object item)
		{
			throw new NotSupportedException("BusinessObjectListReader is readonly collection.");
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		bool IList.IsFixedSize
		{
			get { return true; }
		}

		void IList.Remove(object item)
		{
			throw new NotSupportedException("BusinessObjectListReader is readonly collection.");
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException("BusinessObjectListReader is readonly collection.");
		}

		object IList.this[int index]
		{
			get
			{
				if (index == 0)
				{
					return Factory.LoadTop1(BusinessObjectType, ObjectFilter);
				}
				throw new NotSupportedException("BusinessObjectListReader is one-direction sequentional reader.");
			}
			set { throw new NotSupportedException("BusinessObjectListReader is readonly collection."); }
		}

		#endregion
	}
}
