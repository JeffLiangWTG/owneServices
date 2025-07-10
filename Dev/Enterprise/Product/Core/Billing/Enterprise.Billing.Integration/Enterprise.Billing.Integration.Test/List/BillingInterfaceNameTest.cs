using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Billing.Integration.Test
{
	public class BillingInterfaceNameTest : TestCaseWithFactory
	{
		public void TestAllBillingInterfaceNameValid()
		{
			Type type = typeof(BillingInterfaceName);
			foreach (var p in type.GetFields())
			{
				if (p.IsStatic)
				{
					var databaseValue = p.GetValue(null).ToString();
					Assert(!String.IsNullOrEmpty(databaseValue));
					AssertEquals(p.Name, databaseValue.Replace(" ", string.Empty));
				}
			}
		}

		public void TestShouldBillingInterfaceNameSuspendValidation()
		{
			var billingInterfaceName = BillingInterfaceName.ConstructorExposed("databaseValue");
			AssertEquals(billingInterfaceName.ShouldSuspendValidation, true);

			billingInterfaceName = BillingInterfaceName.ConstructorExposed("databaseValue", true);
			AssertEquals(billingInterfaceName.ShouldSuspendValidation, true);

			billingInterfaceName = BillingInterfaceName.ConstructorExposed("databaseValue", false);
			AssertEquals(billingInterfaceName.ShouldSuspendValidation, false);
		}
	}
}
