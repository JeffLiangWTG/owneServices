using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagDefinition))]
	public class TagDefinitionAuditParentTest : AuditParentTest<TagDefinition>
	{
		protected override TagDefinition NewTestAuditParent()
		{
			return Factory.New<TagDefinition>();
		}
	}
}
