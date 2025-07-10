using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	class BaseDummyBusinessObjectForRowFinderTest : DummyBusinessObject
	{
		public BaseDummyBusinessObjectForRowFinderTest(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public new DummyChildBusinessObjectWithMultilingualCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new DummyChildBusinessObjectWithMultilingualCollection(Factory);
				}
				return collection;
			}
			set => collection = value;
		}
		DummyChildBusinessObjectWithMultilingualCollection collection;
	}
}
