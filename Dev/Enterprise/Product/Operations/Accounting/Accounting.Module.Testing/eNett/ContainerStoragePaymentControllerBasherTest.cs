using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.Business.eNett_Integration.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ContainerStoragePaymentController))]
	internal class ContainerStoragePaymentControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ContainerStoragePayment;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new StorageFeeInvoicePayment(new MockContainerStorageDataProvider());
		}
	}
}
