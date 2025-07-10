using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class GoodsReferenceDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		CombineAssertions(() =>
		{
			AssertNull("null", GoodsReferenceDataProvider.NewCollection(null));

			AssertNotNull("HeaderContainer != null", GoodsReferenceDataProvider.NewCollection(NctsHeader.DepartureHeaderContainers.AddNew()));
		});
	}

	public void TestProvider()
	{
		var headerContainer = NctsHeader.DepartureHeaderContainers.AddNew();
		var bill = NctsHeader.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		var goodsItem2 = bill.GoodsItems.AddNew();
		var goodsItem3 = bill.GoodsItems.AddNew();
		var package11 = goodsItem1.Packages.AddNew();
		package11.ContainersPivot.AddPivotFor(headerContainer);
		var package12 = goodsItem1.Packages.AddNew();
		package12.ContainersPivot.AddPivotFor(headerContainer);
		var package21 = goodsItem2.Packages.AddNew();
		package21.ContainersPivot.AddPivotFor(headerContainer);
		goodsItem1.BY_DeclarationGoodsItemNumber = 123;

		var goodsReferencesDataProvider = GoodsReferenceDataProvider.NewCollection(headerContainer).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("Sequence Number", 1, goodsReferencesDataProvider.ElementAt(0).SequenceNumber);
			AssertEquals("Declaration Goods Item Number", 123, goodsReferencesDataProvider.ElementAt(0).DeclarationGoodsItemNumber);
		});
	}

	NctsHeader CreateNctsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;
}
