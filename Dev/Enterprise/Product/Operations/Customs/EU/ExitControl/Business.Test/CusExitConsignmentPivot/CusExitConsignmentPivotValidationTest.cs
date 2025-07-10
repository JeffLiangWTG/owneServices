using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXP_CXN_Container()
		{
			(var pivot, _, _, var header) = CusExitConsignmentPivotTest.GetNewBusinessObject(Factory);
			pivot.CNP_CXN_Container = ZGuid.Empty;
			var info = pivot.CNP_CXN_ContainerInfo;
			AssertNoNotifications(info);
			var container1 = header.CusExitContainers.AddNew();
			var container2 = header.CusExitContainers.AddNew();
			pivot.Validation.ValidateCNP_CXN_Container();
			var messageError = "It is mandatory to associate a package to its container. If a package is not in a container but in a conveyance and the conveyance is sealed, then it is also mandatory to associate the package to the correct conveyance.";
			AssertHasMessageError(info, messageError);
			AssertNoWarnings(info);
			container1.CXN_IsEquipment = ZBool.True;
			pivot.Validation.ValidateCNP_CXN_Container();
			AssertNoMessageErrors(info);
			AssertHasWarning(info, messageError);
			pivot.CNP_CXN_Container = container2.PK;
			AssertNoMessageErrors(info);
			AssertNoWarnings(info);
		}
	}
}
