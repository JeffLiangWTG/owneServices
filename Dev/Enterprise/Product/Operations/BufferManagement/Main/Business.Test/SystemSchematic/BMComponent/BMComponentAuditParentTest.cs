using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponent))]
	class BMComponentAuditParentTest : AuditParentTest<BMComponent>
	{
		protected override BMComponent NewTestAuditParent()
		{
			return Factory.New<BMComponent>();
		}
	}
}
