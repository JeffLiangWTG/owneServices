using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class DeclarationH7LineWrapperTest : DataProviderTestCase<DeclarationH7LineWrapper>
	{
		public void TestNumberOfPackages()
		{
			AssertEquals(3, wrapper.NumberOfPackages);
		}

		public void TestComplementaryUnitsQty()
		{
			item.API_CustomsQty2 = 2;
			AssertEquals(2M, wrapper.ComplementaryUnitsQty);
		}

		public void TestGoodsDescription()
		{
			item.API_GoodsDescription = "GoodsDescription";
			AssertEquals("GoodsDescription", wrapper.GoodsDescription);
		}

		public void TestLineNumber()
		{
			item.SequenceNumber = 1;
			AssertEquals(1, wrapper.LineNumber);
		}

		public void TestCommodityCode()
		{
			item.API_Tariff = "1234567890";
			AssertEquals("12345678", wrapper.CommodityCode);

			item.API_Tariff = "1234567";
			AssertEquals("123456", wrapper.CommodityCode);
		}

		public void TestGrossWeight()
		{
			item.API_GrossWeight = 100;
			item.API_GrossWeightUQ = Core.Constants.Weight.Grams;

			AssertEquals(0.1M, wrapper.GrossWeight);
		}

		public void TestValue()
		{
			AssertType<LineValueWrapper>(wrapper.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			var bill = header.Bills.AddNew();
			item = bill.PackedItems.AddNew();

			wrapper = new DeclarationH7LineWrapper(item, 3);
		}

		protected override DeclarationH7LineWrapper GetProvider()
		{
			return wrapper;
		}

		DeclarationH7LineWrapper wrapper;
		AsycudaPackedItem item;
	}
}
