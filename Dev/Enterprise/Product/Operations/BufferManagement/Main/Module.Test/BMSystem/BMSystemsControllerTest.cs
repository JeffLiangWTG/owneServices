using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMSystemsController))]
	class BMSystemsControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMSystems;
		}
	}
}
