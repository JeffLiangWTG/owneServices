using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXP_CXN_Container()
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var package = header.CusExitConsignmentPackages.AddNew();
			package.CXP_Sequence = 1;
			var consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			var pivot = consignmentItem.CusExitConsignmentPivots.AddNew();

			var info = pivot.CNP_CXN_ContainerInfo;
			AssertNoNotifications(info);
			var container1 = header.CusExitContainers.AddNew();

			pivot.CNP_CXP_Package = package.PK;
			const string messageError = "It is mandatory to associate a package to its container/equipment. If a package is not in a container, but in a transport equipment, then it is also mandatory to associate the package.";

			container1.CXN_IsEquipment = ZBool.True;
			container1.CXN_ContainerNumber = "xy";
			pivot.Validation.ValidateCNP_CXN_Container();
			AssertHasMessageError(info, messageError);

			pivot.CNP_CXN_Container = container1.PK;
			AssertNoMessageError(info, messageError);
		}
	}
}
