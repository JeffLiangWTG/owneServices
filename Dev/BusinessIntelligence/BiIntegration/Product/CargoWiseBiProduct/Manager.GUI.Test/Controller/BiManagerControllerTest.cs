using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Controller.Testing
{
	[TestedType(typeof(BiManagerController))]
	public class BiManagerControllerTest : ZSingletonControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(null, new BiManagerController().ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BiManager;
		}
	}
}
