using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

sealed class NctsPackagePhase5ValidationTest : TestCaseWithFactory
{
	public void TestCheckRuleC0670_SingleContainer()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.DepartureHeaderContainers.AddNew();
		var goodsItems = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var nctsPackage = goodsItems.Packages.AddNew();

		var pivot = nctsPackage.ContainersPivotsForBindingOnly[0];

		CombineAssertions(() =>
		{
			pivot.ContainerSelected = true;
			nctsPackage.Validation.ValidateAll();
			AssertNoRowMessageError("No error should be shown if at least one container is selected.", nctsPackage, RuleC0670MessageError);

			pivot.ContainerSelected = false;
			nctsPackage.Validation.ValidateAll();
			AssertHasRowMessageError("An error should be shown if no container is selected.", nctsPackage, RuleC0670MessageError);
		});
	}

	public void TestCheckRuleC0670_MultipleContainersOnSinglePackage()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		nctsHeader.DepartureHeaderContainers.AddNew();
		nctsHeader.DepartureHeaderContainers.AddNew();
		var goodsItems = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var nctsPackage = goodsItems.Packages.AddNew();

		var pivot1 = nctsPackage.ContainersPivotsForBindingOnly[0];
		var pivot2 = nctsPackage.ContainersPivotsForBindingOnly[1];

		CombineAssertions(() =>
		{
			pivot1.ContainerSelected = true;
			pivot2.ContainerSelected = false;
			nctsPackage.Validation.ValidateAll();
			AssertNoRowMessageError("No error should be shown if at least one container is selected.", nctsPackage, RuleC0670MessageError);

			pivot1.ContainerSelected = false;
			pivot2.ContainerSelected = false;
			nctsPackage.Validation.ValidateAll();
			AssertHasRowMessageError("An error should be shown if no container is selected.", nctsPackage, RuleC0670MessageError);
		});
	}

	const string RuleC0670MessageError = "[C0670] You have not selected at least one Container Number for this package.";
}
