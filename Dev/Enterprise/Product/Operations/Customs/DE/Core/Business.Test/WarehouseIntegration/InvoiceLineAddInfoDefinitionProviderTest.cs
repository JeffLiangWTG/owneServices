using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class InvoiceLineAddInfoDefinitionProviderTest : TestCaseWithFactory
	{
		public void TestCountryOfSupply()
		{
			AssertEquals("DE", provider.CountryOfSupply);
		}

		public void TestLineNetPrice()
		{
			AssertEquals(100m, provider.LineNetPrice);
		}

		public void TestLinePrice()
		{
			AssertEquals(10m, provider.LinePrice);
		}

		protected override void SetUp()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
			var whsReceiveLine = helper.GetNewWhsReceiveLine(whsReceive.PK, helper.Part.PK, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			var addInfo = "LinePrice=10*CountryOfSupply=DE*LineNetPrice=100";
			var attribute = helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo, "EN00123", 1);
			provider = new InvoiceLineAddInfoDefinitionProvider(attribute);
		}
		InvoiceLineAddInfoDefinitionProvider provider;
	}
}
