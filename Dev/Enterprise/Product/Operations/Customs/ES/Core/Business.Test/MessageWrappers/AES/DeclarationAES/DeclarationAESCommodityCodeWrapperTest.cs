using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationAESCommodityCodeWrapperTest : WrapperHelperTest<DeclarationAESCommodityCodeWrapper>
	{
		public void TestTariffAdditionalCodes()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TariffAdditionalCodes", 0, wrapper.TariffAdditionalCodes.Count);

				invoiceLine.JI_SupplementaryCode1 = "AAAA";
				wrapper = GetWrapper(invoiceLine);
				AssertEquals("Expected filled TariffAdditionalCodes when at least one supplementary code is not empty", 1, wrapper.TariffAdditionalCodes.Count);

				invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
				wrapper = GetWrapper(invoiceLine);
				AssertEquals("Expected empty TariffAdditionalCodes when both supplementary codes are empty", 0, wrapper.TariffAdditionalCodes.Count);

				invoiceLine.JI_SupplementaryCode2 = "BBBB";

				wrapper = GetWrapper(invoiceLine);
				var tariffAdditionalCodes = wrapper.TariffAdditionalCodes;
				AssertEquals("Expected filled TariffAdditionalCodes", 1, tariffAdditionalCodes.Count);
				AssertSame("Cached TariffSupplementaryCodes", wrapper.TariffAdditionalCodes, tariffAdditionalCodes);
			});
		}

		public void TestNationalAdditionalCodes()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty NationalAdditionalCodes", 0, wrapper.NationalAdditionalCodes.Count);

				invoiceLine.AdditionalSupplementaryCodes.AddNew("AA");

				wrapper = GetWrapper(invoiceLine);
				var nationalAdditionalCodes = wrapper.NationalAdditionalCodes;
				AssertEquals("Expected filled NationalAdditionalCodes when there is at least one additional supplementary code", 1, nationalAdditionalCodes.Count);
				AssertSame("Cached NationalAdditionalCodes", wrapper.NationalAdditionalCodes, nationalAdditionalCodes);
			});
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
		DeclarationAESCommodityCodeWrapper wrapper;

		DeclarationAESCommodityCodeWrapper GetWrapper(JobComInvoiceLine invoiceLine) => new DeclarationAESCommodityCodeWrapper(invoiceLine);

		protected override DeclarationAESCommodityCodeWrapper GetProvider() => wrapper;
	}
}
