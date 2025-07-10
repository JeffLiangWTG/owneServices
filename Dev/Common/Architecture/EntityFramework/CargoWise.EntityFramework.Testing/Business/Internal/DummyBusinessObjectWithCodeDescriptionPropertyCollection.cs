using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBusinessObjectWithCodeDescriptionPropertyCollection : DummyBusinessObjectCollection
	{
		public DummyBusinessObjectWithCodeDescriptionPropertyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DummyBusinessObjectWithCodeDescriptionProperty);
		}

		internal override BusinessObject CreateNewBusinessObject()
		{
			return Factory.New<DummyBusinessObjectWithCodeDescriptionProperty>();
		}
	}
}
