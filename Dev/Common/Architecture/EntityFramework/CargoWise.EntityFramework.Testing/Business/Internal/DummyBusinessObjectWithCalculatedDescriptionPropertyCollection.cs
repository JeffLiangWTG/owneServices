using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBusinessObjectWithCalculatedDescriptionPropertyCollection : DummyBusinessObjectCollection
	{
		public DummyBusinessObjectWithCalculatedDescriptionPropertyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DummyBusinessObjectWithCalculatedDescriptionProperty);
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			return Factory.New<DummyBusinessObjectWithCalculatedDescriptionProperty>();
		}
	}
}
