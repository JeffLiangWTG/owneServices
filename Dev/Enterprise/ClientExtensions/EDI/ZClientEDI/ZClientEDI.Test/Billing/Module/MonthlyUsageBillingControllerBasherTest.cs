using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	[TestedType(typeof(MonthlyUsageBillingController))]
	internal class MonthlyUsageBillingControllerBasherTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.MonthlyUsageBilling;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.New<MonthlyUsageBilling>();
		}
	}
}
