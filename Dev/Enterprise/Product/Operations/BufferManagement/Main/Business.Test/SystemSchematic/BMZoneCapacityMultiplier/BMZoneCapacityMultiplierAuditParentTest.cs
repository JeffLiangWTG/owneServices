using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMZoneCapacityMultiplier))]
	class BMZoneCapacityMultiplierAuditParentTest : AuditParentTest<BMZoneCapacityMultiplier>
	{
		protected override BMZoneCapacityMultiplier NewTestAuditParent()
		{
			return Factory.New<BMZoneCapacityMultiplier>();
		}
	}
}
