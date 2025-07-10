using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class CustomDummyChildBusinessObjectCollection : DummyChildBusinessObjectCollection
	{
		public CustomDummyChildBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new CustomDummyChildBusinessObject this[int i]
		{
			get { return (CustomDummyChildBusinessObject)base[i]; }
		}

		public new CustomDummyChildBusinessObject AddNew()
		{
			return (CustomDummyChildBusinessObject)base.AddNew();
		}
	}
}
