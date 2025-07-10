using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGARequirementProviderTest : TestCaseWithFactory
	{
		public void TestTariffNoAndEffectiveDate()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", "3333333333");
			var invoiceLine01 = Factory.New<JobComInvoiceLine>();
			invoiceLine01.JI_Tariff = "3333333333";
			var provider01 = new PGARequirementProvider(invoiceLine01);

			var invoiceLine02 = Factory.New<JobComInvoiceLine>();
			invoiceLine02.JI_Tariff = "3333333333";
			var provider02 = new PGARequirementProvider(invoiceLine02);

			Assert(provider01.TariffNo.Equals(provider02.TariffNo));
			Assert(provider01.TariffEffectiveDate.Equals(provider02.TariffEffectiveDate));
		}
	}
}
