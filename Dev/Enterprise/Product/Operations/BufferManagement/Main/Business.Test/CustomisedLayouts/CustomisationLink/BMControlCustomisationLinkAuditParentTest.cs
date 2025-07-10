using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisationLink))]
	public class BMControlCustomisationLinkAuditParentTest : AuditParentTest<BMControlCustomisationLink>
	{
		protected override BMControlCustomisationLink NewTestAuditParent()
		{
			return Factory.New<BMControlCustomisationLink>();
		}
	}
}
