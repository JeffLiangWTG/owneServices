using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoard))]
	public class BMBoardAuditParentTest : AuditParentTest<BMBoard>
	{
		protected override BMBoard NewTestAuditParent()
		{
			return Factory.New<BMBoard>();
		}
	}
}
