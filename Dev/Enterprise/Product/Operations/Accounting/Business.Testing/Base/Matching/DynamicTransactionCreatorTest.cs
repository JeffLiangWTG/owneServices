using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public abstract class DynamicTransactionCreatorTest : NonPersistentBusinessObjectTestCase
	{
		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
