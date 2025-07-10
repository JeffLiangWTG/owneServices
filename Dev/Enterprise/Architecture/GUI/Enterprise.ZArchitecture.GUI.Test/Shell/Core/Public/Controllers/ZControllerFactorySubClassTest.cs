using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ZControllerFactorySubClassTest : TestCaseWithDummy
	{
		public void TestGetControllerForTypeOrItsBaseTypes()
		{
			var sub = Factory.New<SubClass>();
			var controller = ZControllerFactory.Instance.GetControllerForBizo(sub);
			AssertNull(controller);

			controller = ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(sub.GetType());
			AssertNotNull(controller);
			AssertEquals(DummyModuleIDs.Dummy, controller.ModuleID);
		}

		class SubClass : DummyEnterpriseBusinessObject
		{
			public SubClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
