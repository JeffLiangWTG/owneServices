using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentLink))]
	class BMComponentLinkAuditParentTest : AuditParentTest<BMComponentLink>
	{
		protected override BMComponentLink NewTestAuditParent()
		{
			return Factory.New<BMComponentLink>();
		}
	}
}
