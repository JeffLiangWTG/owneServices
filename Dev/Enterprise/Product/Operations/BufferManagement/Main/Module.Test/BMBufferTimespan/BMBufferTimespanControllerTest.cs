using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMBufferTimespanController))]
	class BMBufferTimespanControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.BMBufferTimespan;

		#region ModuleID

		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.BMBufferTimespan, new BMBufferTimespanController().ModuleID);
		}

		#endregion
	}
}
