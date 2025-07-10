using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget.Testing
{
	[TestedType(typeof(GLBudget))]
	public class GLBudgetAuditParentTest : AuditParentTest<GLBudget>
	{
		protected override GLBudget NewTestAuditParent()
		{
			return Factory.New<GLBudget>();
		}
	}
}
