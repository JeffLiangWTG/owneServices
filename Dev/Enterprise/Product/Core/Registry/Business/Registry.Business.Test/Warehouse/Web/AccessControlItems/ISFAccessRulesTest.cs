using Enterprise.Registry.Business.Web;

namespace Enterprise.Registry.Business.Testing
{
	sealed class ISFAccessRulesTest : AccessRulesBaseTest
	{
		protected override AccessRulesBase GetTestRules()
		{
			return new ISFAccessRules();
		}
	}
}
