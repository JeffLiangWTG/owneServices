using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBufferTimespan))]
	class BMBufferTimespanAuditParentTest : AuditParentTest<BMBufferTimespan>
	{
		protected override BMBufferTimespan NewTestAuditParent()
		{
			return Factory.New<BMBufferTimespan>();
		}
	}
}
