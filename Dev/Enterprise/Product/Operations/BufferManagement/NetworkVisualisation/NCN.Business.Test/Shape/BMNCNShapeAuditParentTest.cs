using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNShape))]
	public class BMNCNShapeAuditParentTest : AuditParentTest<BMNCNShape>
	{
		protected override BMNCNShape NewTestAuditParent()
		{
			return Factory.New<BMNCNShape>();
		}
	}
}
