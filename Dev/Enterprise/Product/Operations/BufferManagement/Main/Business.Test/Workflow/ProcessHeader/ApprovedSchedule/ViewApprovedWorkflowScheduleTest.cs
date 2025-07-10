using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ViewApprovedWorkflowSchedule))]
	class ViewApprovedWorkflowScheduleTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			// This bizo is for a db View, so can't save or delete
		}
	}
}
