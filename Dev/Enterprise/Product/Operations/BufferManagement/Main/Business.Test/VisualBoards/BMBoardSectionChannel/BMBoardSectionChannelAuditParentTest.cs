using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSectionChannel))]
	public class BMBoardSectionChannelAuditParentTest : AuditParentTest<BMBoardSectionChannel>
	{
		protected override BMBoardSectionChannel NewTestAuditParent()
		{
			return Factory.New<BMBoardSectionChannel>();
		}
	}
}
