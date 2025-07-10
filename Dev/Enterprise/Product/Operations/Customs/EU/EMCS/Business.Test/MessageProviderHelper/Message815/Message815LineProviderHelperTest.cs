using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message815LineProviderHelperTest : TestCaseWithFactory
	{
		public void TestTariff()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", string.Empty, helper.Tariff);
				emcsInvoiceLine.JI_Tariff = "22042926";
				AssertEquals("Returns Correct value", "22042926", helper.Tariff);
			});
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", decimal.Zero, helper.Quantity);
				emcsInvoiceLine.JI_CustomsQuantity = 3.13m;
				AssertEquals("Returns value", 3.13m, helper.Quantity);
				emcsInvoiceLine.JI_CustomsQuantity = 3.00m;
				AssertEquals("Returns Integer", "3", helper.Quantity.ToString());
				emcsInvoiceLine.JI_CustomsQuantity = 10.2000m;
				AssertEquals("Value is normalized", "10.2", helper.Quantity.ToString());
			});
		}

		public void TestFiscalMarkUsedFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", false, helper.FiscalMarkUsedFlag);
				emcsInvoiceLine.ZG_FiscalMarkUsed = true;
				AssertEquals("Returns Correct value", true, helper.FiscalMarkUsedFlag);
			});
		}

		public void TestGrossWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, helper.GrossWeight);
				emcsInvoiceLine.JI_Weight = 123.89m;
				AssertEquals("Returns Correct value", 123.89m, helper.GrossWeight);
				emcsInvoiceLine.JI_Weight = 123.00m;
				AssertEquals("Value is integer", "123", helper.GrossWeight.ToString());
				emcsInvoiceLine.JI_Weight = 123.500m;
				AssertEquals("Value is normalized", "123.5", helper.GrossWeight.ToString());
			});
		}

		public void TestNetWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, helper.NetWeight);
				emcsInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				emcsInvoiceLine.JI_NetWeight = 23.45m;
				AssertEquals("Returns Correct value", 23.45m, helper.NetWeight);
				emcsInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
				AssertEquals("Using the convert to KGM proeprty", 10.636741m, helper.NetWeight);

				emcsInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				emcsInvoiceLine.JI_NetWeight = 200.200m;
				AssertEquals("Normalized", "200.2", helper.NetWeight.ToString());
				emcsInvoiceLine.JI_NetWeight = 200.000m;
				AssertEquals("Normalized", "200", helper.NetWeight.ToString());
			});
		}

		public void TestAlcoholicStrength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, helper.AlcoholicStrength);
				emcsInvoiceLine.ZG_AlcoholicStrength = 12.3m;
				AssertEquals("Returns Correct value", 12.3m, helper.AlcoholicStrength);

				emcsInvoiceLine.ZG_AlcoholicStrength = 13.300m;
				AssertEquals("Normalized", "13.3", helper.AlcoholicStrength.ToString());
				emcsInvoiceLine.ZG_AlcoholicStrength = 10.000m;
				AssertEquals("Normalized", "10", helper.AlcoholicStrength.ToString());
			});
		}

		public void TestDegreePlato()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, helper.DegreePlato);
				emcsInvoiceLine.ZG_DegreePlato = 5.1m;
				AssertEquals("Returns Correct value", 5.1m, helper.DegreePlato);

				emcsInvoiceLine.ZG_DegreePlato = 13.300m;
				AssertEquals("Normalized", "13.3", helper.DegreePlato.ToString());
				emcsInvoiceLine.ZG_DegreePlato = 10.000m;
				AssertEquals("Normalized", "10", helper.DegreePlato.ToString());
			});
		}

		public void TestSizeOfProducer()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, helper.SizeOfProducer);
				emcsInvoiceLine.ZG_SizeOfProducer = 10000m;
				AssertEquals("Returns Correct value", 10000m, helper.SizeOfProducer);
			});
		}

		public void TestDensity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, helper.Density);
				emcsInvoiceLine.ZG_Density = 2.3m;
				AssertEquals("Returns Correct value", 2.3m, helper.Density);

				emcsInvoiceLine.ZG_Density = 13.300m;
				AssertEquals("Normalized", "13.3", helper.Density.ToString());
				emcsInvoiceLine.ZG_Density = 10.000m;
				AssertEquals("Normalized", "10", helper.Density.ToString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			helper = new Message815LineProviderHelper(emcsInvoiceLine);
		}
		EMCSJobDeclaration emcsDeclaration;
		EMCSJobComInvoiceLine emcsInvoiceLine;
		Message815LineProviderHelper helper;
	}
}
