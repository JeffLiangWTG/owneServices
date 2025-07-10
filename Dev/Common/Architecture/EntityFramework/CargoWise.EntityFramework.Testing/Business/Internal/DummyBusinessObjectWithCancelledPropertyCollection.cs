using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class DummyBusinessObjectWithCancelledPropertyCollection : DummyBusinessObjectCollection
	{
		public DummyBusinessObjectWithCancelledPropertyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(DummyBusinessObjectWithCancelledProperty);
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			return Factory.New<DummyBusinessObjectWithCancelledProperty>();
		}
	}
}
