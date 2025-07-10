using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(DummyNonPersistentBusinessObjectWithoutAttribute))]
	sealed class DummyNonPersistentBusinessObjectWithoutAttributeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DummyNonPersistentBusinessObjectWithoutAttribute();
		}
	}
}
