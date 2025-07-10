using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportLicense.Testing
{
	public class DrawbackNcmItemDetailProviderTest : TestCaseWithFactory
	{
		public void TestDrawbackItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 1412.00m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			AddLine(invoice);

			var provider = DrawbackNcmItemDetailProvider.New(invoice.InvoiceLines[0]);

			AssertEquals("1", provider.LineNumber);
			AssertEquals("Test Description", provider.GoodsDescription);
			AssertEquals(10m, provider.NetWeight);
			AssertEquals(10m, provider.InvoiceQty);
			AssertEquals("Quilogramas", provider.InvoiceQuantityUQDescription);
			AssertEquals(10m, provider.CustomsQty);
			AssertEquals(500m, provider.FobValue);
			AssertEquals(50m, provider.UnitValue);
			AssertEquals("1", provider.DrawbackAcItem);
			AssertEquals("TestBrand", provider.Brand);
			AssertEquals("TestModel", provider.Model);
			AssertEquals("2022", provider.ManufactureYear);
			AssertEquals("123", provider.SerialNumber);

			provider = DrawbackNcmItemDetailProvider.New(invoice.InvoiceLines[1]);

			AssertEquals("2", provider.LineNumber);
			AssertEquals("Test Description2", provider.GoodsDescription);
			AssertEquals(30m, provider.NetWeight);
			AssertEquals("Quilogramas", provider.InvoiceQuantityUQDescription);
			AssertEquals(30m, provider.InvoiceQty);
			AssertEquals(30m, provider.CustomsQty);
			AssertEquals(1000m, provider.FobValue);
			AssertEquals(33.3333333m, provider.UnitValue);
			AssertEquals("", provider.DrawbackAcItem);
			AssertEquals("TestBrand2", provider.Brand);
			AssertEquals("TestModel2", provider.Model);
			AssertEquals("2022", provider.ManufactureYear);
			AssertEquals("1232", provider.SerialNumber);
		}

		void AddLine(JobComInvoiceHeader invoice)
		{
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;
			line.JI_LineNo = 1;
			line.JI_Description = "Test Description";
			line.JI_Weight = 11m;
			line.JI_NetWeight = 10m;
			line.JI_InvoiceQuantity = 10m;
			line.JI_InvoiceUQ = "KG";
			line.JI_CustomsQuantity = 10m;
			line.JI_LinePrice = 500m;
			line.JI_BrandName = "TestBrand";
			line.JI_Model = "TestModel";
			line.JI_UsedMaterialSerialNumber = "123";
			line.JI_UsedMaterialManufactureYear = "2022";

			line.DrawbackImportLicense.CSI_ItemNumber = 1;

			line = invoice.InvoiceLines.AddNew();
			line.JI_LineNo = 2;
			line.JI_Description = "Test Description2";
			line.JI_Weight = 31m;
			line.JI_NetWeight = 30m;
			line.JI_InvoiceQuantity = 30m;
			line.JI_InvoiceUQ = "KG";
			line.JI_CustomsQuantity = 30m;
			line.JI_LinePrice = 1000m;
			line.JI_BrandName = "TestBrand2";
			line.JI_Model = "TestModel2";
			line.JI_UsedMaterialSerialNumber = "1232";
			line.JI_UsedMaterialManufactureYear = "2022";

			line.DrawbackImportLicense.CSI_ItemNumber = 0;
		}
	}
}
