using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Module.Test;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Module.Test
{
	[TestedType(typeof(NetworkDiagramController))]
	class NetworkDiagramControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.NetworkDiagram;
		}

		public void TestModuleId()
		{
			AssertEquals(ModuleIDs.NetworkDiagram, Controller.ModuleID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
