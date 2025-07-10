using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	public class CusInBondContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBC_ContainerNumInList()
		{
			var dec = Factory.New<JobDeclaration>();
			var container1 = dec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C01";
			var container2 = dec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C02";
			var moveDetail = dec.CusEntryInstruction.CusInBondPermitsHeaders.AddNew().FirstMoveDetail;
			var inBondContianer = moveDetail.Containers.AddNew();
			inBondContianer.BC_ContainerNum = "C03";
			AssertHasWarningContaining("Contianer Number not in list", inBondContianer.BC_ContainerNumInfo, "You have not entered a valid code");
			inBondContianer.BC_ContainerNum = "C01";
			AssertNoWarningContaining("Contianer Number in list", inBondContianer.BC_ContainerNumInfo, "You have not entered a valid code");
			var inBondContianer2 = moveDetail.Containers.AddNew();
			inBondContianer2.BC_ContainerNum = "C01";
			AssertHasMessageErrorContaining("Same number has been added multiple times.", inBondContianer2.BC_ContainerNumInfo, "Same number has been added multiple times.");
			inBondContianer2.BC_ContainerNum = "C02";
			AssertNoMessageErrorContaining("Same number has not been added multiple times.", inBondContianer2.BC_ContainerNumInfo, "Same number has been added multiple times.");
		}

		public void TestBC_ContainerNumMandatory()
		{
			var inBondContianer = Factory.New<CusInBondContainer>();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inBondContianer.BC_ContainerNumInfo);
		}
	}
}
