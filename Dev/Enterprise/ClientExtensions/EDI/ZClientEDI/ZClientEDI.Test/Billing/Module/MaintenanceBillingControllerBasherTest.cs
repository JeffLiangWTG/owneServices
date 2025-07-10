using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business.Maintenance;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	[TestedType(typeof(MaintenanceBillingController))]
	internal class MaintenanceBillingControllerBasherTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.MaintenanceBilling;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.New<MaintenanceBilling>();
		}
	}
}
