using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class FRNctsDepartureHeaderContainerValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingBC_RC()
		{
			var movementHeader = PrepareCalculationData();
			movementHeader.ChargePaymentOrDestinationID = "395";
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);

			var goodItem1 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 1);
			var goodItem2 = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().FirstOrDefault(x => x.BY_LineNo == 2);

			AssertEquals(0, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);

			var headerContainer1 = movementHeader.Header.DepartureHeaderContainers.Cast<FRNctsDepartureHeaderContainer>().FirstOrDefault(x => x.BC_Mode == "FCL");
			var headerContainer2 = movementHeader.Header.DepartureHeaderContainers.Cast<FRNctsDepartureHeaderContainer>().FirstOrDefault(x => x.BC_Mode == "LCL");

			var refContainerFCL1 = Factory.New<RefContainer>();
			refContainerFCL1.RC_ContainerType = "20";
			refContainerFCL1.RC_StorageClass = "20";
			refContainerFCL1.RC_Code = "C20FCL";
			headerContainer1.BC_RC = refContainerFCL1.PK;

			var refContainerLCL1 = Factory.New<RefContainer>();
			refContainerLCL1.RC_ContainerType = "21";
			refContainerLCL1.RC_StorageClass = "20";
			refContainerLCL1.RC_Code = "C20LCL";
			headerContainer1.BC_RC = refContainerLCL1.PK;

			AssertEquals(1, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);
			var fee = goodItem1.Fees.FirstOrDefault();
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(8m, fee.BFE_ChargeAmount);
		}

		public void TestSettingBC_Mode()
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

			var headerContainer1 = movementHeader.Header.DepartureHeaderContainers.Cast<FRNctsDepartureHeaderContainer>().FirstOrDefault(x => x.BC_Mode == "FCL");
			headerContainer1.BC_Mode = "LCL";
			AssertEquals(0, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);
		}

		NctsDepartureMovementHeader PrepareCalculationData()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "108", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[TFCL] * 1.1341", "FR");
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "[20FCL] * 7.9077", "FR");
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
	}
}
