using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSectionAdditionalComponent))]
	public class BMBoardSectionAdditionalComponentAuditParentTest : AuditParentTest<BMBoardSectionAdditionalComponent>
	{
		protected override BMBoardSectionAdditionalComponent NewTestAuditParent()
		{
			return Factory.New<BMBoardSectionAdditionalComponent>();
		}
	}
}
