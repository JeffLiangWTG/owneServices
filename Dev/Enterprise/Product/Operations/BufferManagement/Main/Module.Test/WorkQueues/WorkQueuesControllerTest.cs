using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(WorkQueuesController))]
	class WorkQueuesControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WorkQueues;
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
