using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureHeaderContainerPhase5ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAtLeastOneGoodsItemIsLinkedToThisContainer()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
		headerContainer.BC_ContainerNum = "XYZ";
		headerContainer.Validation.ValidateAll();
		AssertHasRowMessageError("When header container is entered but there are no goods items", headerContainer, ValidationCaptions.NctsHeaderContainer.NoGoodsItemIsLinkedToThisContainer);

		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		var package = goodsItem.Packages.AddNew();
		package.ContainersPivot.AddPivotFor(headerContainer);
		package.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
		headerContainer.Validation.ValidateAll();
		AssertNoRowMessageError("When header container is linked to a goods item", headerContainer, ValidationCaptions.NctsHeaderContainer.NoGoodsItemIsLinkedToThisContainer);

		package.ContainersPivotsForBindingOnly[0].ContainerSelected = false;
		headerContainer.Validation.ValidateAll();
		AssertHasRowMessageError("When header container is unlinked from a goods item", headerContainer, ValidationCaptions.NctsHeaderContainer.NoGoodsItemIsLinkedToThisContainer);
	}
}
