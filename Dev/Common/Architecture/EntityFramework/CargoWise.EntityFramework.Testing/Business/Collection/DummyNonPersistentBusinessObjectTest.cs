using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(DummyNonPersistentBusinessObject))]
	sealed class DummyNonPersistentBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DummyNonPersistentBusinessObject();
		}
	}
}
