using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentReleaseGroupLink))]
	class BMComponentReleaseGroupLinkAuditParentTest : AuditParentTest<BMComponentReleaseGroupLink>
	{
		protected override BMComponentReleaseGroupLink NewTestAuditParent()
		{
			return Factory.New<BMComponentReleaseGroupLink>();
		}
	}
}
