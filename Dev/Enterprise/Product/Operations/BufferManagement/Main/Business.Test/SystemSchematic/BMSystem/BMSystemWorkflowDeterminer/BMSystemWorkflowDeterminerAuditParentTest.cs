using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystemWorkflowDeterminer))]
	class BMSystemWorkflowDeterminerAuditParentTest : AuditParentTest<BMSystemWorkflowDeterminer>
	{
		protected override BMSystemWorkflowDeterminer NewTestAuditParent()
		{
			return Factory.New<BMSystemWorkflowDeterminer>();
		}
	}
}
