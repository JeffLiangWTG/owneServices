using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class InvoiceHeaderGroupDefinitionProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_NullParameter()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("null attribute", () => new InvoiceHeaderGroupingDefinitionProvider(null, Factory));
				AssertExceptionThrown<ArgumentNullException>("null factory", () => new InvoiceHeaderGroupingDefinitionProvider(attribute, null));
			});
		}

		public void TestLinePriceCurrency()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid CurrencyCode", "USD", provider.LinePriceCurrency);

				var addInfo = "LinePriceCurrency=XXX";
				attribute = helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo, "EN00123", 1);
				provider = new InvoiceHeaderGroupingDefinitionProvider(attribute, Factory);
				AssertEquals("Invalid CurrencyCode", ZString.Empty, provider.LinePriceCurrency);
			});
		}

		public void TestIncotermCode()
		{
			AssertEquals("FOB", provider.IncotermCode);
		}

		public void TestIncotermPlace()
		{
			AssertEquals("Frankfurt", provider.IncotermPlace);
		}

		public void TestTransNature()
		{
			AssertEquals("21", provider.TransNature);
		}

		public void TestInvoiceDate()
		{
			AssertEquals(new ZDateTime(2023, 8, 21), provider.InvoiceDate);
		}

		public void TestInvoiceNumber()
		{
			AssertEquals("1", provider.InvoiceNumber);
		}

		protected override void SetUp()
		{
			helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
			whsReceiveLine = helper.GetNewWhsReceiveLine(whsReceive.PK, helper.Part.PK, "PACKAGE1", 100m, 1000m, 900m, bondedEntryKey: "EN00123-1");
			var addInfo = "LinePriceCurrency=USD*IncotermCode=FOB*IncotermPlace=Frankfurt*TransNature=21*InvoiceDate=21-Aug-23*InvoiceNumber=1";
			attribute = helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 1000m, 50m, "KG", ZString.Empty, 100m, "NO", addInfo, "EN00123", 1);
			provider = new InvoiceHeaderGroupingDefinitionProvider(attribute, Factory);
		}
		WhsDataTestHelper helper;
		InvoiceHeaderGroupingDefinitionProvider provider;
		IWhsBondedWarehouseAttribute attribute;
		IWhsReceiveLine whsReceiveLine;
	}
}
