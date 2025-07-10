using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentAcceptabilityBand))]
	public class BMComponentAcceptabilityBandAuditParentTest : AuditParentTest<BMComponentAcceptabilityBand>
	{
		protected override BMComponentAcceptabilityBand NewTestAuditParent()
		{
			return Factory.New<BMComponentAcceptabilityBand>();
		}
	}
}
