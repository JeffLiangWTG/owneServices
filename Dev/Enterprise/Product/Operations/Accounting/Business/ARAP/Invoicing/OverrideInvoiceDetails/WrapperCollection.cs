using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class WrapperCollection<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject
	{
		public WrapperCollection(BusinessObjectFactory factory) : base(factory) { }

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException("Allow new is false so shouldn't get called");
		}
	}
}
