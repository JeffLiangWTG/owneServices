using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBusinessObjectWithCalculatedCodePropertyCollection : DummyBusinessObjectCollection
	{
		public DummyBusinessObjectWithCalculatedCodePropertyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DummyBusinessObjectWithCalculatedCodeProperty);
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			return Factory.New<DummyBusinessObjectWithCalculatedCodeProperty>();
		}
	}
}
