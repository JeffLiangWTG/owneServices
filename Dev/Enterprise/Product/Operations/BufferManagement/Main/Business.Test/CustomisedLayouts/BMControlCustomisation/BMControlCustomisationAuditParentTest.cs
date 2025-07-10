using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMControlCustomisation))]
	public class BMControlCustomisationAuditParentTest : AuditParentTest<BMControlCustomisation>
	{
		protected override BMControlCustomisation NewTestAuditParent()
		{
			return Factory.New<BMControlCustomisation>();
		}
	}
}
