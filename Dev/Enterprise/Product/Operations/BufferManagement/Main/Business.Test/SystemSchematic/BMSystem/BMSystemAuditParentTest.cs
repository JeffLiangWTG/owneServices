using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystem))]
	class BMSystemAuditParentTest : AuditParentTest<BMSystem>
	{
		protected override BMSystem NewTestAuditParent()
		{
			return Factory.New<BMSystem>();
		}
	}
}
