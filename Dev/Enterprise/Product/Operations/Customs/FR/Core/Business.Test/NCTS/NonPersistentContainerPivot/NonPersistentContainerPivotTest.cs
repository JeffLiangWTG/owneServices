using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NonPersistentDepartureContainerPivot))]
	public class NonPersistentContainerPivotTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSettingContainerSelected()
		{
			var movementHeader = PrepareCalculationData();
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);

			var goodItem1 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 1);
			var goodItem2 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 2);

			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);

			var fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(2m, fee.BFE_ChargeAmount);

			var containersPivot1 = goodItem1.ContainersPivots.Cast<NonPersistentDepartureContainerPivot>().FirstOrDefault(x => x.ContainerNumber == "1");
			containersPivot1.ContainerSelected = false;
			AssertEquals(0, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);
		}

		NctsDepartureMovementHeader PrepareCalculationData()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 1.1341", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 2.1341", "FR");
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var headerContainer1 = header.DepartureHeaderContainers.AddNew();
			headerContainer1.BC_Mode = "FCL";
			var headerContainer2 = header.DepartureHeaderContainers.AddNew();
			headerContainer2.BC_Mode = "LCL";

			var movementHeader = header.MovementHeader;
			var goodItem1 = movementHeader.GoodsItems.AddNew();
			goodItem1.BY_LineNo = 1;
			goodItem1.BY_GrossWeight = 2000m;
			goodItem1.BY_GrossWeightUnit = "KG";
			var containersPivot = goodItem1.ContainersPivots.AddNew();
			containersPivot.Container = headerContainer1;
			containersPivot.ContainerSelected = true;
			containersPivot.ContainerNumber = "1";
			var goodItem2 = movementHeader.GoodsItems.AddNew();
			goodItem2.BY_LineNo = 2;
			goodItem2.BY_GrossWeight = 3000m;
			goodItem2.BY_GrossWeightUnit = "KG";
			var containersPivot2 = goodItem1.ContainersPivots.AddNew();
			containersPivot2.Container = headerContainer2;
			containersPivot2.ContainerSelected = true;
			containersPivot2.ContainerNumber = "2";

			movementHeader.ChargePaymentOrDestinationID = "108";

			Factory.Save();
			return movementHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var goodItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_Mode = "FCL";

			var containersPivot = goodItem.ContainersPivots.AddNew();
			containersPivot.Container = headerContainer;
			return containersPivot;
		}
	}
}
