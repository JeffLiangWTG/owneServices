using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class HarbourFeeDepartureMovementCalculationManagerTest : TestCaseWithFactory
	{
		public void TestCalculate()
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

			var vatFeeOrigin = goodItem1.Fees.AddNew();
			vatFeeOrigin.BFE_ChargeType = Customs.Business.ChargeTypesList.Codes.VAT;
			vatFeeOrigin.BFE_ChargeAmount = 5m;

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
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);
			AssertEquals(2, goodItem1.Fees.Count);
			AssertEquals(0, goodItem2.Fees.Count);

			var feeVat = goodItem1.Fees[0];
			AssertEquals("VAT", feeVat.BFE_ChargeType);
			AssertEquals(5m, feeVat.BFE_ChargeAmount);

			var fee = goodItem1.Fees[1];
			AssertEquals("V905", fee.BFE_ChargeType);
			AssertEquals(2m, fee.BFE_ChargeAmount);

			fee.BFE_RateOverrideReasonCode = "OVR";
			fee.BFE_ChargeAmount = 3m;
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);
			fee = goodItem1.Fees.FirstOrDefault(x => x.BFE_ChargeType == "V905");
			AssertEquals(3m, fee.BFE_ChargeAmount);
			AssertEquals("OVR", fee.BFE_RateOverrideReasonCode);

			fee.BFE_RateOverrideReasonCode = "";
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);
			fee = goodItem1.Fees.FirstOrDefault(x => x.BFE_ChargeType == "V905");
			AssertEquals(2m, fee.BFE_ChargeAmount);

			fee.BFE_ChargeType = "";
			fee.BFE_RateOverrideReasonCode = "ADD";
			fee.BFE_ChargeAmount = 1m;
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);
			AssertEquals(3, goodItem1.Fees.Count);
			fee = goodItem1.Fees[1];
			AssertEquals(1m, fee.BFE_ChargeAmount);
			AssertEquals("ADD", fee.BFE_RateOverrideReasonCode);
			var fee2 = goodItem1.Fees.FirstOrDefault(x => x.BFE_ChargeType == "V905");
			AssertEquals(2m, fee2.BFE_ChargeAmount);

			fee.BFE_RateOverrideReasonCode = "";
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);
			fee = goodItem1.Fees.FirstOrDefault(x => x.BFE_ChargeType == "V905");
			AssertEquals(2m, fee.BFE_ChargeAmount);

			movementHeader.ChargePaymentOrDestinationID = "";
			Factory.Save();
			new HarbourFeeDepartureMovementCalculationManager(Factory).Calculate(movementHeader);
			AssertEquals(2, goodItem1.Fees.Count);
			AssertEquals(false, goodItem1.Fees.Any(x => new HarbourFeeCodes().GetAllCodesZString().Contains(x.BFE_ChargeType)));
			AssertEquals(0, goodItem2.Fees.Count);
		}
	}
}
