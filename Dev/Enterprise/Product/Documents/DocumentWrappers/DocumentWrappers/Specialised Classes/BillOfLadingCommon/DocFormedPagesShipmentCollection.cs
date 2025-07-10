using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers
{
	public class DocFormedPagesShipmentCollection : NonPersistentBusinessObjectCollection<DocFormedPagesShipment>
	{
		internal DocFormedPagesShipmentCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
