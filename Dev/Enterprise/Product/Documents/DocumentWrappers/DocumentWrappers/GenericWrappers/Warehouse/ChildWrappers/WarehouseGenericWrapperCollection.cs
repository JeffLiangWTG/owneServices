using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class WarehouseGenericWrapperCollection<T> : WarehouseGenericWrapperCollection where T : WarehouseGenericLineWrapper
	{
		#region Constructors

		public WarehouseGenericWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseGenericWrapperCollection(BusinessObjectCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		#endregion

		public new T this[int index]
		{
			get { return (T)base[index]; }
		}

		public new T this[string index]
		{
			get { return (T)base[index]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}
	}

	public abstract class WarehouseGenericWrapperCollection : GenericWrapperCollection
	{
		#region Constructors

		public WarehouseGenericWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WarehouseGenericWrapperCollection(BusinessObjectCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		#endregion

		public new WarehouseGenericLineWrapper this[int index]
		{
			get { return (WarehouseGenericLineWrapper)base[index]; }
		}

		public new WarehouseGenericLineWrapper this[string index]
		{
			get { return (WarehouseGenericLineWrapper)base[index]; }
		}

		public new WarehouseGenericLineWrapper AddNew()
		{
			return (WarehouseGenericLineWrapper)base.AddNew();
		}
	}
}
