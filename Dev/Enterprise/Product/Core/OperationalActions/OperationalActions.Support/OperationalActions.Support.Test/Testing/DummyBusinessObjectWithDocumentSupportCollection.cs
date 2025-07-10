using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	public class DummyBusinessObjectWithDocumentSupportCollection : DummyBusinessObjectCollection
	{
		public DummyBusinessObjectWithDocumentSupportCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DummyBusinessObjectWithDocumentSupportCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public new DummyBusinessObjectWithDocumentSupport this[int index]
		{
			get
			{
				return (DummyBusinessObjectWithDocumentSupport)base[index];
			}
		}

		public new DummyBusinessObjectWithDocumentSupport AddNew()
		{
			return (DummyBusinessObjectWithDocumentSupport)base.AddNew();
		}
	}
}
