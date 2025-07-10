using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(DummyChildEnterpriseBusinessObjectCollection))]
	sealed class DummyChildBusinessObject2CollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DummyChildEnterpriseBusinessObjectCollection(Factory);
		}
	}
}
