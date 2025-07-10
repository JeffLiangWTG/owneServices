using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class IncompleteImportH1CommodityWrapperTest : WrapperHelperTest<IncompleteImportH1CommodityWrapper>
{
	public void TestCommodityCode()
	{
		var commodityCode = wrapper.CommodityCode;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled CommodityCode", commodityCode);
			AssertSame("Cached CommodityCode", wrapper.CommodityCode, commodityCode);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		invoiceLine = Factory.New<JobComInvoiceLine>();
		wrapper = GetWrapper(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	IncompleteImportH1CommodityWrapper wrapper;

	IncompleteImportH1CommodityWrapper GetWrapper(JobComInvoiceLine invoiceLine) => new IncompleteImportH1CommodityWrapper(invoiceLine);

	protected override IncompleteImportH1CommodityWrapper GetProvider() => wrapper;
}
