using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	sealed class UnloadedGoodsItemWrapperTest : Customs.Business.Testing.DataProviderTestCase<UnloadedGoodsItemWrapper>
	{
		public void TestItemNumber()
		{
			AssertEquals(1, wrapper.ItemNumber);
			goodItem.BY_LineNo = 10;
			AssertEquals(10, wrapper.ItemNumber);
		}

		public void TestCommodityCode()
		{
			AssertEquals(ZString.Empty, wrapper.CommodityCode);
			goodItem.BY_HarmonisedTariff = "Test";
			AssertEquals("Test", wrapper.CommodityCode);
		}

		public void TestGoodsDescription()
		{
			AssertEquals(ZString.Empty, wrapper.GoodsDescription);
			goodItem.BY_Description = "Description";
			AssertEquals("Description", wrapper.GoodsDescription);
		}

		public void TestGrossWeight()
		{
			AssertEquals(ZDecimal.Zero, wrapper.GrossWeight);
			goodItem.BY_GrossWeight = 10m;
			AssertEquals(10m, wrapper.GrossWeight);
		}

		public void TestNetWeight()
		{
			AssertEquals(ZDecimal.Zero, wrapper.NetWeight);
			goodItem.BY_NetWeight = 10m;
			AssertEquals(10m, wrapper.NetWeight);
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, wrapper.SupportingDocuments.Count);
			var supportingDocument = goodItem.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "TST";
			wrapper = UnloadedGoodsItemWrapper.New(goodItem);
			AssertEquals(1, wrapper.SupportingDocuments.Count);
			AssertType<FR.Business.MessagesWrappers.Common.DocumentWrapper>(wrapper.SupportingDocuments.FirstOrDefault());
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TST" }, wrapper.SupportingDocuments.Select(x => x.Code));
		}

		public void TestControlResults()
		{
			AssertEquals(1, wrapper.ControlResults.Count);
			var controlResult = goodItem.ResultsOfControlCollection.AddNew();
			controlResult.Data.G9_CorrectedValue = "TST";
			wrapper = UnloadedGoodsItemWrapper.New(goodItem);
			AssertEquals(2, wrapper.ControlResults.Count);
			AssertType<EU.NCTS.Business.ControlResultWrapper>(wrapper.ControlResults.FirstOrDefault());
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1", "TST" }, wrapper.ControlResults.Select(x => x.CorrectedValue));
		}

		public void TestContainers()
		{
			AssertEquals(0, wrapper.Containers.Count);
			var container = goodItem.Containers.AddNew();
			container.ContainerNumber = "123456";
			wrapper = UnloadedGoodsItemWrapper.New(goodItem);
			AssertEquals(1, wrapper.Containers.Count);
			AssertType<ZString>(wrapper.Containers.FirstOrDefault());
			AssertContainsExactElementsInAnyOrder(new ZString[] { "123456" }, wrapper.Containers);
		}

		public void TestPackages()
		{
			AssertEquals(0, wrapper.Packages.Count);
			var package = goodItem.Packages.AddNew();
			package.B5_UnitCount = 1;
			wrapper = UnloadedGoodsItemWrapper.New(goodItem);
			AssertEquals(1, wrapper.Packages.Count);
			AssertType<EU.NCTS.Business.PackageWrapper>(wrapper.Packages.FirstOrDefault());
			AssertContainsExactElementsInAnyOrder(new ZLong[] { 1 }, wrapper.Packages.Select(x => x.NumberOfPackages));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			goodItem = header.UnloadingMovementHeader.GoodsItems.AddNew();
			wrapper = UnloadedGoodsItemWrapper.New(goodItem);
		}

		protected override UnloadedGoodsItemWrapper GetProvider() => (UnloadedGoodsItemWrapper)wrapper;

		EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc goodItem;
		NctsHeader header;
		IUnloadedGoodsItem wrapper;
	}
}
