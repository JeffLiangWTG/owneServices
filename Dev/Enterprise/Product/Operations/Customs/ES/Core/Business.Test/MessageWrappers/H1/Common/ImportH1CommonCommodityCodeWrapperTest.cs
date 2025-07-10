using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonCommodityCodeWrapperTest : WrapperHelperTest<ImportH1CommonCommodityCodeWrapper>
{
	public void TestTaricCode()
	{
		invoiceLine.JI_Tariff = "2203001023";
		AssertEquals("Expected filled TaricCode", "23", wrapper.TaricCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		invoiceLine = Factory.New<JobComInvoiceLine>();
		wrapper = GetWrapper(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	ImportH1CommonCommodityCodeWrapper wrapper;

	ImportH1CommonCommodityCodeWrapper GetWrapper(JobComInvoiceLine invoiceLine) => new ImportH1CommonCommodityCodeWrapper(invoiceLine);

	protected override ImportH1CommonCommodityCodeWrapper GetProvider() => wrapper;
}
