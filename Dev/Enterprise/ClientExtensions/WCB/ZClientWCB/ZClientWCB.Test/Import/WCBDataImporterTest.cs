using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB.Testing
{
	sealed class WCBDataImporterTest : FlatFileDataImporterTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string PathToTestFile
		{
			get
			{
				return BaseSourcePath + @"Enterprise\ClientExtensions\WCB\ZClientWCB\ZClientWCB.Test\TestFiles\TestFile.txt";
			}
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			return new WCBDataImporter(jobDec);
		}

		public void TestExtractToAdapter()
		{
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();
			WCBDataImporterTestClass importer = new WCBDataImporterTestClass(jobDeclaration);
			Xsd.InvoiceHeader invoiceHeader = new Xsd.InvoiceHeader();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.InvoiceLines[0].ProductNumber = "111111";
			invoiceHeader.InvoiceLines[0].InvoiceQty = new Xsd.DimensionValue();
			invoiceHeader.InvoiceLines[0].InvoiceQty.Value = 112m;
			invoiceHeader.InvoiceLines[0].LinePrice = new Xsd.FinancialValue();
			invoiceHeader.InvoiceLines[0].LinePrice.Value = 1111.11m;
			invoiceHeader.InvoiceLines[0].OrderNumber = "11111";
			invoiceHeader.InvoiceLines[0].LineClassification.OriginOfGoods = "US";
			invoiceHeader.InvoiceNumber = "11111111";
			INotifications notification = new NotificationBuffer();
			importer.ExtractToDataAdapter(invoiceHeader, notification);
			AssertEquals("Number of Invoices should be 1", 1, importer.JobDeclaration.Invoices.Count);
			JobComInvoiceHeader aUInvHeader = importer.JobDeclaration.Invoices[0] as JobComInvoiceHeader;
			AssertNotNull(aUInvHeader);
			BaseJobComInvoiceLine aUInvLine = importer.JobDeclaration.InvoiceLines[0];
			AssertEquals("Invoice Number for 1st Invoice", "11111111", aUInvLine.InvoiceNumber);
			AssertEquals("Invoice Amount", 1111.11m, aUInvLine.JI_LinePrice);
			AssertEquals("Order Number", "11111", aUInvLine.JI_OrderNumber);
			AssertEquals("Invoice Quantity", 112m, aUInvLine.JI_InvoiceQuantity);
			AssertEquals("Product/Part Number", "111111", aUInvLine.JI_PartNo);
			AssertEquals("Origin of Goods", "US", aUInvLine.JI_CountryOfOrigin);
		}
	}
}
