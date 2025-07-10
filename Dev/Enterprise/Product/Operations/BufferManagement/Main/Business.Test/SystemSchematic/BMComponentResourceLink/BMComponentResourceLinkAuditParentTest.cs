using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentResourceLink))]
	class BMComponentResourceLinkAuditParentTest : AuditParentTest<BMComponentResourceLink>
	{
		protected override BMComponentResourceLink NewTestAuditParent()
		{
			return Factory.New<BMComponentResourceLink>();
		}
	}
}
