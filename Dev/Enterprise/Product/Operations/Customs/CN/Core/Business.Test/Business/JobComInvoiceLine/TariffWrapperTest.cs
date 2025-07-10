using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class TariffWrapperTest : TestCaseWithFactory
	{
		public void SetTariffCodeTest()
		{
			AssertEquals("Tariff code should be set.", "0000000020", iTariff.Code);
		}

		public void SetTariffDescriptionTest()
		{
			AssertEquals("Tariff Description should be set.", "This is Description!", iTariff.Description);
		}

		public void SetTariffUQ1Test()
		{
			AssertEquals("Tariff UQ1 should be set.", "001", iTariff.UQ1);
		}

		public void SetTariffUQ2Test()
		{
			AssertEquals("Tariff UQ2 should be set.", "035", iTariff.UQ2);
		}

		public void SetTariffUQ3Test()
		{
			AssertEquals("Tariff UQ3 should be set.", string.Empty, iTariff.UQ3);
		}

		public void SetTariffUQ4Test()
		{
			AssertEquals("Tariff UQ4 should be set.", string.Empty, iTariff.UQ4);
		}

		public void SetTariffUQ5Test()
		{
			AssertEquals("Tariff UQ5 should be set.", string.Empty, iTariff.UQ5);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shbType = helper.CreateTariffType("CN", "HSN");
			Factory.Save();

			var tariff = helper.CreateTariff("CN", shbType.PK, "0000000020", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "This is Description!");
			helper.CreateTariffUOM(tariff, "CU1", "001");
			helper.CreateTariffUOM(tariff, "CU2", "035");
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000020";
			tariffWrapper = new TariffWrapper(invoiceLine);

			iTariff = tariffWrapper;
		}

		TariffWrapper tariffWrapper;
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		ITariff iTariff;
	}
}
