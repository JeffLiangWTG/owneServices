using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	public class DocumentWrapperCollectionWithCustomPropertiesForTest : BusinessObjectCollection<DummyBOForTest>
	{
		public DocumentWrapperCollectionWithCustomPropertiesForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DocumentWrapperCollectionWithCustomPropertiesForTest(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		public ZString CustomZStringInCollection1 { get; set; }
		public ZString CustomZStringInCollection2 { get; set; }

		public DocumentWrapperCollectionWithCustomPropertiesForTest SelfCollection { get; set; }
	}
}
