using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsDepartureHeaderContainerPhase4ValidationTest : TestCaseWithFactory
	{
		public void TestValidateAllContainers()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONT1";

			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONT2";

			var goodsItem = header.MovementHeader.GoodsItems.AddNew();
			var containerPivot1 = (EU.NCTS.Business.NonPersistentDepartureContainerPivot)goodsItem.ContainersPivots.First(x => ((EU.NCTS.Business.NonPersistentDepartureContainerPivot)x).ContainerNumber == "CONT1");

			CombineAssertions(() =>
			{
				containerPivot1.ContainerSelected = true;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertNoRowWarningContaining(container1, "The container CONT1 is not assigned to any Good Item");
				AssertHasRowWarning(container2, "The container CONT2 is not assigned to any Good Item");

				containerPivot1.ContainerSelected = false;
				container1.Validation.ValidateAll();
				container2.Validation.ValidateAll();

				AssertHasRowWarning(container1, "The container CONT1 is not assigned to any Good Item");
				AssertHasRowWarning(container2, "The container CONT2 is not assigned to any Good Item");
			});
		}
	}
}
