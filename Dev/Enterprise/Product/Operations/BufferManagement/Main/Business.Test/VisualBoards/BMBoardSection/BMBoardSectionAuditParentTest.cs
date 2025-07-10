using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSection))]
	public class BMBoardSectionAuditParentTest : AuditParentTest<BMBoardSection>
	{
		protected override BMBoardSection NewTestAuditParent()
		{
			return Factory.New<BMBoardSection>();
		}
	}
}
