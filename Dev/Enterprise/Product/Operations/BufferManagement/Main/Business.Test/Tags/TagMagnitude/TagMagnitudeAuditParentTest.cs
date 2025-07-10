using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagMagnitude))]
	public class TagMagnitudeAuditParentTest : AuditParentTest<TagMagnitude>
	{
		protected override TagMagnitude NewTestAuditParent()
		{
			return Factory.New<TagMagnitude>();
		}
	}
}
