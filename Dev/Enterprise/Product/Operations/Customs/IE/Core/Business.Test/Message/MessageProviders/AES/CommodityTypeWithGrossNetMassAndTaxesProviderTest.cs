namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class CommodityTypeWithGrossNetMassAndTaxesProviderTest : CommodityTypeProviderTest<CommodityTypeWithGrossNetMassAndTaxesProvider>
	{
		public void TestGrossMass()
		{
			var line1 = invoiceLines[0];
			line1.JI_Weight = 10m;
			line1.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			var line2 = invoiceLines[1];
			line2.JI_Weight = 100m;
			line2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("GrossMass", 10100m, Provider.GrossMass);
		}

		public void TestNetMass()
		{
			var line1 = invoiceLines[0];
			line1.JI_CustomsQuantity = 200m;
			var line2 = invoiceLines[1];
			line2.JI_CustomsQuantity = 22m;

			AssertEquals("NetMass", 222m, Provider.NetMass);
		}

		public void TestCalculationOfTaxes()
		{
			AssertNull("CalculationOfTaxes empty for now.", Provider.CalculationOfTaxes);
		}

		protected override CommodityTypeWithGrossNetMassAndTaxesProvider GetProvider() => new CommodityTypeWithGrossNetMassAndTaxesProvider(entryLineWrapper);
	}
}
