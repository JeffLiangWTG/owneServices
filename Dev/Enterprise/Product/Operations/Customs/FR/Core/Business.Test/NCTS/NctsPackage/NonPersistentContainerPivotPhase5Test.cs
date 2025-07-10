using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NonPersistentContainerPivotPhase5))]
	class NonPersistentContainerPivotPhase5Test : NonPersistentBusinessObjectTestCase
	{
		public void TestContainerSelected()
		{
			CombineAssertions(() =>
			{
				var containerPivot = (NonPersistentContainerPivotPhase5)GetNewBusinessObject();
				Factory.Save();

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
				containerPivot.ContainerSelected = false;
				containerPivot.ContainerSelected = true;
				AssertEquals(2, goodItem1.Fees.Count);

				var fee = goodItem1.Fees.FirstOrDefault(x => x.BFE_ChargeType == "V905");
				AssertEquals("Harbour fee should have been recalculated when ContainerSelected has changed - Type", "V905", fee.BFE_ChargeType);
				AssertEquals("Harbour fee should have been recalculated Amount", 2m, fee.BFE_ChargeAmount);

				fee.BFE_RateOverrideReasonCode = "OVR";
				fee.BFE_ChargeAmount = 3m;
				containerPivot.ContainerSelected = false;
				containerPivot.ContainerSelected = true;
				fee = goodItem1.Fees.FirstOrDefault(x => x.BFE_ChargeType == "V905");
				AssertEquals("Harbour fee should have been recalculated when ContainerSelected has changed - Amount", 3m, fee.BFE_ChargeAmount);
				AssertEquals("OVR", fee.BFE_RateOverrideReasonCode);
			});
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject() => CreateNewBusinessObject().ContainerPivot;

		(NctsPackage Package, NonPersistentContainerPivotPhase5 ContainerPivot) CreateNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			var containerPivot = (NonPersistentContainerPivotPhase5)package.ContainersPivotsForBindingOnly[0];
			containerPivot.ContainerSelected = true;
			return (package, containerPivot);
		}
	}
}
