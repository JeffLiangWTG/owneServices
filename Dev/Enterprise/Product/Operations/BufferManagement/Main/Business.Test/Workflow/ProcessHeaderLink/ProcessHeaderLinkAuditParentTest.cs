using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeaderLink))]
	public class ProcessHeaderLinkAuditParentTest : AuditParentTest<ProcessHeaderLink>
	{
		protected override ProcessHeaderLink NewTestAuditParent()
		{
			return Factory.New<ProcessHeaderLink>();
		}
	}
}
