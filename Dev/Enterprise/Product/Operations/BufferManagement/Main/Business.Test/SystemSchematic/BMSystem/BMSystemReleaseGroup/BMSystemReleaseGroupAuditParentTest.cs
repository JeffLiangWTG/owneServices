using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystemReleaseGroup))]
	class BMSystemReleaseGroupAuditParentTest : AuditParentTest<BMSystemReleaseGroup>
	{
		protected override BMSystemReleaseGroup NewTestAuditParent()
		{
			return Factory.New<BMSystemReleaseGroup>();
		}
	}
}
