using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2AmendedItemCollection : NonPersistentBusinessObjectCollection<ICS2AmendedItem>
	{
		public ICS2AmendedItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
