using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagRule))]
	public class TagRuleAuditParentTest : AuditParentTest<TagRule>
	{
		protected override TagRule NewTestAuditParent()
		{
			return Factory.New<TagRule>();
		}
	}
}
