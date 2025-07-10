using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBusinessObjectWithMultilingualStringPropertyCollection : DummyBusinessObjectCollection
	{
		public DummyBusinessObjectWithMultilingualStringPropertyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DummyBusinessObjectWithMultilingualStringProperty);
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			return Factory.New<DummyBusinessObjectWithMultilingualStringProperty>();
		}
	}
}
