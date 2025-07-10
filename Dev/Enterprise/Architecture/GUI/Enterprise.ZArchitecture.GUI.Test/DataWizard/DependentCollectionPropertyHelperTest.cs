using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Integration;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DependentCollectionPropertyHelperTest : TestCaseWithFactory
	{
		public void TestGetDependentCollection()
		{
			var helper = new DependentCollectionPropertyHelper();
			AssertNull(helper.GetDependentCollection(null, null, null));

			var bizO = Factory.New<IHVLVConsignment>();
			var collection = helper.GetDependentCollection((BusinessObject)bizO, ObjectFactory.GetType<IHVLVItem>(), ObjectFactory.GetType<IHVLVConsignment>());
			AssertNotNull(collection);
		}
	}
}
