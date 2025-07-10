using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalAction))]
	sealed class OperationalActionAuditParentTest : AuditParentTest<OperationalAction>
	{
		protected override OperationalAction NewTestAuditParent()
		{
			return Factory.New<OperationalAction>();
		}
	}
}
