using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(DummyIOrgHeaderBusinessObjectCollection))]
	sealed class DummyIOrgHeaderBusinessObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DummyIOrgHeaderBusinessObjectCollection(Factory);
		}
	}
}
