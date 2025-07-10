using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CommodityCodeCommonWrapperTest : WrapperHelperTest<CommodityCodeCommonWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if invoiceLine is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","invoiceLine"), () => GetWrapper(null));
		}

		public void TestTariffCode()
		{
			invoiceLine.JI_Tariff = "2203001023";
			AssertEquals("Expected filled TariffCode", "220300", wrapper.TariffCode);
		}

		public void TestTariffCodeCombined()
		{
			invoiceLine.JI_Tariff = "2203001023";
			AssertEquals("Expected filled TariffCodeCombined", "10", wrapper.TariffCodeCombined);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			wrapper = GetWrapper(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		CommodityCodeCommonWrapper wrapper;

		CommodityCodeCommonWrapper GetWrapper(JobComInvoiceLine invoiceLine) => new CommodityCodeCommonWrapper(invoiceLine);

		protected override CommodityCodeCommonWrapper GetProvider() => wrapper;
	}
}
