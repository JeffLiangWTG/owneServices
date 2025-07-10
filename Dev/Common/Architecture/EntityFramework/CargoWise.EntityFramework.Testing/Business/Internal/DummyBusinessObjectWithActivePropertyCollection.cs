using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class DummyBusinessObjectWithActivePropertyCollection : DummyBusinessObjectCollection
	{
		public DummyBusinessObjectWithActivePropertyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(DummyBusinessObjectWithActiveProperty);
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			return Factory.New<DummyBusinessObjectWithActiveProperty>();
		}
	}
}
