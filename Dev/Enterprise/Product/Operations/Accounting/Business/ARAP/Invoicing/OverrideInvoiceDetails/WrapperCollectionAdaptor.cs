using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class WrapperCollectionAdaptor<T> : NonPersistentBusinessObject, IObsoleteValidation where T : NonPersistentBusinessObject
	{
		readonly ZGuid[] pks;
		WrapperCollection<T> wrapperCollection;

		public WrapperCollectionAdaptor(BusinessObjectFactory factory, params ZGuid[] pks)
			: base(factory)
		{
			this.pks = pks;
		}

		public WrapperCollection<T> WrappedObjects
		{
			get
			{
				if (wrapperCollection == null)
				{
					wrapperCollection = new WrapperCollection<T>(Factory);
					var loadedBizOs = LoadInnerObjects(pks);
					foreach (var bizO in loadedBizOs)
					{
						wrapperCollection.Add(Wrap(bizO));
					}

					RegisterEditableChildObject(wrapperCollection);
				}

				return wrapperCollection;
			}
		}

		public abstract Type InnerObjectType { get; }
		protected abstract BusinessObject[] LoadInnerObjects(ZGuid[] pks);
		protected abstract T Wrap(BusinessObject bizo);
	}
}