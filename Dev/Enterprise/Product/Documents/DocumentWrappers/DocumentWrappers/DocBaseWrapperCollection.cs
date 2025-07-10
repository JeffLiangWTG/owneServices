using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocBaseWrapperCollection<T> : DocBaseWrapperCollection where T : DocBaseWrapper
	{
		protected DocBaseWrapperCollection(BusinessObjectFactory factory) : base(factory) { }
		protected DocBaseWrapperCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory) : base(collectionToWrap, factory) { }

		public new T this[int index]
		{
			get { return (T)base[index]; }
		}

		public new T this[string index]
		{
			get { return (T)base[index]; }
		}
	}

	public abstract class DocBaseWrapperCollection : DocumentWrapperCollection
	{
		protected DocBaseWrapperCollection(BusinessObjectFactory factory) : base(factory) { }
		protected DocBaseWrapperCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory) : base(collectionToWrap, factory) { }
	}
}
