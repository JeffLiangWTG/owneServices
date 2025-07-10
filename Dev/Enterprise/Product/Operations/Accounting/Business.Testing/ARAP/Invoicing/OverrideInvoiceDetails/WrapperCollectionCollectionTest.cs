using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class WrapperCollectionCollectionTest : TestCaseWithFactory
	{
		public void TestAllowNewCoreFalse()
		{
			var collection = new WrapperCollection<NonPersistentBusinessObject>(Factory);
			AssertEquals("We shouldn't allow new objects when showing overrides to the user", false, collection.AllowNew);
		}
	}
}