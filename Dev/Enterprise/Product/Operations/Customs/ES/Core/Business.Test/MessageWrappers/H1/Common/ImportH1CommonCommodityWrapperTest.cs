using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonCommodityWrapperTest : WrapperHelperTest<ImportH1CommonCommodityWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if invoiceLine is null", typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: invoiceLine", () => GetWrapper(null));
		});
	}

	public void TestGoodsDescription()
	{
		invoiceLine.JI_Description = "description\r\nof\ngoods";
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled GoodsDescription", "description of goods", wrapper.GoodsDescription);

			invoiceLine.JI_Description = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			var expectedTrimDescription = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
			AssertEquals("Expected filled GoodsDescription", expectedTrimDescription, wrapper.GoodsDescription);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		invoiceLine = Factory.New<JobComInvoiceLine>();
		wrapper = GetWrapper(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	ImportH1CommonCommodityWrapper wrapper;

	ImportH1CommonCommodityWrapper GetWrapper(JobComInvoiceLine invoiceLine) => new ImportH1CommonCommodityWrapper(invoiceLine);

	protected override ImportH1CommonCommodityWrapper GetProvider() => wrapper;
}
