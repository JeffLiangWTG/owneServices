using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CASSBillingController))]
	public class CASSBillingControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CASSCostFileImport;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new CASSBilling(Factory);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(CASSBilling);
		}
	}
}
